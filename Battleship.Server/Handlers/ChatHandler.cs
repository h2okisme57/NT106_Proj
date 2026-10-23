using System;
using System.Text.Json;
using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server.Handlers
{
    public class ChatHandler : IPacketHandler
    {
        private readonly PacketRouter _router;

        public ChatHandler(PacketRouter router)
        {
            _router = router;
        }

        public async Task HandleAsync(ClientSession session, Packet packet)
        {
            var chatMsg = JsonSerializer.Deserialize<ChatMessageDto>(packet.Payload);
            if (chatMsg == null) return;

            // Ghi đè thông tin xác thực từ Server để chống Client giả mạo
            chatMsg.Sender = session.Username ?? "Khách";
            chatMsg.Timestamp = DateTime.UtcNow;

            // Đóng gói lại với payload đã được cập nhật
            packet.Payload = JsonSerializer.Serialize(chatMsg);

            await _router.BroadcastAsync(packet);
        }
    }
}