# KẾ HOẠCH DEMO TUẦN SAU: KẾT NỐI SUPABASE + WEB SERVICE

> **Cập nhật:** 25/10/2026 · **Người lập:** Hoàng (core thuyết trình)
> **Yêu cầu:** thuyết trình có demo kết nối **Firebase/Supabase + web services**, chứng minh **"móc được dữ liệu"** từ cloud.
> **Giả định:** buổi thuyết trình vào khoảng **01–03/11**. Nếu ngày thật khác, dời mốc ở Mục 5 theo tỷ lệ.

---

## 1. Quyết định chốt

| Câu hỏi | Quyết định | Lý do |
|---|---|---|
| Chọn Firebase hay Supabase? | **Supabase** | Supabase là **PostgreSQL**, nên EF Core hiện tại gần như giữ nguyên (đổi provider `UseSqlite` → `UseNpgsql`). Firebase là NoSQL, **không có EF Core provider**, phải viết lại toàn bộ lớp dữ liệu (Users/Matches/MatchDetails), tốn thời gian và dễ vỡ. |
| Có quay lại SQLite không? | **Có, giữ làm dự phòng** | Đề gốc ghi "SQL Server **hoặc SQLite**" nên SQLite vẫn hợp lệ. Mạng trường chập chờn (rủi ro đã ghi trong Roadmap 6.4) thì chuyển về SQLite bằng **1 biến cấu hình**, không sửa code, không build lại. |
| Web service là gì? | **REST API tự sinh của Supabase (PostgREST)** | Mỗi bảng/view trong Supabase tự có endpoint `https://<ref>.supabase.co/rest/v1/<tên>`. Client C# gọi bằng `HttpClient` để lấy **Bảng xếp hạng**. Không cần tự dựng thêm máy chủ web. |
| Có bỏ TCP Server không? | **Không** | TCP/Socket là lõi môn NT106 (framing, AES, handshake...). Cloud DB chỉ thay chỗ **lưu trữ**, không thay giao thức game. |

> Không hỏi được giảng viên thì chọn hướng **giữ nguyên lõi + thêm lớp cloud** như trên: đáp ứng được cả hai cách hiểu của đề ("kết nối cloud DB" và "dùng web service") mà không phá phần đã làm.

---

## 2. "Móc được dữ liệu" nghĩa là chứng minh 3 điều (đây là kịch bản demo)

1. **Ghi:** đăng ký tài khoản trong game → mở **Supabase Table Editor** thấy dòng user mới (mật khẩu là chuỗi **BCrypt**, không phải chữ rõ).
2. **Đọc qua web service:** mở trình duyệt/Postman gọi `GET .../rest/v1/leaderboard` → trả **JSON** bảng xếp hạng.
3. **Đọc trong app:** form **Bảng xếp hạng** trong Client lấy dữ liệu qua REST và hiển thị lên `ListView`.

Nếu chỉ kịp 2 điều thì ưu tiên **1 và 2**.

---

## 3. Kiến trúc sau khi thêm lớp cloud

```
 Client WinForms ──(TCP + AES-128, framing 4-byte)──► TCP Server (C#)
        │                                                  │
        │                                       EF Core + Npgsql (hoặc SQLite)
        │                                                  ▼
        └──(HTTPS, REST, apikey)──►  Supabase  ◄───  PostgreSQL (Users, Matches, MatchDetails)
                  Leaderboard                       view "leaderboard" (chỉ cột công khai)
```

- **Đường game** (đăng nhập, phòng, trận) vẫn đi qua **TCP Server**; Server ghi/đọc DB qua EF Core.
- **Đường web service** (bảng xếp hạng) đi thẳng từ Client tới **REST của Supabase**, chỉ đọc view `leaderboard`.

### Chuyển DB bằng cấu hình (không sửa code khi demo)

| Biến môi trường | Giá trị | Kết quả |
|---|---|---|
| `BATTLESHIP_DB` | `supabase` | Server dùng PostgreSQL trên Supabase |
| `BATTLESHIP_DB` | `sqlite` (mặc định) | Server dùng `battleship.db` cục bộ |
| `SUPABASE_DB_CONNECTION` | chuỗi kết nối Npgsql | Chỉ cần khi `supabase` |

---

## 4. Phân công

> **Hoàng là core thuyết trình**: mở bài, trình bày kiến trúc, làm demo live. Đức và Dũng phụ trách phần của mình trong slide, điều khiển máy thứ 2 khi demo và trả lời câu hỏi thuộc mảng mình.

### 4.1. Hoàng (Thành viên C, core thuyết trình)
| # | Việc | Branch | Hạn |
|---|---|---|---|
| H1 | Tạo project Supabase, lấy chuỗi kết nối + `publishable key` (tên mới của `anon key`) (**không commit**) | (không code) | 26/10 |
| H2 | Server chuyển DB bằng cấu hình (SQLite ⇄ Supabase), tự tạo bảng khi khởi động | `hoang/supabase-db-switch` | 27/10 |
| H3 | Client: `LeaderboardService` gọi REST + form Bảng xếp hạng | `hoang/leaderboard-rest` | 29/10 |
| H4 | Soạn **slide chính** (file `docs/slides/slide_demo.tex`) | `hoang/slides-demo` | 30/10 |
| H5 | **Thuyết trình + demo live**, làm chủ kịch bản Mục 6 | | ngày TT |

### 4.2. Dũng (Thành viên B, UI + dữ liệu)
| # | Việc | Branch | Hạn |
|---|---|---|---|
| D1 | Rà lại **Models/AppDbContext** chạy được trên PostgreSQL (kiểu dữ liệu, quan hệ `Friend`) và báo Hoàng nếu lỗi | `dung/models-postgres-check` | 27/10 |
| D2 | Chạy `docs/supabase_setup.sql` trên Supabase (bật RLS, tạo view `leaderboard`) và **seed** vài user mẫu có điểm | (làm trực tiếp trên Supabase) | 28/10 |
| D3 | Gắn nút **"Bảng xếp hạng"** ở Lobby mở form của Hoàng | `dung/lobby-leaderboard-button` | 29/10 |
| D4 | Soạn **slide mục "Cơ sở dữ liệu"** (ERD Users/Matches/MatchDetails, vì sao RLS) | gửi cho Hoàng ghép | 29/10 |

### 4.3. Đức (Thành viên A, logic server)
| # | Việc | Branch | Hạn |
|---|---|---|---|
| Đ1 | Hoàn tất `RoomHandler` còn dang dở để demo có **phòng + trận** thật (nếu chưa kịp thì demo đăng nhập + bảng xếp hạng) | `duc/room-handler` | 28/10 |
| Đ2 | Soạn **slide mục "Kiến trúc server và game"** (RoomManager, State Machine, luồng gói tin) | gửi cho Hoàng ghép | 29/10 |
| Đ3 | **Quay video demo dự phòng** (toàn bộ kịch bản Mục 6) và chuẩn bị máy thứ 2 | | 31/10 |

### 4.4. Cả nhóm
| Việc | Hạn |
|---|---|
| Merge hết PR vào `main` | 30/10 |
| **Tập dượt demo** trên đúng máy và mạng sẽ dùng (Hoàng nói, Đức/Dũng thao tác) | 31/10 |
| Kiểm tra Supabase **không bị tạm dừng** (xem Mục 7) | 1 ngày trước TT |

---

## 5. Mốc thời gian

| Ngày | Mốc |
|---|---|
| 26/10 | Hoàng tạo Supabase; mọi người pull `main` |
| 27/10 | Server chạy được trên Supabase (H2); Dũng rà Models (D1) |
| 28/10 | Chạy SQL + seed (D2); `RoomHandler` xong (Đ1) |
| 29/10 | Form bảng xếp hạng + nút ở Lobby (H3, D3); Đức và Dũng gửi slide phần mình |
| 30/10 | **Đóng băng code**, Hoàng ghép slide, merge hết vào `main` |
| 31/10 | **Tập dượt toàn bộ**, quay video dự phòng |
| 01–03/11 | Thuyết trình |

---

## 6. Kịch bản demo (khoảng 5 phút)

1. **(Hoàng)** Giới thiệu 30 giây: Battleship, Socket TCP raw, AES-128, nay thêm cloud.
2. **(Hoàng)** Mở **Supabase dashboard → Table Editor**, cho thấy bảng `Users` đang có dữ liệu seed.
3. **(Dũng thao tác)** Mở Client, **đăng ký tài khoản mới** `demo01` → Hoàng chuyển sang dashboard, **F5**: dòng `demo01` xuất hiện, cột `Password` là chuỗi `$2a$...` (BCrypt).
4. **(Hoàng)** Mở trình duyệt, gọi URL REST `.../rest/v1/leaderboard?select=*&order=Score.desc` (dùng Postman hoặc extension, vì cần header `apikey`) → hiện **JSON**.
5. **(Dũng thao tác)** Đăng nhập, bấm **Bảng xếp hạng** ở Lobby → danh sách hiện trong app.
6. **(Đức + Dũng)** Nếu `RoomHandler` xong: chơi nhanh 1 trận 2 máy. Nếu chưa, bỏ bước này.
7. **(Hoàng)** Kết: nói về dự phòng SQLite (đổi biến môi trường là chạy offline) và bảo mật (RLS, view, AES).

---

## 7. Rủi ro và cách xử lý

| Rủi ro | Cách phòng |
|---|---|
| **Mạng trường chập chờn** (đã nằm trong Roadmap 6.4) | Hotspot 4G dự phòng; **đặt `BATTLESHIP_DB=sqlite`** để vẫn chạy được phần game; phát **video dự phòng** của Đức |
| **Supabase free tạm dừng project** sau khoảng 1 tuần không hoạt động | Hôm trước buổi TT vào dashboard, nếu thấy "Paused" thì bấm **Restore** và chờ vài phút |
| Chuỗi kết nối **direct** không nối được (chỉ hỗ trợ IPv6) | Dùng chuỗi **Session pooler** (host dạng `aws-0-...pooler.supabase.com`, user `postgres.<ref>`), lấy ở dashboard → Connect |
| **Lộ khóa/mật khẩu DB** khi commit | **Không** ghi vào code hay file được commit. Dùng biến môi trường. `service_role key` tuyệt đối **không** đưa vào Client. Chỉ `publishable key` (tên mới của `anon key`) mới được dùng ở Client |
| Anon key đọc được bảng `Users` (có hash mật khẩu) | Chạy `docs/supabase_setup.sql`: bật RLS, thu quyền `anon`, chỉ cấp quyền đọc cho **view** `leaderboard` |
| Lệch schema giữa SQLite và PostgreSQL | Dũng rà ở D1; dùng `EnsureCreated()` cho từng DB, không trộn migration |
| Hỏi "sao không dùng Firebase?" | Trả lời bằng bảng so sánh ở slide "Vì sao Supabase" |

---

## 8. Quy tắc làm việc (tránh conflict như các tuần trước)

1. `git checkout main && git pull origin main` trước khi rẽ branch.
2. Mỗi việc một branch, PR nhỏ. Tên branch như bảng ở Mục 4.
3. **File nóng**: `Battleship.Server.csproj`, `Battleship.Client.csproj`, `TcpServer.cs`, `AppDbContext.cs`. Gặp conflict ở `.csproj` thì **luôn "Accept both"**.
4. `AppDbContext.cs` do **Hoàng sửa ở H2** (đổi provider). **Dũng đừng sửa file này** trong lúc đó, mà báo Hoàng nếu cần đổi.
5. **Không** commit khóa, chuỗi kết nối, hay file `.db`.
