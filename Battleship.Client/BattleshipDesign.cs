using Battleship.Shared;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Battleship.Client
{
    public partial class BattleshipDesign : UserControl
    {
        Color Outline = Color.FromArgb(88, 160, 200); // Màu viền xanh đậm
        private BoardGame playerBoard;
        public BoardGame EnemyBoard;
        private MatchFoundDto _currentMatch;

        public BattleshipDesign()
        {
            InitializeComponent();

            // 1. QUAN TRỌNG: Đăng ký sự kiện Load để hàm vẽ màu xám tự động chạy khi giao diện đã sẵn sàng
            this.Load += BattleshipDesign_Load;

            // 2. Khởi tạo dữ liệu TRƯỚC
            _currentMatch = new MatchFoundDto
            {
                RoomId = "Room_Test_999",
                OpponentName = "Kẻ Hủy Diệt",
                IsHost = true
            };

            // 3. Sau đó mới gọi hàm set tên (nếu gọi ngược lại tên sẽ bị trống)
            SetupPlayerNames();
            SetupPlayerBoard();
        }

        // Sự kiện này sẽ tự động chạy sau khi các Control đã có kích thước thực tế
        private void BattleshipDesign_Load(object sender, EventArgs e)
        {
            SetDefaultGrayColors();
        }

        private void SetupPlayerBoard()
        {
            pnlPlayerBoard.Controls.Clear();
            pnlEnemyBoard.Controls.Clear();

            playerBoard = new BoardGame();
            EnemyBoard = new BoardGame();

            playerBoard.Dock = DockStyle.Fill;
            EnemyBoard.Dock = DockStyle.Fill;

            playerBoard.OnCellClicked += PlayerBoard_OnCellClicked;
            EnemyBoard.OnCellClicked += EnemyBoard_OnCellClicked;

            pnlPlayerBoard.Controls.Add(playerBoard);
            pnlEnemyBoard.Controls.Add(EnemyBoard);

            //  Ép tắt khung lưới mặc định của WinForms
            tableLayoutPanel3.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel4.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel5.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel6.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;

            tableLayoutPanel4.CellPaint += tableLayoutPanel4_CellPaint;
            tableLayoutPanel3.CellPaint += tableLayoutPanel3_CellPaint;
            tableLayoutPanel5.CellPaint += tableLayoutPanel5_CellPaint;
            tableLayoutPanel6.CellPaint += tableLayoutPanel6_CellPaint;
        }

        private void PlayerBoard_OnCellClicked(int col, int row)
        {
            MessageBox.Show($"Bạn vừa nhắm bắn vào tọa độ: Cột {col}, Hàng {row}", "Hệ thống");
        }

        private void EnemyBoard_OnCellClicked(int col, int row)
        {
            MessageBox.Show($"Bạn vừa nhắm bắn vào tọa độ: Cột {col}, Hàng {row}", "Hệ thống");
        }

        private void SetupPlayerNames()
        {
            string myName = PlayerProfile.Username ?? string.Empty;
            string enemyName = _currentMatch?.OpponentName ?? "Waiting for opponent...";

            pnlPlayerName.Text = myName;
            pnlEnemyName.Text = enemyName;
        }

        private Image CreateCircleImage(Color color, int size)
        {
            if (size <= 0) size = 20; // Tránh lỗi size = 0

            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                int margin = 2;
                int drawSize = size - (margin * 2);

                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, margin, margin, drawSize, drawSize);
                }
            }
            return bmp;
        }


        public void UpdateCellStatus(PictureBox targetCell, string status)
        {
            int size = Math.Min(targetCell.Width, targetCell.Height);

            // QUAN TRỌNG: Nếu size = 0 thì gán mặc định là 20 để luôn nhìn thấy chấm tròn
            if (size <= 0) size = 20;

            switch (status.ToLower())
            {
                case "red":
                    targetCell.Image = CreateCircleImage(Color.Red, size);
                    break;
                case "miss":
                    targetCell.Image = CreateCircleImage(Color.LimeGreen, size);
                    break;
                case "destroyed":
                    targetCell.Image = CreateCircleImage(Color.Yellow, size);
                    break;
                case "gray":
                    targetCell.Image = CreateCircleImage(Color.Gray, size);
                    break;
                default:
                    targetCell.Image = null;
                    break;
            }
        }

        private void SetDefaultGrayColors()
        {
            TableLayoutPanel[] allStatusTables = new TableLayoutPanel[]
            {
                TableShots,
                tablePlayerConditionCarrier,
                tablePlayerConditionSubmarine,
                tablePlayerConditionBattleship,
                tablePlayerConditionCruiser,
                tablePlayerConditionDestroyer,
                tableEnemyConditionCarrier,
                tableEnemyConditionSubmarine,
                tableEnemyConditionBattleship,
                tableEnemyConditionCrusier,
                tableEnemyConditionDestroyer
            };

            foreach (TableLayoutPanel table in allStatusTables)
            {
                if (table == null) continue;

                // Xóa rác bên trong bảng (nếu có)
                table.Controls.Clear();

                // Lấy số dòng và số cột thực tế của bảng bạn đã thiết kế
                int cols = table.ColumnCount > 0 ? table.ColumnCount : 1;
                int rows = table.RowCount > 0 ? table.RowCount : 1;

                // Tự động sinh PictureBox và nhét vào từng ô trống của bảng
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        PictureBox picBox = new PictureBox
                        {
                            Dock = DockStyle.Fill, // Phóng to vừa khít ô
                            Margin = new Padding(1),
                            SizeMode = PictureBoxSizeMode.Zoom, // Dùng Zoom để tự co giãn tròn trịa theo ô
                            BackColor = Color.Transparent // Nền trong suốt
                        };

                        // Thêm PictureBox vừa tạo vào đúng cột (col) và hàng (row)
                        table.Controls.Add(picBox, col, row);

                        // Cập nhật trạng thái màu xám cho PictureBox này
                        UpdateCellStatus(picBox, "gray");
                    }
                }
            }
        }
        // 1. Vẽ viền ôm trọn 4 cạnh (Dùng cho Panel, TableLayoutPanel...)
        private void pnlOutlinePaint(Control ctrl, PaintEventArgs e)
        {
            int border_thickness = 4;
            using (Pen pen = new Pen(Outline, border_thickness))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, ctrl.Width - 1, ctrl.Height - 1);
            }
        }


        // 1. THƯỚC ĐO PHÍA TRÊN (1-10): Chỉ vẽ DUY NHẤT 1 đường ngang ở mép dưới
        private void pnlOutlinePaintUP(TableLayoutPanel tbl, TableLayoutCellPaintEventArgs e)
        {
            using (Pen pen = new Pen(Outline, 1))
            {
                // Chỉ kẻ 1 vạch ngang nằm ở đáy của ô (áp sát vào bảng nước)
                e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Top - 1, e.CellBounds.Right, e.CellBounds.Top - 1);
            }
        }

        // 2. THƯỚC ĐO BÊN TRÁI (A-J): Chỉ vẽ DUY NHẤT 1 đường dọc ở mép phải
        private void pnlOutlinePaintleft(TableLayoutPanel tbl, TableLayoutCellPaintEventArgs e)
        {
            using (Pen pen = new Pen(Outline, 1))
            {
                // Chỉ kẻ 1 vạch dọc nằm ở bên phải của ô (áp sát vào bảng nước)
                e.Graphics.DrawLine(pen, e.CellBounds.Left - 1, e.CellBounds.Top, e.CellBounds.Left - 1, e.CellBounds.Bottom);
            }
        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            pnlOutlinePaint(panel2, e);

        }

        private void panel9_Paint(object sender, PaintEventArgs e)
        {
            pnlOutlinePaint(panel9, e);
        }

        // --- CÁC SỰ KIỆN TRỐNG BÊN DƯỚI GIỮ NGUYÊN ---
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void panel8_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionCarrier_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionSubmarine_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionBattleship_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionCruiser_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionDestroyer_Paint(object sender, PaintEventArgs e) { }
        private void TxtTimer_TextChanged(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void TableShots_Paint(object sender, PaintEventArgs e) { }
        private void lblShots_Click(object sender, EventArgs e) { }
        private void tableEnemyConditionCarrier_Paint(object sender, PaintEventArgs e) { }
        private void tableEnemyConditionSubmarine_Paint(object sender, PaintEventArgs e) { }
        private void tableEnemyConditionBattleship_Paint(object sender, PaintEventArgs e) { }
        private void tableEnemyConditionCrusier_Paint(object sender, PaintEventArgs e) { }
        private void tableEnemyConditionDestroyer_Paint(object sender, PaintEventArgs e) { }
        private void pnlPlayerBoard_Paint(object sender, PaintEventArgs e) { }
        private void pnlPlayerName_Paint(object sender, PaintEventArgs e)
        {
            pnlOutlinePaint(pnlPlayerName, e);
        }

        private void pnlEnemyName_Paint(object sender, PaintEventArgs e)
        {
            pnlOutlinePaint(pnlEnemyName, e);
        }

        private void pnlEnemyName_Paint_1(object sender, PaintEventArgs e)
        {
            pnlOutlinePaint(pnlEnemyName, e);
        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        // THAY THẾ BẰNG 3 HÀM NÀY:
        private void tableLayoutPanel4_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            pnlOutlinePaintUP(tableLayoutPanel4, e);
        }

        private void tableLayoutPanel5_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            pnlOutlinePaintleft(tableLayoutPanel5, e);
        }

        private void tableLayoutPanel6_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            pnlOutlinePaintUP(tableLayoutPanel6, e);
        }
        private void tableLayoutPanel3_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            pnlOutlinePaintleft(tableLayoutPanel3, e);
        }

        private void label21_Click(object sender, EventArgs e)
        {

        }



        private void pnlPlayerFrameBoard_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblPlayerName_Click(object sender, EventArgs e)
        {

        }
    }
}