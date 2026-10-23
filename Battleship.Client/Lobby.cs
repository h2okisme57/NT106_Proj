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

namespace Battleship.Client
{
    public partial class Lobby : UserControl
    {
        private bool isLoggedIn = false;
        private joinRoom pnlJoinRoom;
        private Profile pnlProfile;
        private FlowLayoutPanel chatPanel;

        public Lobby()
        {
            InitializeComponent();
            this.Load += Lobby_Load;
            boxChat.KeyDown += boxChat_KeyDown;
        }

        private void Lobby_Load(object sender, EventArgs e)
        {
            boxChat.BorderStyle = BorderStyle.None;
            CreateTransparentChatBox();

            Color textColorBtn = ColorTranslator.FromHtml("#F4D35E");
            Color textColorLogo = ColorTranslator.FromHtml("#8B1E2D");

            if (lblBattleshiptxt != null)
            {
                lblBattleshiptxt.UseCompatibleTextRendering = true;
                lblBattleshiptxt.Font = FontManager.GetFont(24f, FontStyle.Bold);
                lblBattleshiptxt.BackColor = Color.Transparent;
                lblBattleshiptxt.ForeColor = textColorLogo;
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

        // Đăng ký hủy sự kiện khi UserControl bịDispose/đóng
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

        private void CreateTransparentChatBox()
        {
            chatPanel = new FlowLayoutPanel();
            chatPanel.Name = "chatPanel";
            chatPanel.Location = new Point(566, 213);
            chatPanel.Size = new Size(412, 400);
            chatPanel.AutoScroll = true;
            chatPanel.BackColor = Color.FromArgb(120, 0, 0, 0);
            chatPanel.FlowDirection = FlowDirection.TopDown;
            chatPanel.WrapContents = false;

            this.Controls.Add(chatPanel);
        }

        private void AddChatMessage(string sender, string content)
        {
            if (chatPanel.IsDisposed || !chatPanel.IsHandleCreated) return;

            // Dùng Invoke để đảm bảo cập nhật UI trên Main Thread
            chatPanel.Invoke(new Action(() =>
            {
                Label msgLabel = new Label();
                msgLabel.Text = $"[{sender}]: {content}";
                msgLabel.BackColor = Color.Transparent;
                msgLabel.ForeColor = Color.White;
                msgLabel.AutoSize = true;
                msgLabel.MaximumSize = new Size(370, 0);

                chatPanel.Controls.Add(msgLabel);
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
                    // Tạo DTO chứa nội dung tin nhắn và Tên người gửi từ PlayerProfile
                    var chatMsg = new ChatMessageDto
                    {
                        Sender = PlayerProfile.Username,
                        Content = messageText
                    };

                    var packet = Packet.Create(PacketType.ChatMessage, chatMsg);

                    // Xóa khung chat ngay để nâng cao trải nghiệm người dùng
                    boxChat.Clear();

                    // Gửi bất đồng bộ lên Server
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
            // Kiểm tra xem trong PlayerProfile đã có thông tin người dùng chưa
            if (PlayerProfile.CurrentUser != null)
            {
                isLoggedIn = true;
                return true;
            }

            using (Form popupLogin = new Form())
            {
                ucLogin loginScreen = new ucLogin();
                popupLogin.Controls.Add(loginScreen);
                popupLogin.ClientSize = loginScreen.Size;
                popupLogin.FormBorderStyle = FormBorderStyle.None;
                popupLogin.StartPosition = FormStartPosition.CenterScreen;

                DialogResult result = popupLogin.ShowDialog();

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
            // 1. Kiểm tra đăng nhập
            if (!CheckLogin()) return;

            // 2. Xóa các Panel con đang hiển thị đè lên Lobby
            DeleteAccess();

            // 3. Khởi tạo màn hình RoomWait
            RoomWait pnlRoomWait = new RoomWait();

            // Căn giữa giao diện RoomWait trong Lobby
            int centerX = (this.Width - pnlRoomWait.Width) / 2;
            int centerY = (this.Height - pnlRoomWait.Height) / 2;
            pnlRoomWait.Location = new Point(centerX, centerY);
            pnlRoomWait.Name = "pnlRoomWait";

            // Thêm vào Lobby và đưa lên phía trước
            this.Controls.Add(pnlRoomWait);
            pnlRoomWait.BringToFront();

            // 4. Báo cho Server biết để tạo phòng mới cho người chơi này ngồi chờ
            if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
            {
                try
                {
                    // Gửi DTO tạo phòng lên Server
                    var createRoomDto = new CreateRoomDto
                    {
                        RoomName = $"Phòng của {PlayerProfile.Username}"
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
                MessageBox.Show("Chưa kết nối đến máy chủ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnCreateRoom_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra đăng nhập
            if (!CheckLogin()) return;

            // 2. Xóa các Panel con (như pnlJoin, pnlLogin...) đang hiển thị đè lên Lobby
            DeleteAccess();

            // 3. Khởi tạo và hiển thị màn hình phòng chờ (RoomWait)
            RoomWait pnlRoomWait = new RoomWait();

            // Căn giữa giao diện RoomWait
            int centerX = (this.Width - pnlRoomWait.Width) / 2;
            int centerY = (this.Height - pnlRoomWait.Height) / 2;
            pnlRoomWait.Location = new Point(centerX, centerY);
            pnlRoomWait.Name = "pnlRoomWait";

            this.Controls.Add(pnlRoomWait);
            pnlRoomWait.BringToFront();

            // 4. Gửi yêu cầu Tạo phòng lên Server (Người tạo mặc định sẽ là Host)
            if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
            {
                try
                {
                    // Thay PlayerProfile.Username bằng biến lưu tên người dùng hiện tại của bạn
                    string hostName = "Bạn"; // Gán tạm nếu chưa có biến tĩnh lưu Username

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

                // Gỡ RoomWait nếu không có mạng
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

        private void lblBattleshiptxt_Click(object sender, EventArgs e) { }
        private void richTextBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void Lobby_Load_1(object sender, EventArgs e) { }
    }
}