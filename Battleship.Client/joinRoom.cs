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
                var packet = Packet.Create(PacketType.GetRoomListRequest, string.Empty);
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
            if (packet.Type == PacketType.GetRoomListResponse)
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
                // 1. SỬA LẠI KIỂU DTO: Nhận JoinRoomResponseDto thay vì RoomUpdateDto
                var response = packet.DeserializePayload<JoinRoomResponseDto>();

                // Kiểm tra cờ Success thay vì chỉ check Room khác null
                if (response != null && response.Success && response.Room != null)
                {
                    this.Invoke(new Action(() =>
                    {
                        // 2. KHỞI TẠO VÀ HIỂN THỊ MÀN HÌNH ROOMWAIT
                        RoomWait pnlRoomWait = new RoomWait();

                        if (this.Parent != null)
                        {
                            // Căn giữa màn hình RoomWait trên Lobby (Parent)
                            int centerX = (this.Parent.Width - pnlRoomWait.Width) / 2;
                            int centerY = (this.Parent.Height - pnlRoomWait.Height) / 2;
                            pnlRoomWait.Location = new Point(centerX, centerY);

                            this.Parent.Controls.Add(pnlRoomWait);
                            pnlRoomWait.BringToFront();
                        }

                        // Xóa màn hình joinRoom hiện tại
                        this.Parent?.Controls.Remove(this);
                        this.Dispose();
                    }));
                }
                else
                {
                    this.Invoke(new Action(() =>
                    {
                        // Hiển thị lý do lỗi từ Server (ví dụ: Phòng đã đầy, Chủ phòng đã thoát...)
                        string errorMsg = response?.Message ?? "Không thể tham gia phòng này!";
                        MessageBox.Show(errorMsg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        // Làm mới lại danh sách phòng vì dữ liệu hiện tại có thể đã cũ
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