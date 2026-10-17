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
    public partial class Profile : UserControl
    {
        public Profile()
        {
            InitializeComponent();
            Profile_load();
        }

        private void Profile_load()
        {

            tabProfile.Multiline = true;
            //Data
            string username = "admin";
            int id = 0;
            int wincco = 10;
            int lose = 5;


            lblID.Text = "ID: " + id.ToString();
            lblUsername.Text = "Username: " + username;
            lblWin.Text = "Wins: " + wincco.ToString();
            lblLose.Text = "Losses: " + lose.ToString();


            AddFriendCard("Dũng", true);
            AddFriendCard("Đức", false);
            AddFriendCard("Trí", true);
            AddFriendCard("Hùng", false);
        }


        public void AddFriendCard(string friendName, bool isOnline)
        {
            // Ensure parent panel enables vertical scrolling when items exceed height
            flpFriend.AutoScroll = true;

            // Determine available width (account for vertical scrollbar width if active)
            int scrollbarWidth = SystemInformation.VerticalScrollBarWidth;
            int targetWidth = flpFriend.ClientSize.Width - scrollbarWidth - 10;

            Panel card = new Panel
            {
                Size = new Size(Math.Max(targetWidth, 180), 50),
                BackColor = Color.WhiteSmoke,
                Margin = new Padding(0, 0, 0, 4)
            };

            // Horizontally center the card inside flpFriend
            int centerX = Math.Max((flpFriend.ClientSize.Width - card.Width) / 2, 0);

            // Label positioning
            Label lblName = new Label
            {
                Text = friendName,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(10, 9),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Label lblStatus = new Label
            {
                Text = isOnline ? "● Online" : "○ Offline",
                ForeColor = isOnline ? Color.LimeGreen : Color.Gray,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                AutoSize = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // Calculate position for status label relative to right side of card
            lblStatus.Location = new Point(card.Width - 75, 10);

            card.Controls.Add(lblName);
            card.Controls.Add(lblStatus);
                
            // If pnlFriend is a standard Panel, calculate Y offset manually
            if (!(pnlFriend is FlowLayoutPanel))
            {
                int spacing = 4;
                int yOffset = pnlFriend.Controls.Count * (card.Height + spacing);
                card.Location = new Point(centerX, yOffset);
            }
            else
            {
                card.Margin = new Padding(centerX, 2, centerX, 2);
            }

            flpFriend.Controls.Add(card);
        }
        private void tabPageProfile_Click(object sender, EventArgs e)
        {

        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Parent?.Controls.Remove(this);
            this.Dispose();
        }

        private void tabPageFriend_Click(object sender, EventArgs e)
        {

        }
    }
}
