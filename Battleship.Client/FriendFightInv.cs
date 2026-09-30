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
    public partial class FriendFightInv : UserControl
    {
        public event EventHandler AcceptClicked;
        public event EventHandler DeclineClicked;
        public string ChallengerName { get; set; }
        public FriendFightInv()
        {
            InitializeComponent();
        }
        public void SetMessage(string challengerName)
        {
            ChallengerName = challengerName;
            lblInviteMess.Text = $"{challengerName} has challenged you to a fight!";
        }

        //Dua thong tin accept va decline ra ngoai de form chinh bat su kien
        private void btnAccept_Click(object sender, EventArgs e)
        {
            AcceptClicked?.Invoke(this, EventArgs.Empty);
        }

        private void btnDecline_Click(object sender, EventArgs e)
        {
            DeclineClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
