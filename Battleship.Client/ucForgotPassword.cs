using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Battleship.Shared;

namespace Battleship.Client
{
    // Màn hình quên mật khẩu: xin mã qua email, rồi nhập mã + mật khẩu mới
    public class ucForgotPassword : UserControl
    {
        private readonly TextBox txtUser = new TextBox();
        private readonly TextBox txtCode = new TextBox();
        private readonly TextBox txtNewPass = new TextBox();
        private readonly TextBox txtConfirm = new TextBox();
        private readonly Button btnSend = new Button();
        private readonly Button btnReset = new Button();
        private readonly Label lblMessage = new Label();
        private readonly LinkLabel llblBack = new LinkLabel();

        // hệ số phóng theo độ phân giải màn hình
        private readonly float k;

        public ucForgotPassword()
        {
            k = this.DeviceDpi / 96f;
            this.AutoScaleMode = AutoScaleMode.None;
            this.BackColor = Color.White;
            this.Size = new Size(S(545), S(505));

            var title = new Label
            {
                Text = "QUÊN MẬT KHẨU",
                Font = new Font("Segoe UI", 15F),
                AutoSize = true,
                Location = new Point(S(150), S(20))
            };
            this.Controls.Add(title);

            AddField("Tên đăng nhập", txtUser, 70, false);

            btnSend.Text = "GỬI MÃ XÁC NHẬN";
            StyleButton(btnSend, Color.FromArgb(0, 139, 227), 172, 132, 200, 40);
            btnSend.Click += btnSend_Click;
            this.Controls.Add(btnSend);

            AddField("Mã xác nhận (6 số, gửi về email)", txtCode, 185, false);
            AddField("Mật khẩu mới", txtNewPass, 250, true);
            AddField("Nhập lại mật khẩu mới", txtConfirm, 315, true);

            btnReset.Text = "ĐẶT LẠI MẬT KHẨU";
            StyleButton(btnReset, Color.FromArgb(236, 45, 51), 172, 382, 200, 44);
            btnReset.Click += btnReset_Click;
            this.Controls.Add(btnReset);

            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.Location = new Point(S(10), S(434));
            lblMessage.Size = new Size(S(525), S(34));
            this.Controls.Add(lblMessage);

            llblBack.Text = "Quay lại đăng nhập";
            llblBack.AutoSize = true;
            llblBack.LinkBehavior = LinkBehavior.NeverUnderline;
            llblBack.LinkColor = Color.FromArgb(69, 149, 102);
            llblBack.Location = new Point(S(205), S(470));
            llblBack.LinkClicked += (s, e) => SwitchToLogin();
            this.Controls.Add(llblBack);

            SetStep2Enabled(false);
        }

        private int S(int v)
        {
            return (int)Math.Round(v * k);
        }

        private void AddField(string caption, TextBox box, int top, bool isPassword)
        {
            var lbl = new Label
            {
                Text = caption,
                AutoSize = true,
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(S(107), S(top))
            };
            box.Location = new Point(S(107), S(top + 24));
            box.Width = S(335);
            box.BorderStyle = BorderStyle.FixedSingle;
            if (isPassword) box.PasswordChar = '●';
            this.Controls.Add(lbl);
            this.Controls.Add(box);
        }

        private void StyleButton(Button b, Color color, int x, int y, int w, int h)
        {
            b.Location = new Point(S(x), S(y));
            b.Size = new Size(S(w), S(h));
            b.BackColor = color;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Font = new Font(b.Font, FontStyle.Bold);
            b.UseVisualStyleBackColor = false;
        }

        private void SetStep2Enabled(bool enabled)
        {
            txtCode.Enabled = enabled;
            txtNewPass.Enabled = enabled;
            txtConfirm.Enabled = enabled;
            btnReset.Enabled = enabled;
        }

        private void ShowMessage(string text, bool ok)
        {
            lblMessage.ForeColor = ok ? Color.Green : Color.Red;
            lblMessage.Text = text;
        }

        private async Task<bool> EnsureConnectedAsync()
        {
            if (NetworkManager.Instance.IsConnected) return true;
            return await NetworkManager.Instance.ConnectAsync("127.0.0.1", 8000);
        }

        // Bước 1: xin mã
        private async void btnSend_Click(object sender, EventArgs e)
        {
            string user = txtUser.Text.Trim();
            if (string.IsNullOrWhiteSpace(user))
            {
                ShowMessage("Vui lòng nhập tên đăng nhập!", false);
                return;
            }

            btnSend.Enabled = false;
            lblMessage.ForeColor = Color.Blue;
            lblMessage.Text = "Đang gửi mã...";

            try
            {
                if (!await EnsureConnectedAsync())
                {
                    ShowMessage("Không thể kết nối đến máy chủ!", false);
                    return;
                }

                var req = new ForgotPasswordRequestDto { Username = user };
                await NetworkManager.Instance.SendAsync(Packet.Create(PacketType.ForgotPasswordRequest, req));
                var resp = await NetworkManager.Instance.WaitForPacketAsync(PacketType.ForgotPasswordResponse);

                if (resp == null)
                {
                    ShowMessage("Không nhận được phản hồi từ máy chủ!", false);
                    return;
                }

                var res = resp.DeserializePayload<ForgotPasswordResponseDto>();
                if (res != null && res.Success)
                {
                    ShowMessage(res.Message, true);
                    SetStep2Enabled(true);
                    txtCode.Focus();
                }
                else
                {
                    ShowMessage(res?.Message ?? "Không gửi được mã!", false);
                }
            }
            catch (Exception)
            {
                ShowMessage("Lỗi kết nối máy chủ!", false);
            }
            finally
            {
                btnSend.Enabled = true;
            }
        }

        // Bước 2: đặt mật khẩu mới
        private async void btnReset_Click(object sender, EventArgs e)
        {
            string user = txtUser.Text.Trim();
            string code = txtCode.Text.Trim();
            string newPass = txtNewPass.Text;

            if (code.Length != 6 || !code.All(char.IsDigit))
            {
                ShowMessage("Mã xác nhận gồm 6 chữ số.", false);
                return;
            }
            if (string.IsNullOrEmpty(newPass))
            {
                ShowMessage("Vui lòng nhập mật khẩu mới!", false);
                return;
            }
            if (newPass != txtConfirm.Text)
            {
                ShowMessage("Mật khẩu nhập lại không khớp!", false);
                return;
            }

            btnReset.Enabled = false;
            lblMessage.ForeColor = Color.Blue;
            lblMessage.Text = "Đang xử lý...";

            try
            {
                if (!await EnsureConnectedAsync())
                {
                    ShowMessage("Không thể kết nối đến máy chủ!", false);
                    return;
                }

                var req = new ResetPasswordRequestDto { Username = user, Code = code, NewPassword = newPass };
                await NetworkManager.Instance.SendAsync(Packet.Create(PacketType.ResetPasswordRequest, req));
                var resp = await NetworkManager.Instance.WaitForPacketAsync(PacketType.ResetPasswordResponse);

                if (resp == null)
                {
                    ShowMessage("Không nhận được phản hồi từ máy chủ!", false);
                    return;
                }

                var res = resp.DeserializePayload<ResetPasswordResponseDto>();
                if (res != null && res.Success)
                {
                    ShowMessage(res.Message, true);
                    await Task.Delay(1500);
                    SwitchToLogin();
                    return;
                }

                ShowMessage(res?.Message ?? "Đặt lại mật khẩu không thành công!", false);
            }
            catch (Exception)
            {
                ShowMessage("Lỗi kết nối máy chủ!", false);
            }
            finally
            {
                if (!this.IsDisposed) btnReset.Enabled = true;
            }
        }

        private void SwitchToLogin()
        {
            Form popup = this.FindForm();
            if (popup != null)
            {
                popup.Controls.Clear();
                ucLogin loginScreen = new ucLogin();
                popup.Controls.Add(loginScreen);
                popup.ClientSize = loginScreen.Size;
            }
        }
    }
}
