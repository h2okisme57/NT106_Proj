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
    public partial class Lobby : UserControl
    {
        // Biến cờ để đánh dấu trạng thái đăng nhập
        private bool isLoggedIn = false;
        public Lobby()
        {
            InitializeComponent();
            this.Load += Lobby_Load;
        }

        private void Lobby_Load(object sender, EventArgs e)
        {
          

            // Khai báo màu chữ chung từ mã Hex
            Color textColorBtn = ColorTranslator.FromHtml("#F4D35E");
            Color textColorLogo = ColorTranslator.FromHtml("#8B1E2D");

            if (lblBattleshiptxt != null)
            {
                lblBattleshiptxt.UseCompatibleTextRendering = true;
                lblBattleshiptxt.Font = FontManager.GetFont(24f, FontStyle.Bold);
                lblBattleshiptxt.BackColor = Color.Transparent;

                // Sửa màu chữ Label 
                lblBattleshiptxt.ForeColor = textColorLogo;
            }

            // Quét toàn bộ các công cụ nằm trên Lobby
            foreach (Control ctrl in this.Controls)
            {
                // Nếu công cụ đó là Nút bấm (Button)
                if (ctrl is Button btn)
                {
                    btn.UseCompatibleTextRendering = true;

                    btn.Font = FontManager.GetFont(14f, FontStyle.Bold);

                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = Color.Transparent;

                    btn.ForeColor = textColorBtn;
                    btn.TextAlign = ContentAlignment.MiddleLeft;
                    btn.Padding = new Padding(30, 0, 0, 0);

                    btn.FlatAppearance.BorderSize = 0;


                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 22, 46, 147);


                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 26, 25, 83);
                }
            }
        }


        // === HÀM KIỂM TRA ĐĂNG NHẬP ===
        private bool CheckLogin()
        {
            if (isLoggedIn)
                return true; // Nếu đã đăng nhập rồi thì cho qua luôn

            // Nếu chưa đăng nhập, dựng Popup chứa ucLogin lên giữa Lobby
            using (Form popupLogin = new Form())
            {
                ucLogin loginScreen = new ucLogin();
                popupLogin.Controls.Add(loginScreen);
                popupLogin.ClientSize = loginScreen.Size;
                popupLogin.FormBorderStyle = FormBorderStyle.None;
                popupLogin.StartPosition = FormStartPosition.CenterScreen;

                // Hiển thị khung Login chặn màn hình
                DialogResult result = popupLogin.ShowDialog();

                if (result == DialogResult.OK)
                {
                    isLoggedIn = true; 
                    return true;       
                }
                else
                {
                    return false;      
                }
            }
        }

        // === GẮN CHỐT CHẶN VÀO CÁC NÚT BẤM ===
        private void btnFindRoom_Click(object sender, EventArgs e)
        {
            
            if (!CheckLogin()) return;

            // Code tìm phòng của bạn sẽ viết ở đây (chỉ chạy khi đã login)
        }

        private void btnCreateRoom_Click(object sender, EventArgs e)
        {
            if (!CheckLogin()) return;

            // Code tạo phòng
        }

        private void btnJoinRoom_Click(object sender, EventArgs e)
        {
            if (!CheckLogin()) return;
            // Code tham gia phòng
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            if (!CheckLogin()) return;

            // Code mở hồ sơ
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lblBattleshiptxt_Click(object sender, EventArgs e)
        {
            //Code bị thừa
        }
    }
}
