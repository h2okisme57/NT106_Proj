using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

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

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblErrorTxt.ForeColor = Color.Red; // Đảm bảo màu đỏ cho lỗi
                lblErrorTxt.Text = "Vui lòng nhập đầy đủ thông tin!";
                return;
            }

            if (user == "admin" && pass == "123456")
            {
                lblErrorTxt.ForeColor = Color.Green;
                lblErrorTxt.Text = "Đăng nhập thành công! Đang vào sảnh...";

                // DỪNG 0.5 GIÂY: Để người chơi kịp đọc dòng chữ màu xanh trước khi màn hình chuyển đổi
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
                lblErrorTxt.Text = "Sai Username hoặc Password!";

                boxPasswordTxt.Clear();
                boxPasswordTxt.Focus();
            }
        }
    }
}
