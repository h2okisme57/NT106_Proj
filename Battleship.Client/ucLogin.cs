using Battleship.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using Battleship.Shared;


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
        }

        private void uclogin_Load(object sender, EventArgs e)
        {
            this.ActiveControl = null; // Xóa nháy chuột lúc mới mở form

            // BẮT SỰ KIỆN CLICK CHO MỌI THỨ TRÊN MÀN HÌNH
            this.Click += LoseFocus_Click; // Bắt cho mặt nền

            foreach (Control ctrl in this.Controls)
            {
                
                if (!(ctrl is TextBox))
                {
                    ctrl.Click += LoseFocus_Click;
                }
            }

            // TẠO ĐỘ THỤT LỀ TRÁI 
           
            SendMessage(boxUsernameTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);
            SendMessage(boxPasswordTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);

            // CÀI ĐẶT USERNAME
            boxUsernameTxt.Text = "Tên đăng nhập";
            boxUsernameTxt.ForeColor = Color.Gray;

            // Ký tự '\0' (Null) lệnh cho Windows hiển thị rõ chữ, không che gì cả
            boxPasswordTxt.PasswordChar = '\0';
            boxPasswordTxt.Text = "Mật khẩu";
            boxPasswordTxt.ForeColor = Color.Gray;

            // Gắn sự kiện
            boxUsernameTxt.Enter += boxUsernameTxt_Enter;
            boxUsernameTxt.Leave += boxUsernameTxt_Leave;

            boxPasswordTxt.Enter += boxPasswordTxt_Enter;
            boxPasswordTxt.Leave += boxPasswordTxt_Leave;





            //SỰ KIỆN CLICK CHO MỌI THỨ TRÊN MÀN HÌNH
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
            // Tìm Form gốc đang chứa ucLogin này
            Form parentForm = this.FindForm();

            if (parentForm != null)
            {
                // Tước quyền Focus từ cấp độ Form cha, TextBox sẽ bắt buộc phải nhả con trỏ chuột
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


                boxPasswordTxt.PasswordChar = '*';
            }
        }

        private void boxPasswordTxt_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(boxPasswordTxt.Text))
            {
                // PHẢI TẮT DẤU CHẤM TRƯỚC BẰNG KÝ TỰ '\0' (NULL)
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
                        lblErrorTxt.ForeColor = Color.Green;
                        lblErrorTxt.Text = "Đăng nhập thành công! Đang vào sảnh...";

                        await Task.Delay(500); // Đợi 0.5s để người dùng đọc thông báo thành công

                        Form popup = this.FindForm();
                        if (popup != null)
                        {
                            popup.DialogResult = DialogResult.OK; // Đóng Form Login để vào sảnh chính
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
            catch (Exception ex)
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
