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
        SwitchView(new ucLogin());
    }

    private void pnlContainer_Paint(object sender, PaintEventArgs e)
    {

    }
}
