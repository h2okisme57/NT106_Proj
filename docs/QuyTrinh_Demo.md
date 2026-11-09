# QUY TRÌNH THỰC HIỆN DEMO HỆ THỐNG BATTLESHIP

**Đề tài:** Trò chơi Battleship nhiều người chơi trên giao thức Raw Socket TCP
**Nội dung demo:** Kết nối cơ sở dữ liệu đám mây (Supabase) và truy xuất dữ liệu qua Web Service (REST API)
**Học phần:** Lập trình mạng căn bản (NT106) · **Nhóm thực hiện:** Hoàng, Đức, Dũng
**Người trình bày và điều khiển:** Hoàng · **Hỗ trợ thao tác:** Đức, Dũng
**Thời lượng:** khoảng 5 phút · **Địa điểm thực hiện:** một máy tính duy nhất (máy chủ và máy khách cùng máy)

---

## 1. MỤC ĐÍCH VÀ PHẠM VI

### 1.1. Mục đích
Buổi trình diễn nhằm chứng minh ba luận điểm kỹ thuật:

| STT | Luận điểm | Bằng chứng thu được |
|---|---|---|
| 1 | Hệ thống **ghi** dữ liệu lên cơ sở dữ liệu đám mây | Tài khoản đăng ký trong ứng dụng xuất hiện trong bảng `Users` trên Supabase, mật khẩu được lưu dưới dạng băm BCrypt |
| 2 | Dữ liệu **truy xuất được qua Web Service** | Một yêu cầu HTTPS tới REST API của Supabase trả về dữ liệu dạng JSON |
| 3 | Cơ chế **xác thực an toàn** | Tài khoản bị khóa tạm thời sau 5 lần nhập sai mật khẩu |

### 1.2. Phạm vi
Quy trình bao gồm: khởi động máy chủ, khởi động máy khách, đăng ký tài khoản, kiểm tra dữ liệu trên đám mây, gọi Web Service, hiển thị bảng xếp hạng và kiểm tra cơ chế khóa đăng nhập.
Quy trình **không** bao gồm phần chơi trận (xếp tàu, bắn theo lượt), do phần này đang trong giai đoạn phát triển.

---

## 2. SƠ ĐỒ TỔNG QUAN

```
 Máy khách (WinForms) ──TCP + AES-128──►  Máy chủ TCP (C#) ──EF Core──►  Supabase (PostgreSQL)
        │                                                                     ▲
        └──────────────── HTTPS + khóa công khai ──► REST API ────────────────┘
                          (bảng xếp hạng)           (Web Service)
```

Luồng trò chơi (đăng ký, đăng nhập, phòng) đi qua máy chủ TCP. Luồng truy xuất bảng xếp hạng đi trực tiếp từ máy khách tới REST API của Supabase.

---

## 3. ĐIỀU KIỆN TIÊN QUYẾT

### 3.1. Phần mềm và cấu hình
| Hạng mục | Yêu cầu |
|---|---|
| Hệ điều hành | Windows 10/11 |
| Nền tảng | .NET SDK 8.0 trở lên (`dotnet --version`) |
| Mã nguồn | Nhánh `main` đã tích hợp đầy đủ, **bao gồm nút "Leader Board" ở sảnh chờ** (xem lưu ý 3.3) |
| Biến môi trường | `BATTLESHIP_DB=supabase` và `SUPABASE_DB_CONNECTION=<chuỗi kết nối Session pooler>` |
| Mạng | Kết nối Internet ổn định; có sẵn điểm phát sóng 4G dự phòng |
| Cổng mạng | Cổng `8000` trên máy cục bộ không bị chiếm dụng |

### 3.2. Trạng thái dịch vụ đám mây
- Dự án Supabase ở trạng thái **Healthy** (không ở trạng thái *Paused*).
- Đã áp dụng tập lệnh `docs/supabase_setup.sql`: bật Row Level Security trên ba bảng gốc và tạo view `leaderboard`.
- View `leaderboard` đã có **dữ liệu mẫu** (tối thiểu 3 dòng) để kết quả hiển thị không rỗng.
- Tài khoản dùng cho buổi demo (`demo01`) **chưa tồn tại** trong bảng `Users`.

### 3.3. Lưu ý về tích hợp mã nguồn
Nút **Leader Board** ở sảnh chờ nằm trong nhánh `dung/board-placement-ui`. Tính đến thời điểm lập tài liệu, nhánh này **chưa được tích hợp vào `main`**. Đã kiểm tra: việc tích hợp **không phát sinh xung đột** và toàn bộ giải pháp **biên dịch thành công (0 lỗi)**. Cần hoàn tất việc tích hợp trước buổi demo; nếu không, **Bước 5** được lược bỏ và thay bằng Bước 4.

---

## 4. CÔNG TÁC CHUẨN BỊ

### 4.1. Trước buổi demo một ngày
1. Cập nhật mã nguồn và kiểm tra biên dịch:
   ```powershell
   cd D:\NT106
   git checkout main
   git pull origin main
   dotnet build BattleshipNetwork.slnx
   ```
   Kết quả yêu cầu: `0 Error(s)`.
2. Truy cập bảng điều khiển Supabase, xác nhận dự án ở trạng thái **Healthy**. Nếu hiển thị *Paused*, chọn **Restore** và chờ hoàn tất.
3. Thực hiện diễn tập toàn bộ quy trình tại mục 5 tối thiểu hai lần, có bấm giờ.
4. Ghi hình quy trình làm tư liệu dự phòng (xem mục 7).

### 4.2. Trước buổi demo 15 phút
1. Kết nối nguồn điện cho máy tính; tắt các thông báo ứng dụng.
2. Chạy công cụ kiểm tra sẵn sàng:
   ```powershell
   cd D:\NT106
   powershell -ExecutionPolicy Bypass -File docs\demo\demo_check.ps1
   ```
   Công cụ kiểm tra: biến môi trường, khả năng kết nối tới cơ sở dữ liệu, thời gian phản hồi của REST (dưới 3 giây) và tình trạng cổng 8000. Kết quả cuối cùng là **GO** (tiến hành demo trực tiếp) hoặc **NO-GO** (chuyển sang phương án dự phòng, mục 7).
3. Chuẩn bị sẵn bốn cửa sổ theo thứ tự: (a) bản trình chiếu; (b) trình duyệt tại *Supabase → Table Editor → bảng Users*; (c) cửa sổ PowerShell thứ nhất (máy chủ); (d) cửa sổ PowerShell thứ hai (Web Service và máy khách).
4. Xác nhận không còn tiến trình `Battleship.Server` cũ đang chạy.

---

## 5. CÁC BƯỚC THỰC HIỆN

### Bước 1. Khởi động máy chủ
**Mục tiêu:** Chứng minh máy chủ kết nối trực tiếp tới cơ sở dữ liệu đám mây.
**Thực hiện tại:** cửa sổ PowerShell thứ nhất.

```powershell
cd D:\NT106
dotnet run --project Battleship.Server
```

**Kết quả mong đợi** (giữ nguyên cửa sổ, không tắt):
```
[DB] Đang dùng: Supabase (PostgreSQL)
[DB] Kết nối và kiểm tra bảng thành công.
=== Battleship Server ===
[TcpServer] Đang lắng nghe tại cổng 8000...
```
**Tiêu chí đạt:** dòng đầu ghi đúng *Supabase (PostgreSQL)*; không xuất hiện thông báo lỗi kết nối.

---

### Bước 2. Khởi động ứng dụng khách
**Mục tiêu:** Mở giao diện người dùng.
**Thực hiện tại:** cửa sổ PowerShell thứ hai.

```powershell
cd D:\NT106
dotnet run --project Battleship.Client
```

**Kết quả mong đợi:** xuất hiện cửa sổ trò chơi với màn hình **USER LOGIN**. Nhật ký máy chủ ghi nhận một kết nối mới (`[TcpServer] Client ... đã kết nối`).

---

### Bước 3. Đăng ký tài khoản và kiểm chứng dữ liệu trên đám mây
**Mục tiêu:** Chứng minh luận điểm 1 (ghi dữ liệu lên đám mây).

**Thao tác:**
1. Trên màn hình đăng nhập, chọn liên kết **"Chưa có tài khoản?"**.
2. Nhập thông tin đăng ký:

   | Trường | Giá trị |
   |---|---|
   | Tên đăng nhập | `demo01` |
   | Mật khẩu | `Demo@2026` |
   | Xác nhận mật khẩu | `Demo@2026` |

   *Quy tắc hợp lệ: tên đăng nhập 3–20 ký tự gồm chữ không dấu, chữ số hoặc dấu gạch dưới; mật khẩu 6–64 ký tự và không trùng tên đăng nhập.*
3. Xác nhận thông báo **đăng ký thành công**.
4. Chuyển sang trình duyệt: *Supabase → Table Editor → bảng `Users`* → chọn **Refresh**.

**Kết quả mong đợi:**
- Xuất hiện dòng mới có `Username = demo01`.
- Cột `Password` là chuỗi băm bắt đầu bằng **`$2a$`** (BCrypt), **không phải** mật khẩu gốc.

**Tiêu chí đạt:** dòng `demo01` hiện diện trên đám mây và mật khẩu không ở dạng văn bản thuần.

---

### Bước 4. Truy xuất dữ liệu qua Web Service
**Mục tiêu:** Chứng minh luận điểm 2 (truy xuất qua Web Service) và kiểm tra cơ chế bảo vệ dữ liệu.
**Thực hiện tại:** cửa sổ PowerShell thứ hai (cửa sổ máy khách vẫn mở song song).

```powershell
cd D:\NT106
powershell -ExecutionPolicy Bypass -File docs\demo\demo_rest.ps1
```

Tập lệnh thực hiện lần lượt bốn thao tác:

| Thao tác | Nội dung | Kết quả mong đợi |
|---|---|---|
| [1] | Gọi `GET /rest/v1/leaderboard?select=*&order=Score.desc&limit=10` kèm tiêu đề `apikey` | Yêu cầu thành công |
| [2] | Hiển thị dữ liệu dạng bảng | Danh sách người chơi theo điểm giảm dần, có `demo01` (0 điểm) |
| [3] | Hiển thị dữ liệu JSON thô | Mảng JSON gồm các trường `Username`, `Wins`, `Losses`, `Score` |
| [4] | Đọc **trực tiếp bảng gốc `Users`** bằng cùng khóa công khai | **Bị từ chối** (mã HTTP 401 hoặc 404) |

**Tiêu chí đạt:** (i) dữ liệu JSON trả về đúng; (ii) JSON **không chứa** trường mật khẩu; (iii) thao tác [4] bị từ chối.
**Giải thích kỹ thuật cho thao tác [4]:** bảng gốc đã được bảo vệ bằng Row Level Security và bị thu hồi quyền truy cập công khai; chỉ view `leaderboard` (không có cột mật khẩu) được phép đọc.

---

### Bước 5. Hiển thị bảng xếp hạng trong ứng dụng
**Mục tiêu:** Chứng minh ứng dụng tích hợp Web Service.
*(Điều kiện: nút Leader Board đã được tích hợp, xem mục 3.3.)*

**Thao tác trên máy khách:**
1. Đăng nhập bằng tài khoản `demo01` / `Demo@2026`.
2. Tại sảnh chờ, chọn mục **Leader Board**.
3. Quan sát biểu mẫu **Bảng xếp hạng**; chọn **Làm mới** để tải lại.

**Kết quả mong đợi:** biểu mẫu hiển thị danh sách người chơi (cột *Hạng, Tên người chơi, Thắng, Thua, Điểm*) và dòng trạng thái *"Đã tải N người chơi."*
**Tiêu chí đạt:** dữ liệu hiển thị khớp với kết quả ở Bước 4.

---

### Bước 6. Kiểm tra cơ chế khóa đăng nhập
**Mục tiêu:** Chứng minh luận điểm 3 (xác thực an toàn).

> ⚠️ **Bước này phải thực hiện cuối cùng.** Cơ chế khóa tính theo **địa chỉ IP**, do đó sau khi bị khóa, **mọi tài khoản trên máy này** đều không thể đăng nhập trong 60 giây. Thực hiện sớm sẽ làm gián đoạn các bước còn lại.

**Thao tác:**
1. Mở một cửa sổ máy khách mới (lặp lại lệnh ở Bước 2).
2. Đăng nhập với tên `demo01` và **mật khẩu sai**, lặp lại **5 lần liên tiếp**.
3. Ở lần thứ 6, nhập **đúng** mật khẩu `Demo@2026`.

**Kết quả mong đợi:** thông báo đỏ dạng **"Thử lại sau N giây"** với N đếm ngược từ 60; việc nhập đúng mật khẩu **vẫn bị từ chối** trong thời gian khóa.
**Tiêu chí đạt:** hệ thống từ chối đăng nhập trong thời gian khóa, kể cả khi mật khẩu đúng.

**Khôi phục sau bước này:** chờ hết 60 giây, hoặc dừng và khởi động lại máy chủ (bộ đếm lưu trong bộ nhớ nên được xóa khi máy chủ khởi động lại).

---

### Bước 7. Phương án dự phòng: chạy ngoại tuyến bằng SQLite
**Mục tiêu:** Chứng minh hệ thống vẫn vận hành khi mất kết nối Internet.
*(Chỉ thực hiện khi có yêu cầu hoặc khi gặp sự cố mạng.)*

Dừng máy chủ (`Ctrl + C`), sau đó chạy lại **trong cùng cửa sổ**:
```powershell
$env:BATTLESHIP_DB = "sqlite"
dotnet run --project Battleship.Server
```
**Kết quả mong đợi:** dòng đầu của nhật ký ghi `[DB] Đang dùng: SQLite (battleship.db)`; đăng ký và đăng nhập hoạt động bình thường với dữ liệu lưu cục bộ.
*Biến `$env:BATTLESHIP_DB` chỉ có hiệu lực trong cửa sổ PowerShell hiện tại, không làm thay đổi cấu hình hệ thống.*

---

## 6. PHÂN BỔ THỜI GIAN VÀ VAI TRÒ

| Bước | Nội dung | Thời gian | Người thực hiện |
|---|---|---|---|
| 1–2 | Khởi động máy chủ và máy khách | 0:40 | Hoàng |
| 3 | Đăng ký và kiểm chứng trên Supabase | 1:00 | Dũng thao tác máy khách; Hoàng giải thích |
| 4 | Truy xuất qua Web Service | 1:00 | Hoàng |
| 5 | Bảng xếp hạng trong ứng dụng | 0:30 | Dũng thao tác; Hoàng giải thích |
| 6 | Kiểm tra khóa đăng nhập | 0:50 | Đức thao tác máy khách thứ hai; Hoàng giải thích |
| 7 | Phương án dự phòng (nói miệng) | 0:15 | Hoàng |
| | **Tổng cộng** | **khoảng 4:15 đến 5:00** | |

---

## 7. XỬ LÝ SỰ CỐ VÀ PHƯƠNG ÁN DỰ PHÒNG

### 7.1. Quy tắc quyết định trước buổi demo
| Kết quả `demo_check.ps1` | Hành động |
|---|---|
| **GO** | Thực hiện demo trực tiếp theo mục 5 |
| **NO-GO** (mạng không ổn định) | Bật điểm phát sóng 4G và chạy lại công cụ kiểm tra |
| Vẫn **NO-GO** | Thực hiện Bước 7 (SQLite) cho phần trò chơi và **phát video đã ghi** cho phần Supabase và REST |

### 7.2. Sự cố trong quá trình demo
| Hiện tượng | Nguyên nhân khả dĩ | Cách xử lý |
|---|---|---|
| Máy chủ báo lỗi kết nối cơ sở dữ liệu | Mạng gián đoạn hoặc dự án đang *Paused* | Chuyển sang Bước 7; đồng thời phát video phần Supabase |
| REST không phản hồi quá 5 giây | Đường truyền chậm | Dừng thao tác, chuyển sang phát video |
| Cổng 8000 đang bị chiếm | Còn tiến trình máy chủ cũ | `Get-NetTCPConnection -LocalPort 8000` rồi dừng tiến trình `Battleship.Server` |
| Bảng xếp hạng rỗng | Chưa có dữ liệu mẫu | Chạy đoạn `INSERT` dữ liệu mẫu trong `docs/supabase_setup.sql` tại *SQL Editor* |
| Không đăng nhập được dù đúng mật khẩu | Đang trong thời gian khóa (60 giây) | Chờ hết thời gian khóa hoặc khởi động lại máy chủ |
| Không hiện mục Leader Board | Máy khách chưa chứa nhánh `dung/board-placement-ui` | Bỏ Bước 5, dùng kết quả Bước 4 làm minh chứng |
| Quên mật khẩu tài khoản demo | | Đăng ký tài khoản mới (`demo02`) |

---

## 8. TIÊU CHÍ NGHIỆM THU

Buổi demo được coi là **đạt** khi thỏa mãn đồng thời:

| STT | Tiêu chí | Bước |
|---|---|---|
| 1 | Máy chủ kết nối thành công tới Supabase (nhật ký ghi *Supabase (PostgreSQL)*) | 1 |
| 2 | Tài khoản đăng ký từ ứng dụng xuất hiện trên Supabase, mật khẩu dạng BCrypt | 3 |
| 3 | Web Service trả về JSON bảng xếp hạng, không chứa mật khẩu | 4 |
| 4 | Việc đọc trực tiếp bảng gốc bằng khóa công khai bị từ chối | 4 |
| 5 | Cơ chế khóa đăng nhập hoạt động đúng sau 5 lần nhập sai | 6 |

---

## 9. CÔNG TÁC SAU BUỔI DEMO

1. Dừng máy chủ (`Ctrl + C`) và đóng ứng dụng khách.
2. Xóa dữ liệu thử trên Supabase (*SQL Editor*):
   ```sql
   DELETE FROM "Users" WHERE "Username" IN ('demo01', 'demo02');
   DELETE FROM "Users" WHERE "Id" LIKE 'seed-%';
   ```
3. **Đặt lại mật khẩu cơ sở dữ liệu** (*Project Settings → Database → Reset database password*), sau đó cập nhật lại biến môi trường `SUPABASE_DB_CONNECTION`.

---

## PHỤ LỤC. BẢNG LỆNH TỔNG HỢP

| Mục đích | Lệnh |
|---|---|
| Kiểm tra sẵn sàng | `powershell -ExecutionPolicy Bypass -File docs\demo\demo_check.ps1` |
| Khởi động máy chủ | `dotnet run --project Battleship.Server` |
| Khởi động máy khách | `dotnet run --project Battleship.Client` |
| Gọi Web Service | `powershell -ExecutionPolicy Bypass -File docs\demo\demo_rest.ps1` |
| Chạy ngoại tuyến (SQLite) | `$env:BATTLESHIP_DB = "sqlite"` rồi `dotnet run --project Battleship.Server` |
| Kiểm tra cổng 8000 | `Get-NetTCPConnection -LocalPort 8000 -State Listen` |
| Dừng máy chủ | `Ctrl + C` |
