using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server.Handlers
{
    public class HeartbeatHandler : IPacketHandler
    {
        public async Task HandleAsync(ClientSession session, Packet packet)
        {
            session.MarkPongReceived();

            // Nếu Client chủ động gửi Ping lên thì trả lại Pong
            if (packet.Type == PacketType.Ping)
            {
                await session.SendAsync(new Packet { Type = PacketType.Pong });
            }
        }
    }
}