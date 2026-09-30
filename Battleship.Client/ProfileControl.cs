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
    public partial class ProfileControl : UserControl
    {
        public ProfileControl()
        {
            InitializeComponent();
            LoadFriendsList();
            LoadProfile();
        }

        private void LoadProfile()
        {
            string username = "Player";
            string condition = "Online";
            lblUsername.Text = username;
            lblCondition.Text = condition;
            int iWin = 9;
            int iLose = 6;
            string sWin = "Win: " + iWin.ToString();
            string sLose = "Lose: " + iLose.ToString();
            lblWin.Text = sWin;
            lblLose.Text = sLose;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void LoadFriendsList()
        {
            string[] friends = { "Alice", "Bob", "Charlie", "David", "Eve" };


            foreach (string friend in friends)
            {
                FriendProfile friendProfile = new FriendProfile();
                bool IsOnline;
                friendProfile.lblFriendName.Text = friend;
                if (friend == "Alice")
                {
                    IsOnline = true;
                    friendProfile.lblFriendCondition.ForeColor = Color.Green;
                }
                else
                {
                    IsOnline = false;
                    friendProfile.lblFriendCondition.ForeColor = Color.Gray;
                }
                if (IsOnline)
                {
                    friendProfile.lblFriendCondition.Text = "Online";
                }
                else
                {
                    friendProfile.lblFriendCondition.Text = "Offline";
                }
                LayoutFriend.Controls.Add(friendProfile);


                //Ket noi de solo ban be
                friendProfile.btnFight.Visible = IsOnline;
                friendProfile.btnFight.Click += (s, ev) => {
                    //Gui di yeu kien de bat dau solo voi ban be nay
                };
                //Cho nhan yeu cau solo tu ban be

            }
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {


        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            Form mainForm = FindForm();
            if (mainForm != null)
            {
                mainForm.Controls.Clear();

                ucLogin loginScreen = new ucLogin();
                loginScreen.Dock = DockStyle.Fill;
                mainForm.Controls.Add(loginScreen);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Visible = false;
        }
    }
    }
