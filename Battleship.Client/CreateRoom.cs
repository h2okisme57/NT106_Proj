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
    public partial class CreateRoom : UserControl
    {
        public event EventHandler CancelClicked;
        private int dotCount = 0;
        public CreateRoom()
        {
            InitializeComponent();
            timeWait.Tick += TimeWait_Tick;
        }
        public void SetRoomId(string roomId)
        {
            lblID.Text = "ID room: " + roomId;
        }

        private void TimeWait_Tick(object sender, EventArgs e)
        {
            dotCount++;
            if (dotCount > 3) dotCount = 0;

            // Cập nhật hiệu ứng dấu chấm
            lblWait.Text = "Waiting" + new string('.', dotCount);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible)
            {
                dotCount = 0;
                lblWait.Text = "Waiting";
                timeWait.Start(); // Bắt đầu hiệu ứng khi bảng hiện lên
            }
            else
            {
                timeWait.Stop(); // Tắt hiệu ứng khi bảng bị ẩn
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
           
        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
