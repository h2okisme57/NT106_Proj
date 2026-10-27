# GIAO VIỆC CHO BUỔI BÁO CÁO THỨ SÁU 30/10

> Cập nhật 25/10/2026 · **Hoàng là core thuyết trình.** File này **thay** các mốc ngày trong `KeHoach_Demo_Supabase.md` (bản đó giả định 01–03/11). Chi tiết kỹ thuật và kịch bản demo vẫn xem file đó.

## Tình trạng hiện tại
**Hoàng đã xong:** tạo Supabase, đổi DB bằng biến môi trường (`BATTLESHIP_DB`), bảng đã tạo trên Supabase và đã khoá RLS, REST `leaderboard` đã gọi được (kiểm chứng thật), slide 14 trang đã soạn.

**`main` đang KHÔNG build được, chặn cả nhóm:**
| Lỗi | Của ai | Vị trí |
|---|---|---|
| `RoomHandler` implement sai interface, dùng type không tồn tại | **Đức** | `Battleship.Server/Handlers/RoomHandler.cs` |
| Client dùng `PacketType.GetRoomsRequest/GetRoomsResponse` không có | **Dũng** | `Battleship.Client/joinRoom.cs` dòng 57 và 71 |

## Mốc thời gian (gấp)
| Ngày | Việc |
|---|---|
| **25–26/10** | Đức và Dũng **sửa 2 lỗi build ở trên**. Hoàng hoàn tất PR bảng xếp hạng |
| **27/10** | Hoàng merge theo thứ tự (bên dưới), `main` phải **xanh**, Hoàng chạy thử cả hệ thống với Supabase |
| **28/10** | Đức và Dũng gửi slide phần mình; Dũng gắn nút Bảng xếp hạng. **Đóng băng code tối 28/10** |
| **29/10** | Tập dượt toàn bộ (Hoàng nói, Đức/Dũng thao tác), **Đức quay video dự phòng** |
| **30/10** | Báo cáo |

## Thứ tự merge (Hoàng làm)
1. `hoang/supabase-db-switch` (sửa mô hình dữ liệu, đổi DB)
2. Bản sửa của Đức (`RoomHandler`)
3. Bản sửa của Dũng (`joinRoom.cs`)
4. `hoang/leaderboard-rest`
5. (Tuỳ chọn) `hoang/matchmaking-queue`, `hoang/aes-encryption`: chỉ merge nếu main đã xanh từ 27/10, vì AES đổi định dạng gói tin, ai chưa pull sẽ không kết nối được

---

## ĐỨC

| # | Việc | Hạn | Xong khi |
|---|---|---|---|
| Đ1 | **Sửa `RoomHandler` cho Server build được** (GẤP) | **26/10** | `dotnet build Battleship.Server` ra **0 error** |
| Đ2 | Soạn **2–3 slide "Kiến trúc server và game"** (RoomManager, State Machine, luồng gói tin) | 28/10 | gửi Hoàng (file hoặc ảnh) |
| Đ3 | **Quay video demo dự phòng** toàn bộ kịch bản + chuẩn bị máy thứ 2 | 29/10 | có file video |

**Gợi ý làm Đ1** (mất khoảng 30 phút nếu bám hạ tầng có sẵn):
- Hàm phải tên **`HandleAsync(ClientSession session, Packet packet)`** (không phải `HandlePacketAsync`).
- Constructor phải là `RoomHandler(RoomManager roomManager, PacketRouter router)` vì `TcpServer` gọi hai tham số.
- Dùng `packet.DeserializePayload<T>()` và `Packet.Create(type, dto)` thay vì `packet.JsonPayload`.
- Các DTO đã có sẵn trong `Battleship.Shared/Dtos/RoomDtos.cs` (`CreateRoomDto`, `JoinRoomDto`, `LeaveRoomDto`, `RoomInfoDto`, `RoomUpdateDto`). Cần DTO mới thì **thêm vào Shared**, không tự đặt trong Server.
- Các `PacketType` có sẵn: `CreateRoomRequest`, `JoinRoomRequest`, `LeaveRoom`, `RoomUpdate`. Cần cái mới thì **thêm vào cuối enum**.
- **Nhớ `dotnet build` cả solution trước khi mở PR.** Lần trước PR #14 merge mà không build nên làm vỡ main.

Nếu Đ1 không kịp thì **demo bỏ phần chơi trận**, chỉ demo đăng nhập + bảng xếp hạng. Báo Hoàng sớm để đổi kịch bản.

---

## DŨNG

| # | Việc | Hạn | Xong khi |
|---|---|---|---|
| D1 | **Sửa lỗi build Client** (GẤP): `joinRoom.cs` | **26/10** | `dotnet build Battleship.Client` ra **0 error** |
| D2 | Sau khi Hoàng merge PR đầu: **pull, kiểm tra đăng ký/đăng nhập** vẫn chạy (Hoàng đã sửa `User.Id` và khoá ngoại sang `string`) | 27/10 | báo Hoàng kết quả |
| D3 | Thêm **nút "Bảng xếp hạng" ở Lobby**, bấm thì mở `new frmLeaderboard().ShowDialog();` (form do Hoàng viết, có trong PR `hoang/leaderboard-rest`) | 28/10 | bấm nút ra danh sách |
| D4 | Soạn **2 slide "Cơ sở dữ liệu"**: ERD 3 bảng (`Users`, `Matches`, `MatchDetails`) và vì sao bật RLS | 28/10 | gửi Hoàng |

**Gợi ý làm D1:** `Shared` đã có sẵn **`PacketType.GetRoomListRequest`** và **`GetRoomListResponse`**. Cách nhanh nhất là đổi `GetRoomsRequest` thành `GetRoomListRequest` và `GetRoomsResponse` thành `GetRoomListResponse` trong `joinRoom.cs`. Đừng tạo enum mới. Nếu thật sự cần thêm thì **thêm vào cuối** `PacketType`.

**Lưu ý D2:** `User.Id` là `string` (tự sinh Guid), khoá ngoại `Match.Player1Id`... cũng là `string`. **Đừng sửa lại về `int`**, và đừng sửa `Models/*` hay `AppDbContext.cs` trong lúc chờ PR của Hoàng merge.

---

## HOÀNG (core thuyết trình)

| # | Việc | Hạn | Trạng thái |
|---|---|---|---|
| H1 | Supabase project | | ✅ xong |
| H2 | Đổi DB SQLite ⇄ Supabase, tạo bảng, khoá RLS | | ✅ xong, đã test thật |
| H3 | Service REST + form Bảng xếp hạng | 27/10 | 🟡 code xong, chưa chạy test, chưa commit |
| H4 | Slide chính | 29/10 | 🟡 14 trang đã có, chờ ghép slide của Đức và Dũng và chụp màn hình demo thật |
| H5 | Merge theo thứ tự, smoke test cả hệ thống | 27/10 | chờ Đ1 và D1 |
| H6 | **Thuyết trình và demo live** | 30/10 | |

## Quy tắc (rút từ các lần trước)
1. **`dotnet build` cả solution trước khi mở PR.** Main đang vỡ vì vi phạm điều này.
2. Chỉ **thêm vào cuối** `PacketType` và DTO, không đổi kiểu dữ liệu đang dùng.
3. Conflict ở `.csproj` thì **luôn "Accept both"**.
4. **Không commit** mật khẩu DB, chuỗi kết nối, `service_role`/secret key.
5. Có vấn đề gì chặn thì **nhắn Hoàng ngay**, đừng đợi đến cuối ngày.
