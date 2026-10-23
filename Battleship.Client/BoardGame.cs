using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Drawing;
using System.Reflection; // Dùng để bật DoubleBuffer
using System.Windows.Forms;

namespace Battleship.Client
{
    public partial class BoardGame : UserControl
    {
        private Button[,] cells = new Button[10, 10];

        // Cache ảnh nền nước để tái sử dụng, tránh tốn RAM khi Load/Clear nhiều lần
        private Image waterTile;

        // Biến cờ chống vòng lặp đệ quy khi tự động thay đổi kích thước
        private bool isResizing = false;

        // Tạo sự kiện (Event) để báo ra ngoài mỗi khi có ô bị bấm
        public event Action<int, int> OnCellClicked;

        public BoardGame()
        {
            InitializeComponent();

            waterTile = Properties.Resources.OceanBox;

            // Kỹ thuật bật DoubleBuffered ẩn của TableLayoutPanel để triệt tiêu hiện tượng 
            // nhấp nháy (flicker) bảng khi thu phóng kích thước UserControl liên tục.
            typeof(Panel).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                         ?.SetValue(tableGame, true, null);

            GenerateSingleBoard();
        }

        private void GenerateSingleBoard()
        {
            tableGame.BackColor = Color.FromArgb(20, 170, 215);

            // Ép tỷ lệ tuyệt đối để các ô chia đều 10x10
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
                        BackgroundImageLayout = ImageLayout.Stretch, // Phù hợp hiển thị tàu/vụ nổ
                        BackgroundImage = waterTile,
                        Tag = new Point(col, row),
                        TabStop = false,
                        Cursor = Cursors.Hand, // Tối ưu Battleship: Bật cursor hình bàn tay để báo hiệu bấm được
                        BackColor = Color.Transparent
                    };

                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

                    btn.Click += Cell_Click;
                    btn.Paint += Btn_Paint;

                    cells[col, row] = btn;
                    tableGame.Controls.Add(btn, col, row);
                }
            }
        }

        // =================================================================================
        // GHI ĐÈ RE-SIZE: ÉP KÍCH THƯỚC CHUẨN & TÍNH TOÁN VIỀN
        // =================================================================================
        protected override void OnResize(EventArgs e)
        {
            if (isResizing) return;
            isResizing = true;

            // 1. Tìm cạnh ngắn nhất để ép bàn cờ thành hình vuông (tránh biến dạng hcn)
            int baseSize = Math.Min(this.Width, this.Height);

            // 2. Kích thước lý tưởng của 1 ô
            int cellSize = baseSize / 10;

            // 3. Ép kích thước bàn cờ chuẩn chỉnh chia hết cho 10 (vd: 504 -> 500)
            int perfectSize = cellSize * 10;
            this.Size = new Size(perfectSize, perfectSize);

            // 4. Viền (Margin) bằng 1/10 ô. Phép chia số nguyên (int) trong C# sẽ TỰ ĐỘNG LÀM TRÒN XUỐNG.
            int marginSize = cellSize / 10;
            Padding dynamicPadding = new Padding(marginSize);

            // Áp dụng margin mới cho 100 ô
            if (cells[0, 0] != null)
            {
                foreach (Button btn in cells)
                {
                    if (btn != null && btn.Margin != dynamicPadding)
                    {
                        btn.Margin = dynamicPadding;
                    }
                }
            }

            isResizing = false;
            base.OnResize(e);
        }

        private void Btn_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Chỉnh lại sương mù: Tính độ dày của sương mù linh hoạt theo kích thước nút
            // Giúp khi phóng to không bị mờ nhạt, và thu nhỏ không bị lấn hết ô
            int shadowThickness = Math.Max(1, btn.Width / 15);

            for (int i = 0; i < shadowThickness; i++)
            {
                // Nội suy độ trong suốt (Alpha)
                int alpha = Math.Max(0, 80 - (i * (80 / shadowThickness)));

                using (Pen pen = new Pen(Color.FromArgb(alpha, 0, 0, 0), 1))
                {
                    Rectangle rect = new Rectangle(
                        i,
                        i,
                        btn.Width - 1 - (i * 2),
                        btn.Height - 1 - (i * 2)
                    );
                    g.DrawRectangle(pen, rect);
                }
            }
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Point coords = (Point)btn.Tag;
            OnCellClicked?.Invoke(coords.X, coords.Y);
        }

        public void SetCellImage(int col, int row, Image img)
        {
            if (col >= 0 && col < 10 && row >= 0 && row < 10)
            {
                cells[col, row].BackgroundImage = img;
            }
        }

        public void ClearCellImage(int col, int row)
        {
            if (col >= 0 && col < 10 && row >= 0 && row < 10)
            {
                // Tối ưu Battleship: Xóa ảnh không phải là gán null (sẽ ra màu xanh nền) 
                // mà là đưa nó về lại hình dạng mặt nước mặc định ban đầu.
                cells[col, row].BackgroundImage = waterTile;
            }
        }

        private void tableGame_Paint(object sender, PaintEventArgs e)
        {
            // Để trống nếu không dùng tới
        }

        private void BoardGame_Load(object sender, EventArgs e)
        {

        }
    }
}