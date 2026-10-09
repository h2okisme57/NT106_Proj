using System;
using System.Collections.Concurrent;
using System.Threading;

public class GameRoom
{
    public string RoomId { get; private set; }
    public ClientSession Host { get; private set; }
    public ClientSession Player2 { get; private set; }

    // Đối tượng lock để đảm bảo luồng an toàn (Thread-safe) trong nội bộ phòng
    public readonly object RoomLock = new object();

    public bool IsFull => Host != null && Player2 != null;

    public GameRoom(string roomId, ClientSession host)
    {
        RoomId = roomId;
        Host = host;
    }

    public void AddPlayer(ClientSession player)
    {
        if (Player2 == null) Player2 = player;
    }

    public void RemovePlayer(ClientSession player)
    {
        if (Player2 != null && Player2.SessionId == player.SessionId)
            Player2 = null;
    }
}

public class RoomManager
{
    // Dùng ConcurrentDictionary theo chuẩn thiết kế để chống Race-condition ở cấp độ danh sách phòng
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new ConcurrentDictionary<string, GameRoom>();

    // 1. Tạo phòng mới bằng ID
    public GameRoom CreateRoom(string roomId, ClientSession host)
    {
        var newRoom = new GameRoom(roomId, host);
        if (_rooms.TryAdd(roomId, newRoom))
        {
            Console.WriteLine($"[RoomManager] Đã tạo phòng {roomId}. Chủ phòng: {host.SessionId}");
            return newRoom;
        }
        Console.WriteLine($"[RoomManager] Tạo phòng thất bại, ID {roomId} đã tồn tại.");
        return null;
    }

    // 2. Vào phòng
    public bool JoinRoom(string roomId, ClientSession player)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            // Bọc lock tránh trường hợp 2 client ấn Join vào phòng trống cùng 1 mili-giây
            lock (room.RoomLock)
            {
                if (!room.IsFull)
                {
                    room.AddPlayer(player);
                    Console.WriteLine($"[RoomManager] Client {player.SessionId} đã vào phòng {roomId}");
                    return true;
                }
            }
        }
        return false;
    }

    // 3. Quyền Kick thành viên
    public bool KickPlayer(string roomId, ClientSession requestClient, ClientSession targetPlayer)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            lock (room.RoomLock)
            {
                // Xác thực quyền: Chỉ Chủ phòng (Host) mới được thực hiện lệnh Kick
                if (room.Host.SessionId == requestClient.SessionId &&
                    room.Player2 != null && room.Player2.SessionId == targetPlayer.SessionId)
                {
                    room.RemovePlayer(targetPlayer);
                    Console.WriteLine($"[RoomManager] Host {requestClient.SessionId} đã Kick {targetPlayer.SessionId} khỏi phòng {roomId}");
                    return true;
                }
            }
        }
        return false;
    }
}