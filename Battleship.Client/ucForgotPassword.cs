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
            this.DoubleBuffered = true;
            this.AutoScaleMode = AutoScaleMode.None;
            this.BackColor = Theme.BgBottom;
            this.Size = new Size(S(545), S(505));

            var title = new Label
            {
                Text = "QUÊN MẬT KHẨU",
                Font = FontManager.GetFont(15F, FontStyle.Bold),
                ForeColor = Theme.Gold,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(S(340), S(38)),
                Location = new Point((this.Width - S(340)) / 2, S(18))
            };
            this.Controls.Add(title);

            AddField("Tên đăng nhập", txtUser, 76, false);

            btnSend.Text = "GỬI MÃ XÁC NHẬN";
            StyleButton(btnSend, Theme.Cyan, Theme.CyanDark, 172, 140, 200, 40);
            btnSend.Click += btnSend_Click;
            this.Controls.Add(btnSend);

            AddField("Mã xác nhận (6 số, gửi về email)", txtCode, 190, false);
            AddField("Mật khẩu mới", txtNewPass, 254, true);
            AddField("Nhập lại mật khẩu mới", txtConfirm, 318, true);

            btnReset.Text = "ĐẶT LẠI MẬT KHẨU";
            StyleButton(btnReset, Theme.Red, Theme.RedDark, 172, 386, 200, 44);
            btnReset.Click += btnReset_Click;
            this.Controls.Add(btnReset);

            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.BackColor = Color.Transparent;
            lblMessage.Font = FontManager.GetFont(8.5f, FontStyle.Regular);
            lblMessage.Location = new Point(S(24), S(438));
            lblMessage.Size = new Size(this.Width - S(48), S(30));
            this.Controls.Add(lblMessage);

            llblBack.Text = "Quay lại đăng nhập";
            llblBack.AutoSize = true;
            llblBack.BackColor = Color.Transparent;
            llblBack.Font = FontManager.GetFont(8.5f, FontStyle.Regular);
            llblBack.LinkBehavior = LinkBehavior.NeverUnderline;
            llblBack.LinkColor = Theme.Gold;
            llblBack.ActiveLinkColor = Color.White;
            llblBack.VisitedLinkColor = Theme.Gold;
            llblBack.Location = new Point(S(198), S(472));
            llblBack.LinkClicked += (s, e) => SwitchToLogin();
            this.Controls.Add(llblBack);

            SetStep2Enabled(false);
            Theme.ApplyRetroFont(this);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Theme.DrawCrtBackground(e.Graphics, this.ClientRectangle);
            Theme.DrawCard(e.Graphics, new Rectangle(S(46), S(62), this.Width - S(92), S(374)));
            Theme.DrawInputSlot(e.Graphics, txtUser, S(6));
            Theme.DrawInputSlot(e.Graphics, txtCode, S(6));
            Theme.DrawInputSlot(e.Graphics, txtNewPass, S(6));
            Theme.DrawInputSlot(e.Graphics, txtConfirm, S(6));
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
                BackColor = Color.Transparent,
                Font = FontManager.GetFont(8.5f, FontStyle.Regular),
                ForeColor = Theme.Text,
                Location = new Point(S(98), S(top))
            };
            Theme.StyleTextBox(box, 9.5f);
            box.ForeColor = Theme.Text;
            box.Location = new Point(S(108), S(top + 26));
            box.Width = S(329);
            box.Enter += (s, e) => this.Invalidate();
            box.Leave += (s, e) => this.Invalidate();
            if (isPassword) box.PasswordChar = '●';
            this.Controls.Add(lbl);
            this.Controls.Add(box);
        }

        private void StyleButton(Button b, Color face, Color lip, int x, int y, int w, int h)
        {
            b.Location = new Point(S(x), S(y));
            b.Size = new Size(S(w), S(h));
            Theme.StyleButton(b, face, lip, 9.5f);
        }

        private void SetStep2Enabled(bool enabled)
        {
            txtCode.Enabled = enabled;
            txtNewPass.Enabled = enabled;
            txtConfirm.Enabled = enabled;
            btnReset.Enabled = enabled;
            btnReset.BackColor = enabled ? Theme.Red : Theme.Slate;
        }

        private void ShowMessage(string text, bool ok)
        {
            lblMessage.ForeColor = ok ? Theme.Green : Theme.Red;
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
            lblMessage.ForeColor = Theme.Cyan;
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
            lblMessage.ForeColor = Theme.Cyan;
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
