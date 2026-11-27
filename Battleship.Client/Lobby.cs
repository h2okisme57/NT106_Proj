using Battleship.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Battleship.Client
{
    public partial class Lobby : UserControl
    {
        private bool isLoggedIn = false;
        private joinRoom pnlJoinRoom;
        private Profile pnlProfile;
        private FlowLayoutPanel chatPanel;


        // màu cho logo Battleship
        private Font _logoFont;

        private static readonly Color LogoTop = Color.FromArgb(255, 210, 76);    // #FFD24C
        private static readonly Color LogoBottom = Color.FromArgb(232, 59, 59);  // #E83B3B
        private static readonly Color LogoExtrude = Color.FromArgb(122, 20, 34); // #7A1422
        private static readonly Color LogoOutline = Color.FromArgb(8, 14, 28);   // #080E1C

        private static readonly Size LockedSize = new Size(978, 641);

        public Lobby()
        {
            InitializeComponent();

            // Tắt tự động co giãn theo DPI/Font của Windows
            this.AutoScaleMode = AutoScaleMode.None;
            this.Dock = DockStyle.None;
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // Khóa cứng kích thước UserControl
            this.MinimumSize = LockedSize;
            this.MaximumSize = LockedSize;
            this.Size = LockedSize;

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            this.Load += Lobby_Load;
            boxChat.KeyDown += boxChat_KeyDown;
        }

        private void Lobby_Load(object sender, EventArgs e)
        {
            Theme.ApplyRetroFont(this);
            boxChat.BorderStyle = BorderStyle.None;
            CreateTransparentChatBox();

            Color textColorBtn = ColorTranslator.FromHtml("#F4D35E");
            _logoFont = FontManager.GetFont(40f, FontStyle.Bold);

            if (lblBattleshiptxt != null)
            {
                lblBattleshiptxt.Font = _logoFont;
                lblBattleshiptxt.Visible = false;
            }

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.UseCompatibleTextRendering = true;
                    btn.Font = FontManager.GetFont(14f, FontStyle.Bold);
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = textColorBtn;
                    btn.TextAlign = ContentAlignment.MiddleLeft;
                    btn.Padding = new Padding(30, 0, 0, 0);
                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 22, 46, 147);
                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 26, 25, 83);
                }
            }

            // =========================================================
            // LẮNG NGHE TIN NHẮN CHAT TỪ NETWORKMANAGER
            // =========================================================
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnPacketReceived += NetworkManager_OnPacketReceived;
            }
        }

        // Đăng ký hủy sự kiện khi UserControl bị Dispose/đóng
        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnPacketReceived -= NetworkManager_OnPacketReceived;
            }
            base.OnHandleDestroyed(e);
        }

        // =========================================================
        // NHẬN PACKET BẤT ĐỒNG BỘ TỪ SERVER
        // =========================================================
        private void NetworkManager_OnPacketReceived(Packet packet)
        {
            if (packet.Type == PacketType.ChatMessage)
            {
                try
                {
                    var chatMsg = packet.DeserializePayload<ChatMessageDto>();
                    if (chatMsg != null)
                    {
                        AddChatMessage(chatMsg.Sender, chatMsg.Content);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi giải mã tin nhắn: {ex.Message}");
                }
            }
        }

        public void CreateTransparentChatBox()
        {
            chatPanel = new FlowLayoutPanel();
            chatPanel.Name = "chatPanel";
            chatPanel.Location = new Point(566, 213);
            chatPanel.Size = new Size(412, 400);
            chatPanel.AutoScroll = true;
            chatPanel.BackColor = Color.FromArgb(120, 0, 0, 0);
            chatPanel.FlowDirection = FlowDirection.TopDown;
            chatPanel.WrapContents = false;

            // Ẩn chatPanel khi khởi tạo
            chatPanel.Visible = false;

            this.Controls.Add(chatPanel);

            // Gán sự kiện hiển thị khi bấm/focus vào boxChat
            boxChat.Enter += TextBox1_Enter;

            // Ẩn khi click ra vùng trống trên Lobby
            this.Click += Lobby_Click;


        }

        private void AddChatMessage(string sender, string content)
        {
            if (chatPanel.IsDisposed || !chatPanel.IsHandleCreated) return;

            // Dùng Invoke để đảm bảo cập nhật UI trên Main Thread
            chatPanel.Invoke(new Action(() =>
            {
                Label msgLabel = new Label();
                msgLabel.Text = $"[{sender}]: {content}";
                msgLabel.Font = FontManager.GetFont(8.5f, FontStyle.Regular);
                msgLabel.BackColor = Color.Transparent;
                msgLabel.ForeColor = Color.White;
                msgLabel.AutoSize = true;
                msgLabel.MaximumSize = new Size(370, 0);

                chatPanel.Controls.Add(msgLabel);

                // Tự động hiển thị khung chat khi có tin nhắn mới tới
                chatPanel.Visible = true;

                chatPanel.ScrollControlIntoView(msgLabel);
            }));
        }

        // =========================================================
        // GỬI TIN NHẮN CHAT LÊN SERVER
        // =========================================================
        private async void boxChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Ngăn tiếng píp beep của Windows
                string messageText = boxChat.Text.Trim();

                if (string.IsNullOrWhiteSpace(messageText)) return;

                if (!NetworkManager.Instance.IsConnected)
                {
                    AddChatMessage("Hệ thống", "Chưa kết nối đến máy chủ!");
                    return;
                }

                try
                {
                    var chatMsg = new ChatMessageDto
                    {
                        Sender = PlayerProfile.Username,
                        Content = messageText
                    };

                    var packet = Packet.Create(PacketType.ChatMessage, chatMsg);

                    boxChat.Clear();

                    await NetworkManager.Instance.SendAsync(packet);
                }
                catch (Exception ex)
                {
                    AddChatMessage("Hệ thống", $"Lỗi gửi tin nhắn: {ex.Message}");
                }
            }
        }

        private void DeleteAccess()
        {
            if (pnlJoinRoom != null && this.Controls.Contains(pnlJoinRoom))
            {
                this.Controls.Remove(pnlJoinRoom);
                pnlJoinRoom.Dispose();
                pnlJoinRoom = null;
            }

            if (pnlProfile != null && this.Controls.Contains(pnlProfile))
            {
                this.Controls.Remove(pnlProfile);
                pnlProfile.Dispose();
                pnlProfile = null;
            }
        }

        private bool CheckLogin()
        {
            if (PlayerProfile.CurrentUser != null)
            {
                isLoggedIn = true;
                return true;
            }

            using (Form popupLogin = new Form())
            {
                ucLogin loginScreen = new ucLogin();

                // 1. Đặt vị trí UserControl bám góc (0,0) của Form chứa
                loginScreen.Location = new Point(0, 0);

                // 2. Thêm Control vào Form trước
                popupLogin.Controls.Add(loginScreen);

                // 3. Khóa kích thước Form vừa khít 100% với UserControl
                popupLogin.ClientSize = loginScreen.Size;
                popupLogin.FormBorderStyle = FormBorderStyle.None;
                popupLogin.StartPosition = FormStartPosition.CenterParent;

                // Tùy chọn: Đảm bảo nền Form khớp với nền UserControl
                popupLogin.BackColor = loginScreen.BackColor;

                // 4. Hiển thị Form căn giữa theo Form chính (Lobby)
                DialogResult result = popupLogin.ShowDialog(this.FindForm());

                if (result == DialogResult.OK)
                {
                    isLoggedIn = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private async void btnFindRoom_Click(object sender, EventArgs e)
        {
            if (!CheckLogin()) return;
            DeleteAccess();

            RoomWait pnlRoomWait = new RoomWait();
            pnlRoomWait.SetAsMatchmaking(); // Chế độ tìm trận: cả 2 đều không có quyền Host

            int centerX = (this.Width - pnlRoomWait.Width) / 2;
            int centerY = (this.Height - pnlRoomWait.Height) / 2;
            pnlRoomWait.Location = new Point(centerX, centerY);
            pnlRoomWait.Name = "pnlRoomWait";

            this.Controls.Add(pnlRoomWait);
            pnlRoomWait.BringToFront();

            if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
            {
                try
                {
                    var packet = Packet.Create(PacketType.MatchmakingJoinRequest, new { });
                    await NetworkManager.Instance.SendAsync(packet);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi gửi yêu cầu tìm trận: {ex.Message}", "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Chưa kết nối đến máy chủ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnCreateRoom_Click(object sender, EventArgs e)
        {
            if (!CheckLogin()) return;
            DeleteAccess();

            RoomWait pnlRoomWait = new RoomWait();
            pnlRoomWait.SetAsHost(); // Tạo phòng mới -> có quyền Host

            int centerX = (this.Width - pnlRoomWait.Width) / 2;
            int centerY = (this.Height - pnlRoomWait.Height) / 2;
            pnlRoomWait.Location = new Point(centerX, centerY);
            pnlRoomWait.Name = "pnlRoomWait";

            this.Controls.Add(pnlRoomWait);
            pnlRoomWait.BringToFront();

            if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
            {
                try
                {
                    string hostName = PlayerProfile.Username ?? "Bạn";

                    var createRoomDto = new CreateRoomDto
                    {
                        RoomName = $"Phòng của {hostName}"
                    };

                    var packet = Packet.Create(PacketType.CreateRoomRequest, createRoomDto);
                    await NetworkManager.Instance.SendAsync(packet);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi gửi yêu cầu tạo phòng: {ex.Message}", "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Mất kết nối với máy chủ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                this.Controls.Remove(pnlRoomWait);
                pnlRoomWait.Dispose();
            }
        }

        private void btnJoinRoom_Click(object sender, EventArgs e)
        {
            if (!CheckLogin()) return;
            DeleteAccess();

            if (pnlJoinRoom != null && this.Controls.Contains(pnlJoinRoom))
            {
                pnlJoinRoom.BringToFront();
                return;
            }

            pnlJoinRoom = new joinRoom();
            int centerX = (this.Width - pnlJoinRoom.Width) / 2;
            int centerY = (this.Height - pnlJoinRoom.Height) / 2;
            pnlJoinRoom.Location = new Point(centerX, centerY);
            pnlJoinRoom.BorderStyle = BorderStyle.FixedSingle;

            this.Controls.Add(pnlJoinRoom);
            pnlJoinRoom.BringToFront();
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            if (!CheckLogin()) return;
            DeleteAccess();

            pnlProfile = new Profile();
            int centerX = (this.Width - pnlProfile.Width) / 2;
            int centerY = (this.Height - pnlProfile.Height) / 2;
            pnlProfile.Location = new Point(centerX, centerY);
            pnlProfile.BorderStyle = BorderStyle.FixedSingle;

            this.Controls.Add(pnlProfile);
            pnlProfile.BringToFront();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnLeaderBoard_Click_1(object sender, EventArgs e)
        {
            DeleteAccess();

            frmLeaderboard leaderboardForm = new frmLeaderboard();
            leaderboardForm.ShowDialog();
        }

        private void TextBox1_Enter(object sender, EventArgs e)
        {
            if (chatPanel != null)
            {
                chatPanel.Visible = true;
                chatPanel.BringToFront();
            }
        }

        private void Lobby_Click(object sender, EventArgs e)
        {
            // Nếu bấm ra ngoài boxChat và chatPanel thì ẩn chatbox
            if (chatPanel != null && !boxChat.Focused)
            {
                chatPanel.Visible = false;
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawBattleshipLogo(e.Graphics);
        }

        private void DrawBattleshipLogo(Graphics g)
        {
            if (lblBattleshiptxt == null || _logoFont == null || string.IsNullOrWhiteSpace(lblBattleshiptxt.Text))
                return;

            GraphicsState state = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var path = new GraphicsPath())
            using (var sf = CreateStringFormat(lblBattleshiptxt.TextAlign))
            {
                // Đổi cỡ chữ từ pt sang px để GraphicsPath khớp kích thước với Label gốc
                float emSize = g.DpiY * _logoFont.SizeInPoints / 72f;
                RectangleF layoutRect = lblBattleshiptxt.Bounds;

                path.AddString(lblBattleshiptxt.Text, _logoFont.FontFamily, (int)_logoFont.Style, emSize, layoutRect, sf);

                RectangleF bounds = path.GetBounds();
                if (bounds.Width <= 0 || bounds.Height <= 0)
                {
                    g.Restore(state);
                    return;
                }

                // 1. Bóng đổ mềm tách logo khỏi mảng mây sáng
                using (var shadowPath = (GraphicsPath)path.Clone())
                using (var shadowMatrix = new Matrix())
                using (var shadowBrush = new SolidBrush(Color.FromArgb(145, 2, 8, 20)))
                {
                    shadowMatrix.Translate(4, 10);
                    shadowPath.Transform(shadowMatrix);
                    g.FillPath(shadowBrush, shadowPath);
                }

                // 2. Khối nổi 3D dày 5px kèm viền xanh đen
                int extrudeDepth = 5;
                using (var outlinePen = new Pen(LogoOutline, 6f) { LineJoin = LineJoin.Round })
                using (var extrudeBrush = new SolidBrush(LogoExtrude))
                {
                    for (int i = extrudeDepth; i >= 1; i--)
                    {
                        using (var layer = (GraphicsPath)path.Clone())
                        using (var m = new Matrix())
                        {
                            m.Translate(0, i);
                            layer.Transform(m);
                            g.DrawPath(outlinePen, layer);
                            g.FillPath(extrudeBrush, layer);
                        }
                    }
                }

                // 3. Viền chính sắc nét quanh mặt chữ
                using (var mainOutlinePen = new Pen(LogoOutline, 5f) { LineJoin = LineJoin.Round })
                {
                    g.DrawPath(mainOutlinePen, path);
                }

                // 4. Tô mặt chữ Gradient dọc vừa khít chiều cao thực tế của chữ
                RectangleF gradRect = new RectangleF(bounds.X, bounds.Y - 1, bounds.Width, bounds.Height + 2);
                using (var gradBrush = new LinearGradientBrush(gradRect, LogoTop, LogoBottom, LinearGradientMode.Vertical))
                {
                    g.FillPath(gradBrush, path);
                }

                // 5. Đường bắt sáng mảnh dọc mép trên mặt chữ
                GraphicsState clipState = g.Save();
                g.SetClip(path);
                using (var innerHighlight = (GraphicsPath)path.Clone())
                using (var m = new Matrix())
                using (var highlightPen = new Pen(Color.FromArgb(120, 255, 255, 255), 1.4f) { LineJoin = LineJoin.Round })
                {
                    m.Translate(0, 1.5f);
                    innerHighlight.Transform(m);
                    g.DrawPath(highlightPen, innerHighlight);
                }
                g.Restore(clipState);
            }

            g.Restore(state);
        }

        private static StringFormat CreateStringFormat(ContentAlignment align)
        {
            var sf = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);

            if (align == ContentAlignment.TopCenter || align == ContentAlignment.MiddleCenter || align == ContentAlignment.BottomCenter)
                sf.Alignment = StringAlignment.Center;
            else if (align == ContentAlignment.TopRight || align == ContentAlignment.MiddleRight || align == ContentAlignment.BottomRight)
                sf.Alignment = StringAlignment.Far;
            else
                sf.Alignment = StringAlignment.Near;

            if (align == ContentAlignment.MiddleLeft || align == ContentAlignment.MiddleCenter || align == ContentAlignment.MiddleRight)
                sf.LineAlignment = StringAlignment.Center;
            else if (align == ContentAlignment.BottomLeft || align == ContentAlignment.BottomCenter || align == ContentAlignment.BottomRight)
                sf.LineAlignment = StringAlignment.Far;
            else
                sf.LineAlignment = StringAlignment.Near;

            return sf;
        }

        private void lblBattleshiptxt_Click(object sender, EventArgs e) { }
        private void richTextBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void Lobby_Load_1(object sender, EventArgs e) { }
        private void btnLeaderboard_Click(object sender, EventArgs e) { }

        private void button1_Click(object sender, EventArgs e)
        {
            frmMain.SwitchToGameScreen();
        }


    }
}