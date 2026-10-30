using Battleship.Shared;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

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
                // 1. Kết nối và Bắt tay với Server. Đăng nhập đi qua TCP, KHÔNG gọi thẳng cơ sở dữ liệu:
                //    server mới là nơi kiểm tra BCrypt, khóa dò mật khẩu và chống đăng nhập trùng.
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

                // 2. Gửi LoginRequest rồi chờ LoginResponse
                var loginReq = new LoginRequestDto { Username = user, Password = pass };
                await NetworkManager.Instance.SendAsync(Packet.Create(PacketType.LoginRequest, loginReq));
                var responsePacket = await NetworkManager.Instance.WaitForPacketAsync(PacketType.LoginResponse);

                if (responsePacket == null)
                {
                    lblErrorTxt.ForeColor = Color.Red;
                    lblErrorTxt.Text = "Không nhận được phản hồi từ máy chủ!";
                    return;
                }

                var loginRes = responsePacket.DeserializePayload<LoginResponseDto>();

                if (loginRes != null && loginRes.Success)
                {
                    failedLoginCount = 0;
                    PlayerProfile.CurrentUser = loginRes;   // bê nguyên gói DTO vào PlayerProfile

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
                            lblErrorTxt.ForeColor = Color.Red;
                            lblErrorTxt.Text = $"Tài khoản hoặc mật khẩu không chính xác! ({failedLoginCount}/5)";
                            boxPasswordTxt.Clear();
                            boxPasswordTxt.Focus();
                        }
                    }
                    else
                    {
                        // Các lý do khác (ví dụ tài khoản đang đăng nhập ở nơi khác): hiện nguyên thông báo của server
                        lblErrorTxt.ForeColor = Color.Red;
                        lblErrorTxt.Text = message;
                    }
                }
            }
            catch (Exception ex)
            {
                lblErrorTxt.ForeColor = Color.Red;
                lblErrorTxt.Text = "Lỗi kết nối máy chủ!";
            }
            finally
            {
                // Mở lại nút bấm nếu không trong trạng thái bị khóa
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
            lblErrorTxt.ForeColor = Color.Red;

            // Chạy đếm ngược 60 giây và cập nhật trực tiếp lên Label
            for (int i = seconds; i > 0; i--)
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