using System;
using System.Linq;
using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server.Handlers
{
    /// <summary>
    /// Lớp xử lý các gói tin (Packet) liên quan đến quản lý phòng chơi (Lobby, Tạo, Vào, Rời phòng).
    /// Hiện thực interface IPacketHandler để tích hợp vào hệ thống định tuyến gói tin của Server.
    /// </summary>
    public class RoomHandler : IPacketHandler
    {
        private readonly RoomManager _roomManager; // Quản lý vòng đời và danh sách phòng chơi toàn cục
        private readonly PacketRouter _router;     // Điều hướng gói tin nội bộ (giữ lại để khớp 100% với TcpServer.cs)

        /// <summary>
        /// Khởi tạo RoomHandler. Khớp 100% với cách khởi tạo `new RoomHandler(_roomManager, _router)` bên TcpServer.cs.
        /// </summary>
        public RoomHandler(RoomManager roomManager, PacketRouter router)
        {
            _roomManager = roomManager;
            _router = router;
        }

        /// <summary>
        /// Hàm xử lý chính (Entry Point) nhận gói tin từ ClientSession và phân phối đến các hàm logic tương ứng dựa trên loại gói tin.
        /// </summary>
        public async Task HandleAsync(ClientSession session, Packet packet)
        {
            switch (packet.Type)
            {
                // Yêu cầu lấy danh sách phòng đang mở
                case PacketType.GetRoomListRequest:
                    await HandleGetRoomListAsync(session);
                    break;

                // Yêu cầu tạo phòng chơi mới
                case PacketType.CreateRoomRequest:
                    await HandleCreateRoomAsync(session, packet);
                    break;

                // Yêu cầu tham gia vào một phòng hiện có
                case PacketType.JoinRoomRequest:
                    await HandleJoinRoomAsync(session, packet);
                    break;

                // Yêu cầu chủ động rời phòng chơi
                case PacketType.LeaveRoom:
                    await HandleLeaveRoomAsync(session, packet);
                    break;

                // Trường hợp nhận loại gói tin không thuộc phạm vi xử lý của RoomHandler
                default:
                    Console.WriteLine($"[RoomHandler] Không hỗ trợ loại gói tin: {packet.Type}");
                    break;
            }
        }

        /// <summary>
        /// Lấy toàn bộ danh sách phòng hiện tại, chuyển đổi sang dạng DTO thu gọn và gửi phản hồi cho Client yêu cầu.
        /// </summary>
        private async Task HandleGetRoomListAsync(ClientSession session)
        {
            // Lấy danh sách gốc từ RoomManager, dùng LINQ chuyển đổi (Map) từng GameRoom thành RoomInfoDto
            var roomList = _roomManager.GetAllRooms()
                .Select(ToRoomInfoDto)
                .ToList();

            // Đóng gói danh sách phòng vào gói tin phản hồi DTO
            var responseDto = new RoomListResponseDto
            {
                Rooms = roomList
            };

            // Gửi bất đồng bộ gói tin phản hồi GetRoomListResponse về lại duy nhất Client đã yêu cầu
            await session.SendAsync(Packet.Create(PacketType.GetRoomListResponse, responseDto));
        }

        /// <summary>
        /// Xử lý yêu cầu tạo phòng. Nếu Client không gửi mã phòng cụ thể, Server sẽ tự sinh mã ngẫu nhiên 6 ký tự.
        /// </summary>
        private async Task HandleCreateRoomAsync(ClientSession session, Packet packet)
        {
            // Giải mã dữ liệu (Payload) đi kèm gói tin để lấy thông tin tạo phòng
            var dto = packet.DeserializePayload<CreateRoomDto>();

            // Logic sinh mã phòng: Nếu Client truyền mã hợp lệ thì dùng, ngược lại sinh chuỗi GUID rút gọn (Ví dụ: "A1B2C3")
            string roomId = (dto != null && !string.IsNullOrWhiteSpace(dto.RoomId))
                ? dto.RoomId
                : Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            // Gọi xuống tầng nghiệp vụ RoomManager để đăng ký tạo phòng mới, Client gửi yêu cầu sẽ đóng vai trò Host
            var room = _roomManager.CreateRoom(roomId, session);

            // Chuẩn bị dữ liệu cập nhật phòng (Khởi tạo Room mới trống thay vì gán null để tránh crash ở Client)
            var updateDto = new RoomUpdateDto
            {
                Room = room != null ? ToRoomInfoDto(room) : new RoomInfoDto()
            };

            // Phản hồi kết quả tạo phòng về cho chính Client (Host) để họ chuyển màn hình vào phòng chờ
            await session.SendAsync(Packet.Create(PacketType.RoomUpdate, updateDto));
        }

        /// <summary>
        /// Xử lý yêu cầu vào phòng của người chơi thứ 2 (Guest). Thông báo trạng thái mới cho cả Guest lẫn Host.
        /// </summary>
        private async Task HandleJoinRoomAsync(ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<JoinRoomDto>();

            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId))
            {
                await session.SendAsync(Packet.Create(PacketType.JoinRoomResponse, new RoomUpdateDto { Room = new RoomInfoDto() }));
                return;
            }

            // Thực hiện hành động vào phòng trong RoomManager (Đã được bọc cơ chế thread-safe)
            bool joined = _roomManager.JoinRoom(dto.RoomId, session);
            var room = joined ? _roomManager.GetRoom(dto.RoomId) : null;

            // Đóng gói thông tin phòng hiện tại sau khi xử lý Join
            var updateDto = new RoomUpdateDto
            {
                Room = room != null ? ToRoomInfoDto(room) : new RoomInfoDto()
            };

            // Bước 1: Phản hồi kết quả trực tiếp cho Client vừa gửi yêu cầu JoinRoomRequest (Chấp nhận hoặc Từ chối)
            await session.SendAsync(Packet.Create(PacketType.JoinRoomResponse, updateDto));

            // Bước 2: Nếu vào phòng thành công, gửi một gói tin cập nhật (Broadcast ngầm) để thông báo cho Host biết đối thủ đã xuất hiện
            if (joined && room != null)
            {
                await room.Host.SendAsync(Packet.Create(PacketType.RoomUpdate, updateDto));
            }
        }

        /// <summary>
        /// Xử lý yêu cầu rời phòng chơi. Giải tán phòng nếu Host thoát, hoặc giải phóng slot nếu Player 2 thoát.
        /// </summary>
        private async Task HandleLeaveRoomAsync(ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<LeaveRoomDto>();
            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId)) return;

            var room = _roomManager.GetRoom(dto.RoomId);
            ClientSession? remainingPlayer = null;

            // Xác định xem ai là người ở lại trước khi tiến hành xóa/sửa phòng trong bộ nhớ
            if (room != null)
            {
                // Nếu người thoát là Host -> Người còn lại (nếu có) là Player2. Ngược lại, người còn lại là Host.
                remainingPlayer = (room.Host.SessionId == session.SessionId) ? room.Player2 : room.Host;
            }

            // Gọi RoomManager xử lý xóa phòng hoặc xóa người chơi thứ 2
            bool left = _roomManager.LeaveRoom(dto.RoomId, session);
            if (!left) return; // Nếu xử lý rời phòng thất bại (sai ID phòng/session) thì dừng lại

            // Lấy lại thông tin phòng sau khi cập nhật để kiểm tra xem phòng đó còn tồn tại hay đã bị xóa (giải tán)
            var updatedRoom = _roomManager.GetRoom(dto.RoomId);

            // Đóng gói thông tin
            var updateDto = new RoomUpdateDto
            {
                Room = updatedRoom != null ? ToRoomInfoDto(updatedRoom) : new RoomInfoDto()
            };

            // Nếu trong phòng từng có người thứ 2 và họ vẫn đang trực tuyến, gửi cập nhật trạng thái mới cho họ
            if (remainingPlayer != null)
            {
                // - Nếu Host thoát: RoomInfoDto là object trống -> Client còn lại hiểu là phòng đã sập và tự động văng về Lobby.
                // - Nếu Player2 thoát: RoomInfoDto chứa thông tin Host -> Host nhận được và biết Player2 đã rời ghế.
                await remainingPlayer.SendAsync(Packet.Create(PacketType.RoomUpdate, updateDto));
            }
        }

        /// <summary>
        /// Hàm trợ giúp lấy tên hiển thị chuẩn xác (Username nếu đã đăng nhập, ngược lại dùng SessionId).
        /// </summary>
        private static string GetDisplayName(ClientSession? client)
        {
            if (client == null) return "Unknown";
            return (client.IsLoggedIn && !string.IsNullOrEmpty(client.Username))
                ? client.Username
                : client.SessionId;
        }

        /// <summary>
        /// Hàm trợ giúp chuyển đổi từ thực thể GameRoom (Server) sang đối tượng dữ liệu RoomInfoDto (Shared).
        /// Khớp chính xác hoàn toàn với các thuộc tính HostName, GuestName, State trong file Shared.
        /// Được bọc trong lock để đảm bảo thread-safe (tránh xung đột dữ liệu đa luồng).
        /// </summary>
        private static RoomInfoDto ToRoomInfoDto(GameRoom room)
        {
            lock (room.RoomLock)
            {
                // Đồng bộ và chuyển đổi State từ Server sang Shared
                Shared.RoomState sharedState = Shared.RoomState.Waiting;
                if (room.State == Server.RoomState.TurnBasedPlay)
                {
                    sharedState = Shared.RoomState.Playing;
                }
                else if (room.State == Server.RoomState.GameOver)
                {
                    sharedState = Shared.RoomState.Finished;
                }

                string hostDisplayName = GetDisplayName(room.Host);

                return new RoomInfoDto
                {
                    RoomId = room.RoomId,
                    RoomName = $"Phòng của {hostDisplayName}",
                    HostName = hostDisplayName,
                    GuestName = room.Player2 != null ? GetDisplayName(room.Player2) : null,
                    State = sharedState
                };
            }
        }
    }
}
