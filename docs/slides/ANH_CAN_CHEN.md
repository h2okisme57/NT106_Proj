# Ảnh cần chèn cho bộ slide giữa kỳ (`slide_giuaky.tex`)

Đặt ảnh PNG vào `docs/slides/images/` đúng tên file. Chưa có ảnh thì slide hiện khung xám ghi sẵn tên file. Có ảnh rồi thì tự hiện, không cần sửa code. Quy tắc chụp và các thứ **không được để lộ** (mật khẩu DB, chuỗi kết nối, secret key) xem `images/README.md`.

| Slide | File | Cần chụp gì | Ai chụp | Trạng thái |
|---|---|---|---|---|
| Giới thiệu đề tài | `gk-01-man-hinh-tran.png` | Màn hình bàn cờ 10x10 của Client (chưa có gameplay thì chụp lưới 10x10) | Dũng | Đã có |
| Bảo mật: AES và BCrypt | `01-supabase-tables.png` | Bảng `Users` trên Supabase, cột Password bắt đầu bằng `$2a$` | Dũng | Đã có |
| Máy trạng thái | `12-state-machine.png` | Hình State Machine trong README trên GitHub | Đức | Đã có |
| Thiết kế giao diện | `04-client-login.png` | Màn hình Đăng nhập | Dũng | Đã có |
| Thiết kế giao diện | `05-client-lobby.png` | Màn hình Lobby | Dũng | Đã có |
| Thiết kế giao diện | `06-client-leaderboard.png` | Form Bảng xếp hạng có dữ liệu | Dũng | Đã có |
| Thiết kế giao diện | `gk-02-register.png` | Màn hình Đăng ký | Dũng | Đã có |
| Thiết kế giao diện | `gk-03-roomwait.png` | Phòng chờ có 2 người chơi và nút Ready | Dũng | Đã có |
| Thiết kế giao diện | `gk-04-board.png` | Lưới 10x10 (chụp từ Client) | Dũng | Đã có |
| Kịch bản demo | `10-server-log.png` | Cửa sổ Server có dòng "Đang dùng: Supabase", "Đang lắng nghe tại cổng 8000" và "worker ghép cặp" | Hoàng | Cần chụp |
| Quản lý phòng đấu | `11-room-list.png` | Màn hình Join Room có ít nhất một phòng (một Client tạo phòng, Client kia mở Join Room rồi chụp) | Dũng hoặc Hoàng | Cần chụp |
| Giao diện Chat trận | `13-chat-in-match.png` | Màn hình trận đấu mở khung chat (bấm nút Chat để hiện box nhập và tin nhắn) | Dũng | Cần chụp |
| Quên mật khẩu OTP | `14-forgot-password.png` | Giao diện Quên mật khẩu (ucForgotPassword) nhập username và ô nhập mã OTP | Dũng | Cần chụp |
| Bàn cờ đặt tàu 10x10 | `15-ship-placement.png` | Bàn cờ đang xếp 5 loại tàu (Carrier, Battleship, Cruiser, Submarine, Destroyer) | Dũng | Cần chụp |

### Hướng dẫn chụp ảnh minh họa:
1. **Dùng phím tắt**: `Windows + Shift + S` để quét vùng cửa sổ rõ nét.
2. **Lưu định dạng**: PNG, đúng tên file ở cột `File` ở trên và chép vào thư mục `docs/slides/images/`.
3. **Tuyệt đối không để lộ**: Mật khẩu DB Supabase, chuỗi kết nối (`SUPABASE_DB_CONNECTION`), secret key trong code.
