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
    public partial class Lobby : UserControl
    {
        private FindRandomRoom findingRoomPanel;
        private FriendFightInv inviteAlert;
        private ProfileControl myProfileControl;
        private CreateRoom createRoomPanel;
        private JoinRoom joinRoomPanel;
        public Lobby()
        {
            InitializeComponent();
        }
        public void ReceiveChallenge(string challengerName)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ReceiveChallenge(challengerName)));
                return;
            }
            if (inviteAlert == null)
            {
                inviteAlert = new FriendFightInv();

                // Đăng ký sự kiện
                inviteAlert.AcceptClicked += FriendFightInv_AcceptClicked;
                inviteAlert.DeclineClicked += FriendFightInv_DeclineClicked;

                // Căn giữa màn hình
                inviteAlert.Location = new Point(
                    (this.ClientSize.Width - inviteAlert.Width) / 2,
                    (this.ClientSize.Height - inviteAlert.Height) / 2
                );

                this.Controls.Add(inviteAlert);
            }
        }

        private string GenerateRoomId()
        {
            Random rand = new Random();
            return rand.Next(1000, 9999).ToString();
        }

        private void FriendFightInv_AcceptClicked(object sender, EventArgs e)
        {
            inviteAlert.Visible = false;

            string challenger = inviteAlert.ChallengerName;
            // Gửi thông tin chấp nhận lời mời đến server

        }

        private void FriendFightInv_DeclineClicked(object sender, EventArgs e)
        {
            inviteAlert.Visible = false;

            string challenger = inviteAlert.ChallengerName;
            // Gửi thông tin từ chối lời mời đến server
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Lobby_Load(object sender, EventArgs e)
        {

        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            if (myProfileControl == null)
            {
                myProfileControl = new ProfileControl();
                int x = (ClientSize.Width - myProfileControl.Width) / 2;
                int y = (ClientSize.Height - myProfileControl.Height) / 2;
                myProfileControl.Location = new Point(x, y);
            }
            myProfileControl.Visible = true;
            myProfileControl.BringToFront();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (findingRoomPanel == null)
            {
                findingRoomPanel = new FindRandomRoom();

                // Đăng ký sự kiện khi người chơi bấm Hủy trên bảng ucFindingRoom
                findingRoomPanel.CancelClicked += FindingRoomPanel_CancelClicked;

                // Căn giữa màn hình sảnh
                findingRoomPanel.Location = new Point(
                    (this.ClientSize.Width - findingRoomPanel.Width) / 2,
                    (this.ClientSize.Height - findingRoomPanel.Height) / 2
                );

                // Nhúng vào giao diện
                this.Controls.Add(findingRoomPanel);
            }

            // Hiện bảng lên và đưa lên lớp trên cùng (đè các nút khác)
            findingRoomPanel.Visible = true;
            findingRoomPanel.BringToFront();

            // --------------------------------------------------------
            // TẠI ĐÂY: Thêm code gửi Socket TCP tới Server yêu cầu tìm trận
            // --------------------------------------------------------
        }

        private void FindingRoomPanel_CancelClicked(object sender, EventArgs e)
        {
            findingRoomPanel.Visible = false;

            // --------------------------------------------------------
            // TẠI ĐÂY: Thêm code gửi Socket TCP tới Server báo hủy tìm trận
            // --------------------------------------------------------
        }



        private void btnMakeRoom_Click(object sender, EventArgs e)
        {
            string newRoomId = GenerateRoomId();
            createRoomPanel.Visible = false;
            if (createRoomPanel == null)
            {
                createRoomPanel = new CreateRoom();
                createRoomPanel.CancelClicked += createRoomPanel_CancelClicked;

                createRoomPanel.Location = new Point((this.ClientSize.Width - createRoomPanel.Width) / 2, (this.ClientSize.Height - createRoomPanel.Height) / 2);
                this.Controls.Add(createRoomPanel);
            }
            createRoomPanel.SetRoomId(newRoomId);
            createRoomPanel.Visible = true;
            createRoomPanel.BringToFront();
            // (TẠI ĐÂY) Gửi gói tin TCP báo cho Server biết bạn vừa tạo phòng ID này
        }

        private void createRoomPanel_CancelClicked(object sender, EventArgs e)
        {
            createRoomPanel.Visible = false;


            // (TẠI ĐÂY) Gửi TCP báo Server xóa phòng này
        }

        private void btnJoinRoom_Click(object sender, EventArgs e)
        {
            if (joinRoomPanel == null)
            {
                joinRoomPanel = new JoinRoom();

                // Đăng ký sự kiện: Nhận ID và Hủy
                joinRoomPanel.JoinClicked += JoinRoomPanel_JoinClicked;
                joinRoomPanel.CancelClicked += (s, ev) => joinRoomPanel.Visible = false; // Ẩn bảng khi bấm Hủy

                // Căn giữa màn hình sảnh
                joinRoomPanel.Location = new Point(
                    (this.ClientSize.Width - joinRoomPanel.Width) / 2,
                    (this.ClientSize.Height - joinRoomPanel.Height) / 2
                );

                this.Controls.Add(joinRoomPanel);
            }

            // Hiện bảng lên lớp trên cùng
            joinRoomPanel.Visible = true;
            joinRoomPanel.BringToFront();
        }
        private void JoinRoomPanel_JoinClicked(object sender, EventArgs e)
        {
            string targetRoomId = joinRoomPanel.RoomId;


            joinRoomPanel.Visible = false;

            MessageBox.Show($"Đang xin vào phòng có ID: {targetRoomId}...", "Hệ thống");

            // --------------------------------------------------------
            // TẠI ĐÂY: Thêm code gửi gói tin Socket TCP lên Server
            // --------------------------------------------------------
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }

}
