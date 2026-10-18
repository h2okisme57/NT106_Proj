using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server;

// Xử lý 2 loại gói: bấm "Tìm trận" và bấm "Hủy tìm trận"
public class MatchmakingHandler : IPacketHandler
{
    private readonly MatchmakingQueue _queue;

    public MatchmakingHandler(MatchmakingQueue queue)
    {
        _queue = queue;
    }

    public Task HandleAsync(ClientSession session, Packet packet)
    {
        if (packet.Type == PacketType.MatchmakingJoinRequest)
        {
            _queue.Join(session);
        }
        else if (packet.Type == PacketType.MatchmakingCancelRequest)
        {
            _queue.Cancel(session);
        }

        // hàm này không có await nào nên trả về Task đã xong
        return Task.CompletedTask;
    }
}
