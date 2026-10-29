using Battleship.Shared;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
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

        // Bổ sung biến theo dõi khóa đăng nhập
        private int failedLoginCount = 0;
        private bool isLockedOut = false;

        public ucLogin()
        {
            InitializeComponent();
            this.Load += ucLogin_Load;
        }

        private void ucLogin_Load(object sender, EventArgs e)
        {
            this.ActiveControl = null;

            SendMessage(boxUsernameTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);
            SendMessage(boxPasswordTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);

            boxUsernameTxt.Text = "Tên đăng nhập";
            boxUsernameTxt.ForeColor = Color.Gray;

            boxPasswordTxt.PasswordChar = '\0';
            boxPasswordTxt.Text = "Mật khẩu";
            boxPasswordTxt.ForeColor = Color.Gray;

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
                boxUsernameTxt.ForeColor = Color.Black;
            }
        }

        private void boxUsernameTxt_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(boxUsernameTxt.Text))
            {
                boxUsernameTxt.Text = "Tên đăng nhập";
                boxUsernameTxt.ForeColor = Color.Gray;
            }
        }

        private void boxPasswordTxt_Enter(object sender, EventArgs e)
        {
            if (boxPasswordTxt.Text == "Mật khẩu")
            {
                boxPasswordTxt.Text = "";
                boxPasswordTxt.ForeColor = Color.Black;
                boxPasswordTxt.PasswordChar = '●';
            }
        }

        private void boxPasswordTxt_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(boxPasswordTxt.Text))
            {
                boxPasswordTxt.PasswordChar = '\0';
                boxPasswordTxt.Text = "Mật khẩu";
                boxPasswordTxt.ForeColor = Color.Gray;
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
            // Chặn click nếu đang bị khóa
            if (isLockedOut) return;

            string user = boxUsernameTxt.Text.Trim();
            string pass = boxPasswordTxt.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) ||
                user == "Tên đăng nhập" || pass == "Mật khẩu")
            {
                lblErrorTxt.ForeColor = Color.Red;
                lblErrorTxt.Text = "Vui lòng nhập đầy đủ thông tin!";
                return;
            }

            lblErrorTxt.ForeColor = Color.Blue;
            lblErrorTxt.Text = "Đang kết nối...";
            btnLogin.Enabled = false;

            try
            {
                if (!NetworkManager.Instance.IsConnected)
                {
                    bool connected = await NetworkManager.Instance.ConnectAsync("127.0.0.1", 8000);
                    if (!connected)
                    {
                        lblErrorTxt.ForeColor = Color.Red;
                        lblErrorTxt.Text = "Không thể kết nối đến máy chủ!";
                        return;
                    }
                }

                var loginReq = new LoginRequestDto
                {
                    Username = user,
                    Password = pass
                };
                var packet = Packet.Create(PacketType.LoginRequest, loginReq);

                await NetworkManager.Instance.SendAsync(packet);

                var responsePacket = await NetworkManager.Instance.WaitForPacketAsync(PacketType.LoginResponse);

                if (responsePacket != null)
                {
                    var loginRes = responsePacket.DeserializePayload<LoginResponseDto>();

                    if (loginRes != null && loginRes.Success)
                    {
                        failedLoginCount = 0; // Reset khi thành công
                        PlayerProfile.CurrentUser = loginRes;

                        lblErrorTxt.ForeColor = Color.Green;
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
                        failedLoginCount++; // Tăng biến đếm khi sai

                        if (failedLoginCount >= 5)
                        {
                            // Vượt quá 5 lần thì gọi hàm khóa
                            await HandleLockoutAsync();
                        }
                        else
                        {
                            lblErrorTxt.ForeColor = Color.Red;
                            lblErrorTxt.Text = loginRes?.Message ?? "Tài khoản hoặc mật khẩu không chính xác!";
                            boxPasswordTxt.Clear();
                            boxPasswordTxt.Focus();
                        }
                    }
                }
                else
                {
                    lblErrorTxt.ForeColor = Color.Red;
                    lblErrorTxt.Text = "Không nhận được phản hồi từ máy chủ!";
                }
            }
            catch (Exception)
            {
                lblErrorTxt.ForeColor = Color.Red;
                lblErrorTxt.Text = "Lỗi kết nối máy chủ!";
            }
            finally
            {
                // Chỉ mở lại nút bấm nếu user không đang trong trạng thái bị khóa
                if (!isLockedOut)
                {
                    btnLogin.Enabled = true;
                }
            }
        }

        // === HÀM XỬ LÝ KHÓA ĐĂNG NHẬP 60 GIÂY ===
        private async Task HandleLockoutAsync()
        {
            isLockedOut = true;
            btnLogin.Enabled = false;
            boxUsernameTxt.Enabled = false;
            boxPasswordTxt.Enabled = false;
            lblErrorTxt.ForeColor = Color.Red;

            // Chạy đếm ngược 60 giây và cập nhật trực tiếp lên Label
            for (int i = 60; i > 0; i--)
            {
                lblErrorTxt.Text = $"Đăng nhập sai quá nhiều lần. Thử lại sau {i} giây.";
                await Task.Delay(1000);
            }

            // Mở khóa sau khi đếm xong
            failedLoginCount = 0;
            isLockedOut = false;
            btnLogin.Enabled = true;
            boxUsernameTxt.Enabled = true;
            boxPasswordTxt.Enabled = true;
            boxPasswordTxt.Clear();
            lblErrorTxt.Text = "";
        }
    }
}