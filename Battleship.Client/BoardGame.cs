using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Battleship.Client
{
    public class ShipInfo
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public bool IsPlaced { get; set; } = false;
        public List<Point> OccupiedCells { get; set; } = new List<Point>();

        public ShipInfo(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }

    public partial class BoardGame : UserControl
    {
        private Button[,] cells = new Button[10, 10];
        private Image waterTile;
        private bool isResizing = false;
        private Point currentHover = new Point(-1, -1);

        public bool IsPlacementPhase { get; set; } = true;
        public bool IsHorizontal { get; set; } = true;
        public ShipInfo SelectedShip { get; set; }
        public int[,] BoardGrid { get; private set; } = new int[10, 10];
        public List<ShipInfo> PlayerShips { get; private set; } = new List<ShipInfo>();

        public event Action<int, int> OnCellClicked;
        public event Action<ShipInfo> OnShipPlacedSuccessfully;

        public BoardGame()
        {
            InitializeComponent();
            waterTile = Properties.Resources.OceanBox;

            typeof(TableLayoutPanel).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                                  ?.SetValue(tableGame, true, null);

            InitializeDefaultShips();
            GenerateSingleBoard();

            tableGame.Paint += tableGame_Paint;
        }

        private void InitializeDefaultShips()
        {
            PlayerShips = new List<ShipInfo>
            {
                new ShipInfo("Carrier", 5),
                new ShipInfo("Battleship", 4),
                new ShipInfo("Cruiser", 3),
                new ShipInfo("Submarine", 3),
                new ShipInfo("Destroyer", 2)
            };
            SelectedShip = PlayerShips[0];
        }

        private void GenerateSingleBoard()
        {
            tableGame.BackgroundImage = waterTile;
            tableGame.BackgroundImageLayout = ImageLayout.Stretch;
            tableGame.Dock = DockStyle.None;
            tableGame.Margin = new Padding(0);
            tableGame.Padding = new Padding(0);
            tableGame.RowCount = 10;
            tableGame.ColumnCount = 10;
            tableGame.RowStyles.Clear();
            tableGame.ColumnStyles.Clear();

            for (int i = 0; i < 10; i++)
            {
                tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
                tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            }

            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    Button btn = new Button()
                    {
                        Dock = DockStyle.Fill,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.Transparent,
                        Tag = new Point(col, row),
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0),
                        TabStop = false
                    };

                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

                    // Thay Click bằng MouseDown để bắt được Chuột trái (đặt tàu) và Chuột phải (xoay tàu)
                    btn.MouseDown += Cell_MouseDown;
                    btn.MouseEnter += Cell_MouseEnter;
                    btn.MouseLeave += Cell_MouseLeave;

                    cells[col, row] = btn;
                    tableGame.Controls.Add(btn, col, row);
                }
            }
        }

        public void ToggleRotation()
        {
            IsHorizontal = !IsHorizontal;
            tableGame.Invalidate();
        }

        public bool CanPlaceShip(int startCol, int startRow, int size, bool isHorizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int c = startCol + (isHorizontal ? i : 0);
                int r = startRow + (isHorizontal ? 0 : i);

                if (c >= 10 || r >= 10) return false;
                if (BoardGrid[c, r] == 1) return false;
            }
            return true;
        }

        private void Cell_MouseDown(object sender, MouseEventArgs e)
        {
            // Nếu bấm Chuột phải -> Xoay tàu
            if (e.Button == MouseButtons.Right)
            {
                if (IsPlacementPhase) ToggleRotation();
                return;
            }

            // Nếu bấm Chuột trái -> Tiến hành đặt tàu
            if (e.Button == MouseButtons.Left)
            {
                Button btn = sender as Button;
                Point coords = (Point)btn.Tag;
                int col = coords.X;
                int row = coords.Y;

                if (IsPlacementPhase && SelectedShip != null && !SelectedShip.IsPlaced)
                {
                    if (CanPlaceShip(col, row, SelectedShip.Size, IsHorizontal))
                    {
                        SelectedShip.OccupiedCells.Clear();
                        for (int i = 0; i < SelectedShip.Size; i++)
                        {
                            int c = col + (IsHorizontal ? i : 0);
                            int r = row + (IsHorizontal ? 0 : i);

                            BoardGrid[c, r] = 1;
                            SelectedShip.OccupiedCells.Add(new Point(c, r));
                        }

                        SelectedShip.IsPlaced = true;
                        OnShipPlacedSuccessfully?.Invoke(SelectedShip);

                        SelectedShip = PlayerShips.FirstOrDefault(s => !s.IsPlaced);
                        if (SelectedShip == null)
                        {
                            IsPlacementPhase = false;
                            MessageBox.Show("Đã bố trí xong hạm đội 5 tàu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }

                OnCellClicked?.Invoke(col, row);
                tableGame.Invalidate();
            }
        }

        private void Cell_MouseEnter(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            currentHover = (Point)btn.Tag;
            tableGame.Invalidate();
        }

        private void Cell_MouseLeave(object sender, EventArgs e)
        {
            currentHover = new Point(-1, -1);
            tableGame.Invalidate();
        }

        private void tableGame_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int w = tableGame.Width;
            int h = tableGame.Height;
            float cellW = w / 10f;
            float cellH = h / 10f;

            // 1. VẼ ĐƯỜNG LƯỚI VIỀN XANH TRƯỚC (Để tàu có thể đè lên trên)
            Color Outline = Color.FromArgb(88, 160, 200);
            using (Pen pen = new Pen(Outline, 2))
            {
                for (int i = 0; i <= 10; i++)
                {
                    g.DrawLine(pen, i * cellW, 0, i * cellW, h);
                    g.DrawLine(pen, 0, i * cellH, w, i * cellH);
                }
            }

            // 2. VẼ TÀU DẠNG HÌNH CHỮ NHẬT (Sát mép ô, đè che luôn viền)
            using (SolidBrush shipBrush = new SolidBrush(Color.FromArgb(210, 80, 90, 100)))
            using (Pen shipBorderPen = new Pen(Color.FromArgb(255, 200, 220, 240), 1.5f))
            {
                foreach (var ship in PlayerShips)
                {
                    if (ship.IsPlaced && ship.OccupiedCells.Count > 0)
                    {
                        int minX = int.MaxValue, minY = int.MaxValue;
                        int maxX = int.MinValue, maxY = int.MinValue;

                        foreach (Point pt in ship.OccupiedCells)
                        {
                            if (pt.X < minX) minX = pt.X;
                            if (pt.Y < minY) minY = pt.Y;
                            if (pt.X > maxX) maxX = pt.X;
                            if (pt.Y > maxY) maxY = pt.Y;
                        }

                        // Vẽ sát lề 100%, không bị hở padding
                        float rectX = minX * cellW;
                        float rectY = minY * cellH;
                        float rectW = (maxX - minX + 1) * cellW;
                        float rectH = (maxY - minY + 1) * cellH;

                        g.FillRectangle(shipBrush, rectX, rectY, rectW, rectH);
                        g.DrawRectangle(shipBorderPen, rectX, rectY, rectW, rectH);
                    }
                }
            }

            // 3. VẼ BÓNG MỜ TRƯỚC KHI ĐẶT TÀU (Xanh/Đỏ)
            if (IsPlacementPhase && SelectedShip != null && !SelectedShip.IsPlaced && currentHover.X >= 0)
            {
                bool isValid = CanPlaceShip(currentHover.X, currentHover.Y, SelectedShip.Size, IsHorizontal);
                Color hoverColor = isValid ? Color.FromArgb(140, 46, 204, 113) : Color.FromArgb(140, 231, 76, 60);

                using (SolidBrush hoverBrush = new SolidBrush(hoverColor))
                {
                    for (int i = 0; i < SelectedShip.Size; i++)
                    {
                        int c = currentHover.X + (IsHorizontal ? i : 0);
                        int r = currentHover.Y + (IsHorizontal ? 0 : i);

                        if (c < 10 && r < 10)
                        {
                            g.FillRectangle(hoverBrush, c * cellW, r * cellH, cellW, cellH);
                        }
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if (isResizing) return;
            isResizing = true;

            int baseSize = Math.Min(this.Width, this.Height);
            int perfectSize = (baseSize / 10) * 10;

            tableGame.Size = new Size(perfectSize, perfectSize);
            tableGame.Location = new Point((this.Width - perfectSize) / 2, (this.Height - perfectSize) / 2);

            isResizing = false;
            base.OnResize(e);
        }

        public void SetCellImage(int col, int row, Image img)
        {
            if (col >= 0 && col < 10 && row >= 0 && row < 10)
                cells[col, row].BackgroundImage = img;
        }

        public void ClearCellImage(int col, int row)
        {
            if (col >= 0 && col < 10 && row >= 0 && row < 10)
                cells[col, row].BackgroundImage = null;
        }
    }
}