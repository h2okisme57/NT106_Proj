using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Battleship.Client
{
    public partial class ucRegister : UserControl
    {
        // 1. GỌI HÀM LÕI WINDOWS ĐỂ TẠO THỤT LỀ (PADDING)
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int EM_SETMARGINS = 0xd3;
        private const int EC_LEFTMARGIN = 1;

        public ucRegister()
        {
            InitializeComponent();

            // Gắn sự kiện Load trực tiếp trong constructor
            this.Load += ucRegister_Load;
        }

        private void ucRegister_Load(object sender, EventArgs e)
        {

            // 2. THỤT LỀ TRÁI  CHO CẢ 3 Ô
            SendMessage(boxUsernameTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);
            SendMessage(boxPasswordTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);
            SendMessage(boxConfirmPassword.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);

            // CÀI ĐẶT PLACEHOLDER BAN ĐẦU
            boxUsernameTxt.Text = "Tên đăng nhập";
            boxUsernameTxt.ForeColor = Color.Gray;

            boxPasswordTxt.PasswordChar = '\0';
            boxPasswordTxt.Text = "Mật khẩu";
            boxPasswordTxt.ForeColor = Color.Gray;

            boxConfirmPassword.PasswordChar = '\0';
            boxConfirmPassword.Text = "Xác nhận mật khẩu";
            boxConfirmPassword.ForeColor = Color.Gray;

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
                // Gắn sự kiện nhả Focus cho tất cả mọi thứ TRỪ TextBox
                if (!(ctrl is TextBox))
                {
                    ctrl.Click += LoseFocus_Click;
                }

                // hàm sẽ tự động gọi lại chính nó để đào sâu vào tận lớp trong cùng.
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

        // ================= XỬ LÝ CONFIRM PASSWORD =================
        private void boxConfirmPassword_Enter(object sender, EventArgs e)
        {
            if (boxConfirmPassword.Text == "Xác nhận mật khẩu")
            {
                boxConfirmPassword.Text = "";
                boxConfirmPassword.ForeColor = Color.Black;
                boxConfirmPassword.PasswordChar = '●';
            }
        }

        private void boxConfirmPassword_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(boxConfirmPassword.Text))
            {
                boxConfirmPassword.PasswordChar = '\0';
                boxConfirmPassword.Text = "Xác nhận mật khẩu";
                boxConfirmPassword.ForeColor = Color.Gray;
            }
        }

        // ================= XỬ LÝ NÚT BẤM & ĐIỀU HƯỚNG =================
        private async void btnRegister_Click(object sender, EventArgs e)
        {
            string user = boxUsernameTxt.Text.Trim();
            string pass = boxPasswordTxt.Text.Trim();
            string confirm = boxConfirmPassword.Text.Trim();

            // Kiểm tra rỗng hoặc chưa nhập (đang là chữ gợi ý)
            if (string.IsNullOrWhiteSpace(user) || user == "Tên đăng nhập" ||
                string.IsNullOrWhiteSpace(pass) || pass == "Mật khẩu" ||
                string.IsNullOrWhiteSpace(confirm) || confirm == "Xác nhận mật khẩu")
            {
                lblErrorTxt.ForeColor = Color.Red;
                lblErrorTxt.Text = "Vui lòng nhập đầy đủ thông tin!";
                return;
            }

            // Kiểm tra mật khẩu khớp nhau
            if (pass != confirm)
            {
                lblErrorTxt.ForeColor = Color.Red;
                lblErrorTxt.Text = "Mật khẩu xác nhận không khớp!";
                return;
            }

            // Chỗ này sau này bạn sẽ nối với Database/Server. 
            // Tạm thời giả lập đăng ký thành công:
            lblErrorTxt.ForeColor = Color.Green;
            lblErrorTxt.Text = "Đăng ký thành công! Đang quay lại Đăng nhập...";

            await Task.Delay(800); // Dừng một lát để người dùng đọc thông báo

            SwitchToLogin();
        }

        private void llblLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Bấm chữ "Đã có tài khoản? Đăng nhập" thì quay về màn hình Login
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
    }
}