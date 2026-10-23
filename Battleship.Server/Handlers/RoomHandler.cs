using System;
using System.Text.Json;
using System.Threading.Tasks;
using Battleship.Shared; // Namespace chứa IPacketHandler, Packet, PacketType và các DTOs

namespace Battleship.Server.Handlers
{
    // Kế thừa chính thức IPacketHandler để PacketRouter tự động nhận diện
    public class RoomHandler : IPacketHandler
    {
        private readonly RoomManager _roomManager;
        private readonly PacketRouter _router; // Bổ sung biến _router

        // Khớp 100% với hàm khởi tạo bên TcpServer.cs
        public RoomHandler(RoomManager roomManager)
        {
            _roomManager = roomManager;
        }

        // Hàm điều hướng gói tin liên quan đến Phòng (Room)
        public async Task HandlePacketAsync(ClientSession session, Packet packet)
        {
            switch (packet.Type)
            {
                case PacketType.CreateRoomRequest:
                    await HandleCreateRoomAsync(session, packet);
                    break;

                case PacketType.JoinRoomRequest:
                    await HandleJoinRoomAsync(session, packet);
                    break;

                case PacketType.LeaveRoomRequest:
                    await HandleLeaveRoomAsync(session, packet);
                    break;

                default:
                    // Log cảnh báo nếu Router gửi nhầm gói tin không thuộc phạm vi xử lý
                    Console.WriteLine($"[RoomHandler] Không hỗ trợ loại gói tin: {packet.Type}");
                    break;
            }
        }

        private async Task HandleCreateRoomAsync(ClientSession session, Packet packet)
        {
            var req = JsonSerializer.Deserialize<CreateRoomRequest>(packet.JsonPayload);
            if (req == null) return;

            // Xử lý tạo phòng qua RoomManager
            var room = _roomManager.CreateRoom(req.RoomId, session);
            bool success = room != null;

            // Phản hồi kết quả tạo phòng về cho Client gửi yêu cầu
            var response = new CreateRoomResponse
            {
                IsSuccess = success,
                RoomId = req.RoomId
            };

            var responsePacket = new Packet
            {
                Type = PacketType.CreateRoomResponse,
                JsonPayload = JsonSerializer.Serialize(response)
            };

            await session.SendAsync(responsePacket);
        }

        private async Task HandleJoinRoomAsync(ClientSession session, Packet packet)
        {
            var req = JsonSerializer.Deserialize<JoinRoomRequest>(packet.JsonPayload);
            if (req == null) return;

            // Xử lý vào phòng qua RoomManager
            bool success = _roomManager.JoinRoom(req.RoomId, session);

            // Phản hồi kết quả vào phòng về cho người xin vào
            var response = new JoinRoomResponse
            {
                IsSuccess = success,
                RoomId = req.RoomId
            };

            var responsePacket = new Packet
            {
                Type = PacketType.JoinRoomResponse,
                JsonPayload = JsonSerializer.Serialize(response)
            };

            await session.SendAsync(responsePacket);

            // Nếu vào phòng thành công, cập nhật và đồng bộ trạng thái mới cho toàn bộ người trong phòng
            if (success)
            {
                await BroadcastRoomUpdateAsync(req.RoomId, $"{session.PlayerName} đã tham gia phòng!");
            }
        }

        private async Task HandleLeaveRoomAsync(ClientSession session, Packet packet)
        {
            var req = JsonSerializer.Deserialize<LeaveRoomRequest>(packet.JsonPayload);
            if (req == null) return;

            // Gọi RoomManager để xử lý logic rời phòng (Kiểm tra xem người rời là Host hay Player2)
            bool success = _roomManager.LeaveRoom(req.RoomId, session);
            if (!success) return;

            // Lấy thông tin phòng sau khi có người rời để quyết định hành động tiếp theo
            var room = _roomManager.GetRoom(req.RoomId);

            if (room == null)
            {
                // Nếu phòng đã bị giải tán (ví dụ do Host thoát) -> Không cần broadcast nữa
                Console.WriteLine($"[RoomHandler] Phòng {req.RoomId} đã được giải tán thành công.");
            }
            else
            {
                // Nếu phòng vẫn tồn tại (ví dụ Player2 thoát, Host ở lại) -> Cập nhật tình hình cho Host biết
                await BroadcastRoomUpdateAsync(req.RoomId, $"{session.PlayerName} đã rời phòng.");
            }
        }

        // Hàm tiện ích để đồng bộ trạng thái phòng (RoomUpdate) cho tất cả các bên liên quan
        private async Task BroadcastRoomUpdateAsync(string roomId, string notificationMessage)
        {
            var room = _roomManager.GetRoom(roomId);
            if (room == null) return;

            // Chuẩn bị dữ liệu cập nhật (gồm thông báo chữ và State hiện tại của phòng từ Server)
            var updateData = new RoomUpdateResponse
            {
                RoomId = roomId,
                Message = notificationMessage,
                State = room.State // Gửi kèm State (Waiting, Ready, Playing...) để Client cập nhật UI chính xác
            };

            var updatePacket = new Packet
            {
                Type = PacketType.RoomUpdateResponse,
                JsonPayload = JsonSerializer.Serialize(updateData)
            };

            // Gửi cho Host (nếu có)
            if (room.HostSession != null)
            {
                await room.HostSession.SendAsync(updatePacket);
            }

            // Gửi cho Player 2 (nếu có)
            if (room.Player2Session != null)
            {
                await room.Player2Session.SendAsync(updatePacket);
            }
        }
    }
}
