using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server;

// Handler mẫu cho các bạn tham khảo cách viết + đồng thời là CHAT SẢNH chạy thật.
// Nhận gói ChatMessage từ 1 client -> phát lại cho toàn bộ client đang online.
public class ChatHandler : IPacketHandler
{
    private readonly PacketRouter _router;

    public ChatHandler(PacketRouter router)
    {
        _router = router;
    }

    public async Task HandleAsync(ClientSession session, Packet packet)
    {
        // (Tùy chọn) có thể gắn tên người gửi từ session trước khi phát:
        // var dto = packet.DeserializePayload<ChatMessageDto>();
        // Ở đây phát lại nguyên gói cho đơn giản.
        await _router.BroadcastAsync(packet);
    }
}
