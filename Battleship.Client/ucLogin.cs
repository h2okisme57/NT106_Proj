using Battleship.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http; // Bắt buộc phải có để gọi REST API
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battleship.Client
{
    public partial class ucLogin : UserControl
    {
        // Địa chỉ URL và Key của Supabase
        private const string DefaultUrl = "https://hqhggiixuqhhpuydparb.supabase.co";
        private const string DefaultKey = "sb_publishable__CEmQx8woYvxjvD0qUCeKg_4Gf8LMA3";

        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        private readonly string _baseUrl;
        private readonly string _key;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int EM_SETMARGINS = 0xd3;
        private const int EC_LEFTMARGIN = 1;

        private int failedLoginCount = 0;
        private bool isLockedOut = false;

        public ucLogin()
        {
            InitializeComponent();
            this.Load += ucLogin_Load;

            string envUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
            string envKey = Environment.GetEnvironmentVariable("SUPABASE_PUBLISHABLE_KEY");

            _baseUrl = (!string.IsNullOrEmpty(envUrl) ? envUrl : DefaultUrl).TrimEnd('/');
            _key = !string.IsNullOrEmpty(envKey) ? envKey : DefaultKey;
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
                // URL khớp 100% tên bảng Users và tên cột Username, Password
                string url = $"{_baseUrl}/rest/v1/Users?Username=eq.{Uri.EscapeDataString(user)}&Password=eq.{Uri.EscapeDataString(pass)}&select=*";

                using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    req.Headers.TryAddWithoutValidation("apikey", _key);
                    req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_key}");

                    using (var res = await _http.SendAsync(req))
                    {
                        string json = await res.Content.ReadAsStringAsync();

                        if (!res.IsSuccessStatusCode)
                        {
                            lblErrorTxt.ForeColor = Color.Red;
                            // Hiển thị thêm chi tiết lỗi từ Supabase nếu có
                            lblErrorTxt.Text = $"Lỗi HTTP {(int)res.StatusCode}: Bảng hoặc tên cột không khớp!";
                            return;
                        }

                        var userList = JsonSerializer.Deserialize<List<LoginResponseDto>>(json, JsonConfig.Options);

                        if (userList != null && userList.Count > 0)
                        {
                            var currentUser = userList[0];
                            failedLoginCount = 0;

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
                            failedLoginCount++;

                            if (failedLoginCount >= 5)
                            {
                                await HandleLockoutAsync();
                            }
                            else
                            {
                                lblErrorTxt.ForeColor = Color.Red;
                                lblErrorTxt.Text = $"Sai tài khoản hoặc mật khẩu! ({failedLoginCount}/5)";
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
                if (!isLockedOut)
                {
                    btnLogin.Enabled = true;
                }
            }
        }

        private async Task HandleLockoutAsync()
        {
            isLockedOut = true;
            btnLogin.Enabled = false;
            boxUsernameTxt.Enabled = false;
            boxPasswordTxt.Enabled = false;
            lblErrorTxt.ForeColor = Color.Red;

            for (int i = 60; i > 0; i--)
            {
                lblErrorTxt.Text = $"Thử lại sau {i} giây.";
                await Task.Delay(1000);
            }

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