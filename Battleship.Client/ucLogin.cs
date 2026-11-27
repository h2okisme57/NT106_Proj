using Battleship.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http; // Bắt buộc phải có để gọi REST API
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battleship.Client
{
    public partial class ucLogin : UserControl
    {


        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int EM_SETMARGINS = 0xd3;
        private const int EC_LEFTMARGIN = 1;

        // Bảng màu phong cách Balatro (CRT felt + dark slate card + neon accents)
        private static readonly Color BalatroBgTop = Color.FromArgb(24, 48, 39);
        private static readonly Color BalatroBgBottom = Color.FromArgb(15, 25, 31);
        private static readonly Color BalatroPanelBg = Color.FromArgb(35, 43, 51);
        private static readonly Color BalatroPanelBorder = Color.FromArgb(228, 220, 203);
        private static readonly Color BalatroShadow = Color.FromArgb(11, 15, 19);
        private static readonly Color BalatroInputBg = Color.FromArgb(20, 26, 32);
        private static readonly Color BalatroInputBorder = Color.FromArgb(78, 95, 110);
        private static readonly Color BalatroCyan = Color.FromArgb(80, 215, 255);
        private static readonly Color BalatroGold = Color.FromArgb(245, 183, 49);
        private static readonly Color BalatroRed = Color.FromArgb(254, 95, 85);
        private static readonly Color BalatroRedDark = Color.FromArgb(168, 43, 38);
        private static readonly Color BalatroText = Color.FromArgb(242, 236, 225);
        private static readonly Color BalatroMuted = Color.FromArgb(156, 202, 232);
        private static readonly Color BalatroGreen = Color.FromArgb(76, 217, 100);

        private int failedLoginCount = 0;
        private bool isLockedOut = false;
        private LinkLabel llblForgot;

        public ucLogin()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            Applytheme();
            AddForgotPasswordLink();
            Theme.ApplyRetroFont(this);
            this.Load += ucLogin_Load;
        }

        private void Applytheme()
        {
            this.BackColor = BalatroBgBottom;
            this.BorderStyle = BorderStyle.None;

            // Tiêu đề kiểu thẻ bài / arcade Balatro
            lblLoginLogoTxt.AutoSize = false;
            lblLoginLogoTxt.Size = new Size(340, 44);
            lblLoginLogoTxt.Location = new Point((this.Width - 340) / 2, 34);
            lblLoginLogoTxt.TextAlign = ContentAlignment.MiddleCenter;
            lblLoginLogoTxt.Font = FontManager.GetFont(16f, FontStyle.Bold);
            lblLoginLogoTxt.ForeColor = BalatroGold;
            lblLoginLogoTxt.BackColor = Color.Transparent;

            // Ô nhập liệu tối recessed
            StyleInputBox(boxUsernameTxt, new Point(95, 126), 355);
            StyleInputBox(boxPasswordTxt, new Point(95, 182), 355);

            // Link đăng ký
            llblRegister.Font = FontManager.GetFont(8.5f, FontStyle.Regular);
            llblRegister.BackColor = Color.Transparent;
            llblRegister.LinkColor = BalatroCyan;
            llblRegister.ActiveLinkColor = Color.White;
            llblRegister.VisitedLinkColor = BalatroCyan;
            llblRegister.Location = new Point(282, 232);

            // Nút LOGIN chunky 3D đỏ Balatro
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 118, 109);
            btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(214, 62, 54);
            btnLogin.BackColor = BalatroRed;
            btnLogin.ForeColor = Color.White;
            btnLogin.Font = FontManager.GetFont(11.5f, FontStyle.Bold);
            btnLogin.Size = new Size(176, 48);
            btnLogin.Location = new Point((this.Width - 176) / 2, 286);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Paint += (s, e) => DrawBalatroButtonChrome(btnLogin, e.Graphics, BalatroRed, BalatroRedDark);

            // Nút đóng X góc phải
            btnOut.FlatStyle = FlatStyle.Flat;
            btnOut.FlatAppearance.BorderSize = 0;
            btnOut.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 118, 109);
            btnOut.FlatAppearance.MouseDownBackColor = Color.FromArgb(214, 62, 54);
            btnOut.BackColor = BalatroRed;
            btnOut.ForeColor = Color.White;
            btnOut.Font = FontManager.GetFont(9.5f, FontStyle.Bold);
            btnOut.Size = new Size(42, 34);
            btnOut.Location = new Point(this.Width - 56, 14);
            btnOut.Cursor = Cursors.Hand;
            btnOut.Paint += (s, e) => DrawBalatroButtonChrome(btnOut, e.Graphics, BalatroRed, BalatroRedDark);

            // Dòng thông báo lỗi/trạng thái
            lblErrorTxt.BackColor = Color.Transparent;
            lblErrorTxt.Font = FontManager.GetFont(8.5f, FontStyle.Regular);
            lblErrorTxt.ForeColor = BalatroRed;
            lblErrorTxt.Location = new Point(45, 352);
            lblErrorTxt.Size = new Size(this.Width - 90, 34);
            lblErrorTxt.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void StyleInputBox(TextBox tb, Point location, int width)
        {
            tb.BorderStyle = BorderStyle.None;
            tb.BackColor = BalatroInputBg;
            tb.ForeColor = BalatroMuted;
            tb.Font = FontManager.GetFont(10.5f, FontStyle.Regular);
            tb.Location = new Point(location.X + 10, location.Y + 8);
            tb.Size = new Size(width - 20, 22);
            tb.Enter += (s, e) => this.Invalidate();
            tb.Leave += (s, e) => this.Invalidate();
        }

        private static void DrawBalatroButtonChrome(Button btn, Graphics g, Color faceColor, Color lipColor)
        {
            g.SmoothingMode = SmoothingMode.None;
            int w = btn.Width;
            int h = btn.Height;

            // Viền dưới 3D đậm kiểu nút bấm Balatro
            using (var lipBrush = new SolidBrush(btn.Enabled ? lipColor : Color.FromArgb(55, 58, 64)))
            {
                g.FillRectangle(lipBrush, 0, h - 5, w, 5);
            }

            // Viền sáng mảnh ở mép trên & viền ngoài tối
            using (var topPen = new Pen(Color.FromArgb(90, 255, 255, 255), 1))
            using (var borderPen = new Pen(BalatroShadow, 2))
            {
                g.DrawLine(topPen, 2, 2, w - 3, 2);
                g.DrawRectangle(borderPen, 1, 1, w - 2, h - 2);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            // 1. Nền bàn CRT tối chuyển sắc xanh rêu -> xanh than
            using (var bgBrush = new LinearGradientBrush(this.ClientRectangle, BalatroBgTop, BalatroBgBottom, LinearGradientMode.Vertical))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // 2. Hiệu ứng sọc CRT (scanlines) nhẹ
            using (var scanPen = new Pen(Color.FromArgb(24, 0, 0, 0), 1))
            {
                for (int y = 0; y < this.Height; y += 3)
                {
                    g.DrawLine(scanPen, 0, y, this.Width, y);
                }
            }

            // 3. Khung viền ngoài kiểu CRT / thẻ Balatro
            using (var outerPen = new Pen(BalatroShadow, 4))
            using (var innerPen = new Pen(BalatroPanelBorder, 2))
            {
                g.DrawRectangle(outerPen, 2, 2, this.Width - 4, this.Height - 4);
                g.DrawRectangle(innerPen, 6, 6, this.Width - 12, this.Height - 12);
            }

            // 4. Card trung tâm chứa form đăng nhập + bóng đổ cứng
            Rectangle cardRect = new Rectangle(58, 86, this.Width - 116, 260);
            Rectangle shadowRect = new Rectangle(cardRect.X + 5, cardRect.Y + 6, cardRect.Width, cardRect.Height);

            using (var shadowBrush = new SolidBrush(BalatroShadow))
            using (var cardBrush = new SolidBrush(BalatroPanelBg))
            using (var cardBorder = new Pen(BalatroPanelBorder, 2))
            {
                g.FillRectangle(shadowBrush, shadowRect);
                g.FillRectangle(cardBrush, cardRect);
                g.DrawRectangle(cardBorder, cardRect);
            }

            // 5. Viền slot cho 2 ô nhập liệu (đổi sang Cyan khi đang focus)
            DrawInputSlot(g, boxUsernameTxt);
            DrawInputSlot(g, boxPasswordTxt);
        }

        private static void DrawInputSlot(Graphics g, TextBox tb)
        {
            Rectangle slotRect = new Rectangle(tb.Left - 10, tb.Top - 8, tb.Width + 20, 38);
            Color borderColor = tb.Focused ? BalatroCyan : BalatroInputBorder;

            using (var bgBrush = new SolidBrush(BalatroInputBg))
            using (var borderPen = new Pen(borderColor, 2))
            {
                g.FillRectangle(bgBrush, slotRect);
                g.DrawRectangle(borderPen, slotRect);
            }
        }

        // Liên kết "Quên mật khẩu?" thêm bằng code (không sửa Designer), đặt cùng hàng với "Chưa có tài khoản?"
        private void AddForgotPasswordLink()
        {
            llblForgot = new LinkLabel();
            llblForgot.AutoSize = true;
            llblForgot.Text = "Quên mật khẩu?";
            llblForgot.Font = llblRegister.Font;
            llblForgot.BackColor = Color.Transparent;
            llblForgot.LinkBehavior = LinkBehavior.NeverUnderline;
            llblForgot.LinkColor = BalatroGold;
            llblForgot.ActiveLinkColor = Color.White;
            llblForgot.VisitedLinkColor = BalatroGold;
            llblForgot.Location = new Point(85, llblRegister.Location.Y);
            llblForgot.LinkClicked += llblForgot_LinkClicked;
            this.Controls.Add(llblForgot);
        }

        private void llblForgot_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form popup = FindForm();
            if (popup != null)
            {
                popup.Controls.Clear();
                var forgotScreen = new ucForgotPassword();
                popup.Controls.Add(forgotScreen);
                popup.ClientSize = forgotScreen.Size;
            }
        }

        private void ucLogin_Load(object sender, EventArgs e)
        {
            this.ActiveControl = null;

            SendMessage(boxUsernameTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);
            SendMessage(boxPasswordTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);

            boxUsernameTxt.Text = "Tên đăng nhập";
            boxUsernameTxt.ForeColor = BalatroMuted;

            boxPasswordTxt.PasswordChar = '\0';
            boxPasswordTxt.Text = "Mật khẩu";
            boxPasswordTxt.ForeColor = BalatroMuted;

            boxUsernameTxt.Enter += boxUsernameTxt_Enter;
            boxUsernameTxt.Leave += boxUsernameTxt_Leave;

            boxPasswordTxt.Enter += boxPasswordTxt_Enter;
            boxPasswordTxt.Leave += boxPasswordTxt_Leave;

            this.Click += LoseFocus_Click;
            AttachClickToAllControls(this);
        }

        private void AttachClickToAllControls(Control container)
        {
            foreach (Control ctrl in container.Controls)
            {
                if (!(ctrl is TextBox))
                {
                    ctrl.Click += LoseFocus_Click;
                }

                if (ctrl.HasChildren)
                {
                    AttachClickToAllControls(ctrl);
                }
            }
        }

        private void LoseFocus_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.ActiveControl = null;
            }
        }

        private void boxUsernameTxt_Enter(object sender, EventArgs e)
        {
            if (boxUsernameTxt.Text == "Tên đăng nhập")
            {
                boxUsernameTxt.Text = "";
                boxUsernameTxt.ForeColor = BalatroText;
            }
        }

        private void boxUsernameTxt_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(boxUsernameTxt.Text))
            {
                boxUsernameTxt.Text = "Tên đăng nhập";
                boxUsernameTxt.ForeColor = BalatroMuted;
            }
        }

        private void boxPasswordTxt_Enter(object sender, EventArgs e)
        {
            if (boxPasswordTxt.Text == "Mật khẩu")
            {
                boxPasswordTxt.Text = "";
                boxPasswordTxt.ForeColor = BalatroText;
                boxPasswordTxt.PasswordChar = '●';
            }
        }

        private void boxPasswordTxt_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(boxPasswordTxt.Text))
            {
                boxPasswordTxt.PasswordChar = '\0';
                boxPasswordTxt.Text = "Mật khẩu";
                boxPasswordTxt.ForeColor = BalatroMuted;
            }
        }

        private void llblRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form popup = FindForm();
            if (popup != null)
            {
                popup.Controls.Clear();
                ucRegister registerScreen = new ucRegister();
                popup.Controls.Add(registerScreen);
                popup.ClientSize = registerScreen.Size;
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            if (isLockedOut) return;

            string user = boxUsernameTxt.Text.Trim();
            string pass = boxPasswordTxt.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) ||
                user == "Tên đăng nhập" || pass == "Mật khẩu")
            {
                lblErrorTxt.ForeColor = BalatroRed;
                lblErrorTxt.Text = "Vui lòng nhập đầy đủ thông tin!";
                return;
            }

            lblErrorTxt.ForeColor = BalatroCyan;
            lblErrorTxt.Text = "Đang kết nối...";
            btnLogin.Enabled = false;

            try
            {
                // 1. Kết nối và Bắt tay với Server. Đăng nhập đi qua TCP, KHÔNG gọi thẳng cơ sở dữ liệu:
                //    server mới là nơi kiểm tra BCrypt, khóa dò mật khẩu và chống đăng nhập trùng.
                if (!NetworkManager.Instance.IsConnected)
                {
                    bool connected = await NetworkManager.Instance.ConnectAsync("127.0.0.1", 8000);
                    if (!connected)
                    {
                        lblErrorTxt.ForeColor = BalatroRed;
                        lblErrorTxt.Text = "Không thể kết nối đến máy chủ!";
                        return;
                    }
                }

                // 2. Gửi LoginRequest rồi chờ LoginResponse
                var loginReq = new LoginRequestDto { Username = user, Password = pass };
                await NetworkManager.Instance.SendAsync(Packet.Create(PacketType.LoginRequest, loginReq));
                var responsePacket = await NetworkManager.Instance.WaitForPacketAsync(PacketType.LoginResponse);

                if (responsePacket == null)
                {
                    lblErrorTxt.ForeColor = BalatroRed;
                    lblErrorTxt.Text = "Không nhận được phản hồi từ máy chủ!";
                    return;
                }

                var loginRes = responsePacket.DeserializePayload<LoginResponseDto>();

                if (loginRes != null && loginRes.Success)
                {
                    failedLoginCount = 0;
                    PlayerProfile.CurrentUser = loginRes;   // bê nguyên gói DTO vào PlayerProfile

                    lblErrorTxt.ForeColor = BalatroGreen;
                    lblErrorTxt.Text = "Đăng nhập thành công! Đang vào sảnh...";

                    await Task.Delay(500);

                    Form popup = this.FindForm();
                    if (popup != null)
                    {
                        popup.DialogResult = DialogResult.OK;
                    }
                }
                else
                {
                    string message = loginRes?.Message ?? "Tài khoản hoặc mật khẩu không chính xác!";

                    // Server đang khóa vì sai quá nhiều lần: lấy số giây còn lại từ thông báo của server
                    var lockMatch = System.Text.RegularExpressions.Regex.Match(message, @"(\d+)\s*giây");
                    if (message.Contains("quá nhiều") && lockMatch.Success)
                    {
                        await HandleLockoutAsync(int.Parse(lockMatch.Groups[1].Value));
                    }
                    else if (message.StartsWith("Sai tài khoản"))
                    {
                        failedLoginCount++;   // chỉ đếm khi sai tên/mật khẩu

                        if (failedLoginCount >= 5)
                        {
                            await HandleLockoutAsync(60);
                        }
                        else
                        {
                            lblErrorTxt.ForeColor = BalatroRed;
                            lblErrorTxt.Text = $"Tài khoản hoặc mật khẩu không chính xác! ({failedLoginCount}/5)";
                            boxPasswordTxt.Clear();
                            boxPasswordTxt.Focus();
                        }
                    }
                    else
                    {
                        // Các lý do khác (ví dụ tài khoản đang đăng nhập ở nơi khác): hiện nguyên thông báo của server
                        lblErrorTxt.ForeColor = BalatroRed;
                        lblErrorTxt.Text = message;
                    }
                }
            }
            catch (Exception ex)
            {
                lblErrorTxt.ForeColor = BalatroRed;
                lblErrorTxt.Text = "Lỗi kết nối máy chủ!";
            }
            finally
            {
                if (!isLockedOut)
                {
                    btnLogin.Enabled = true;
                }
            }
        }

        // === HÀM XỬ LÝ KHÓA ĐĂNG NHẬP 60 GIÂY ===
        private async Task HandleLockoutAsync(int seconds = 60)
        {
            isLockedOut = true;
            btnLogin.Enabled = false;
            boxUsernameTxt.Enabled = false;
            boxPasswordTxt.Enabled = false;
            lblErrorTxt.ForeColor = BalatroRed;

            // Chạy đếm ngược 60 giây và cập nhật trực tiếp lên Label
            for (int i = seconds; i > 0; i--)
            {
                lblErrorTxt.Text = $"Thử lại sau {i} giây.";
                await Task.Delay(1000);
            }

            failedLoginCount = 0;
            isLockedOut = false;
            btnLogin.Enabled = true;
            boxUsernameTxt.Enabled = true;
            boxPasswordTxt.Enabled = true;
            boxPasswordTxt.Clear();
            lblErrorTxt.Text = "";
        }

        private void btnOut_Click(object sender, EventArgs e)
        {
            Form popup = this.FindForm();
            if (popup != null && popup.Modal)
            {
                popup.DialogResult = DialogResult.Cancel;
                popup.Close();
                return;
            }

            this.Parent?.Controls.Remove(this);
            this.Dispose();
        }
    }
}