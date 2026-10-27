using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Battleship.Shared;

// Mã hóa / giải mã gói tin bằng AES-128 (mã hóa đối xứng).
// Client và Server dùng CHUNG 1 khóa nên 2 bên đều phải dùng đúng file này.
public static class CryptoHelper
{
    // Bật/tắt mã hóa. Mặc định là bật.
    // Khi demo Wireshark (mục 5.2) muốn so sánh gói thường với gói mã hóa thì đổi thành false,
    // nhưng nhớ đổi cả Client lẫn Server, lệch nhau là không đọc được gói.
    public static bool Enabled { get; set; } = true;

    // Khóa AES-128 = 16 byte. Mình đặt 1 câu bí mật rồi băm SHA256, lấy 16 byte đầu làm khóa
    // (đỡ phải gõ tay 16 ký tự cho đúng độ dài).
    // Lưu ý: khóa đang nằm trong code nên chỉ phù hợp đồ án môn học, thực tế không nên làm vậy.
    private const string SecretPhrase = "NT106-Battleship-2026-SecretKey";
    private static readonly byte[] Key = MakeKey(SecretPhrase);

    private const int IvSize = 16; // AES luôn dùng IV 16 byte

    private static byte[] MakeKey(string phrase)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(phrase));
        byte[] key = new byte[16];
        Array.Copy(hash, key, 16);
        return key;
    }

    // Mã hóa: kết quả = [16 byte IV] + [dữ liệu đã mã hóa]
    // IV được tạo ngẫu nhiên MỖI GÓI nên cùng 1 nội dung gửi 2 lần thì bytes trên đường truyền vẫn khác nhau.
    public static byte[] Encrypt(byte[] plain)
    {
        if (!Enabled)
            return plain;

        using Aes aes = Aes.Create();
        aes.KeySize = 128;
        aes.Key = Key;
        aes.GenerateIV();

        byte[] cipher = aes.EncryptCbc(plain, aes.IV, PaddingMode.PKCS7);

        // ghép IV vào đầu để bên nhận biết mà giải mã
        byte[] result = new byte[IvSize + cipher.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, IvSize);
        Buffer.BlockCopy(cipher, 0, result, IvSize, cipher.Length);
        return result;
    }

    // Giải mã: tách 16 byte IV ở đầu ra rồi giải phần còn lại.
    // Sai khóa / gói bị sửa / gửi gói không mã hóa thì ném InvalidDataException
    // (ClientSession đã có catch loại lỗi này nên chỉ đóng session đó thôi, server không sập).
    public static byte[] Decrypt(byte[] data)
    {
        if (!Enabled)
            return data;

        // ít nhất phải có IV (16) + 1 block dữ liệu (16)
        if (data.Length < IvSize + 16)
            throw new InvalidDataException("Goi tin ma hoa qua ngan.");

        byte[] iv = new byte[IvSize];
        Buffer.BlockCopy(data, 0, iv, 0, IvSize);

        byte[] cipher = new byte[data.Length - IvSize];
        Buffer.BlockCopy(data, IvSize, cipher, 0, cipher.Length);

        try
        {
            using Aes aes = Aes.Create();
            aes.KeySize = 128;
            aes.Key = Key;
            return aes.DecryptCbc(cipher, iv, PaddingMode.PKCS7);
        }
        catch (CryptographicException)
        {
            throw new InvalidDataException("Giai ma that bai (sai khoa hoac goi tin bi sua).");
        }
    }
}
