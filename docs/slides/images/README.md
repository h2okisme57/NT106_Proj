# ẢNH CẦN CHỤP CHO SLIDE

Mỗi ảnh có **tên file cố định**. Đặt đúng tên vào thư mục này (`docs/slides/images/`), slide tự nhận.
Chưa có ảnh thì trong slide hiện khung xám ghi sẵn tên file cần bỏ vào.

## Cách chụp
- Windows: **`Win + Shift + S`** → kéo chọn vùng → dán vào Paint → lưu **PNG** đúng tên.
- Chụp **cắt gọn**, chỉ phần cần thiết, **chữ phải đọc được** khi chiếu (đừng chụp cả màn hình 4K nhỏ xíu).
- Nên dùng **giao diện sáng**, tắt thông báo, không để lộ tab/cửa sổ không liên quan.

## ⛔ TUYỆT ĐỐI KHÔNG để lộ trong ảnh
- **Mật khẩu** cơ sở dữ liệu, **chuỗi kết nối** (`Host=...;Password=...`), **secret key** (`sb_secret_...`).
- Mật khẩu thật của bất kỳ ai. Dùng tài khoản thử như `demo01`.
- (Publishable key `sb_publishable_...` và URL project thì được phép, vì khóa này vốn công khai.)

## Danh sách ảnh

| File | Người chụp | Chụp gì | Cách chụp |
|---|---|---|---|
| `01-supabase-tables.png` | **Dũng** | Supabase **Table Editor**, bảng **Users** có vài dòng. Phải thấy cột **Password** là chuỗi bắt đầu `$2a$` | Đăng nhập Supabase (Hoàng sẽ mời Dũng vào project) → Table Editor → Users. Cần có dữ liệu: đăng ký 2–3 tài khoản trong game, hoặc chạy phần INSERT mẫu trong `docs/supabase_setup.sql` |
| `02-rest-blocked.png` | **Dũng** | Cửa sổ PowerShell, dòng **"Bị chặn đúng như mong đợi: HTTP 401"** (bước [4]) | Chạy `powershell -ExecutionPolicy Bypass -File docs\demo\demo_rest.ps1`, chụp phần cuối |
| `03-rest-json.png` | **Dũng** | Cùng cửa sổ trên, phần **bảng xếp hạng + JSON** (bước [2] và [3]) | Như trên, chụp phần đầu. Bảng xếp hạng phải **có người** (xem ghi chú dữ liệu mẫu bên dưới) |
| `04-client-login.png` | **Dũng** | Màn hình **Đăng nhập** của Client | Chạy Server + Client (xem hướng dẫn mục "Chạy offline bằng SQLite") |
| `05-client-lobby.png` | **Dũng** | Màn hình **Lobby** sau khi đăng nhập | Như trên |
| `06-client-leaderboard.png` | **Dũng** | Form **Bảng xếp hạng** có dữ liệu | Chỉ chụp được **sau khi gắn nút** vào Lobby. Chưa gắn được thì **bỏ qua**, xóa khung ảnh này trong slide |
| `07-login-lockout.png` | **Dũng** | Thông báo khi nhập sai mật khẩu 5 lần: *"Đăng nhập sai quá nhiều lần. Thử lại sau 60 giây."* | Ở Client, đăng nhập sai mật khẩu 5 lần rồi lần 6 nhập đúng. Nếu Client chỉ báo chung chung "thất bại" thì **báo Hoàng ngay** |
| `10-server-log.png` | **Hoàng** | Cửa sổ Server có dòng "Đang dùng: Supabase" và "Đang lắng nghe tại cổng 8000" | Hoàng tự chụp (cần chuỗi kết nối Supabase là bí mật nên chỉ máy Hoàng chạy được) |
| `11-room-list.png` | **Đức** | Màn hình **Join Room** ở Client (danh sách phòng) | Chạy Server (SQLite) + Client. Nếu Client chưa có nút tạo phòng nên danh sách có thể trống: chụp vẫn được, hoặc **xóa slide này** |
| `12-state-machine.png` | **Đức** | Hình **State Machine** Đức đã vẽ (đang ở README trên GitHub) | Mở README trên GitHub → chuột phải vào hình → *Lưu ảnh thành...* (hoặc kéo ảnh ra) |
| `13-wireshark-aes.png` *(tùy chọn)* | **Đức** | Wireshark bắt gói tin đăng nhập: dữ liệu là **byte lộn xộn**, không thấy chữ rõ | Cần Wireshark + Npcap Loopback (vì chạy trên 127.0.0.1). Làm được thì tốt, **không bắt buộc** |

## Dữ liệu mẫu để ảnh đẹp
Bảng xếp hạng đang **trống**. Trước khi chụp `01`, `03`, `06`, cần có vài người chơi:
- Đăng ký 2–3 tài khoản trong game (điểm đều 0), **hoặc**
- Chạy đoạn **INSERT dữ liệu mẫu** trong `docs/supabase_setup.sql` (bỏ dấu `--`) trong **Supabase → SQL Editor** để có điểm số khác nhau cho đẹp. Hoàng sẽ làm giúp bước này nếu Dũng không có quyền.
