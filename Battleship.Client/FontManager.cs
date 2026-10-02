using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Client
{
    internal class FontManager
    {
        private static PrivateFontCollection pfc = new PrivateFontCollection();
        private static bool isLoaded = false;

        public static Font GetFont(float size, FontStyle style = FontStyle.Regular)
        {
            if (!isLoaded)
            {
                LoadEmbeddedFont();
                isLoaded = true;
            }

            if (pfc.Families.Length > 0)
            {
                return new Font(pfc.Families[0], size, style);
            }

            return new Font("Arial", size, style);
        }

        private static void LoadEmbeddedFont()
        {
            // noi de font chu 
            string resourceName = "Battleship.Client.PixelFonts.RetroGaming.ttf";

            // Truy cập vào ruột file .exe để moi cái font ra
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    MessageBox.Show("Không tìm thấy file font trong Embedded Resource!\nHãy kiểm tra lại tên file hoặc Build Action.", "Lỗi Font");
                    return;
                }

                // Đọc dữ liệu từ file nhúng ra bộ nhớ RAM
                byte[] fontData = new byte[stream.Length];
                stream.Read(fontData, 0, (int)stream.Length);

                // Xin Windows cấp phát một vùng nhớ trống
                IntPtr data = Marshal.AllocCoTaskMem((int)stream.Length);

                // Copy dữ liệu font vào vùng nhớ đó
                Marshal.Copy(fontData, 0, data, (int)stream.Length);

                // Nạp font từ vùng nhớ vào bộ sưu tập Font của phần mềm
                pfc.AddMemoryFont(data, (int)stream.Length);

                // Giải phóng vùng nhớ để không bị tràn RAM
                Marshal.FreeCoTaskMem(data);
            }
        }
    }
}
