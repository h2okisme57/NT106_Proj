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
    public partial class BoardGame : UserControl
    {
        private Button[,] cells = new Button[10, 10];

        // Tạo sự kiện (Event) để báo ra ngoài mỗi khi có ô bị bấm
        public event Action<int, int> OnCellClicked;

        public BoardGame()
        {
            InitializeComponent();
            GenerateSingleBoard();
        }

        // Hàm đổ 100 nút vào bàn cờ duy nhất
        private void GenerateSingleBoard()
        {
            tableGame.BackColor = Color.FromArgb(20, 170, 215);

            // Ép tỷ lệ tuyệt đối để các ô chia đều 10x10, không bị ô to ô nhỏ
            tableGame.RowCount = 10;
            tableGame.ColumnCount = 10;
            tableGame.RowStyles.Clear();
            tableGame.ColumnStyles.Clear();
            for (int i = 0; i < 10; i++)
            {
                tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
                tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            }

            Image waterTile = Properties.Resources.OceanBox;

            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    Button btn = new Button()
                    {
                        Dock = DockStyle.Fill,


                        Margin = new Padding(1),

                        FlatStyle = FlatStyle.Flat,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        BackgroundImage = waterTile,
                        Tag = new Point(col, row),
                        TabStop = false
                    };

                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.BorderColor = Color.LightSteelBlue; // Đồng bộ màu viền
                    btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

                    btn.Click += Cell_Click;

                    // Kích hoạt cọ vẽ để làm nhòe mép ảnh
                    btn.Paint += Btn_Paint;

                    cells[col, row] = btn;
                    tableGame.Controls.Add(btn, col, row);
                }
            }
        }

        private void Btn_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;


            // Vẽ 2 lớp sương mù bán trong suốt dọc theo viền nút để hòa trộn ảnh với rãnh 1px
            int shadowThickness = 2;

            for (int i = 0; i < shadowThickness; i++)
            {

                int alpha = 80 - (i * 40);

                // Dùng màu Trắng mờ (hoặc thay bằng màu Xanh nhạt) để tạo cảm giác phát sáng/nhòe
                using (Pen pen = new Pen(Color.FromArgb(alpha, 0,0,0), 1))
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

            // Bắn tín hiệu ra ngoài, truyền theo tọa độ (col, row) vừa bị bấm
            OnCellClicked?.Invoke(coords.X, coords.Y);
        }

        //Gọi hàm  gán ảnh cho một ô cụ thể
        public void SetCellImage(int col, int row, Image img)
        {
            // Kiểm tra tọa độ hợp lệ để tránh lỗi văng game
            if (col >= 0 && col < 10 && row >= 0 && row < 10)
            {
                cells[col, row].BackgroundImage = img;
            }
        }

        //  Hàm  xóa ảnh của một ô (trả về trạng thái trống)
        public void ClearCellImage(int col, int row)
        {
            if (col >= 0 && col < 10 && row >= 0 && row < 10)
            {
                cells[col, row].BackgroundImage = null;
            }
        }

        private void tableGame_Paint(object sender, PaintEventArgs e)
        {
            //Code thừa
        }
    }
}