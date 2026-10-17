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
    public partial class joinRoom : UserControl
    {
        public joinRoom()
        {
            InitializeComponent();
            Join_Load();
        }
        private void Join_Load()
        {
            ClearRoomList();

            // Dữ liệu giả lập để test giao diện
            AddRoomCard("R101", "Dũng", 1);
            AddRoomCard("R102", "Đức", 2);
            AddRoomCard("R103", "Trí", 1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Parent?.Controls.Remove(this);
            this.Dispose();
        }

        private void joinRoom_Load(object sender, EventArgs e)
        {
            pnlJoin.AutoScroll = true;
            // Làm nền mờ bán trong suốt (Đen mờ) để giao diện hiện đại giống Lobby
            pnlJoin.BackColor = Color.FromArgb(120, 0, 0, 0);
            pnlJoin.FlowDirection = FlowDirection.TopDown;
            pnlJoin.WrapContents = false;

            // Ép vẽ lại để chống nhòe khi cuộn thanh cuộn
            pnlJoin.Scroll += (s, ev) => { pnlJoin.Invalidate(); };
        }

        public void ClearRoomList()
        {
            if (pnlJoin.InvokeRequired)
            {
                pnlJoin.Invoke(new Action(ClearRoomList));
                return;
            }

            // Giải phóng triệt để RAM của các control cũ chống lag
            foreach (Control ctrl in pnlJoin.Controls)
            {
                ctrl.Dispose();
            }
            pnlJoin.Controls.Clear();
        }


        // =========================================================
        // HÀM 2: VẼ THẺ PHÒNG MỚI (Luồng mạng sẽ gọi hàm này)
        // =========================================================
        public void AddRoomCard(string roomId, string hostName, int currentPlayers)
        {
            // Bắt buộc dùng Invoke vì hàm này được gọi từ luồng nhận dữ liệu của Server
            if (pnlJoin.InvokeRequired)
            {
                pnlJoin.Invoke(new Action(() => AddRoomCard(roomId, hostName, currentPlayers)));
                return;
            }

            // 1. Khung của thẻ phòng
            Panel card = new Panel();
            card.Size = new Size(pnlJoin.Width - 10, 60); // Trừ hao thanh cuộn
            card.BackColor = Color.FromArgb(80, 255, 255, 255); // Trắng mờ
            card.Margin = new Padding(5, 5, 5, 5);

            // 2. Tên chủ phòng & ID Phòng
            Label lblHost = new Label();
            lblHost.Text = $"Phòng của {hostName} (ID: {roomId})";
            lblHost.ForeColor = Color.White;
            lblHost.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblHost.Location = new Point(10, 18);
            lblHost.AutoSize = true;
            lblHost.BackColor = Color.Transparent;

            // 3. Số người chơi (Màu xanh nếu 1 người, màu đỏ nếu đã đầy)
            Label lblCount = new Label();
            lblCount.Text = $"{currentPlayers}/2";
            lblCount.ForeColor = currentPlayers == 1 ? Color.LimeGreen : Color.Red;
            lblCount.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblCount.Location = new Point(card.Width - 140, 18);
            lblCount.AutoSize = true;
            lblCount.BackColor = Color.Transparent;

            // 4. Nút Tham Gia
            Button btnJoin = new Button();
            btnJoin.Text = "Join";
            btnJoin.Size = new Size(80, 35);
            btnJoin.Location = new Point(card.Width - 90, 12);
            btnJoin.BackColor = ColorTranslator.FromHtml("#8B1E2D"); // Đỏ đô
            btnJoin.ForeColor = Color.White;
            btnJoin.FlatStyle = FlatStyle.Flat;
            btnJoin.FlatAppearance.BorderSize = 0;
            btnJoin.Cursor = Cursors.Hand;
            btnJoin.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            // Xử lý logic nếu phòng đã đầy
            if (currentPlayers >= 2)
            {
                btnJoin.Enabled = false;
                btnJoin.BackColor = Color.Gray;
                btnJoin.Text = "Full";
            }
            else
            {
                // Gắn sự kiện click gửi Packet lên Server
                btnJoin.Click += async (s, ev) =>
                {
                    // Vô hiệu hóa nút tạm thời để tránh bấm 2 lần
                    btnJoin.Enabled = false;
                    btnJoin.Text = "Đang vào...";

                    // TODO: Gửi Packet JoinRoomRequest lên Server
                    // var packet = new Packet { Type = PacketType.JoinRoomRequest, Payload = roomId };
                    // await ClientNetwork.SendAsync(packet);
                };
            }

            // Nhét Label và Nút vào Thẻ phòng
            card.Controls.Add(lblHost);
            card.Controls.Add(lblCount);
            card.Controls.Add(btnJoin);

            // Nhét Thẻ phòng vào Danh sách
            pnlJoin.Controls.Add(card);
        }


        private void flowLayoutPanel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Ham tim id

        }
    }
}