namespace Battleship.Server;

// Chống dò mật khẩu (brute force): sai quá nhiều lần thì bị khóa một lúc.
// Dùng chung cho nhiều loại "khóa" (key): theo địa chỉ IP, theo tên tài khoản...
public class LoginRateLimiter
{
    private class Entry
    {
        public int Count;                // số lần sai trong khung thời gian hiện tại
        public DateTime WindowStart;     // lúc bắt đầu tính khung
        public DateTime LockedUntil;     // bị khóa tới khi nào
    }

    private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();
    private readonly object _lock = new object();   // nhiều session chạy cùng lúc nên phải khóa khi sửa Dictionary

    private readonly int _maxFailures;
    private readonly TimeSpan _window;
    private readonly TimeSpan _lockTime;

    // maxFailures: sai bao nhiêu lần thì khóa; window: tính trong bao lâu; lockTime: khóa bao lâu
    public LoginRateLimiter(int maxFailures, TimeSpan window, TimeSpan lockTime)
    {
        _maxFailures = maxFailures;
        _window = window;
        _lockTime = lockTime;
    }

    // Đang bị khóa không? Nếu có thì trả về số giây còn phải chờ
    public bool IsLocked(string key, out int secondsLeft)
    {
        lock (_lock)
        {
            if (_entries.TryGetValue(key, out var e) && e.LockedUntil > DateTime.UtcNow)
            {
                secondsLeft = (int)Math.Ceiling((e.LockedUntil - DateTime.UtcNow).TotalSeconds);
                return true;
            }
        }
        secondsLeft = 0;
        return false;
    }

    // Ghi nhận thêm 1 lần sai
    public void RegisterFailure(string key)
    {
        lock (_lock)
        {
            DateTime now = DateTime.UtcNow;

            // dọn bớt các mục đã hết hạn để Dictionary không phình to mãi
            if (_entries.Count > 1000)
            {
                var old = _entries.Where(p => p.Value.LockedUntil < now && now - p.Value.WindowStart > _window)
                                  .Select(p => p.Key).ToList();
                foreach (var k in old) _entries.Remove(k);
            }

            if (!_entries.TryGetValue(key, out var e))
            {
                e = new Entry { WindowStart = now };
                _entries[key] = e;
            }

            // quá khung thời gian cũ thì tính lại từ đầu
            if (now - e.WindowStart > _window)
            {
                e.Count = 0;
                e.WindowStart = now;
            }

            e.Count++;

            if (e.Count >= _maxFailures)
            {
                e.LockedUntil = now + _lockTime;
                e.Count = 0;
                e.WindowStart = now;
            }
        }
    }

    // Đăng nhập đúng rồi thì xóa bộ đếm sai
    public void Reset(string key)
    {
        lock (_lock)
        {
            _entries.Remove(key);
        }
    }
}
