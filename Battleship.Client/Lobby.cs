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
        //vo hieu cac button khi dang hien thi access

        private bool isPopupOpen = false;
        public Lobby()
        {
            InitializeComponent();
        }

        public void ShowPopup(UserControl popup)
        {
            if (popup == null) return;

            isPopupOpen = true; // Bật cờ khóa

            popup.Visible = true;
            popup.BringToFront();
        }

        public void HidePopup(UserControl popup)
        {
            if (popup != null) popup.Visible = false;

            isPopupOpen = false; // Tắt cờ khóa
        }

        private string GenerateRoomId()
        {
            Random rand = new Random();
            return rand.Next(1000, 9999).ToString();
        }



        //-------Ham xu ly su kien khi nhan loi moi choi tu ban be
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
                    (ClientSize.Width - inviteAlert.Width) / 2,
                    (ClientSize.Height - inviteAlert.Height) / 2
                );

                Controls.Add(inviteAlert);
            }
            inviteAlert.ChallengerName = challengerName;
            ShowPopup(inviteAlert);
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


        //-------Ham cac nut
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Lobby_Load(object sender, EventArgs e)
        {

        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            if (isPopupOpen) return;
            if (myProfileControl == null)
            {
                myProfileControl = new ProfileControl();
                int x = (ClientSize.Width - myProfileControl.Width) / 2;
                int y = (ClientSize.Height - myProfileControl.Height) / 2;
                myProfileControl.Location = new Point(x, y);
                Controls.Add(myProfileControl);
                myProfileControl.CancelClicked += MyProfileControl_CancelClicked;
            }
            myProfileControl.Visible = true;
            myProfileControl.BringToFront();

            ShowPopup(myProfileControl);
        }

        private void MyProfileControl_CancelClicked(object? sender, EventArgs e)
        {
            myProfileControl.Visible = false;
            HidePopup(myProfileControl);
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (isPopupOpen) return;
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
            ShowPopup(findingRoomPanel);

            // --------------------------------------------------------
            // TẠI ĐÂY: Thêm code gửi Socket TCP tới Server yêu cầu tìm trận
            // --------------------------------------------------------
        }

        private void FindingRoomPanel_CancelClicked(object sender, EventArgs e)
        {
            findingRoomPanel.Visible = false;
            HidePopup(findingRoomPanel);

            // --------------------------------------------------------
            // TẠI ĐÂY: Thêm code gửi Socket TCP tới Server báo hủy tìm trận
            // --------------------------------------------------------
        }



        private void btnMakeRoom_Click(object sender, EventArgs e)
        {
            if (isPopupOpen) return;
            string newRoomId = GenerateRoomId();
            if (createRoomPanel == null)
            {
                createRoomPanel = new CreateRoom();
                createRoomPanel.CancelClicked += createRoomPanel_CancelClicked;

                createRoomPanel.Location = new Point((this.ClientSize.Width - createRoomPanel.Width) / 2, (this.ClientSize.Height - createRoomPanel.Height) / 2);
                this.Controls.Add(createRoomPanel);
            }
            createRoomPanel.SetRoomId(newRoomId);
            ShowPopup(createRoomPanel);

            // (TẠI ĐÂY) Gửi gói tin TCP báo cho Server biết bạn vừa tạo phòng ID này
        }

        private void createRoomPanel_CancelClicked(object sender, EventArgs e)
        {
            createRoomPanel.Visible = false;
            HidePopup(createRoomPanel);

            // (TẠI ĐÂY) Gửi TCP báo Server xóa phòng này
        }

        private void btnJoinRoom_Click(object sender, EventArgs e)
        {
            if (isPopupOpen) return;
            if (joinRoomPanel == null)
            {
                joinRoomPanel = new JoinRoom();

                // Đăng ký sự kiện: Nhận ID và Hủy
                joinRoomPanel.JoinClicked += JoinRoomPanel_JoinClicked;
                joinRoomPanel.CancelClicked += joinRoomPanel_CancelClicked; // Ẩn bảng khi bấm Hủy

                // Căn giữa màn hình sảnh
                joinRoomPanel.Location = new Point(
                    (this.ClientSize.Width - joinRoomPanel.Width) / 2,
                    (this.ClientSize.Height - joinRoomPanel.Height) / 2
                );

                this.Controls.Add(joinRoomPanel);
            }

            // Hiện bảng lên lớp trên cùng
            ShowPopup(joinRoomPanel);
        }


        private void JoinRoomPanel_JoinClicked(object sender, EventArgs e)
        {
            string targetRoomId = joinRoomPanel.RoomId;


            joinRoomPanel.Visible = false;


            // --------------------------------------------------------
            // TẠI ĐÂY: Thêm code gửi gói tin Socket TCP lên Server
            // --------------------------------------------------------
        }

         private void joinRoomPanel_CancelClicked(object sender, EventArgs e)
        {
            joinRoomPanel.Visible = false;
            HidePopup(joinRoomPanel);
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }

}
