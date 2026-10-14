using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server;

// Giao diện chung cho mọi bộ xử lý gói tin.
// Mỗi mảng (Auth của Dũng, Room/Game của Đức...) viết 1 class riêng implement interface này
// rồi tự đăng ký vào PacketRouter -> không ai phải sửa chung một chỗ switch nữa.
public interface IPacketHandler
{
    Task HandleAsync(ClientSession session, Packet packet);
}
