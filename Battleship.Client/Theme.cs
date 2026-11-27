using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace Battleship.Client
{
    internal static class Theme
    {
        private static readonly PrivateFontCollection _retroPfc = new PrivateFontCollection();
        private static FontFamily _retroFamily;
        private static string _retroFamilyName;
        private static bool _retroFontChecked = false;

        // Biến kiểm soát kích thước phông chữ chung cho toàn bộ UI (VD: 0.75f - 1.0f)
        public static float RetroFontScale = 0.84f;
        public static float MinRetroFontSize = 6f;

        // Nền biển sâu chuyển sắc từ xanh đại dương đậm xuống đáy biển
        public static readonly Color BgTop = Color.FromArgb(14, 58, 96);
        public static readonly Color BgBottom = Color.FromArgb(5, 22, 44);

        // Khung panel & đổ bóng tông xanh navy đậm thay vì đen/xám
        public static readonly Color PanelBg = Color.FromArgb(14, 40, 70);
        public static readonly Color PanelBorder = Color.FromArgb(164, 210, 236);
        public static readonly Color Shadow = Color.FromArgb(3, 14, 30);

        // Ô nhập liệu xanh trầm
        public static readonly Color InputBg = Color.FromArgb(7, 25, 48);
        public static readonly Color InputBorder = Color.FromArgb(46, 102, 148);

        // Màu nhấn & trạng thái nổi bật trên nền xanh biển
        public static readonly Color Cyan = Color.FromArgb(80, 215, 255);
        public static readonly Color CyanDark = Color.FromArgb(0, 110, 178);
        public static readonly Color Gold = Color.FromArgb(245, 190, 60);
        public static readonly Color Red = Color.FromArgb(255, 92, 88);
        public static readonly Color RedDark = Color.FromArgb(168, 42, 44);
        public static readonly Color Green = Color.FromArgb(64, 218, 130);

        // Nút phụ tông xanh thép hải quân
        public static readonly Color Slate = Color.FromArgb(34, 78, 114);
        public static readonly Color SlateDark = Color.FromArgb(17, 45, 72);

        // Chữ trắng ánh xanh mát và chữ phụ xanh sáng nhẹ
        public static readonly Color Text = Color.FromArgb(232, 244, 252);
        public static readonly Color Muted = Color.FromArgb(156, 202, 232);

        private static void EnsureRetroFontLoaded()
        {
            if (_retroFontChecked) return;
            _retroFontChecked = true;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates =
            {
                Path.Combine(baseDir, "retrogaming.ttf"),
                Path.Combine(baseDir, "Fonts", "retrogaming.ttf"),
                Path.Combine(baseDir, "Resources", "retrogaming.ttf"),
                Path.Combine(baseDir, "Assets", "retrogaming.ttf"),
                Path.Combine(baseDir, "Retro Gaming.ttf"),
                Path.Combine(baseDir, "Fonts", "Retro Gaming.ttf")
            };

            foreach (string path in candidates)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        _retroPfc.AddFontFile(path);
                        if (_retroPfc.Families.Length > 0)
                        {
                            _retroFamily = _retroPfc.Families[0];
                            _retroFamilyName = _retroFamily.Name;
                            return;
                        }
                    }
                    catch
                    {
                        // Bỏ qua nếu file lỗi và chuyển sang fallback
                    }
                }
            }
        }

        public static Font GetRetroFont(float size, FontStyle style = FontStyle.Regular)
        {
            EnsureRetroFontLoaded();

            float scaledSize = Math.Max(MinRetroFontSize, size * RetroFontScale);

            if (_retroFamily != null)
            {
                FontStyle safeStyle = _retroFamily.IsStyleAvailable(style) ? style : FontStyle.Regular;
                return new Font(_retroFamily, scaledSize, safeStyle, GraphicsUnit.Point);
            }

            Font fallback = FontManager.GetFontRaw(scaledSize, style);
            if (string.IsNullOrEmpty(_retroFamilyName) && fallback != null)
            {
                _retroFamilyName = fallback.FontFamily.Name;
            }
            return fallback;
        }

        public static void DrawCrtBackground(Graphics g, Rectangle bounds, bool withOuterFrame = true)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            g.SmoothingMode = SmoothingMode.None;

            using (var bgBrush = new LinearGradientBrush(bounds, BgTop, BgBottom, LinearGradientMode.Vertical))
            {
                g.FillRectangle(bgBrush, bounds);
            }

            using (var scanPen = new Pen(Color.FromArgb(32, 2, 14, 32), 1))
            {
                for (int y = 0; y < bounds.Height; y += 3)
                {
                    g.DrawLine(scanPen, 0, y, bounds.Width, y);
                }
            }

            if (withOuterFrame && bounds.Width > 16 && bounds.Height > 16)
            {
                using (var outerPen = new Pen(Shadow, 4))
                using (var innerPen = new Pen(PanelBorder, 2))
                {
                    g.DrawRectangle(outerPen, 2, 2, bounds.Width - 4, bounds.Height - 4);
                    g.DrawRectangle(innerPen, 6, 6, bounds.Width - 12, bounds.Height - 12);
                }
            }
        }

        public static void DrawCard(Graphics g, Rectangle rect, Color? borderColor = null)
        {
            g.SmoothingMode = SmoothingMode.None;
            Rectangle shadowRect = new Rectangle(rect.X + 5, rect.Y + 6, rect.Width, rect.Height);

            using (var shadowBrush = new SolidBrush(Shadow))
            using (var cardBrush = new SolidBrush(PanelBg))
            using (var cardBorder = new Pen(borderColor ?? PanelBorder, 2))
            {
                g.FillRectangle(shadowBrush, shadowRect);
                g.FillRectangle(cardBrush, rect);
                g.DrawRectangle(cardBorder, rect);
            }
        }

        public static void DrawInputSlot(Graphics g, TextBox tb, int verticalPadding = 8)
        {
            if (tb == null || !tb.Visible) return;
            Rectangle slotRect = new Rectangle(tb.Left - 10, tb.Top - verticalPadding, tb.Width + 20, tb.Height + verticalPadding * 2);
            Color borderColor = tb.Focused ? Cyan : InputBorder;

            using (var bgBrush = new SolidBrush(InputBg))
            using (var borderPen = new Pen(borderColor, 2))
            {
                g.FillRectangle(bgBrush, slotRect);
                g.DrawRectangle(borderPen, slotRect);
            }
        }

        public static void DrawRetroText(Graphics g, string text, Font font, Rectangle rect, Color color, ContentAlignment align = ContentAlignment.MiddleCenter)
        {
            if (string.IsNullOrEmpty(text) || font == null) return;

            var oldHint = g.TextRenderingHint;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            using (var brush = new SolidBrush(color))
            using (var sf = new StringFormat())
            {
                if (align == ContentAlignment.TopCenter || align == ContentAlignment.MiddleCenter || align == ContentAlignment.BottomCenter)
                    sf.Alignment = StringAlignment.Center;
                else if (align == ContentAlignment.TopRight || align == ContentAlignment.MiddleRight || align == ContentAlignment.BottomRight)
                    sf.Alignment = StringAlignment.Far;
                else
                    sf.Alignment = StringAlignment.Near;

                if (align == ContentAlignment.MiddleLeft || align == ContentAlignment.MiddleCenter || align == ContentAlignment.MiddleRight)
                    sf.LineAlignment = StringAlignment.Center;
                else if (align == ContentAlignment.BottomLeft || align == ContentAlignment.BottomCenter || align == ContentAlignment.BottomRight)
                    sf.LineAlignment = StringAlignment.Far;
                else
                    sf.LineAlignment = StringAlignment.Near;

                g.DrawString(text, font, brush, rect, sf);
            }

            g.TextRenderingHint = oldHint;
        }

        public static void StyleTextBox(TextBox tb, float fontSize = 8.5f)
        {
            tb.BorderStyle = BorderStyle.None;
            tb.BackColor = InputBg;
            tb.ForeColor = Muted;
            tb.Font = GetRetroFont(fontSize, FontStyle.Regular);
        }

        public static void StyleButton(Button btn, Color faceColor, Color lipColor, float fontSize = 9f)
        {
            btn.UseCompatibleTextRendering = true;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(faceColor, 0.12f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(faceColor, 0.1f);
            btn.BackColor = faceColor;
            btn.ForeColor = Color.White;
            btn.Font = GetRetroFont(fontSize, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
            btn.Paint += (s, e) => DrawButtonChrome(btn, e.Graphics, lipColor);
        }

        public static void DrawButtonChrome(Button btn, Graphics g, Color lipColor)
        {
            g.SmoothingMode = SmoothingMode.None;
            int w = btn.Width;
            int h = btn.Height;

            using (var lipBrush = new SolidBrush(btn.Enabled ? lipColor : Color.FromArgb(26, 52, 78)))
            {
                g.FillRectangle(lipBrush, 0, h - 5, w, 5);
            }

            using (var topPen = new Pen(Color.FromArgb(90, 210, 240, 255), 1))
            using (var borderPen = new Pen(Shadow, 2))
            {
                g.DrawLine(topPen, 2, 2, w - 3, 2);
                g.DrawRectangle(borderPen, 1, 1, w - 2, h - 2);
            }
        }

        public static void ApplyRetroFont(Control root)
        {
            if (root == null) return;

            EnsureRetroFontLoaded();

            if (root is Label lbl)
            {
                lbl.UseCompatibleTextRendering = true;
            }
            else if (root is ButtonBase btn)
            {
                btn.UseCompatibleTextRendering = true;
            }
            else if (root is GroupBox gb)
            {
                gb.UseCompatibleTextRendering = true;
            }

            // Tắt AutoScaleMode.Font trước khi đổi Font để WinForms không tự thu nhỏ Form/UserControl
            if (root is ContainerControl container && container.AutoScaleMode == AutoScaleMode.Font)
            {
                container.AutoScaleMode = AutoScaleMode.None;
            }

            // Chỉ áp font nếu control chưa dùng font retro để tránh bị nhân RetroFontScale 2 lần
            if (root.Font != null &&
                (string.IsNullOrEmpty(_retroFamilyName) ||
                 !string.Equals(root.Font.FontFamily.Name, _retroFamilyName, StringComparison.OrdinalIgnoreCase)))
            {
                root.Font = GetRetroFont(root.Font.Size, root.Font.Style);
            }

            foreach (Control child in root.Controls)
            {
                ApplyRetroFont(child);
            }
        }
    }
}