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
        // Biến cờ để đánh dấu trạng thái đăng nhập
        private bool isLoggedIn = false;
        private joinRoom pnlJoinRoom;
        public Lobby()
        {
            InitializeComponent();
            this.Load += Lobby_Load;
            boxChat.KeyDown += boxChat_KeyDown;
            //SetupTestButton();
        }

        private void Lobby_Load(object sender, EventArgs e)
        {
            boxChat.BorderStyle = BorderStyle.None;
            CreateTransparentChatBox();
            // Khai báo màu chữ chung từ mã Hex
            Color textColorBtn = ColorTranslator.FromHtml("#F4D35E");
            Color textColorLogo = ColorTranslator.FromHtml("#8B1E2D");

            if (lblBattleshiptxt != null)
            {
                lblBattleshiptxt.UseCompatibleTextRendering = true;
                lblBattleshiptxt.Font = FontManager.GetFont(24f, FontStyle.Bold);
                lblBattleshiptxt.BackColor = Color.Transparent;

                // Sửa màu chữ Label 
                lblBattleshiptxt.ForeColor = textColorLogo;
            }

            // Tạo các nút bấm với font chữ, màu sắc và hiệu ứng hover
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
        }

        //code thu nghiem chat lobby
        private void SetupTestButton()
        {
            Button btnTest = new Button();
            btnTest.Text = "Thử nhận tin nhắn";
            // Tọa độ đặt ngay bên dưới khung chat (chatPanel cao 300, tọa độ Y 20 -> Đặt nút ở Y 330)
            btnTest.Location = new Point(20, 330);
            btnTest.Size = new Size(150, 35);

            int count = 0;
            // Bắt sự kiện click để tự động bơm dòng chat giả vào
            btnTest.Click += (s, e) =>
            {
                count++;
                AddChatMessage("Hệ thống", $"Dòng tin nhắn thử nghiệm số {count}. Nền trong suốt hoạt động rất tốt!");
            };

            this.Controls.Add(btnTest);
            btnTest.BringToFront(); // Đẩy lên trên cùng để không bị ảnh nền đè
        }

        private FlowLayoutPanel chatPanel;

        private void CreateTransparentChatBox()
        {
            chatPanel = new FlowLayoutPanel();
            chatPanel.Name = "chatPanel";
            chatPanel.Location = new Point(566, 213);
            chatPanel.Size = new Size(412, 400);

            chatPanel.AutoScroll = true;

            // --- SỬA Ở ĐÂY: TẠO NỀN MỜ BÁN TRONG SUỐT ---
            // Thông số: 120 là độ mờ (0=trong vắt, 255=đặc xịt). 
            // 0, 0, 0 là mã màu Đen (RGB).
            chatPanel.BackColor = Color.FromArgb(120, 0, 0, 0);

            chatPanel.FlowDirection = FlowDirection.TopDown;
            chatPanel.WrapContents = false;

            this.Controls.Add(chatPanel);
        }


        private void AddChatMessage(string sender, string content)
        {
            Label msgLabel = new Label();
            msgLabel.Text = $"[{sender}]: {content}";

            // Cấu hình nhãn trong suốt và tự động giãn chữ
            msgLabel.BackColor = Color.Transparent;
            msgLabel.ForeColor = Color.White;
            msgLabel.AutoSize = true;
            msgLabel.MaximumSize = new Size(370, 0); // Ép xuống dòng nếu chat quá dài

            // Dùng Invoke nếu đang ở luồng mạng
            chatPanel.Invoke(new Action(() =>
            {
                chatPanel.Controls.Add(msgLabel);

                // Tự động cuộn xuống dưới cùng
                chatPanel.ScrollControlIntoView(msgLabel);
            }));
        }

        private void boxChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (string.IsNullOrWhiteSpace(boxChat.Text)) return;

                var chatMsg = new ChatMessageDto { Content = boxChat.Text };
                var packet = new Packet
                {
                    Type = PacketType.ChatMessage,
                    Payload = JsonSerializer.Serialize(chatMsg)
                };

                // Gửi lên Server (thay _client.SendAsync bằng hàm gửi mạng của bạn)
                // Chua co ClientNetwork, nên tạm thời comment lại
                //await SendAsync(packet);

                boxChat.Clear();
            }
        }


        // === HÀM KIỂM TRA ĐĂNG NHẬP ===
        private bool CheckLogin()
        {
            if (isLoggedIn)
                return true; // Nếu đã đăng nhập rồi thì cho qua luôn

            // Nếu chưa đăng nhập, dựng Popup chứa ucLogin lên giữa Lobby
            using (Form popupLogin = new Form())
            {
                ucLogin loginScreen = new ucLogin();
                popupLogin.Controls.Add(loginScreen);
                popupLogin.ClientSize = loginScreen.Size;
                popupLogin.FormBorderStyle = FormBorderStyle.None;
                popupLogin.StartPosition = FormStartPosition.CenterScreen;

                // Hiển thị khung Login chặn màn hình
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

        // === CÁC NÚT BẤM ===
        private void btnFindRoom_Click(object sender, EventArgs e)
        {

            if (!CheckLogin()) return;

            // Code tìm phòng của bạn sẽ viết ở đây (chỉ chạy khi đã login)
        }

        private void btnCreateRoom_Click(object sender, EventArgs e)
        {
            if (!CheckLogin()) return;

            // Code tạo phòng
        }

        private void btnJoinRoom_Click(object sender, EventArgs e)
        {
            if (!CheckLogin()) return;
            // Code tham gia phòng

            if (pnlJoinRoom != null && this.Controls.Contains(pnlJoinRoom))
            {
                pnlJoinRoom.BringToFront();
                return;
            }

            // 2. Khởi tạo UserControl joinRoom
            pnlJoinRoom = new joinRoom();

            // 3. TÍNH TOÁN CĂN GIỮA MÀN HÌNH LOBBY
            int centerX = (this.Width - pnlJoinRoom.Width) / 2;
            int centerY = (this.Height - pnlJoinRoom.Height) / 2;
            pnlJoinRoom.Location = new Point(centerX, centerY);

            // 4. Gắn thêm viền (tùy chọn) để bảng nổi bật lên khỏi nền Lobby
            pnlJoinRoom.BorderStyle = BorderStyle.FixedSingle;

            // 5. Đưa bảng vào Lobby và ép hiển thị trên cùng (che khuất các thành phần khác)
            this.Controls.Add(pnlJoinRoom);
            pnlJoinRoom.BringToFront();
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            if (!CheckLogin()) return;

            // Code mở hồ sơ
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lblBattleshiptxt_Click(object sender, EventArgs e)
        {
            //Code bị thừa
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Lobby_Load_1(object sender, EventArgs e)
        {

        }
    }
}
