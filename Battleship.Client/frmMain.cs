using System.Threading.Tasks.Dataflow;

namespace Battleship.Client;

public partial class frmMain : Form
{
    public frmMain()
    {
        InitializeComponent();
        Load += frmMain_Load;
    }
    // Chuyen đổi màn hình hiển thị trong pnlContainer sang UserControl khác
    public void SwitchView(UserControl uc)
    {
        pnlContainer.Controls.Clear();
        uc.Dock = DockStyle.Fill;
        pnlContainer.Controls.Add(uc);

        uc.BringToFront();
    }

    // Khi Form Main vừa mở lên, tự động hiển thị màn hình Login
    private void frmMain_Load(object sender, EventArgs e)
    {
        SwitchView(new Lobby());
        //TestBoardGame();
    }

    public void TestBoardGame()
    {
        // 1. Tạo một Form giả lập để chứa bàn cờ
        Form testForm = new Form
        {
            Text = "Kiểm thử Bàn Cờ Battleship",
            Size = new Size(600, 600), // Bạn có thể thử kéo giãn Form này lúc chạy để test hàm OnResize
            StartPosition = FormStartPosition.CenterScreen
        };

        // 2. Khởi tạo bàn cờ
        BoardGame testBoard = new BoardGame
        {
            Dock = DockStyle.Fill // Cho phép bàn cờ tự động lấp đầy Form
        };

        // 3. Kiểm thử sự kiện Click (Bắn tín hiệu)
        testBoard.OnCellClicked += (col, row) =>
        {
            // Mỗi khi bấm vào một ô, hiển thị tọa độ
            MessageBox.Show($"Bạn vừa bấm vào ô: Cột {col} - Hàng {row}", "Test Click", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Test thử hàm SetCellImage (Gán một màu/hình ảnh để đánh dấu đã bắn)
            // Lưu ý: Mở comment dòng dưới nếu bạn đã có ảnh Properties.Resources.Hit
            // testBoard.SetCellImage(col, row, Properties.Resources.Hit);
        };

        // 4. Gắn bàn cờ vào Form và hiển thị
        testForm.Controls.Add(testBoard);
        testForm.ShowDialog();
    }

    private void pnlContainer_Paint(object sender, PaintEventArgs e)
    {

    }
}
