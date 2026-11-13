# VIẾT BÁO CÁO: PHẦN CỦA ĐỨC

Báo cáo viết theo **khung của cô** (6 mục). Mỗi người viết phần của mình rồi gửi Hoàng, Hoàng ghép. Viết gọn, đúng việc, **chỗ nào có ảnh/hình thì ghi rõ `[Hình X.Y: mô tả]` ngay trong bài** và gửi kèm file ảnh.

## Khung của cô và phần của Đức
| Mục | Nội dung cô yêu cầu | Đức viết | Hạn |
|---|---|---|---|
| §1 Giới thiệu | giải thuật liên quan, hiện trạng, lý do chọn, đối tượng, mục tiêu | **Giải thuật liên quan (phần Server)** | 20/11 |
| §2 Cơ sở lý thuyết | tech stack, giải thích vì sao dùng | **Bất đồng bộ, đa luồng, khóa luồng** | 20/11 |
| §3 Phân tích và thiết kế | luồng dữ liệu, CSDL, bảo mật, tính năng, vai trò | **State Machine, luồng một lượt bắn, luật chơi, thiết kế RoomManager** | 20/11 |
| §4 Hiện thực | kết quả có gì so với §3, hình ảnh | **Hiện thực Server: TcpServer, ClientSession, RoomManager, RoomHandler, Turn Engine** | 02/12 |
| §5 Kiểm thử | độ tin cậy, trường hợp thường gặp và ngoại lệ, ổn định thế nào, cải thiện gì | **Test tải, race condition, ngắt kết nối giữa trận** | 08/12 |
| §6 Kết luận | làm được gì, chưa làm được gì, hướng phát triển | 3-5 gạch cho phần Server | 08/12 |
| Tài liệu tham khảo | chuẩn IEEE | Thêm nguồn mình dùng | 08/12 |

Ghép toàn báo cáo: Hoàng, **09/12 - 13/12**.

## Viết gì ở từng mục (gợi ý, tự tìm thêm)
**§1 Giải thuật liên quan (khoảng nửa trang).** Nêu thuật toán Server dùng và ý tưởng chính: hàng đợi FIFO ghép cặp (Hoàng làm, Đức chỉ nhắc), tra bản đồ theo ô, đếm ô còn lại để biết chìm hay thắng, kiểm tra đúng lượt. Mỗi cái 1-2 câu + độ phức tạp.

**§2 Cơ sở lý thuyết (khoảng 1 trang, không kể lể).** `async/await`, Task khác Thread, race condition, `lock`, `SemaphoreSlim`, `ConcurrentDictionary`. Mỗi khái niệm: là gì, vì sao đồ án cần, dùng ở file nào.

**§3 Phân tích và thiết kế (khoảng 2 trang).**
- State Machine 5 trạng thái `RoomState` và điều kiện chuyển.
- Luồng một lượt bắn (client gửi tọa độ, Server kiểm tra đúng lượt, tra bản đồ, trả kết quả cho cả hai).
- **Luật chuyển lượt đã chốt:** trúng/chìm được bắn tiếp; hụt đổi lượt; quá 30 giây tự đổi lượt; chìm tàu cuối thì GameOver.
- Thiết kế RoomManager: vì sao `ConcurrentDictionary` ở danh sách phòng và `lock` ở từng phòng.

**§4 Hiện thực (khoảng 3 trang).** Với mỗi file: làm gì, đoạn code quan trọng (ngắn, 5-15 dòng), so với thiết kế §3 thì xong tới đâu. Phần chưa xong thì ghi rõ là chưa xong.
- `TcpServer.cs`, `ClientSession.cs` (khóa gửi `SemaphoreSlim`, đọc khung 4 byte, xử lý ngắt kết nối).
- `RoomManager.cs`, `RoomHandler.cs` (tạo, vào, rời, kick, host thoát thì giải tán phòng).
- Turn Engine: nhận Ready, PlaceShips, FireShot, GameOver.

**§5 Kiểm thử (khoảng 2 trang).** Mỗi test ghi: kịch bản, cách làm, kết quả mong đợi, kết quả thật, cải thiện gì.
- Test tải: 50, 100 client kết nối cùng lúc, đo bộ nhớ và thời gian phản hồi.
- Race condition: hai người cùng vào một phòng trống.
- Ngắt kết nối: tắt Client giữa trận, rút mạng, Server có sập không, bên còn lại có được xử thắng không.
- Lệnh bắn ngoài lượt, bắn ô đã bắn.

**§6.** Làm được gì, chưa làm được gì (ví dụ Spectator), hướng phát triển (reconnect, nhiều phòng song song, TLS).

## Ảnh và hình cần có
Đặt vào thư mục riêng, tên theo mẫu `H<mục>-<số>-<mô tả>.png`. Chữ phải đọc được, **không để lộ mật khẩu hay chuỗi kết nối**.

| Hình | Tên file gợi ý | Lấy ở đâu |
|---|---|---|
| State Machine | `H3-1-state-machine.png` | Slide `slide_giuaky_daydu.pdf`, trang **30** |
| Luồng một lượt bắn | `H3-2-luot-ban.png` | Trang **26** |
| Luật chuyển lượt | (đưa thành **Bảng 3.1**) | Trang **27** |
| Kiến trúc tổng thể | `H3-3-kien-truc.png` | Trang **23** |
| Log Server khởi động | `H4-1-server-log.png` | Chạy Server rồi chụp cửa sổ |
| Danh sách phòng | `H4-2-room-list.png` | Một Client tạo phòng, Client kia mở Join Room rồi chụp |
| Đoạn code `lock(room.RoomLock)` | `H4-3-code-lock.png` | Chụp trong Visual Studio |
| Kết quả test tải | `H5-1-test-tai.png` | Bảng số client, RAM, thời gian (tự đo) |
| Log ngắt kết nối giữa trận | `H5-2-ngat-ket-noi.png` | Chụp cửa sổ Server lúc tắt Client |

## Chuẩn trình bày
- Hình: chú thích **dưới** hình, `Hình 4.2: Danh sách phòng trên Client`. Bảng: chú thích **trên** bảng, `Bảng 3.1: Luật chuyển lượt`. Đánh số theo mục.
- Tài liệu tham khảo chuẩn **IEEE**, trích dẫn trong bài bằng `[1]`, `[2]`. Mẫu:
  - `[1] J. Postel, "Transmission Control Protocol," IETF, RFC 793, Sep. 1981.`
  - `[2] Microsoft, "Task-based asynchronous programming," Microsoft Learn. [Online]. Available: <URL>. Accessed: Nov. 2026.`
- Đưa vào **phụ lục tài liệu lưu trữ**: link repo GitHub, README, SQL, video demo. Đức gửi Hoàng link nếu có thêm tài liệu riêng.

## Lưu ý
- Chỉ viết cái **đã làm hoặc đã thiết kế rõ**. Chưa làm thì nói "chưa làm, hướng giải quyết là...". Cô hỏi sâu rất dễ lộ.
- Tên file, tên lớp viết đúng như trong code (`ClientSession`, `RoomManager`).
- Có gì chưa rõ nhắn Hoàng, đừng tự đoán.
