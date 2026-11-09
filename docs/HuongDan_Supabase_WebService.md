# HƯỚNG DẪN: KHỞI TẠO VÀ DEMO SUPABASE + WEB SERVICE

> Dành cho cả nhóm (Hoàng, Đức, Dũng) và **bất kỳ ai** chưa từng dùng Supabase. Làm theo từng bước là chạy được.
> Dùng để **học** trước buổi báo cáo và để **làm lại từ đầu** nếu cần dựng project mới.
> Tên menu trên trang Supabase có thể hơi khác theo thời điểm. Nếu không thấy đúng tên thì tìm chữ gần giống.

---

## PHẦN A: HIỂU TRƯỚC KHI LÀM (đọc 5 phút)

### Mình đang làm gì?
Trước đây game lưu dữ liệu (tài khoản, điểm) vào file `battleship.db` **trên máy server**. Giờ mình đưa dữ liệu lên **cloud** để dùng chung và để **bên ngoài truy vấn được** bằng web service.

```
 Client (WinForms) ──TCP + AES──►  TCP Server (C#) ──EF Core──►  Supabase (PostgreSQL)
        │                                                              ▲
        └──────────── HTTPS + khóa công khai ──► REST API ─────────────┘
                       (lấy bảng xếp hạng)       (web service)
```

### Các khái niệm (hay bị hỏi khi báo cáo)
| Khái niệm | Giải thích ngắn |
|---|---|
| **Supabase** | Dịch vụ cloud cho cơ sở dữ liệu **PostgreSQL** + tự sinh sẵn **REST API**. Có gói miễn phí. |
| **PostgreSQL** | Hệ quản trị cơ sở dữ liệu quan hệ (giống SQL Server, SQLite). Vì là SQL nên dùng được **Entity Framework Core** của mình. |
| **Web service / REST API** | Chương trình nhận yêu cầu qua **HTTP** và trả dữ liệu (thường là **JSON**). Ở đây Supabase tự sinh, mình chỉ việc gọi. |
| **Endpoint** | Địa chỉ URL của một web service. Ví dụ `.../rest/v1/leaderboard`. |
| **JSON** | Định dạng dữ liệu dạng chữ, ví dụ `{"Username":"abc","Score":100}`. |
| **Publishable key** (trước đây gọi *anon key*) | Khóa **công khai**, cho phép gọi REST. Được phép nằm trong ứng dụng Client. |
| **Secret key** (trước đây *service_role key*) | Khóa **quản trị**, bỏ qua mọi bảo vệ. **TUYỆT ĐỐI không** đưa vào Client hay commit lên GitHub. |
| **Connection string** | Chuỗi chứa địa chỉ máy chủ, tài khoản, **mật khẩu** để Server nối vào cơ sở dữ liệu. Là **bí mật**. |
| **Session pooler** | Cách nối vào DB của Supabase hỗ trợ **IPv4** (mạng nhà, trường thường dùng). Loại *direct* chỉ hỗ trợ IPv6 nên hay không nối được. |
| **RLS (Row Level Security)** | Cơ chế khóa bảng: khi bật mà không có quy tắc nào thì người dùng công khai **không đọc/ghi được** bảng đó. |
| **View** | "Bảng ảo" chỉ chứa một vài cột lấy từ bảng thật. Mình dùng view `leaderboard` để chỉ lộ tên và điểm, **không có mật khẩu**. |

### Vì sao chọn Supabase mà không phải Firebase?
Supabase là **PostgreSQL** nên giữ nguyên EF Core và mô hình bảng có sẵn, chỉ đổi provider. Firebase là NoSQL, **không có EF Core provider**, phải viết lại toàn bộ lớp dữ liệu.

---

## PHẦN B: DỰNG TỪ ĐẦU (làm 1 lần)

### Bước 1: Chuẩn bị máy
- Cài **.NET 8 SDK** (kiểm tra: mở PowerShell gõ `dotnet --version`, phải ra `8.x` trở lên).
- Cài **Git**, clone repo `NT106_Proj`, `git checkout main`, `git pull origin main`.
- Một **tài khoản GitHub** (Supabase cho đăng nhập bằng GitHub rất tiện).

### Bước 2: Tạo project Supabase
1. Vào **https://supabase.com** → **Start your project** → đăng nhập bằng GitHub.
2. Bấm **New project**.
3. Điền:
   - **Name:** `nt106_project` (tên gì cũng được).
   - **Database Password:** đặt một mật khẩu mạnh rồi **GHI LẠI NGAY** (Supabase không cho xem lại, chỉ cho đặt lại).
   - **Region:** chọn vùng gần Việt Nam (Singapore hoặc Tokyo).
4. Bấm **Create new project**. Đợi 1 đến 2 phút đến khi trạng thái là **Healthy**.

### Bước 3: Lấy 3 thông tin cần dùng
Trong trang project, bấm nút **Connect** ở trên cùng (hoặc mục **Project Settings**):

| # | Thông tin | Tìm ở đâu | Dạng |
|---|---|---|---|
| 1 | **Project URL** | Mục **Project URL**, hoặc **Project Settings → API** | `https://<ref>.supabase.co` |
| 2 | **Publishable key** | **Project Settings → API Keys** → *Publishable key* | `sb_publishable_...` |
| 3 | **Connection string (Session pooler)** | **Connect → tab Connection String → Method: Session pooler** | `postgresql://postgres.<ref>:[MẬT-KHẨU]@aws-...pooler.supabase.com:5432/postgres` |

> ⚠️ **Phải chọn "Session pooler", đừng dùng "Direct connection".** Direct chỉ hỗ trợ IPv6, nhiều mạng sẽ báo không nối được.
> ⚠️ Host trong chuỗi (ví dụ `aws-0-ap-northeast-1.pooler.supabase.com`) **khác nhau theo vùng**. Phải **copy đúng từ Supabase**, đừng tự đoán. Đoán sai sẽ gặp lỗi `tenant/user ... not found`.

### Bước 4: Đặt biến môi trường (Windows)
Mở **PowerShell** và chạy từng dòng. Thay phần trong `<...>` bằng giá trị thật của bạn:

```powershell
setx BATTLESHIP_DB "supabase"
setx SUPABASE_DB_CONNECTION "Host=<host-pooler>;Port=5432;Database=postgres;Username=postgres.<ref>;Password=<mật-khẩu-DB>;SSL Mode=Require;Trust Server Certificate=true"
```
- `<host-pooler>`: phần giữa `@` và `:5432` trong chuỗi ở Bước 3 (ví dụ `aws-0-ap-northeast-1.pooler.supabase.com`).
- `<ref>`: mã project (đoạn trước `.supabase.co` trong Project URL).
- Đây là dạng **Npgsql** (khóa=giá trị, cách nhau dấu `;`), khác dạng `postgresql://`. Dạng này không bị lỗi khi mật khẩu có ký tự đặc biệt như `@`.

**Đóng và mở lại PowerShell** (và Visual Studio nếu đang mở), vì `setx` chỉ có tác dụng với cửa sổ mở **sau đó**.

Kiểm tra (không lộ mật khẩu):
```powershell
$env:BATTLESHIP_DB
($env:SUPABASE_DB_CONNECTION -replace 'Password=[^;]*','Password=***')
```
Dòng 1 phải in `supabase`. Dòng 2 in chuỗi kết nối nhưng mật khẩu bị che.

Nếu **Client** cần trỏ tới project của bạn (không phải project mặc định của Hoàng), đặt thêm:
```powershell
setx SUPABASE_URL "https://<ref>.supabase.co"
setx SUPABASE_PUBLISHABLE_KEY "sb_publishable_..."
```

### Bước 5: Chạy Server lần đầu (tự tạo bảng)
```powershell
cd <thư-mục-repo>
dotnet run --project Battleship.Server
```
Phải thấy:
```
[DB] Đang dùng: Supabase (PostgreSQL)
[DB] Đã tạo các bảng Users, Matches, MatchDetails trên Supabase.      <- lần đầu
[DB] Kết nối và kiểm tra bảng thành công.
=== Battleship Server ===
[TcpServer] Đang lắng nghe tại cổng 8000...
```
Vào Supabase → **Table Editor**: thấy 3 bảng **Users**, **Matches**, **MatchDetails**. Tắt Server bằng `Ctrl + C`.

### Bước 6: Khóa bảng và tạo view bảng xếp hạng (RẤT QUAN TRỌNG)
Mặc định Supabase **mở REST cho mọi bảng**, nên ai có publishable key cũng đọc được cả cột mật khẩu băm. Phải khóa lại:

1. Supabase → **SQL Editor** → **New query**.
2. Mở file `docs/supabase_setup.sql` trong repo, copy **toàn bộ phần không bị comment** (từ `ALTER TABLE ...` đến `GRANT SELECT ...`) dán vào, bấm **Run**.
3. Kiểm tra: chạy tiếp `SELECT * FROM public.leaderboard;` (có thể ra rỗng, không sao).

File này làm 4 việc: bật RLS trên 3 bảng, thu quyền công khai, tạo view `leaderboard` (chỉ có `Username, Wins, Losses, Score`), cho phép đọc view đó.

### Bước 7: Thêm dữ liệu mẫu (để bảng xếp hạng có người)
Trong `docs/supabase_setup.sql`, cuối file có đoạn **INSERT** đang bị comment. Bỏ dấu `--` ở đoạn đó rồi chạy trong SQL Editor. Hoặc đăng ký vài tài khoản trong game (điểm đều 0).

> Lưu ý: cột `"Id"` và `"Condition"` bắt buộc phải điền khi INSERT tay (file mẫu đã điền sẵn).

---

## PHẦN C: DEMO WEB SERVICE

### Cách 1: Dùng script có sẵn (khuyên dùng khi báo cáo)
```powershell
powershell -ExecutionPolicy Bypass -File docs\demo\demo_rest.ps1
```
Script làm 3 việc: gọi web service lấy top 10, in dạng bảng và JSON, rồi thử đọc bảng gốc `Users` (phải bị chặn **HTTP 401**).

### Cách 2: Tự gõ lệnh (để hiểu bản chất)
```powershell
$base = "https://<ref>.supabase.co/rest/v1"
$key  = "sb_publishable_..."
Invoke-RestMethod -Uri "$base/leaderboard?select=*&order=Score.desc&limit=10" -Headers @{ apikey = $key }
```
Hoặc bằng `curl.exe` (nhớ viết **`curl.exe`**, vì `curl` trong PowerShell là lệnh khác):
```powershell
curl.exe -H "apikey: sb_publishable_..." "https://<ref>.supabase.co/rest/v1/leaderboard?select=*&limit=3"
```

### Các tham số hay dùng (thêm vào sau dấu `?`)
| Mục đích | Ví dụ |
|---|---|
| Chọn cột | `select=Username,Score` |
| Sắp xếp giảm dần | `order=Score.desc` |
| Giới hạn số dòng | `limit=5` |
| Lọc theo điều kiện | `Username=eq.admiral_a` |
| Điểm lớn hơn 500 | `Score=gt.500` |

Ví dụ gộp: `.../rest/v1/leaderboard?select=Username,Score&Score=gt.500&order=Score.desc&limit=3`

### Hiểu mã lỗi
| Kết quả | Ý nghĩa |
|---|---|
| **200** + JSON | Thành công |
| **401** | Thiếu khóa, sai khóa, hoặc bị **RLS chặn** (đọc bảng gốc `Users`) |
| **404** + `PGRST205` | Chưa tạo bảng/view đó (quên chạy Bước 6) |

### Cách 3: Xem trong ứng dụng
Form **Bảng xếp hạng** (`frmLeaderboard`) của Client gọi đúng REST này và hiện lên `ListView`. Mở bằng `new frmLeaderboard().ShowDialog();`.

---

## PHẦN D: CHẠY CẢ HỆ THỐNG

### Chạy bằng Supabase (cần mạng)
**Cửa sổ 1 (Server):**
```powershell
cd <thư-mục-repo>
dotnet run --project Battleship.Server
```
**Cửa sổ 2 (Client):**
```powershell
cd <thư-mục-repo>
dotnet run --project Battleship.Client
```
Đăng ký tài khoản (tên 3 đến 20 ký tự gồm chữ không dấu, số, `_`; mật khẩu 6 đến 64 ký tự), đăng nhập. Sau đó vào Supabase Table Editor → bảng **Users** → **Refresh**: thấy tài khoản mới, cột `Password` bắt đầu bằng `$2a$` (BCrypt).

### Chạy offline bằng SQLite (không cần Supabase, không cần mật khẩu gì cả)
Dành cho người **không có** thông tin Supabase (ví dụ để chụp ảnh giao diện):
```powershell
$env:BATTLESHIP_DB = "sqlite"
dotnet run --project Battleship.Server
```
(Chỉ có tác dụng trong cửa sổ PowerShell đó.) Dòng đầu ra `[DB] Đang dùng: SQLite (battleship.db)`. Client chạy y như trên.

> Server phải chạy **trước** Client. Client đang nối cứng tới `127.0.0.1:8000`, nên Server và Client phải **ở cùng một máy**.

### Dừng
Server: `Ctrl + C`. Client: đóng cửa sổ game.

---

## PHẦN E: MỜI ĐỒNG ĐỘI VÀO PROJECT (để xem Table Editor, chụp ảnh)
Trên Supabase → bấm tên **Organization** → **Team** (hoặc *Organization settings → Team*) → **Invite** → nhập email GitHub của bạn → chọn vai trò **Developer** (hoặc Administrator nếu cần) → Send. Người được mời nhận email, bấm chấp nhận, rồi thấy project trong danh sách.
**Chỉ chia sẻ quyền xem dashboard, KHÔNG gửi mật khẩu DB** cho người chỉ cần xem.

---

## PHẦN F: LỖI THƯỜNG GẶP

| Triệu chứng | Nguyên nhân | Cách xử lý |
|---|---|---|
| `tenant/user postgres.<ref> not found` | **Sai host/vùng** của pooler | Copy lại host **đúng** từ Connect → Session pooler. Đừng tự đoán vùng |
| `password authentication failed` | Sai mật khẩu DB | Project Settings → Database → **Reset database password**, rồi `setx` lại |
| `relation "Users" does not exist` | Bảng chưa được tạo | Chạy Server ít nhất 1 lần ở chế độ Supabase (Bước 5) |
| `Thiếu biến môi trường SUPABASE_DB_CONNECTION` | Chưa đặt hoặc chưa mở lại terminal | Làm lại Bước 4, **mở cửa sổ PowerShell mới** |
| REST trả **404 `PGRST205`** | Chưa tạo view `leaderboard` | Chạy `docs/supabase_setup.sql` (Bước 6) |
| REST trả **401** khi đọc `leaderboard` | Sai/thiếu `apikey` | Kiểm tra lại publishable key, nhớ header tên `apikey` |
| REST đọc được cả bảng `Users` | **Chưa khóa RLS** (nguy hiểm) | Chạy `docs/supabase_setup.sql` ngay |
| Không nối được DB, mạng chập chờn | Mạng chặn cổng 5432 hoặc project bị **tạm dừng** | Dùng hotspot 4G; vào dashboard bấm **Restore** nếu thấy *Paused* |
| Bị khóa "Thử lại sau 60 giây" | Đăng nhập sai 5 lần (tính theo IP) | Chờ 60 giây, hoặc **khởi động lại Server** (bộ đếm nằm trong bộ nhớ) |
| Server báo cổng 8000 đang bị dùng | Còn Server cũ chưa tắt | `Get-NetTCPConnection -LocalPort 8000` rồi tắt tiến trình `Battleship.Server` |
| Chữ tiếng Việt lỗi font trong terminal | Bảng mã console | Gõ `chcp 65001` trước khi chạy |

---

## PHẦN G: BẢO MẬT (nhớ để trả lời câu hỏi)
**Được phép** để trong code/GitHub: Project URL, **publishable key**.
**KHÔNG** được: mật khẩu DB, connection string, **secret key**, file `.db`.
- Chuỗi kết nối nằm trong **biến môi trường**, không nằm trong mã nguồn.
- Bảng gốc bị khóa bằng **RLS**; chỉ view `leaderboard` (không có cột mật khẩu) mở công khai.
- Mật khẩu người chơi chỉ lưu dạng **BCrypt**. Gói tin TCP mã hóa **AES-128**.
- Sau buổi báo cáo: **đổi mật khẩu DB** (Project Settings → Database → Reset database password) rồi `setx` lại.

---

## PHẦN H: TỰ KIỂM TRA (trả lời được là hiểu bài)
1. Vì sao chọn Supabase mà không chọn Firebase?
2. Web service ở đây là gì? Ai viết ra nó?
3. Khác nhau giữa *publishable key* và *secret key*? Cái nào được đặt trong Client?
4. RLS để làm gì? Nếu không bật RLS thì chuyện gì xảy ra?
5. Vì sao dùng **view** `leaderboard` thay vì cho đọc thẳng bảng `Users`?
6. Vì sao phải dùng **Session pooler** thay vì Direct connection?
7. Làm sao đổi giữa Supabase và SQLite mà không sửa code?
8. Mất mạng giữa buổi demo thì làm gì?
9. Gọi REST mà nhận **401** và **404** khác nhau thế nào?
10. Mật khẩu có nằm trong bảng xếp hạng không? Ở đâu thấy được là mật khẩu lưu dạng BCrypt?

Đáp án nằm ngay trong các phần trên. Phần nào trả lời chưa được thì đọc lại phần đó.
