using Battleship.Shared;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Battleship.Server.Handlers
{
    public class FriendHandler : IPacketHandler
    {
        private readonly ConcurrentDictionary<string, ClientSession> _sessions;

        public FriendHandler(ConcurrentDictionary<string, ClientSession> sessions)
        {
            _sessions = sessions;
        }

        public async Task HandleAsync(ClientSession session, Packet packet)
        {
            // 1. Đọc UserId yêu cầu lấy bạn bè từ Payload
            string userId = packet.DeserializePayload<string>();

            // 2. Query danh sách bạn bè từ Database (Ví dụ mô phỏng truy vấn DB)
            // Trong thực tế bạn gọi: var friendsFromDb = await DbContext.Friends.Where(...)...
            var friendsList = new List<FriendDto>
            {
                new FriendDto { Username = "Dũng", IsOnline = CheckUserOnline("Dũng") },
                new FriendDto { Username = "Đức", IsOnline = CheckUserOnline("Đức") },
                new FriendDto { Username = "Trí", IsOnline = CheckUserOnline("Trí") },
                new FriendDto { Username = "Hùng", IsOnline = CheckUserOnline("Hùng") }
            };

            // 3. Đóng gói Response DTO
            var responseDto = new GetFriendsResponseDto
            {
                Success = true,
                Message = "Tải danh sách bạn bè thành công!",
                Friends = friendsList
            };

            // 4. Gửi Response về cho Client vừa yêu cầu
            var responsePacket = Packet.Create(PacketType.GetFriendsResponse, responseDto);
            await session.SendAsync(responsePacket);
        }

        // Kiểm tra xem bạn bè có nằm trong danh sách Session đang Online hay không
        private bool CheckUserOnline(string username)
        {
            return _sessions.Values.Any(s => s.IsLoggedIn && s.Username == username);
        }
    }
}