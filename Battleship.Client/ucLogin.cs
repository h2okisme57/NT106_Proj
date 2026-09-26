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
    public partial class ucLogin : UserControl
    {
        public ucLogin()
        {
            InitializeComponent();
        }

        private void uclogin_Load(object sender, EventArgs e)
        {

        }

        private void llblRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmMain mainForm = (frmMain)FindForm();
            if (mainForm != null)
            {
                // Chuyển sang giao diện đăng ký
                mainForm.SwitchView(new ucRegister());
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string user = boxUsernameTxt.Text.Trim();
            string pass = boxPasswordTxt.Text.Trim();
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
               lblErrorTxt.Text = "Vui lòng nhập đầy đủ thông tin!";
                return;
            }
            if (user == "admin" && pass == "123456")
            {
                lblErrorTxt.ForeColor = Color.Green;
                lblErrorTxt.Text = "Đăng nhập thành công! Đang chuyển hướng...";

                //code dan den lobby

                //frmMain mainForm = (frmMain)this.FindForm();
                //if (mainForm != null)
                //{
                //    mainForm.SwitchView(new ucLobby());
                //}


            }
            else
            {
               
                lblErrorTxt.Text = "Sai Username hoặc Password!";
                boxPasswordTxt.Clear();
                boxPasswordTxt.Focus();
            }
        }
    }
}
