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
    public partial class JoinRoom : UserControl
    {
        public event EventHandler JoinClicked;
        public event EventHandler CancelClicked;
        public JoinRoom()
        {
            InitializeComponent();
        }
        public string RoomId
        {
            get { return boxJoinEnter.Text.Trim(); } // Trim() giúp xóa khoảng trắng thừa ở 2 đầu
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(boxJoinEnter.Text))
            {
                MessageBox.Show("Vui lòng nhập ID phòng trước khi vào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Phát tín hiệu ra ngoài (LobbyForm sẽ nhận được và đóng bảng này lại)
            JoinClicked?.Invoke(this, EventArgs.Empty);

            // Xóa nội dung đã nhập để lần sau mở lên hộp thoại sẽ trống
            boxJoinEnter.Clear();
        }
    }
}
