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
                case PacketType.KickPlayerRequest:
                    await HandleKickPlayerAsync(session, packet);
                    break;
                case PacketType.PlaceShips:
                    await HandlePlaceShipsAsync(session, packet);
                    break;
                case PacketType.PlayerReady:
                    await HandlePlayerReadyAsync(session, packet);
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
                await session.SendAsync(Packet.Create(PacketType.JoinRoomResponse, new JoinRoomResponseDto
                {
                    Success = false,
                    Message = "Thiếu mã phòng."
                }));
                return;
            }

            bool joined = _roomManager.JoinRoom(dto.RoomId, session);
            var room = joined ? _roomManager.GetRoom(dto.RoomId) : null;
            var updateDto = new RoomUpdateDto
            {
                Room = room != null ? ToRoomInfoDto(room) : null
            };

            // Trả JoinRoomResponse về cho Client vừa bấm Join.
            // Phải đúng kiểu JoinRoomResponseDto (Success, Message, Room): Client đọc cờ Success,
            // gửi RoomUpdateDto (chỉ có Room) thì Client hiểu là thất bại và hiện hộp lỗi rỗng.
            var response = new JoinRoomResponseDto
            {
                Success = joined && room != null,
                Message = (joined && room != null) ? string.Empty : "Không vào được phòng: phòng không tồn tại hoặc đã đầy.",
                Room = updateDto.Room
            };
            await session.SendAsync(Packet.Create(PacketType.JoinRoomResponse, response));

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

        // 5. Kick người chơi khỏi phòng (chỉ dành cho Host)
        private async Task HandleKickPlayerAsync(ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<KickPlayerDto>();
            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId)) return;

            var room = _roomManager.GetRoom(dto.RoomId);
            if (room == null || room.Player2 == null) return;

            // Lưu lại tham chiếu Player2 trước khi bị xóa khỏi phòng
            var kickedPlayer = room.Player2;

            // RoomManager.KickPlayer đã xác thực chỉ Host mới được quyền Kick
            bool kicked = _roomManager.KickPlayer(dto.RoomId, session, kickedPlayer);
            if (!kicked) return;

            // 1. Báo cho người bị kick để thoát về Lobby
            await kickedPlayer.SendAsync(Packet.Create(PacketType.KickedFromRoom, new RoomUpdateDto { Room = null! }));

            // 2. Cập nhật lại phòng chờ (chỉ còn 1 người) cho Host
            var hostUpdateDto = new RoomUpdateDto { Room = ToRoomInfoDto(room) };
            await room.Host.SendAsync(Packet.Create(PacketType.RoomUpdate, hostUpdateDto));
        }

        // 6. Nhận vị trí đặt tàu và kiểm tra hợp lệ trên Server (Mục 4.2)
        private async Task HandlePlaceShipsAsync(ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<PlaceShipsDto>();
            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId)) return;

            var room = _roomManager.GetRoom(dto.RoomId);
            if (room == null) return;

            bool placedOk = false;
            string errorMsg = string.Empty;

            lock (room.RoomLock)
            {
                var board = room.GetBoard(session);
                if (board != null && (room.State == RoomState.PlacementPhase || room.State == RoomState.ReadyCheck))
                {
                    placedOk = board.TryPlaceShips(dto.Ships, out errorMsg);
                }
            }

            if (!placedOk)
            {
                Console.WriteLine($"[TurnEngine] Client {session.SessionId} đặt tàu lỗi: {errorMsg}");
            }
        }

        // 7. Xử lý người chơi bấm Ready & Bắt đầu trận khi đủ 2 người sẵn sàng (Mục 4.2)
        private async Task HandlePlayerReadyAsync(ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<PlayerReadyDto>();
            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId)) return;

            var room = _roomManager.GetRoom(dto.RoomId);
            if (room == null) return;

            bool startMatch = false;
            string firstTurnUsername = string.Empty;

            lock (room.RoomLock)
            {
                if (!room.IsFull || room.Player2 == null) return;

                var board = room.GetBoard(session);
                if (board == null || board.Ships.Count == 0)
                {
                    Console.WriteLine($"[TurnEngine] Client {session.SessionId} bấm Ready nhưng chưa đặt tàu hợp lệ.");
                    return;
                }

                board.IsReady = dto.IsReady;
                room.State = RoomState.ReadyCheck;

                // Khi cả Host và Player2 đều đã đặt tàu hợp lệ và bấm Ready -> Bắt đầu trận!
                if (room.HostBoard.IsReady && room.Player2Board.IsReady)
                {
                    room.State = RoomState.TurnBasedPlay;
                    room.CurrentTurnSessionId = room.Host.SessionId; // Chủ phòng đi lượt đầu tiên
                    firstTurnUsername = room.Host.Username ?? room.Host.SessionId;
                    startMatch = true;
                }
            }

            // Phát thông báo bắt đầu trận (TurnChanged) cho cả 2 Client
            if (startMatch && room.Player2 != null)
            {
                var turnDto = new TurnChangedDto
                {
                    RoomId = room.RoomId,
                    CurrentTurnUsername = firstTurnUsername,
                    TimeLimitSeconds = 30,
                    Reason = "GameStart"
                };
                var startPacket = Packet.Create(PacketType.TurnChanged, turnDto);
                await room.Host.SendAsync(startPacket);
                await room.Player2.SendAsync(startPacket);
                Console.WriteLine($"[TurnEngine] Phòng {room.RoomId} bắt đầu trận đấu! Lượt đầu: {firstTurnUsername}");
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