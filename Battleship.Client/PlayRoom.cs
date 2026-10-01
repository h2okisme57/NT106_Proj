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
    public partial class PlayRoom : UserControl
    {
        private Button[,] boardCells = new Button[10, 10];
        // Biến kiểm soát lượt chơi (True = Lượt của mình, False = Lượt đối thủ)
        private bool isMyTurn = true;


        // Các UI Controls để cập nhật thông tin sau này
        private Label lblTurnIndicator;
        private Label lblMyTimer, lblEnemyTimer;
        private ListBox lstMyHits, lstEnemyHits; // Khung hiển thị phần tàu đã bắn trúng
        public PlayRoom()
        {
            InitializeComponent();
            GenerateSingleBoard();
        }
        // Hàm đổ 100 nút vào bàn cờ duy nhất
        private void GenerateSingleBoard()
        {
            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    Button btn = new Button()
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(1), // Để hở 1 pixel làm viền cho các ô dễ nhìn
                        BackColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Name = $"btnCell_{col}_{row}",
                        Tag = $"{col},{row}"
                    };

                    btn.FlatAppearance.BorderSize = 0; // Xóa viền đen mặc định của WinForms
                    btn.Cursor = Cursors.Cross;
                    btn.Click += Cell_Click;

                    boardCells[col, row] = btn;
                    tableGame.Controls.Add(btn, col, row);
                }
            }


        }
        private void Cell_Click(object sender, EventArgs e)
        {
            // Code Logic sẽ được viết ở đây (kiểm tra isMyTurn để biết ai đang bấm và đổi màu tương ứng)
        }

        private void tableGame_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
