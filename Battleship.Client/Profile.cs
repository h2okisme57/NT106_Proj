using Battleship.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battleship.Client
{
    public partial class Profile : UserControl
    {
        public Profile()
        {
            InitializeComponent();
            _ = LoadProfileAndFriendsAsync();
        }

        private async Task LoadProfileAndFriendsAsync()
        {
            tabProfile.Multiline = true;

            // 1. Hiển thị thông tin cá nhân từ PlayerProfile
            if (PlayerProfile.CurrentUser != null)
            {
                lblID.Text = $"ID: {PlayerProfile.Id}";
                lblUsername.Text = $"Username: {PlayerProfile.Username}";
                lblWin.Text = $"Wins: {PlayerProfile.Wins}";
                lblLose.Text = $"Losses: {PlayerProfile.Losses}";
            }

            // 2. Tải danh sách bạn bè từ Server
            await FetchFriendsListAsync();
        }

        private async Task FetchFriendsListAsync()
        {
            flpFriend.Controls.Clear();

            try
            {
                if (!NetworkManager.Instance.IsConnected) return;

                // Gửi request yêu cầu danh sách bạn bè
                var packet = Packet.Create(PacketType.GetFriendsRequest, PlayerProfile.Id);
                await NetworkManager.Instance.SendAsync(packet);

                // Nhận response từ Server
                var responsePacket = await NetworkManager.Instance.WaitForPacketAsync(PacketType.GetFriendsResponse);

                if (responsePacket != null)
                {
                    var response = responsePacket.DeserializePayload<GetFriendsResponseDto>();

                    if (response != null && response.Success)
                    {
                        foreach (var friend in response.Friends)
                        {
                            AddFriendCard(friend.Username, friend.IsOnline);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Có thể hiển thị thông báo lỗi lên UI nếu cần
                Console.WriteLine($"Lỗi tải bạn bè: {ex.Message}");
            }
        }

        public void AddFriendCard(string friendName, bool isOnline)
        {
            flpFriend.AutoScroll = true;

            int scrollbarWidth = SystemInformation.VerticalScrollBarWidth;
            int targetWidth = flpFriend.ClientSize.Width - scrollbarWidth - 10;

            Panel card = new Panel
            {
                Size = new Size(Math.Max(targetWidth, 180), 50),
                BackColor = Color.WhiteSmoke,
                Margin = new Padding(0, 0, 0, 4)
            };

            int centerX = Math.Max((flpFriend.ClientSize.Width - card.Width) / 2, 0);

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

            lblStatus.Location = new Point(card.Width - 75, 10);

            card.Controls.Add(lblName);
            card.Controls.Add(lblStatus);

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

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Parent?.Controls.Remove(this);
            this.Dispose();
        }

        private void tabPageProfile_Click(object sender, EventArgs e) { }
        private void tabPageFriend_Click(object sender, EventArgs e) { }
        private void lblWin_Click(object sender, EventArgs e) { }
        private void lblLose_Click(object sender, EventArgs e) { }
        private void lblID_Click(object sender, EventArgs e) { }
        private void lblUsername_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
    }
}