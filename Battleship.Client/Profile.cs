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
            Profile_Load();

        }
        private void Profile_Load()
        {
            tabProfile.Multiline = true;
        }

        private void tabPageProfile_Click(object sender, EventArgs e)
        {

        }
    }
}
