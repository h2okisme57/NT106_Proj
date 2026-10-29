using Battleship.Shared;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace Battleship.Client
{
    public partial class ucLogin : UserControl
    {
        private const string DefaultUrl = "https://hqhggiixuqhhpuydparb.supabase.co";
        private const string DefaultKey = "sb_publishable__CEmQx8woYvxjvD0qUCeKg_4Gf8LMA3";
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        private readonly string _baseUrl;
        private readonly string _key;


        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int EM_SETMARGINS = 0xd3;
        private const int EC_LEFTMARGIN = 1;

        // Bổ sung biến theo dõi khóa đăng nhập
        private int failedLoginCount = 0;
        private bool isLockedOut = false;

        public ucLogin()
        {
            InitializeComponent();
            this.Load += ucLogin_Load;
            _baseUrl = (Environment.GetEnvironmentVariable("SUPABASE_URL") ?? DefaultUrl).TrimEnd('/');
            _key = Environment.GetEnvironmentVariable("SUPABASE_PUBLISHABLE_KEY") ?? DefaultKey;
        }

        private void ucLogin_Load(object sender, EventArgs e)
        {
            this.ActiveControl = null;

            SendMessage(boxUsernameTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);
            SendMessage(boxPasswordTxt.Handle, EM_SETMARGINS, (IntPtr)EC_LEFTMARGIN, (IntPtr)10);

            boxUsernameTxt.Text = "Tên đăng nhập";
            boxUsernameTxt.ForeColor = Color.Gray;

            boxPasswordTxt.PasswordChar = '\0';
            boxPasswordTxt.Text = "Mật khẩu";
            boxPasswordTxt.ForeColor = Color.Gray;

            boxUsernameTxt.Enter += boxUsernameTxt_Enter;
            boxUsernameTxt.Leave += boxUsernameTxt_Leave;

            boxPasswordTxt.Enter += boxPasswordTxt_Enter;
            boxPasswordTxt.Leave += boxPasswordTxt_Leave;

            this.Click += LoseFocus_Click;
            AttachClickToAllControls(this);
        }

        private void AttachClickToAllControls(Control container)
        {
            foreach (Control ctrl in container.Controls)
            {
                if (!(ctrl is TextBox))
                {
                    ctrl.Click += LoseFocus_Click;
                }

                if (ctrl.HasChildren)
                {
                    AttachClickToAllControls(ctrl);
                }
            }
        }

        private void LoseFocus_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.ActiveControl = null;
            }
        }

        private void boxUsernameTxt_Enter(object sender, EventArgs e)
        {
            if (boxUsernameTxt.Text == "Tên đăng nhập")
            {
                boxUsernameTxt.Text = "";
                boxUsernameTxt.ForeColor = Color.Black;
            }
        }

        private void boxUsernameTxt_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(boxUsernameTxt.Text))
            {
                boxUsernameTxt.Text = "Tên đăng nhập";
                boxUsernameTxt.ForeColor = Color.Gray;
            }
        }

        private void boxPasswordTxt_Enter(object sender, EventArgs e)
        {
            if (boxPasswordTxt.Text == "Mật khẩu")
            {
                boxPasswordTxt.Text = "";
                boxPasswordTxt.ForeColor = Color.Black;
                boxPasswordTxt.PasswordChar = '●';
            }
        }

        private void boxPasswordTxt_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(boxPasswordTxt.Text))
            {
                boxPasswordTxt.PasswordChar = '\0';
                boxPasswordTxt.Text = "Mật khẩu";
                boxPasswordTxt.ForeColor = Color.Gray;
            }
        }

        private void llblRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form popup = FindForm();
            if (popup != null)
            {
                popup.Controls.Clear();
                ucRegister registerScreen = new ucRegister();
                popup.Controls.Add(registerScreen);
                popup.ClientSize = registerScreen.Size;
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            // Chặn click nếu đang bị khóa
            if (isLockedOut) return;

            string user = boxUsernameTxt.Text.Trim();
            string pass = boxPasswordTxt.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) ||
                user == "Tên đăng nhập" || pass == "Mật khẩu")
            {
                lblErrorTxt.ForeColor = Color.Red;
                lblErrorTxt.Text = "Vui lòng nhập đầy đủ thông tin!";
                return;
            }

            lblErrorTxt.ForeColor = Color.Blue;
            lblErrorTxt.Text = "Đang kết nối...";
            btnLogin.Enabled = false;

            try
            {
                // 1. Tạo đường dẫn REST API truy vấn bảng Users (hoặc accounts) trên Supabase
                // Tìm user trùng khớp Username và Password
                string url = $"{_baseUrl}/rest/v1/users?username=eq.{Uri.EscapeDataString(user)}&password=eq.{Uri.EscapeDataString(pass)}&select=*";

                using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    req.Headers.Add("apikey", _key);
                    // Thêm Authorization header nếu cần thiết cho Supabase
                    req.Headers.Add("Authorization", $"Bearer {_key}");

                    using (var res = await _http.SendAsync(req))
                    {
                        string json = await res.Content.ReadAsStringAsync();

                        if (!res.IsSuccessStatusCode)
                        {
                            lblErrorTxt.ForeColor = Color.Red;
                            lblErrorTxt.Text = "Lỗi kết nối máy chủ!";
                            return;
                        }

                        // 2. Giải mã JSON trả về danh sách User DTO
                        var userList = JsonSerializer.Deserialize<List<LoginResponseDto>>(json, JsonConfig.Options);

                        // 3. Kiểm tra xem có tìm thấy tài khoản hợp lệ không
                        if (userList != null && userList.Count > 0)
                        {
                            var currentUser = userList[0];

                            failedLoginCount = 0; // Reset số lần đăng nhập sai
                           // PlayerProfile.CurrentUser = currentUser;
                            //PlayerProfile.Username = currentUser.Username; // Lưu lại Username

                            lblErrorTxt.ForeColor = Color.Green;
                            lblErrorTxt.Text = "Đăng nhập thành công! Đang vào sảnh...";

                            await Task.Delay(500);

                            Form popup = this.FindForm();
                            if (popup != null)
                            {
                                popup.DialogResult = DialogResult.OK;
                            }
                        }
                        else
                        {
                            failedLoginCount++; // Tăng biến đếm khi nhập sai

                            if (failedLoginCount >= 5)
                            {
                                await HandleLockoutAsync();
                            }
                            else
                            {
                                lblErrorTxt.ForeColor = Color.Red;
                                lblErrorTxt.Text = $"Tài khoản hoặc mật khẩu không chính xác! ({failedLoginCount}/5)";
                                boxPasswordTxt.Clear();
                                boxPasswordTxt.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblErrorTxt.ForeColor = Color.Red;
                lblErrorTxt.Text = $"Lỗi kết nối: {ex.Message}";
            }
            finally
            {
                // Mở lại nút bấm nếu không trong trạng thái bị khóa
                if (!isLockedOut)
                {
                    btnLogin.Enabled = true;
                }
            }
        }

        // === HÀM XỬ LÝ KHÓA ĐĂNG NHẬP 60 GIÂY ===
        private async Task HandleLockoutAsync()
        {
            isLockedOut = true;
            btnLogin.Enabled = false;
            boxUsernameTxt.Enabled = false;
            boxPasswordTxt.Enabled = false;
            lblErrorTxt.ForeColor = Color.Red;

            // Chạy đếm ngược 60 giây và cập nhật trực tiếp lên Label
            for (int i = 60; i > 0; i--)
            {
                lblErrorTxt.Text = $"Đăng nhập sai quá nhiều lần. Thử lại sau {i} giây.";
                await Task.Delay(1000);
            }

            // Mở khóa sau khi đếm xong
            failedLoginCount = 0;
            isLockedOut = false;
            btnLogin.Enabled = true;
            boxUsernameTxt.Enabled = true;
            boxPasswordTxt.Enabled = true;
            boxPasswordTxt.Clear();
            lblErrorTxt.Text = "";
        }
    }
}