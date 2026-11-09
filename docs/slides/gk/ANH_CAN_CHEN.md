# Ảnh cần chèn cho bộ slide giữa kỳ (`slide_giuaky.tex`)

Đặt ảnh PNG vào `docs/slides/images/` đúng tên file. Chưa có ảnh thì slide hiện khung xám ghi sẵn tên file. Có ảnh rồi thì tự hiện, không cần sửa code. Quy tắc chụp và các thứ **không được để lộ** (mật khẩu DB, chuỗi kết nối, secret key) xem `images/README.md`.

| Slide | File | Cần chụp gì | Ai chụp | Trạng thái |
|---|---|---|---|---|
| Giới thiệu đề tài | `gk-01-man-hinh-tran.png` | Màn hình bàn cờ 10x10 của Client (chưa có gameplay thì chụp lưới 10x10) | Dũng | Chưa có |
| Bảo mật: AES và BCrypt | `01-supabase-tables.png` | Bảng `Users` trên Supabase, cột Password bắt đầu bằng `$2a$` | Dũng | Đã có |
| Máy trạng thái | `12-state-machine.png` | Hình State Machine trong README trên GitHub | Đức | Đã có |
| Thiết kế giao diện | `04-client-login.png` | Màn hình Đăng nhập | Dũng | Đã có |
| Thiết kế giao diện | `05-client-lobby.png` | Màn hình Lobby | Dũng | Đã có |
| Thiết kế giao diện | `06-client-leaderboard.png` | Form Bảng xếp hạng có dữ liệu | Dũng | Đã có |
| Thiết kế giao diện | `gk-02-register.png` | Màn hình Đăng ký | Dũng | Chưa có |
| Thiết kế giao diện | `gk-03-roomwait.png` | Phòng chờ có 2 người chơi và nút Ready | Dũng | Chưa có |
| Thiết kế giao diện | `gk-04-board.png` | Lưới 10x10 (chụp từ Client) | Dũng | Chưa có |

Chưa có 4 ảnh `gk-*`: cần chạy Server + Client rồi chụp (`Win + Shift + S`).
