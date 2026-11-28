using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Battleship.Shared; 

namespace Battleship.Client
{
    public partial class ucRegister : UserControl
    {
        // 1. GỌI HÀM LÕI WINDOWS ĐỂ TẠO THỤT LỀ (PADDING)
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int EM_SETMARGINS = 0xd3;
        private const int EC_LEFTMARGIN = 1;

        // Ô Email thêm bằng code (không sửa Designer): dùng để lấy lại mật khẩu khi quên
        private TextBox boxEmailTxt;
        private const string EmailPlaceholder = "Email (để lấy lại mật khẩu)";

        public ucRegister()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            AddEmailBox();
            Applytheme();
            Theme.ApplyRetroFont(this);
            this.Load += ucRegister_Load;
        }

        private void Applytheme()
        {
            this.BackColor = Theme.BgBottom;
            this.BorderStyle = BorderStyle.None;

            lblLoginLogoTxt.AutoSize = false;
            lblLoginLogoTxt.Size = new Size(340, 42);
            lblLoginLogoTxt.Location = new Point((this.Width - 340) / 2, 22);
            lblLoginLogoTxt.TextAlign = ContentAlignment.MiddleCenter;
            lblLoginLogoTxt.Font = FontManager.GetFont(15.5f, FontStyle.Bold);
            lblLoginLogoTxt.ForeColor = Theme.Gold;
            lblLoginLogoTxt.BackColor = Color.Transparent;

            StyleRegisterInput(boxUsernameTxt, new Point(95, 92), 355);
            StyleRegisterInput(boxEmailTxt, new Point(95, 140), 355);
            StyleRegisterInput(boxPasswordTxt, new Point(95, 188), 355);
            StyleRegisterInput(boxConfirmPassword, new Point(95, 236), 355);

            llblLogin.Font = FontManager.GetFont(8.5f, FontStyle.Regular);
            llblLogin.BackColor = Color.Transparent;
            llblLogin.LinkColor = Theme.Cyan;
            llblLogin.ActiveLinkColor = Color.White;
            llblLogin.VisitedLinkColor = Theme.Cyan;
            llblLogin.Location = new Point(286, 280);

            btnRegister.Size = new Size(184, 46);
            btnRegister.Location = new Point((this.Width - 184) / 2, 316);
            Theme.StyleButton(btnRegister, Theme.Cyan, Theme.CyanDark, 11f);

            btnOut.Size = new Size(42, 34);
            btnOut.Location = new Point(this.Width - 56, 14);
            Theme.StyleButton(btnOut, Theme.Red, Theme.RedDark, 9.5f);

            lblErrorTxt.BackColor = Color.Transparent;
            lblErrorTxt.Font = FontManager.GetFont(8.5f, FontStyle.Regular);
            lblErrorTxt.ForeColor = Theme.Red;
            lblErrorTxt.Location = new Point(45, 396);
            lblErrorTxt.Size = new Size(this.Width - 90, 34);
            lblErrorTxt.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void StyleRegisterInput(TextBox tb, Point slotLocation, int width)
        {
            Theme.StyleTextBox(tb, 10f);
            tb.Location = new Point(slotLocation.X + 10, slotLocation.Y + 8);
            tb.Size = new Size(width - 20, 22);
            tb.Enter += (s, e) => this.Invalidate();
            tb.Leave += (s, e) => this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Theme.DrawCrtBackground(e.Graphics, this.ClientRectangle);
            Theme.DrawCard(e.Graphics, new Rectangle(58, 72, this.Width - 116, 312));
            Theme.DrawInputSlot(e.Graphics, boxUsernameTxt);
            Theme.DrawInputSlot(e.Graphics, boxEmailTxt);
            Theme.DrawInputSlot(e.Graphics, boxPasswordTxt);
            Theme.DrawInputSlot(e.Graphics, boxConfirmPassword);
        }

        private void AddEmailBox()
        {
            boxEmailTxt = new TextBox();
            boxEmailTxt.Name = "boxEmailTxt";
            boxEmailTxt.Text = EmailPlaceholder;

            boxEmailTxt.Enter += (s, e) =>
            {
                if (boxEmailTxt.Text == EmailPlaceholder)
                {
                    boxEmailTxt.Text = "";
                    boxEmailTxt.ForeColor = Theme.Text;
                }
            };
            boxEmailTxt.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(boxEmailTxt.Text))
                {
                    boxEmailTxt.Text = EmailPlaceholder;
                    boxEmailTxt.ForeColor = Theme.Muted;
                }
            };

            this.Controls.Add(boxEmailTxt);
        }

        private void ucRegister_Load(object sender, EventArgs e)
        {
            // 2. THỤT LỀ TRÁI CHO CẢ 3 Ô
            SendMessage(boxUsernameTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);
            SendMessage(boxPasswordTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);
            SendMessage(boxConfirmPassword.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);
            SendMessage(boxEmailTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);

            // CÀI ĐẶT PLACEHOLDER BAN ĐẦU
            boxUsernameTxt.Text = "Tên đăng nhập";
            boxUsernameTxt.ForeColor = Theme.Muted;

            boxPasswordTxt.PasswordChar = '\0';
            boxPasswordTxt.Text = "Mật khẩu";
            boxPasswordTxt.ForeColor = Theme.Muted;

            boxConfirmPassword.PasswordChar = '\0';
            boxConfirmPassword.Text = "Xác nhận mật khẩu";
            boxConfirmPassword.ForeColor = Theme.Muted;

            // Móc nối các sự kiện Enter/Leave
            boxUsernameTxt.Enter += boxUsernameTxt_Enter;
            boxUsernameTxt.Leave += boxUsernameTxt_Leave;

            boxPasswordTxt.Enter += boxPasswordTxt_Enter;
            boxPasswordTxt.Leave += boxPasswordTxt_Leave;

            boxConfirmPassword.Enter += boxConfirmPassword_Enter;
            boxConfirmPassword.Leave += boxConfirmPassword_Leave;

            this.Click += LoseFocus_Click;

            // Dùng hàm đệ quy để quét sạch mọi tầng lớp giao diện bên trong
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

        // === HÀM ÉP NHẢ FOCUS TỪ TẬN GỐC ===
        private void LoseFocus_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.ActiveControl = null;
            }
        }

        // ================= XỬ LÝ USERNAME =================
        private void boxUsernameTxt_Enter(object sender, EventArgs e)
        {
            if (boxUsernameTxt.Text == "Tên đăng nhập")
            {
                boxUsernameTxt.Text = "";
                boxUsernameTxt.ForeColor = Theme.Text;
            }
        }

        private void boxUsernameTxt_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(boxUsernameTxt.Text))
            {
                boxUsernameTxt.Text = "Tên đăng nhập";
                boxUsernameTxt.ForeColor = Theme.Muted;
            }
        }

        // ================= XỬ LÝ PASSWORD =================
        private void boxPasswordTxt_Enter(object sender, EventArgs e)
        {
            if (boxPasswordTxt.Text == "Mật khẩu")
            {
                boxPasswordTxt.Text = "";
                boxPasswordTxt.ForeColor = Theme.Text;
                boxPasswordTxt.PasswordChar = '●';
            }
        }

        private void boxPasswordTxt_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(boxPasswordTxt.Text))
            {
                boxPasswordTxt.PasswordChar = '\0';
                boxPasswordTxt.Text = "Mật khẩu";
                boxPasswordTxt.ForeColor = Theme.Muted;
            }
        }

        // ================= XỬ LÝ CONFIRM PASSWORD =================
        private void boxConfirmPassword_Enter(object sender, EventArgs e)
        {
            if (boxConfirmPassword.Text == "Xác nhận mật khẩu")
            {
                boxConfirmPassword.Text = "";
                boxConfirmPassword.ForeColor = Theme.Text;
                boxConfirmPassword.PasswordChar = '●';
            }
        }

        private void boxConfirmPassword_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(boxConfirmPassword.Text))
            {
                boxConfirmPassword.PasswordChar = '\0';
                boxConfirmPassword.Text = "Xác nhận mật khẩu";
                boxConfirmPassword.ForeColor = Theme.Muted;
            }
        }

        // ================= XỬ LÝ NÚT BẤM & ĐIỀU HƯỚNG =================
        private async void btnRegister_Click(object sender, EventArgs e)
        {
            string user = boxUsernameTxt.Text.Trim();
            string pass = boxPasswordTxt.Text.Trim();
            string confirm = boxConfirmPassword.Text.Trim();
            string email = boxEmailTxt.Text.Trim();

            // 1. Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(user) || user == "Tên đăng nhập" ||
                string.IsNullOrWhiteSpace(pass) || pass == "Mật khẩu" ||
                string.IsNullOrWhiteSpace(confirm) || confirm == "Xác nhận mật khẩu" ||
                string.IsNullOrWhiteSpace(email) || email == EmailPlaceholder)
            {
                lblErrorTxt.ForeColor = Theme.Red;
                lblErrorTxt.Text = "Vui lòng nhập đầy đủ thông tin!";
                return;
            }

            if (pass != confirm)
            {
                lblErrorTxt.ForeColor = Theme.Red;
                lblErrorTxt.Text = "Mật khẩu xác nhận không khớp!";
                return;
            }

            lblErrorTxt.ForeColor = Theme.Cyan;
            lblErrorTxt.Text = "Đang kết nối...";
            btnRegister.Enabled = false; // Khóa nút bấm tránh spam

            try
            {
                // 2. Kết nối và Bắt tay với Server
                if (!NetworkManager.Instance.IsConnected)
                {
                    bool connected = await NetworkManager.Instance.ConnectAsync("127.0.0.1", 8000);
                    if (!connected)
                    {
                        lblErrorTxt.ForeColor = Theme.Red;
                        lblErrorTxt.Text = "Không thể kết nối đến máy chủ!";
                        return;
                    }
                }

                // 3. Đóng gói DTO thành Packet và gửi đi
                var regReq = new RegisterRequestDto
                {
                    Username = user,
                    Password = pass,
                    Email = email
                };
                var packet = Packet.Create(PacketType.RegisterRequest, regReq);
                await NetworkManager.Instance.SendAsync(packet);

                // 4. Chờ phản hồi từ Server
                var responsePacket = await NetworkManager.Instance.WaitForPacketAsync(PacketType.RegisterResponse);

                if (responsePacket != null)
                {
                    var regRes = responsePacket.DeserializePayload<RegisterResponseDto>();

                    if (regRes != null && regRes.Success)
                    {
                        lblErrorTxt.ForeColor = Theme.Green;
                        lblErrorTxt.Text = "Đăng ký thành công! Đang chuyển sang Đăng nhập...";

                        await Task.Delay(1000); // Dừng một lát để người dùng đọc thông báo
                        SwitchToLogin();
                    }
                    else
                    {
                        lblErrorTxt.ForeColor = Theme.Red;
                        lblErrorTxt.Text = regRes?.Message ?? "Tên tài khoản đã tồn tại!";
                    }
                }
                else
                {
                    lblErrorTxt.ForeColor = Theme.Red;
                    lblErrorTxt.Text = "Không nhận được phản hồi từ máy chủ!";
                }
            }
            catch (Exception ex)
            {
                lblErrorTxt.ForeColor = Theme.Red;
                lblErrorTxt.Text = "Lỗi kết nối máy chủ!";
            }
            finally
            {
                btnRegister.Enabled = true; // Mở khóa lại nút
            }
        }

        private void llblLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SwitchToLogin();
        }

        // Hàm hỗ trợ đổi ruột Popup từ ucRegister sang ucLogin
        private void SwitchToLogin()
        {
            Form popup = this.FindForm();
            if (popup != null)
            {
                popup.Controls.Clear();
                ucLogin loginScreen = new ucLogin();

                popup.Controls.Add(loginScreen);
                popup.ClientSize = loginScreen.Size; // Tự động co giãn Form vừa với ucLogin
            }
        }

        // Các sự kiện thừa
        private void boxUsernameTxt_TextChanged(object sender, EventArgs e) { }
        private void boxPasswordTxt_TextChanged(object sender, EventArgs e) { }
        private void boxConfirmPassword_TextChanged(object sender, EventArgs e) { }
        private void lblErrorTxt_Click(object sender, EventArgs e) { }

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