# VIẾT BÁO CÁO: PHẦN CỦA DŨNG

Báo cáo viết theo **khung của cô** (6 mục). Mỗi người viết phần của mình rồi gửi Hoàng, Hoàng ghép. Viết gọn, đúng việc, **chỗ nào có ảnh/hình thì ghi rõ `[Hình X.Y: mô tả]` ngay trong bài** và gửi kèm file ảnh.

## Khung của cô và phần của Dũng
| Mục | Nội dung cô yêu cầu | Dũng viết | Hạn |
|---|---|---|---|
| Phần đầu | danh mục hình ảnh, bảng biểu, từ viết tắt | **Làm cả ba danh mục** (lập sau khi cả nhóm viết xong) | 13/12 |
| §1 Giới thiệu | giải thuật, hiện trạng, lý do chọn, đối tượng, mục tiêu | **Đối tượng áp dụng và phạm vi** | 20/11 |
| §2 Cơ sở lý thuyết | tech stack, giải thích vì sao dùng | **WinForms, GDI+, EF Core, SQLite/PostgreSQL** | 20/11 |
| §3 Phân tích và thiết kế | luồng dữ liệu, CSDL, bảo mật, tính năng, vai trò | **Use case và vai trò, ERD cơ sở dữ liệu, thiết kế giao diện** | 20/11 |
| §4 Hiện thực | kết quả có gì so với §3, hình ảnh | **Hiện thực giao diện từng màn hình, CSDL, bảng xếp hạng, sương mù** | 02/12 |
| §5 Kiểm thử | độ tin cậy, trường hợp thường gặp và ngoại lệ, ổn định, cải thiện | **Test giao diện và test bảo mật bằng Wireshark** | 08/12 |
| §6 Kết luận | làm được gì, chưa làm được gì, hướng phát triển | 3-5 gạch cho phần giao diện và CSDL | 08/12 |
| Tài liệu tham khảo | chuẩn IEEE | Thêm nguồn mình dùng | 08/12 |

Ghép toàn báo cáo: Hoàng, **09/12 - 13/12**.

## Viết gì ở từng mục (gợi ý, tự tìm thêm)
**§1 Đối tượng và phạm vi (nửa trang).** Ai dùng (người chơi, người học lập trình mạng, người vận hành). Trong phạm vi: đăng ký, đăng nhập, phòng, ghép trận, chat, chơi trận, bảng xếp hạng. Ngoài phạm vi: Spectator, AI, di động.

**§2 Cơ sở lý thuyết (khoảng 1 trang, không kể lể).**
- **WinForms + GDI+:** vì sao chọn, vẽ bàn cờ thế nào, vì sao phải `Invoke` khi cập nhật giao diện từ luồng mạng (lỗi cross-thread).
- **EF Core (Code-First):** là gì, vì sao dùng, chống SQL injection ra sao, đổi SQLite sang PostgreSQL chỉ đổi provider.
- **BCrypt:** băm một chiều, có salt, vì sao chậm là tốt.

**§3 Phân tích và thiết kế (khoảng 2 trang).**
- **Vai trò và phân quyền:** Người chơi, Chủ phòng (có quyền kick), Người vận hành. Hệ thống không có tài khoản admin.
- **Use case:** các chức năng của người chơi.
- **ERD:** bảng `Users`, `Matches`, `MatchDetails`, khóa chính, khóa ngoại, quan hệ 1-N. View `leaderboard` chỉ lấy tên, thắng, thua, điểm.
- **Thiết kế giao diện:** các màn hình và luồng chuyển giữa chúng.

**§4 Hiện thực (khoảng 3 trang).** Mỗi màn hình: làm gì, nối với gói tin nào, so với thiết kế §3 xong tới đâu.
- Đăng nhập (khóa 60 giây có đếm ngược), Đăng ký, Lobby (chat), Profile, Phòng chờ, Bàn cờ (xếp tàu, kiểm tra hợp lệ), Bảng xếp hạng.
- `AppDbContext`, các model, `PasswordHelper`, Supabase (tạo bảng, view, RLS).
- Sương mù và đổi màu ô bắn (khi xong).

**§5 Kiểm thử (khoảng 2 trang).** Mỗi test ghi: kịch bản, cách làm, kết quả mong đợi, kết quả thật.
- Bảng test case giao diện: nhập thiếu, mật khẩu không khớp, tên trùng, sai mật khẩu nhiều lần, thoát phòng, vào phòng đầy.
- **Wireshark:** bắt gói tin đăng nhập, chứng minh nội dung đã mã hóa (không thấy mật khẩu rõ). Dùng Npcap Loopback vì chạy trên `127.0.0.1`.
- Trong CSDL mật khẩu chỉ là chuỗi `$2a$11$...`.

**§6.** Làm được gì, chưa làm được gì, hướng phát triển (giao diện đẹp hơn, bản di động, mã hóa tốt hơn).

## Ảnh và hình cần có
Đặt vào thư mục riêng, tên theo mẫu `H<mục>-<số>-<mô tả>.png`. Chữ phải đọc được, **không để lộ mật khẩu hay chuỗi kết nối**. Đã có sẵn một số ảnh trong `docs/slides/images/`, dùng lại được.

| Hình | Tên file gợi ý | Lấy ở đâu |
|---|---|---|
| Use case / chức năng | `H3-1-use-case.png` | Slide `slide_giuaky_daydu.pdf`, trang **19** |
| Vai trò và phân quyền | (đưa thành **Bảng 3.1**) | Trang **20** |
| ERD | `H3-2-erd.png` | Trang **31** |
| Thiết kế giao diện | `H3-3-man-hinh.png` | Trang **33** |
| Màn hình Đăng nhập | `H4-1-login.png` | `docs/slides/images/04-client-login.png` |
| Màn hình Đăng ký | `H4-2-register.png` | `docs/slides/images/gk-02-register.png` |
| Màn hình Lobby | `H4-3-lobby.png` | `docs/slides/images/05-client-lobby.png` |
| Phòng chờ | `H4-4-roomwait.png` | `docs/slides/images/gk-03-roomwait.png` |
| Bàn cờ xếp tàu | `H4-5-board.png` | `docs/slides/images/gk-04-board.png` và `gk-01-man-hinh-tran.png` |
| Bảng xếp hạng | `H4-6-leaderboard.png` | `docs/slides/images/06-client-leaderboard.png` |
| Khóa đăng nhập 60 giây | `H5-1-lockout.png` | `docs/slides/images/07-login-lockout.png` |
| Bảng `Users` trên Supabase (cột Password `$2a$`) | `H4-7-users-bcrypt.png` | `docs/slides/images/01-supabase-tables.png` |
| REST bị chặn khi đọc bảng Users | `H4-8-rest-blocked.png` | `docs/slides/images/02-rest-blocked.png` |
| Wireshark: payload đã mã hóa | `H5-2-wireshark.png` | **Tự chụp** (Wireshark + Npcap Loopback) |
| Bảng test case giao diện | (đưa thành **Bảng 5.1**) | Tự lập |

## Chuẩn trình bày
- Hình: chú thích **dưới** hình, `Hình 4.3: Màn hình Lobby`. Bảng: chú thích **trên** bảng, `Bảng 5.1: Test case giao diện`. Đánh số theo mục.
- Tài liệu tham khảo chuẩn **IEEE**, trích dẫn trong bài bằng `[1]`, `[2]`. Mẫu:
  - `[1] Microsoft, "Entity Framework Core," Microsoft Learn. [Online]. Available: https://learn.microsoft.com/en-us/ef/core/. Accessed: Nov. 2026.`
  - `[2] N. Provos and D. Mazières, "A future-adaptable password scheme," in Proc. USENIX Annu. Tech. Conf. (FREENIX Track), Monterey, CA, USA, Jun. 1999, pp. 81-91.`
- Đưa vào **phụ lục tài liệu lưu trữ**: link repo GitHub, README, SQL (`docs/supabase_setup.sql`), video demo. Dũng gửi Hoàng link nếu có thêm tài liệu riêng.
- **Danh mục hình ảnh, bảng biểu, từ viết tắt:** lập **sau cùng**, khi cả nhóm đã đánh số xong hình và bảng. Từ viết tắt có sẵn một danh sách trong slide đầy đủ (trang 3) để tham khảo.

## Lưu ý
- Chỉ viết cái **đã làm hoặc đã thiết kế rõ**. Chưa làm thì nói "chưa làm, hướng giải quyết là...".
- Tên file, tên lớp viết đúng như trong code (`ucLogin`, `AppDbContext`).
- Không đưa **mật khẩu, chuỗi kết nối, secret key** vào báo cáo hay ảnh.
- Có gì chưa rõ nhắn Hoàng, đừng tự đoán.
