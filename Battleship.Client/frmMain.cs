using System;
using System.Drawing;
using System.Windows.Forms;
using Battleship.Shared; // Thêm namespace này để dùng MatchFoundDto

namespace Battleship.Client
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            Load += frmMain_Load;
        }

        // Chuyển qua màn hình trận đấu Battleship
        public void SwitchToGameScreen()
        {
            // 1. TỐI ƯU KÍCH THƯỚC: Dùng ClientSize thay vì Size
            // Thiết lập vùng làm việc thực tế bên trong Form đủ rộng (1100x800) để không bị đè/cắt xén control
            this.ClientSize = new Size(978, 742);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 2. GIẢ LẬP DỮ LIỆU TRẬN ĐẤU (MatchFoundDto) ĐỂ TEST MÀN HÌNH
            MatchFoundDto dummyMatch = new MatchFoundDto
            {
                RoomId = "TEST_ROOM_01",
                OpponentName = "Kẻ Hủy Diệt (Bot)", // Tên đối thủ giả lập
                IsHost = true
            };

            // 3. Mở màn hình BattleshipDesign với dữ liệu test
            SwitchView(new BattleshipDesign());
        }

        // Chuyển đổi màn hình hiển thị trong pnlContainer sang UserControl khác
        public void SwitchView(UserControl uc)
        {
            pnlContainer.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContainer.Controls.Add(uc);
            uc.BringToFront();
        }

        // Khi Form Main vừa mở lên, tự động hiển thị màn hình chơi game
        private void frmMain_Load(object sender, EventArgs e)
        {
            SwitchView(new Lobby());
            
        }

        public void TestBoardGame()
        {
            Form testForm = new Form
            {
                Text = "Kiểm thử Bàn Cờ Battleship",
                Size = new Size(600, 600),
                StartPosition = FormStartPosition.CenterScreen
            };

            BoardGame testBoard = new BoardGame
            {
                Dock = DockStyle.Fill
            };

            testBoard.OnCellClicked += (col, row) =>
            {
                MessageBox.Show($"Bạn vừa bấm vào ô: Cột {col} - Hàng {row}", "Test Click", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            testForm.Controls.Add(testBoard);
            testForm.ShowDialog();
        }

        private void pnlContainer_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}