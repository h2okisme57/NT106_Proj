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
            this.DoubleBuffered = true;
            Applytheme();
            Theme.ApplyRetroFont(this);
        }

        private void Applytheme()
        {
            this.BackColor = Theme.BgBottom;

            lblJoinlgo.AutoSize = false;
            lblJoinlgo.Size = new Size(this.Width - 24, 34);
            lblJoinlgo.Location = new Point(12, 12);
            lblJoinlgo.TextAlign = ContentAlignment.MiddleCenter;
            lblJoinlgo.Font = FontManager.GetFont(13.5f, FontStyle.Bold);
            lblJoinlgo.ForeColor = Theme.Gold;
            lblJoinlgo.BackColor = Color.Transparent;

            pnlJoin.Location = new Point(14, 52);
            pnlJoin.Size = new Size(this.Width - 28, 384);
            pnlJoin.BackColor = Theme.InputBg;

            btnBack.Location = new Point(14, 444);
            btnBack.Size = new Size(this.Width - 28, 34);
            Theme.StyleButton(btnBack, Theme.Slate, Theme.SlateDark, 9.5f);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Theme.DrawCrtBackground(e.Graphics, this.ClientRectangle);
            using (var pen = new Pen(Theme.PanelBorder, 2))
            {
                e.Graphics.DrawRectangle(pen, pnlJoin.Left - 1, pnlJoin.Top - 1, pnlJoin.Width + 1, pnlJoin.Height + 1);
            }
        }

        private async void joinRoom_Load(object sender, EventArgs e)
        {
            pnlJoin.AutoScroll = true;
            pnlJoin.BackColor = Theme.InputBg;
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
                        pnlRoomWait.ApplyRoom(response.Room);   // hiện đúng chủ phòng và người chơi

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

            int cardWidth = pnlJoin.Width - SystemInformation.VerticalScrollBarWidth - 16;
            Panel card = new Panel();
            card.Size = new Size(cardWidth, 60);
            card.BackColor = Theme.PanelBg;
            card.Margin = new Padding(6, 5, 6, 5);
            card.Paint += (s, ev) =>
            {
                using (var pen = new Pen(Theme.PanelBorder, 1))
                {
                    ev.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Font hostFont = FontManager.GetFont(8.5f, FontStyle.Bold);
            int maxHostWidth = card.Width - 154;

            Label lblHost = new Label();
            lblHost.Text = FormatHostRoomText(hostName ?? string.Empty, roomId ?? string.Empty, hostFont, maxHostWidth);
            lblHost.ForeColor = Theme.Text;
            lblHost.Font = hostFont;
            lblHost.Location = new Point(10, 18);
            lblHost.AutoSize = false;
            lblHost.Size = new Size(maxHostWidth, 24);
            lblHost.TextAlign = ContentAlignment.MiddleLeft;
            lblHost.BackColor = Color.Transparent;

            Label lblCount = new Label();
            lblCount.Text = $"{currentPlayers}/2";
            lblCount.ForeColor = (currentPlayers == 1 && state == RoomState.Waiting) ? Theme.Green : Theme.Red;
            lblCount.Font = FontManager.GetFont(9f, FontStyle.Bold);
            lblCount.Location = new Point(card.Width - 142, 19);
            lblCount.AutoSize = true;
            lblCount.BackColor = Color.Transparent;

            Button btnJoin = new Button();
            btnJoin.Text = "Join";
            btnJoin.Size = new Size(84, 34);
            btnJoin.Location = new Point(card.Width - 94, 13);

            // Kiểm tra trạng thái phòng (đầy người hoặc đang chơi)
            if (currentPlayers >= 2 || state != RoomState.Waiting)
            {
                btnJoin.Enabled = false;
                btnJoin.Text = state == RoomState.Playing ? "Playing" : "Full";
                Theme.StyleButton(btnJoin, Theme.Slate, Theme.SlateDark, 8f);
            }
            else
            {
                Theme.StyleButton(btnJoin, Theme.Cyan, Theme.CyanDark, 8.5f);
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

        private static string FormatHostRoomText(string hostName, string roomId, Font font, int maxWidth)
        {
            string prefix = "Phòng của ";
            string suffix = $" (ID: {roomId})";
            string full = prefix + hostName + suffix;

            if (TextRenderer.MeasureText(full, font).Width <= maxWidth)
                return full;

            for (int len = hostName.Length - 1; len >= 0; len--)
            {
                string candidate = prefix + hostName.Substring(0, len) + "..." + suffix;
                if (TextRenderer.MeasureText(candidate, font).Width <= maxWidth)
                    return candidate;
            }

            return prefix + suffix.TrimStart();
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