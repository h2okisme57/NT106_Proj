using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battleship.Client
{
    public partial class ucRegister : UserControl
    {
        public ucRegister()
        {
            InitializeComponent();
        }

        private void llblLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmMain mainForm = (frmMain)FindForm();
            if (mainForm != null)
            {
                mainForm.SwitchView(new ucLogin());
            }
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            string user = boxUsername.Text.Trim();
            string pass = boxPassword.Text.Trim();
            string confirmPass = boxPasswordCofirm.Text.Trim();
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(confirmPass))
            {
                lblErrorTxt.Text = "Vui lòng nhập đầy đủ thông tin!";
                return;
            }
            if (pass != confirmPass)
            {
                lblErrorTxt.Text = "Mật khẩu xác nhận không khớp!";
                boxPasswordCofirm.Clear();
                boxPasswordCofirm.Focus();
                return;
            }
            lblErrorTxt.ForeColor = Color.Green;
            lblErrorTxt.Text = "Đăng ký thành công! Đang chuyển hướng...";
            
            btnRegister.Enabled = false;
            await Task.Delay(2000);
            btnRegister.Enabled = true;
            llblLogin_LinkClicked(sender, null);
        }

        private void boxPassword_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
