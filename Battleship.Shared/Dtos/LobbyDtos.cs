using System;
using System.Collections.Generic;

namespace Battleship.Shared;

// ===== Sảnh chờ: Chat & Danh sách Online (dùng cho phần của Dũng - 3.2 / 3.5) =====
public class FriendDto
{
    public string Username { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
}

public class GetFriendsResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<FriendDto> Friends { get; set; } = new List<FriendDto>();
}
public class ChatMessageDto
{
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Scope { get; set; } = "lobby";   // "lobby" hoặc "room"
    public string? RoomId { get; set; }            // bắt buộc khi Scope = "room"
}

// Server broadcast danh sách người đang online xuống toàn bộ client.
public class OnlineListDto
{
    public List<string> Users { get; set; } = new List<string>();
}
