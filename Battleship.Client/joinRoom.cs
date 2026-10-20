using Battleship.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battleship.Client
{
    public partial class joinRoom : UserControl
    {
        public joinRoom()
        {
            InitializeComponent();
        }

        private async void joinRoom_Load(object sender, EventArgs e)
        {
            pnlJoin.AutoScroll = true;
            pnlJoin.BackColor = Color.FromArgb(120, 0, 0, 0);
            pnlJoin.FlowDirection = FlowDirection.TopDown;
            pnlJoin.WrapContents = false;

            pnlJoin.Scroll += (s, ev) => { pnlJoin.Invalidate(); };

            // Đăng ký nhận Packet từ NetworkManager
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnPacketReceived += NetworkManager_OnPacketReceived;
            }

            // Tải danh sách phòng từ Server khi vừa mở màn hình
            await RequestRoomListAsync();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // Hủy đăng ký sự kiện khi UserControl bị đóng để tránh leak bộ nhớ
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnPacketReceived -= NetworkManager_OnPacketReceived;
            }
            base.OnHandleDestroyed(e);
        }

        // =========================================================
        // GỬI YÊU CẦU LẤY DANH SÁCH PHÒNG LÊN SERVER
        // =========================================================
        private async Task RequestRoomListAsync()
        {
            ClearRoomList();

            if (!NetworkManager.Instance.IsConnected) return;

            try
            {
                var packet = Packet.Create(PacketType.GetRoomsRequest, string.Empty);
                await NetworkManager.Instance.SendAsync(packet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[joinRoom] Lỗi gửi yêu cầu danh sách phòng: {ex.Message}");
            }
        }

        // =========================================================
        // NHẬN PACKET BẤT ĐỒNG BỘ TỪ SERVER
        // =========================================================
        private void NetworkManager_OnPacketReceived(Packet packet)
        {
            if (packet.Type == PacketType.GetRoomsResponse)
            {
                var response = packet.DeserializePayload<RoomListResponseDto>();

                ClearRoomList();

                if (response?.Rooms != null && response.Rooms.Count > 0)
                {
                    foreach (var room in response.Rooms)
                    {
                        // Tính số người hiện tại (Host luôn có 1, nếu có Guest nữa là 2)
                        int currentPlayers = string.IsNullOrEmpty(room.GuestName) ? 1 : 2;
                        AddRoomCard(room.RoomId, room.HostName, currentPlayers, room.State);
                    }
                }
            }
            else if (packet.Type == PacketType.JoinRoomResponse)
            {
                var response = packet.DeserializePayload<RoomUpdateDto>();

                if (response?.Room != null)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show($"Vào phòng {response.Room.RoomId} thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Đóng UserControl joinRoom sau khi vào phòng thành công
                        this.Parent?.Controls.Remove(this);
                        this.Dispose();
                    }));
                }
                else
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Không thể tham gia phòng này!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _ = RequestRoomListAsync();
                    }));
                }
            }
        }

        public void ClearRoomList()
        {
            if (pnlJoin.InvokeRequired)
            {
                pnlJoin.Invoke(new Action(ClearRoomList));
                return;
            }

            foreach (Control ctrl in pnlJoin.Controls)
            {
                ctrl.Dispose();
            }
            pnlJoin.Controls.Clear();
        }

        // =========================================================
        // VẼ THẺ PHÒNG MỚI DỰA TRÊN DỮ LIỆU CHUẨN
        // =========================================================
        public void AddRoomCard(string roomId, string hostName, int currentPlayers, RoomState state)
        {
            if (pnlJoin.InvokeRequired)
            {
                pnlJoin.Invoke(new Action(() => AddRoomCard(roomId, hostName, currentPlayers, state)));
                return;
            }

            Panel card = new Panel();
            card.Size = new Size(pnlJoin.Width - 10, 60);
            card.BackColor = Color.FromArgb(80, 255, 255, 255);
            card.Margin = new Padding(5, 5, 5, 5);

            Label lblHost = new Label();
            lblHost.Text = $"Phòng của {hostName} (ID: {roomId})";
            lblHost.ForeColor = Color.White;
            lblHost.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblHost.Location = new Point(10, 18);
            lblHost.AutoSize = true;
            lblHost.BackColor = Color.Transparent;

            Label lblCount = new Label();
            lblCount.Text = $"{currentPlayers}/2";
            lblCount.ForeColor = (currentPlayers == 1 && state == RoomState.Waiting) ? Color.LimeGreen : Color.Red;
            lblCount.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblCount.Location = new Point(card.Width - 140, 18);
            lblCount.AutoSize = true;
            lblCount.BackColor = Color.Transparent;

            Button btnJoin = new Button();
            btnJoin.Text = "Join";
            btnJoin.Size = new Size(80, 35);
            btnJoin.Location = new Point(card.Width - 90, 12);
            btnJoin.BackColor = ColorTranslator.FromHtml("#8B1E2D");
            btnJoin.ForeColor = Color.White;
            btnJoin.FlatStyle = FlatStyle.Flat;
            btnJoin.FlatAppearance.BorderSize = 0;
            btnJoin.Cursor = Cursors.Hand;
            btnJoin.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            // Kiểm tra trạng thái phòng (đầy người hoặc đang chơi)
            if (currentPlayers >= 2 || state != RoomState.Waiting)
            {
                btnJoin.Enabled = false;
                btnJoin.BackColor = Color.Gray;
                btnJoin.Text = state == RoomState.Playing ? "Playing" : "Full";
            }
            else
            {
                btnJoin.Click += async (s, ev) =>
                {
                    btnJoin.Enabled = false;
                    btnJoin.Text = "Đang vào...";

                    // Khởi tạo JoinRoomDto chuẩn từ Shared
                    var joinDto = new JoinRoomDto
                    {
                        RoomId = roomId
                    };

                    var packet = Packet.Create(PacketType.JoinRoomRequest, joinDto);
                    await NetworkManager.Instance.SendAsync(packet);
                };
            }

            card.Controls.Add(lblHost);
            card.Controls.Add(lblCount);
            card.Controls.Add(btnJoin);

            pnlJoin.Controls.Add(card);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Parent?.Controls.Remove(this);
            this.Dispose();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            // Nút Refresh danh sách phòng
            await RequestRoomListAsync();
        }

        private void flowLayoutPanel1_Paint_1(object sender, PaintEventArgs e) { }
    }
}