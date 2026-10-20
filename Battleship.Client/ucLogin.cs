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
        // GỌI HÀM LÕI WINDOWS ĐỂ TẠO THỤT LỀ (PADDING)
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int EM_SETMARGINS = 0xd3;
        private const int EC_LEFTMARGIN = 1;

        public ucLogin()
        {
            InitializeComponent();
            this.Load += ucLogin_Load;
        }

        private void ucLogin_Load(object sender, EventArgs e)
        {
            this.ActiveControl = null; // Xóa nháy chuột lúc mới mở form

            // 1. TẠO ĐỘ THỤT LỀ TRÁI CHO TEXTBOX
            SendMessage(boxUsernameTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);
            SendMessage(boxPasswordTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);

            // 2. CÀI ĐẶT PLACEHOLDER BAN ĐẦU
            boxUsernameTxt.Text = "Tên đăng nhập";
            boxUsernameTxt.ForeColor = Color.Gray;

            // Ký tự '\0' (Null) lệnh cho Windows hiển thị rõ chữ, không che dấu chấm
            boxPasswordTxt.PasswordChar = '\0';
            boxPasswordTxt.Text = "Mật khẩu";
            boxPasswordTxt.ForeColor = Color.Gray;

            // 3. GẮN SỰ KIỆN FOCUS ENTER / LEAVE
            boxUsernameTxt.Enter += boxUsernameTxt_Enter;
            boxUsernameTxt.Leave += boxUsernameTxt_Leave;

            boxPasswordTxt.Enter += boxPasswordTxt_Enter;
            boxPasswordTxt.Leave += boxPasswordTxt_Leave;

            // 4. BẮT SỰ KIỆN CLICK TOÀN MÀN HÌNH ĐỂ ÉP NHẢ FOCUS TEXTBOX
            this.Click += LoseFocus_Click;
            AttachClickToAllControls(this);
        }

        private void AttachClickToAllControls(Control container)
        {
            foreach (Control ctrl in container.Controls)
            {
                // Gắn sự kiện nhả Focus cho tất cả mọi thứ TRỪ TextBox
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

        // ================= XỬ LÝ PASSWORD =================
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

        // ================= CHUYỂN SANG MÀN HÌNH ĐĂNG KÝ =================
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

        // ================= XỬ LÝ ĐĂNG NHẬP (LÔGIC CHÍNH) =================
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string user = boxUsernameTxt.Text.Trim();
            string pass = boxPasswordTxt.Text.Trim();

            // 1. Kiểm tra rỗng hoặc chữ placeholder (watermark)
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) ||
                user == "Tên đăng nhập" || pass == "Mật khẩu")
            {
                lblErrorTxt.ForeColor = Color.Red;
                lblErrorTxt.Text = "Vui lòng nhập đầy đủ thông tin!";
                return;
            }

            lblErrorTxt.ForeColor = Color.Blue;
            lblErrorTxt.Text = "Đang kết nối...";
            btnLogin.Enabled = false; // Vô hiệu hóa nút bấm để tránh spam

            try
            {
                // 2. Tự động kết nối & Bắt tay (Handshake) với Server nếu chưa kết nối
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

                // 3. Đóng gói DTO thành Packet
                var loginReq = new LoginRequestDto
                {
                    Username = user,
                    Password = pass
                };
                var packet = Packet.Create(PacketType.LoginRequest, loginReq);

                // 4. Gửi lên Server
                await NetworkManager.Instance.SendAsync(packet);

                // 5. Chờ phản hồi LoginResponse từ Server
                var responsePacket = await NetworkManager.Instance.WaitForPacketAsync(PacketType.LoginResponse);

                if (responsePacket != null)
                {
                    var loginRes = responsePacket.DeserializePayload<LoginResponseDto>();

                    if (loginRes != null && loginRes.Success)
                    {
                        // BÊ NGUYÊN GÓI DTO VÀO PLAYER PROFILE. XONG!
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
                        lblErrorTxt.ForeColor = Color.Red;
                        lblErrorTxt.Text = loginRes?.Message ?? "Tài khoản hoặc mật khẩu không chính xác!";
                        boxPasswordTxt.Clear();
                        boxPasswordTxt.Focus();
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
                btnLogin.Enabled = true; // Mở lại nút bấm khi hoàn tất
            }
        }
    }
}