using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Battleship.Server;

// Theo dõi ai đang đăng nhập, để 1 tài khoản không đăng nhập 2 nơi cùng lúc.
// Không phân biệt hoa thường (Admin và admin là cùng 1 người).
public static class OnlineUsers
{
    private static readonly ConcurrentDictionary<string, ClientSession> _byName =
        new ConcurrentDictionary<string, ClientSession>(StringComparer.OrdinalIgnoreCase);

    // Thử đăng nhập: true nếu được, false nếu tài khoản này đang được dùng ở session khác còn sống
    public static bool TryLogin(string username, ClientSession session)
    {
        while (true)
        {
            if (_byName.TryAdd(username, session))
                return true;

            if (_byName.TryGetValue(username, out var old))
            {
                if (old.SessionId == session.SessionId)
                    return true;

                if (IsAlive(old))
                    return false;

                // session cũ đã chết (mất mạng đột ngột chưa kịp dọn) thì gỡ ra rồi thử lại
                _byName.TryRemove(new KeyValuePair<string, ClientSession>(username, old));
            }
        }
    }

    // Gọi khi session ngắt kết nối
    public static void Remove(ClientSession session)
    {
        if (!string.IsNullOrEmpty(session.Username))
        {
            _byName.TryRemove(new KeyValuePair<string, ClientSession>(session.Username, session));
        }
    }

    // Kiểm tra socket còn sống không (đã đóng thì Poll báo "có thể đọc" nhưng không có byte nào)
    private static bool IsAlive(ClientSession s)
    {
        try
        {
            Socket sock = s.Client.Client;
            return !(sock.Poll(1000, SelectMode.SelectRead) && sock.Available == 0);
        }
        catch
        {
            return false;
        }
    }
}
