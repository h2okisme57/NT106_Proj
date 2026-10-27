using System;
using System.Linq;
using System.Threading.Tasks;
using Battleship.Shared; // Đã giữ nguyên dòng này và không dùng .Dtos thừa ở dòng 5

namespace Battleship.Server.Handlers
{
    public class RoomHandler : IPacketHandler
    {
        private readonly RoomManager _roomManager;
        private readonly PacketRouter _router;

        // Constructor đúng chuẩn (RoomManager, PacketRouter)
        public RoomHandler(RoomManager roomManager, PacketRouter router)
        {
            _roomManager = roomManager;
            _router = router;
        }

        // Tên hàm chuẩn theo interface IPacketHandler
        public async Task HandleAsync(ClientSession session, Packet packet)
        {
            switch (packet.Type)
            {
                case PacketType.GetRoomListRequest:
                    await HandleGetRoomListAsync(session);
                    break;

                case PacketType.CreateRoomRequest:
                    await HandleCreateRoomAsync(session, packet);
                    break;

                case PacketType.JoinRoomRequest:
                    await HandleJoinRoomAsync(session, packet);
                    break;

                case PacketType.LeaveRoom:
                    await HandleLeaveRoomAsync(session, packet);
                    break;

                default:
                    Console.WriteLine($"[RoomHandler] Không hỗ trợ gói tin: {packet.Type}");
                    break;
            }
        }

        // 1. Client gửi GetRoomListRequest -> Server trả GetRoomListResponse chứa RoomListResponseDto
        private async Task HandleGetRoomListAsync(ClientSession session)
        {
            var rooms = _roomManager.GetAllRooms()
                .Select(ToRoomInfoDto)
                .ToList();

            var responseDto = new RoomListResponseDto
            {
                Rooms = rooms
            };

            await session.SendAsync(Packet.Create(PacketType.GetRoomListResponse, responseDto));
        }

        // 2. Tạo phòng mới
        private async Task HandleCreateRoomAsync(ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<CreateRoomDto>();
            string roomId = (dto != null && !string.IsNullOrWhiteSpace(dto.RoomId))
                ? dto.RoomId
                : Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            var room = _roomManager.CreateRoom(roomId, session);

            var updateDto = new RoomUpdateDto
            {
                Room = room != null ? ToRoomInfoDto(room) : null
            };

            await session.SendAsync(Packet.Create(PacketType.RoomUpdate, updateDto));
        }

        // 3. Client gửi JoinRoomRequest (JoinRoomDto) -> Server trả JoinRoomResponse chứa RoomUpdateDto (Room == null là thất bại)
        private async Task HandleJoinRoomAsync(ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<JoinRoomDto>();
            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId))
            {
                await session.SendAsync(Packet.Create(PacketType.JoinRoomResponse, new RoomUpdateDto { Room = null }));
                return;
            }

            bool joined = _roomManager.JoinRoom(dto.RoomId, session);
            var room = joined ? _roomManager.GetRoom(dto.RoomId) : null;

            var updateDto = new RoomUpdateDto
            {
                Room = room != null ? ToRoomInfoDto(room) : null
            };

            // Trả JoinRoomResponse về cho Client vừa bấm Join
            await session.SendAsync(Packet.Create(PacketType.JoinRoomResponse, updateDto));

            // Nếu vào thành công, gửi thêm RoomUpdate cho chủ phòng (Host) để cập nhật UI
            if (joined && room != null)
            {
                await room.Host.SendAsync(Packet.Create(PacketType.RoomUpdate, updateDto));
            }
        }

        // 4. Rời phòng
        private async Task HandleLeaveRoomAsync(ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<LeaveRoomDto>();
            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId)) return;

            var room = _roomManager.GetRoom(dto.RoomId);
            ClientSession? otherPlayer = null;

            if (room != null)
            {
                otherPlayer = (room.Host.SessionId == session.SessionId) ? room.Player2 : room.Host;
            }

            bool left = _roomManager.LeaveRoom(dto.RoomId, session);
            if (left && otherPlayer != null)
            {
                var updatedRoom = _roomManager.GetRoom(dto.RoomId);
                var updateDto = new RoomUpdateDto
                {
                    Room = updatedRoom != null ? ToRoomInfoDto(updatedRoom) : null
                };
                await otherPlayer.SendAsync(Packet.Create(PacketType.RoomUpdate, updateDto));
            }
        }

        // Hàm ánh xạ từ GameRoom sang RoomInfoDto đã được sửa theo đúng thuộc tính của Shared DTO
        private static RoomInfoDto ToRoomInfoDto(GameRoom room)
        {
            return new RoomInfoDto
            {
                RoomId = room.RoomId,
                RoomName = room.RoomId,
                HostName = room.Host.Username ?? room.Host.SessionId,
                GuestName = room.Player2 != null ? (room.Player2.Username ?? room.Player2.SessionId) : null,
                State = room.State == RoomState.WaitingForPlayers
                    ? Battleship.Shared.RoomState.Waiting
                    : Battleship.Shared.RoomState.Playing
            };
        }
    }
}
