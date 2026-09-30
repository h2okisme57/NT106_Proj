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
    public partial class FindRandomRoom : UserControl
    {
        public event EventHandler CancelClicked;
        private int dotCount = 0;
        public FindRandomRoom()
        {
            InitializeComponent();
            timeFind.Tick += TimeFind_Tick;
        }
        private void TimeFind_Tick(object sender, EventArgs e)
        {
            dotCount++;

            if (dotCount > 3)
            {
                dotCount = 0;
            }

            string dots = new string('.', dotCount);
            lblFindStatus.Text = "Finding" + dots;
        }
        // Khi UserControl hiển thị hoặc ẩn, bắt đầu hoặc dừng Timer
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (this.Visible)
            {
                dotCount = 0;
                lblFindStatus.Text = "Finding";
                timeFind.Start();
            }
            else
            {
                timeFind.Stop();
            }


        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }

        //Gui thong tin tim tran den server
    }
}
