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
        public static void SwitchToGameScreen(MatchFoundDto? matchInfo = null)
        {
            frmMain mainForm = Application.OpenForms["frmMain"] as frmMain;
            if (mainForm == null) return;

            mainForm.ClientSize = new Size(978, 742);
            mainForm.StartPosition = FormStartPosition.CenterScreen;
            mainForm.Location = new Point(
                (Screen.PrimaryScreen.WorkingArea.Width - mainForm.Width) / 2,
                (Screen.PrimaryScreen.WorkingArea.Height - mainForm.Height) / 2
            );

            // matchInfo == null -> Chế độ Test cục bộ (IsOnlineMode = false)
            // matchInfo != null -> Chế độ Online thật từ RoomWait (IsOnlineMode = true)
            mainForm.Controls.Clear();

            var gameScreen = new BattleshipDesign(matchInfo);
            gameScreen.Dock = DockStyle.Fill;
            mainForm.Controls.Add(gameScreen);
            gameScreen.BringToFront();
        }

        // Chuyển đổi màn hình hiển thị trong pnlContainer sang UserControl khác
        public static void SwitchView(UserControl uc)
        {
            frmMain mainForm = Application.OpenForms["frmMain"] as frmMain;
            if (mainForm != null)
            {
                mainForm.pnlContainer.Controls.Clear();
                uc.Dock = DockStyle.Fill;
                mainForm.pnlContainer.Controls.Add(uc);
                uc.BringToFront();
            }
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