using Battleship.Shared;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battleship.Client
{
    public partial class Profile : UserControl
    {
        // Tông xanh biển sâu đậm hơn, na ná nhau để làm nền tôn lên WINS/LOSSES
        private static readonly Color NeutralCardFill = Color.FromArgb(12, 36, 66);
        private static readonly Color NeutralCardLip = Color.FromArgb(6, 20, 40);

        private static readonly Color AvatarFill = Color.FromArgb(18, 52, 90);
        private static readonly Color AvatarLip = Color.FromArgb(9, 30, 56);

        private static readonly Color RateCardFill = Color.FromArgb(10, 31, 58);
        private static readonly Color RateCardLip = Color.FromArgb(5, 17, 35);

        // Giữ nguyên độ tương phản mạnh riêng cho WINS & LOSSES
        private static readonly Color WinFill = Color.FromArgb(8, 202, 100);       // Xanh lục bảo rực
        private static readonly Color WinLip = Color.FromArgb(4, 132, 62);

        private static readonly Color LoseFill = Color.FromArgb(255, 46, 64);      // Đỏ tươi rực
        private static readonly Color LoseLip = Color.FromArgb(178, 18, 32);

        private static readonly Color BarTrack = Color.FromArgb(4, 15, 32);
        private static readonly Color BarWin = Color.FromArgb(24, 255, 138);       // Xanh neon cho thanh Win Rate
        private static readonly Color BarLose = Color.FromArgb(255, 68, 86);       // Đỏ neon cho phần thua

        private Rectangle _profileCard;
        private Rectangle _avatarRect;
        private Rectangle _winCard;
        private Rectangle _loseCard;
        private Rectangle _rateCard;
        private Rectangle _rateBarRect;

        private Font _headerFont;
        private Font _avatarFont;

        private float _winRate = 0f;
        private string _avatarLetter = "?";

        public Profile()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            ApplyBalatroTheme();
            LayoutControls();
            Theme.ApplyRetroFont(this);
            _ = LoadProfileAndFriendsAsync();
        }

        private void ApplyBalatroTheme()
        {
            this.BackColor = Theme.BgBottom;

            _headerFont = FontManager.GetFont(10f, FontStyle.Bold);
            _avatarFont = FontManager.GetFont(15f, FontStyle.Bold);

            foreach (Control ctrl in this.Controls.Find("lblProfiletxt", true))
            {
                ctrl.Visible = false;
            }

            StyleOverlayLabel(lblUsername, 10f, FontStyle.Bold, Theme.Text, ContentAlignment.MiddleLeft);
            StyleOverlayLabel(lblID, 8.2f, FontStyle.Regular, Theme.Muted, ContentAlignment.MiddleLeft);
            StyleOverlayLabel(lblWin, 9.8f, FontStyle.Bold, Color.White, ContentAlignment.MiddleCenter);
            StyleOverlayLabel(lblLose, 9.8f, FontStyle.Bold, Color.White, ContentAlignment.MiddleCenter);
            StyleOverlayLabel(label1, 8f, FontStyle.Bold, Theme.Muted, ContentAlignment.MiddleCenter);

            StyleSolidButton(btnBack, "BACK", Theme.Slate, Theme.SlateDark, 9.2f);
        }

        private static void StyleSolidButton(Button btn, string text, Color faceColor, Color lipColor, float fontSize)
        {
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(faceColor, 0.12f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(faceColor, 0.08f);
            btn.BackColor = faceColor;
            btn.ForeColor = Theme.Text;
            btn.Font = FontManager.GetFont(fontSize, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;

            btn.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.None;
                int w = btn.Width;
                int h = btn.Height;

                using (var lipBrush = new SolidBrush(lipColor))
                using (var topHighlight = new Pen(Color.FromArgb(60, 190, 230, 255), 1))
                {
                    g.FillRectangle(lipBrush, 0, h - 4, w, 4);
                    g.DrawLine(topHighlight, 0, 0, w - 1, 0);
                }
            };
        }

        private static void StyleOverlayLabel(Label lbl, float fontSize, FontStyle style, Color color, ContentAlignment align)
        {
            if (lbl == null) return;
            lbl.AutoSize = false;
            lbl.BackColor = Color.Transparent;
            lbl.Font = FontManager.GetFont(fontSize, style);
            lbl.ForeColor = color;
            lbl.TextAlign = align;
        }

        private void LayoutControls()
        {
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            if (w <= 48 || h <= 120) return;

            int padX = 16;
            int bottomPad = 14; // Cách viền kép phía dưới một khoảng gọn gàng
            int innerW = w - padX * 2;
            int gap = h < 310 ? 8 : 10;

            // Neo nút BACK từ đáy lên trước để KHÔNG BAO GIỜ bị rớt ra ngoài
            int btnH = h < 270 ? 28 : 32;
            int btnY = h - bottomPad - btnH;
            btnBack.SetBounds(padX, btnY, innerW, btnH);

            // Tính phần chiều cao còn lại giữa tiêu đề và nút BACK để chia đều cho 3 hàng thẻ
            int contentTop = 36;
            int availH = Math.Max(110, (btnY - gap) - contentTop);
            int cardsTotalH = Math.Max(90, availH - gap * 2);

            int profileH = Math.Min(64, (int)(cardsTotalH * 0.34f));
            int statH = Math.Min(64, (int)(cardsTotalH * 0.36f));
            int rateH = Math.Min(50, Math.Max(34, cardsTotalH - profileH - statH));

            // 1. Khối thông tin thuyền trưởng
            int y = contentTop;
            _profileCard = new Rectangle(padX, y, innerW, profileH);

            int cardContentH = profileH - 4;
            int avatarSize = Math.Min(40, Math.Max(28, cardContentH - 12));
            int avatarY = _profileCard.Y + (cardContentH - avatarSize) / 2;
            _avatarRect = new Rectangle(_profileCard.X + 10, avatarY, avatarSize, avatarSize);

            int textX = _avatarRect.Right + 10;
            int textW = Math.Max(40, _profileCard.Right - textX - 8);
            int textMidY = _profileCard.Y + cardContentH / 2;
            lblUsername.SetBounds(textX, textMidY - 17, textW, 19);
            lblID.SetBounds(textX, textMidY + 2, textW, 15);

            // 2. Hai khối WINS & LOSSES
            y = _profileCard.Bottom + gap;
            int statW = (innerW - gap) / 2;

            _winCard = new Rectangle(padX, y, statW, statH);
            _loseCard = new Rectangle(padX + statW + gap, y, innerW - statW - gap, statH);

            lblWin.SetBounds(_winCard.X + 4, _winCard.Y + 2, _winCard.Width - 8, _winCard.Height - 6);
            lblLose.SetBounds(_loseCard.X + 4, _loseCard.Y + 2, _loseCard.Width - 8, _loseCard.Height - 6);

            // 3. Khối tỷ lệ thắng
            y = _winCard.Bottom + gap;
            _rateCard = new Rectangle(padX, y, innerW, rateH);

            int barH = rateH < 40 ? 7 : 9;
            int barY = _rateCard.Bottom - 4 - 6 - barH;
            int lblH = Math.Max(14, barY - _rateCard.Y - 4);
            label1.SetBounds(_rateCard.X + 8, _rateCard.Y + 3, _rateCard.Width - 16, lblH);
            _rateBarRect = new Rectangle(_rateCard.X + 10, barY, _rateCard.Width - 20, barH);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutControls();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            Theme.DrawCrtBackground(g, this.ClientRectangle, withOuterFrame: true);

            if (this.ClientSize.Width <= 48 || this.ClientSize.Height <= 120) return;

            g.SmoothingMode = SmoothingMode.None;

            DrawHeader(g);

            DrawSolidCard(g, _profileCard, NeutralCardFill, NeutralCardLip, highlightAlpha: 40);
            DrawAvatarBadge(g);

            DrawSolidCard(g, _winCard, WinFill, WinLip, highlightAlpha: 95);
            DrawSolidCard(g, _loseCard, LoseFill, LoseLip, highlightAlpha: 95);

            DrawSolidCard(g, _rateCard, RateCardFill, RateCardLip, highlightAlpha: 32);
            DrawWinRateBar(g);
        }

        private static void DrawSolidCard(Graphics g, Rectangle rect, Color fillColor, Color lipColor, int highlightAlpha)
        {
            Rectangle shadowRect = new Rectangle(rect.X + 3, rect.Y + 4, rect.Width, rect.Height);

            using (var shadowBrush = new SolidBrush(Theme.Shadow))
            using (var fillBrush = new SolidBrush(fillColor))
            using (var lipBrush = new SolidBrush(lipColor))
            using (var topHighlight = new Pen(Color.FromArgb(highlightAlpha, 255, 255, 255), 1))
            {
                g.FillRectangle(shadowBrush, shadowRect);
                g.FillRectangle(fillBrush, rect);
                g.FillRectangle(lipBrush, rect.X, rect.Bottom - 4, rect.Width, 4);
                g.DrawLine(topHighlight, rect.X, rect.Y, rect.Right - 1, rect.Y);
            }
        }

        private void DrawHeader(Graphics g)
        {
            string title = "CAPTAIN PROFILE";
            Rectangle titleRect = new Rectangle(16, 11, this.ClientSize.Width - 32, 20);
            Rectangle shadowRect = new Rectangle(titleRect.X + 2, titleRect.Y + 2, titleRect.Width, titleRect.Height);

            Theme.DrawRetroText(g, title, _headerFont, shadowRect, Theme.Shadow, ContentAlignment.MiddleCenter);
            Theme.DrawRetroText(g, title, _headerFont, titleRect, Theme.Text, ContentAlignment.MiddleCenter);
        }

        private void DrawAvatarBadge(Graphics g)
        {
            using (var bgBrush = new SolidBrush(AvatarFill))
            using (var lipBrush = new SolidBrush(AvatarLip))
            using (var topHighlight = new Pen(Color.FromArgb(50, 190, 230, 255), 1))
            {
                g.FillRectangle(bgBrush, _avatarRect);
                g.FillRectangle(lipBrush, _avatarRect.X, _avatarRect.Bottom - 3, _avatarRect.Width, 3);
                g.DrawLine(topHighlight, _avatarRect.X, _avatarRect.Y, _avatarRect.Right - 1, _avatarRect.Y);
            }

            Theme.DrawRetroText(g, _avatarLetter, _avatarFont, _avatarRect, Theme.Text, ContentAlignment.MiddleCenter);
        }

        private void DrawWinRateBar(Graphics g)
        {
            using (var bgBrush = new SolidBrush(BarTrack))
            {
                g.FillRectangle(bgBrush, _rateBarRect);
            }

            int fillWidth = (int)Math.Round(_rateBarRect.Width * _winRate);
            if (fillWidth > 0)
            {
                Rectangle winPart = new Rectangle(_rateBarRect.X, _rateBarRect.Y, fillWidth, _rateBarRect.Height);
                using (var winBrush = new SolidBrush(BarWin))
                using (var sheenBrush = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                {
                    g.FillRectangle(winBrush, winPart);
                    g.FillRectangle(sheenBrush, winPart.X, winPart.Y, winPart.Width, 3);
                }
            }

            if (_winRate > 0f && fillWidth < _rateBarRect.Width)
            {
                Rectangle losePart = new Rectangle(_rateBarRect.X + fillWidth, _rateBarRect.Y, _rateBarRect.Width - fillWidth, _rateBarRect.Height);
                using (var loseBrush = new SolidBrush(BarLose))
                using (var sheenBrush = new SolidBrush(Color.FromArgb(45, 255, 255, 255)))
                {
                    g.FillRectangle(loseBrush, losePart);
                    g.FillRectangle(sheenBrush, losePart.X, losePart.Y, losePart.Width, 3);
                }
            }
        }

        private async Task LoadProfileAndFriendsAsync()
        {
            if (PlayerProfile.CurrentUser != null)
            {
                string username = string.IsNullOrWhiteSpace(PlayerProfile.Username) ? "CAPTAIN" : PlayerProfile.Username;
                int wins = PlayerProfile.Wins;
                int losses = PlayerProfile.Losses;
                int total = wins + losses;

                _avatarLetter = username.Substring(0, 1).ToUpperInvariant();
                _winRate = total > 0 ? (float)wins / total : 0f;
                int winPercent = (int)Math.Round(_winRate * 100);

                lblUsername.Text = username;
                lblID.Text = $"ID #{PlayerProfile.Id}";
                lblWin.Text = $"WINS\n{wins}";
                lblLose.Text = $"LOSSES\n{losses}";
                label1.Text = total > 0
                    ? $"WIN RATE: {winPercent}"
                    : "NO MATCHES PLAYED";

                Invalidate();
            }

            await Task.CompletedTask;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Parent?.Controls.Remove(this);
            this.Dispose();
        }

        private void tabPageProfile_Click(object sender, EventArgs e) { }
        private void tabPageFriend_Click(object sender, EventArgs e) { }
        private void lblWin_Click(object sender, EventArgs e) { }
        private void lblLose_Click(object sender, EventArgs e) { }
        private void lblID_Click(object sender, EventArgs e) { }
        private void lblUsername_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void Profile_Load(object sender, EventArgs e)
        {
            LayoutControls();
        }
        private void lblProfiletxt_Click(object sender, EventArgs e) { }
    }
}