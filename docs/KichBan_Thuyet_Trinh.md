# KỊCH BẢN THUYẾT TRÌNH VÀ DEMO: BÁO CÁO THỨ SÁU 30/10

> Người nói chính: **Hoàng**. Đức và Dũng thao tác máy và trả lời câu hỏi thuộc mảng của mình.
> Slide: `docs/slides/slide_demo.pdf` (23 trang). Script hỗ trợ: `docs/demo/demo_check.ps1`, `docs/demo/demo_rest.ps1`.

---

## 0. QUYẾT ĐỊNH: DEMO TRỰC TIẾP HAY VIDEO?

**Kết luận: demo TRỰC TIẾP là chính, video là dự phòng bắt buộc có sẵn.** Không chọn một trong hai.

| | Demo trực tiếp | Chỉ video |
|---|---|---|
| Ưu | Cô thấy thật, có thể yêu cầu "thử cái này"; chứng minh dữ liệu đúng là từ cloud | Không phụ thuộc mạng, không lỗi bất ngờ |
| Nhược | Phụ thuộc mạng trường, dễ lỗi nếu không tập | Dễ bị nghi là dựng; cô hỏi "bấm thử xem" thì không làm được |

**Lý do chọn live:** chủ đề là "kết nối web service/cloud và móc dữ liệu", và cách thuyết phục nhất là để cô tự thấy dữ liệu thay đổi theo thao tác. Phần live của bạn nhẹ và nhanh (vài request), nên rủi ro thấp.

### Quy tắc go/no-go (15 phút trước giờ báo cáo)
1. Chạy `powershell -ExecutionPolicy Bypass -File docs\demo\demo_check.ps1`.
2. Ra **GO** → demo live.
3. Ra **NO-GO** (mạng chập chờn) → làm lần lượt:
   - Bật **hotspot 4G**, chạy lại check.
   - Vẫn lỗi → đặt `$env:BATTLESHIP_DB="sqlite"` và demo game **offline** (đăng ký, đăng nhập, chặn dò mật khẩu), rồi **phát video** cho phần Supabase/REST.
4. **Đang demo mà đứt mạng:** đừng loay hoay. Nói "mạng đang chập chờn, em xin chuyển sang video quay sẵn" rồi bấm phát ngay.

### Ai quay video (Đức, hạn 29/10)
Quay **đúng kịch bản mục 3**, nói thuyết minh ngắn, có cả màn hình Supabase dashboard. Lưu 2 bản (ổ cứng máy demo và USB).

---

## 1. CHUẨN BỊ

### Trước 1 ngày (29/10)
- [ ] `main` build xanh; **cả nhóm đã pull** bản mới nhất (nhất là sau khi merge AES).
- [ ] Vào **Supabase dashboard**: project phải ở trạng thái **Healthy**. Nếu "Paused" thì bấm **Restore**, chờ vài phút.
- [ ] Chạy `docs/supabase_setup.sql` phần **dữ liệu mẫu** (bỏ dấu `--` ở đoạn INSERT) để bảng xếp hạng **có người**, không trống.
- [ ] Tập toàn bộ kịch bản mục 3 **ít nhất 2 lần**, bấm giờ.
- [ ] Quay xong video dự phòng.
- [ ] Tải sẵn hotspot 4G cho điện thoại.

### Trước 15 phút (30/10)
- [ ] Cắm sạc laptop, tắt thông báo (Zalo, Facebook).
- [ ] Chạy `demo_check.ps1` → phải **GO**.
- [ ] Mở sẵn **4 cửa sổ**, xếp đúng thứ tự:
  1. Slide (PDF toàn màn hình).
  2. Trình duyệt: Supabase **Table Editor → bảng Users** (đã đăng nhập sẵn).
  3. PowerShell tại `D:\NT106` (đã `cd`).
  4. Visual Studio hoặc terminal để chạy Server và Client.
- [ ] Tắt hết Server cũ: không còn tiến trình `Battleship.Server` nào (cổng 8000 phải trống).
- [ ] Xoá tài khoản `demo01` cũ nếu có (xem mục "dọn" cuối file).

---

## 2. KHUNG THỜI GIAN (giả định 15 phút, chỉnh lại theo thời lượng cô cho)

**Hoàng nói tất cả các slide** (kể cả phần Đức và Dũng soạn, theo ý chính). Đức và Dũng bổ sung hoặc trả lời nếu cô hỏi vào mảng của mình.

| Phần | Slide | Thời gian | Soạn slide |
|---|---|---|---|
| Mở đầu, bối cảnh | 1–4 | 2 phút | Hoàng |
| Kiến trúc server và game | 5–8 | 2 phút | **Đức** |
| Cơ sở dữ liệu và giao diện | 9–12 | 2 phút | **Dũng** |
| Cloud: vì sao Supabase, kiến trúc, web service | 13–16 | 2,5 phút | Hoàng |
| Đăng nhập an toàn, bảo mật dữ liệu | 17–18 | 2 phút | Hoàng |
| **DEMO** | 19 | **4 phút** | Hoàng làm, Đức/Dũng hỗ trợ |
| Rủi ro, phân công, kết luận, cảm ơn | 20–23 | 1 phút | Hoàng |

Nếu cô chỉ cho **ngắn hơn** (ví dụ 10 phút), bỏ theo thứ tự này cho đến khi vừa: slide 8 (State Machine đầy đủ) → slide 2 (mục lục) → slide 4 → slide 7 → slide 20 → slide 12. **Giữ lại bằng mọi giá:** 13, 14, 17, 19 (demo).

---

## 3. LỜI NÓI TỪNG SLIDE

**Slide 1: Tiêu đề** (10 giây)
> "Chào cô và các bạn. Nhóm em báo cáo đồ án Battleship, game bắn tàu nhiều người chơi viết bằng raw socket TCP. Hôm nay phần trọng tâm là em đã kết nối game với cơ sở dữ liệu trên cloud và một web service."

**Slide 2: Mục lục** (5 giây)
> "Em đi qua bốn phần: tổng quan, cloud và web service, demo, và nhóm thực hiện."

**Slide 3: Đồ án là gì** (30 giây)
> "Game có đăng ký, đăng nhập, danh sách online, phòng đấu và chat. Công nghệ cốt lõi là C# .NET 8, TcpListener và TcpClient, async await, framing 4 byte, mã hóa AES-128, BCrypt và Entity Framework Core."

**Slide 4: Tình trạng trước khi thêm cloud** (30 giây)
> "Trước đó dữ liệu lưu SQLite ngay trên máy server nên chỉ một máy dùng được và bên ngoài không truy vấn được. Hai việc cần giải quyết là: dùng chung được, và có web service."

**Slide 5: Kiến trúc TCP Server** *(nội dung Đức soạn; Hoàng nói theo ý chính)* (30 giây)
> "Phía server dùng TcpListener nhận kết nối, mỗi client một Task nên không bị nghẽn. ClientSession đọc gói theo framing 4 byte, PacketRouter chuyển từng loại gói cho handler tương ứng."

**Slide 6: Quản lý phòng và State Machine** *(Đức)* (40 giây)
> "Các phòng được quản lý bằng ConcurrentDictionary, mỗi phòng có khóa riêng để hai người vào cùng lúc không bị lỗi. Trạng thái phòng chạy theo State Machine: nét liền là phần đã làm, nét đứt là phần đang phát triển. Server là bên quyết định, client không tự tính được kết quả."

**Slide 7: Phòng đấu trong ứng dụng** *(Đức)* (20 giây)
> "Client có thể xem danh sách phòng, tạo phòng và vào phòng." *(Đức kiểm tra lại cho khớp với những gì đã có.)*

**Slide 8: Bản thiết kế State Machine** *(Đức, tùy chọn)* (10 giây)
> "Đây là bản thiết kế đầy đủ, một phần đã hiện thực như vừa nói." *(Thiếu giờ thì bỏ qua.)*

**Slide 9: Mô hình dữ liệu (ERD)** *(Dũng)* (40 giây)
> "Có ba bảng. Users lưu tài khoản và điểm số, mật khẩu chỉ lưu dạng BCrypt. Matches lưu từng trận. MatchDetails lưu từng lượt bắn của trận đó. Một người chơi có nhiều trận, một trận có nhiều lượt bắn."

**Slide 10: Dữ liệu thật trên Supabase** *(Dũng)* (30 giây)
> "Bên trái là bảng Users trên Supabase, dữ liệu ghi từ game. Bên phải là chính dữ liệu đó đọc qua web service." *(Nếu dùng slide này thì có thể rút ngắn bước C của demo.)*

**Slide 11: Row Level Security** *(Dũng)* (40 giây)
> "Supabase mặc định mở REST cho mọi bảng. Nếu không khóa thì ai có khóa công khai đều đọc được cột mật khẩu băm. Nhóm đã bật RLS, thu quyền trên bảng gốc, chỉ mở view leaderboard không có cột mật khẩu. Ảnh bên phải là kết quả: đọc bảng gốc bị chặn 401." *(Slide 18 của Hoàng nhắc lại RLS, nên ở slide 18 chỉ nói tóm tắt 15 giây.)*

**Slide 12: Giao diện Client** *(Dũng)* (20 giây)
> "Giao diện WinForms gồm đăng ký, đăng nhập, sảnh chờ, danh sách phòng và bảng xếp hạng. Ảnh cuối là thông báo khóa khi nhập sai mật khẩu nhiều lần."

**Slide 13: Vì sao Supabase** (1 phút)
> "Em cân nhắc ba lựa chọn. Firebase là NoSQL và không có provider cho Entity Framework Core, nên phải viết lại toàn bộ lớp dữ liệu. Supabase thực chất là PostgreSQL nên chỉ cần đổi provider, đồng thời cho REST API miễn phí. SQLite em vẫn giữ làm dự phòng khi mất mạng."

**Slide 14: Kiến trúc** (1 phút) — *chỉ vào sơ đồ*
> "Game vẫn đi qua TCP Server như cũ. Server nói chuyện với Supabase qua EF Core. Riêng bảng xếp hạng thì Client gọi thẳng REST API của Supabase bằng HTTPS, không đi qua server. Nét đứt là đường chuyển sang SQLite."

**Slide 15: Kết nối CSDL** (30 giây)
> "Chọn nơi lưu bằng một biến môi trường, không sửa code. Chuỗi kết nối nằm trong biến môi trường chứ không nằm trong mã nguồn. Server tự tạo bảng khi khởi động."

**Slide 16: Web service REST** (30 giây)
> "Mỗi bảng hoặc view trong Supabase tự có một endpoint. Em gọi bằng HttpClient, chỉ cần một header khóa công khai. Thiếu khóa trả về 401."

**Slide 17: Đăng nhập an toàn** (1 phút) — *đây là phần cô yêu cầu, nói chắc*
> "Về đăng nhập, em xử lý các nguy cơ phổ biến. Chưa đăng nhập thì router chặn mọi gói trừ đăng nhập và đăng ký. Dò mật khẩu thì khóa 60 giây sau 5 lần sai. Kiểm tra độ dài và ký tự đầu vào. Một tài khoản chỉ một phiên. So sánh tên không phân biệt hoa thường để không ai giả Admin. Luôn chạy BCrypt kể cả khi tên không tồn tại, để không đoán được tên qua thời gian trả lời. Mật khẩu chỉ lưu dạng băm. Em kiểm thử bằng 19 kịch bản tấn công qua TCP thật và đều đạt."

**Slide 18: Bảo mật dữ liệu** (1 phút)
> "Supabase mặc định mở REST cho mọi bảng. Em đã kiểm tra: nếu không bật Row Level Security thì ai có khóa công khai đều đọc được bảng Users. Nên em khóa bảng gốc và chỉ mở một view tên leaderboard, không có cột mật khẩu. Em sẽ cho cô thấy ngay trong demo."

**Slide 19: Demo** → xem mục 4 bên dưới (4 phút).

**Slide 20: Rủi ro và dự phòng** (20 giây)
> "Rủi ro lớn nhất là mạng trường. Em chuẩn bị hotspot, chế độ SQLite offline và video quay sẵn."

**Slide 21: Phân công** (20 giây)
> "Em phụ trách mạng, Supabase và bảo mật đăng nhập. Bạn Dũng phụ trách giao diện và cơ sở dữ liệu. Bạn Đức phụ trách logic server và phòng đấu."

**Slide 22: Kết luận** (15 giây)
> "Tóm lại: giao thức socket tự thiết kế, đăng nhập an toàn, dữ liệu trên Supabase, web service REST, và vẫn chạy offline được."

**Slide 23: Cảm ơn** → "Nhóm em xin nhận câu hỏi."

---

## 4. KỊCH BẢN DEMO (4 phút)

> Tất cả chạy **trên một máy**. Client hiện đang nối cứng tới `127.0.0.1:8000`, nên không chạy Client ở máy khác được nếu chưa sửa code.

### Bước A: Khởi động Server (20 giây)
Terminal 1:
```powershell
cd D:\NT106
dotnet run --project Battleship.Server
```
Chỉ cho cô dòng log: **`[DB] Đang dùng: Supabase (PostgreSQL)`** rồi **`Kết nối và kiểm tra bảng thành công`**.
> "Server vừa nối thẳng tới cơ sở dữ liệu trên cloud."

### Bước B: Đăng ký, thấy dữ liệu trên cloud (60 giây)
1. Mở Client, **đăng ký** tài khoản `demo01` / mật khẩu `Demo@2026`.
2. Chuyển sang trình duyệt: **Supabase Table Editor → Users → bấm Refresh**.
3. Chỉ dòng `demo01` mới xuất hiện, và cột `Password` là chuỗi bắt đầu bằng **`$2a$`**.
> "Tài khoản vừa tạo trong game đã nằm trên cloud. Mật khẩu là chuỗi băm BCrypt, không ai đọc được mật khẩu thật."

### Bước C: Web service trả dữ liệu (60 giây)
PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File docs\demo\demo_rest.ps1
```
Chỉ 3 thứ trên màn hình: bảng xếp hạng dạng bảng, JSON thô, và dòng cuối **"Bị chặn: HTTP 401"** khi đọc thẳng bảng `Users`.
> "Đây là web service: một request HTTPS trả về JSON. Cùng khóa công khai đó mà đọc bảng gốc thì bị chặn, chỉ view leaderboard đọc được, và view này không có cột mật khẩu."

### Bước D (nếu nút đã được gắn): Bảng xếp hạng trong ứng dụng (30 giây)
Đăng nhập `demo01` bằng mật khẩu đúng → Lobby → bấm **Bảng xếp hạng** → danh sách hiện lên.
**Nếu chưa có nút: bỏ bước này**, bước C đã đủ minh chứng. Đăng xuất hoặc đóng Client trước khi sang bước E.

### Bước E: Bảo mật đăng nhập, LÀM CUỐI CÙNG (45 giây), cực kỳ nên làm
Mở Client mới, đăng nhập `demo01` với **mật khẩu sai 5 lần liên tiếp**, rồi lần thứ 6 nhập **đúng** mật khẩu.
Kết quả: **"Đăng nhập sai quá nhiều lần. Thử lại sau 60 giây."**
> "Dù lần này em nhập đúng mật khẩu vẫn bị khóa, nên kẻ dò mật khẩu không thể thử hàng loạt."

*(Bước này đáp ứng đúng yêu cầu "login an toàn" của cô, và nhìn trực quan nhất.)*

> ⚠️ **Phải làm CUỐI.** Khóa tính theo **địa chỉ IP**, nên sau khi bị khóa thì **mọi tài khoản trên máy này** đều không đăng nhập được trong 60 giây (không chỉ `demo01`). Làm sớm sẽ chặn luôn các bước sau. Muốn demo lại lần nữa thì chờ hết 60 giây, hoặc khởi động lại Server (bộ đếm nằm trong bộ nhớ, tắt Server là xóa).

### Bước F: Phương án dự phòng, nói miệng 15 giây
> "Nếu mất mạng, em đặt BATTLESHIP_DB bằng sqlite và game vẫn chạy bình thường với dữ liệu cục bộ."
Chỉ làm thật nếu cô muốn xem:
```powershell
$env:BATTLESHIP_DB = "sqlite"
dotnet run --project Battleship.Server
```
(Chú ý: lệnh này chỉ có tác dụng trong cửa sổ PowerShell đó, không ảnh hưởng biến vĩnh viễn.)

### Dọn dẹp sau demo
Supabase SQL Editor:
```sql
DELETE FROM "Users" WHERE "Username" = 'demo01';
DELETE FROM "Users" WHERE "Id" LIKE 'seed-%';
```

---

## 5. NHỮNG ĐIỀU TUYỆT ĐỐI KHÔNG NÓI (vì chưa đúng sự thật)
- Đừng nói **"game hoàn chỉnh, chơi được cả trận"**. Logic bắn theo lượt (Turn Engine), xếp tàu và Fog of War **chưa hoàn thành**. Nói: "phần mạng, tài khoản và dữ liệu đã hoàn thiện; phần gameplay đang phát triển".
- Đừng nói **"đã stress test"**. Chưa kiểm thử tải.
- Đừng nói matchmaking chạy trong game nếu `hoang/matchmaking-queue` chưa merge.
- Đừng nói **"hệ thống an toàn tuyệt đối"**. Nói: "đã xử lý các nguy cơ phổ biến và kiểm thử 19 kịch bản".

---

## 6. CÂU HỎI THƯỜNG GẶP VÀ ĐÁP ÁN

**1. Sao chọn Supabase mà không phải Firebase?**
Supabase là PostgreSQL nên dùng lại được Entity Framework Core và mô hình quan hệ hiện có, chỉ đổi provider. Firebase là NoSQL, không có EF Core provider, phải viết lại lớp dữ liệu.

**2. Web service ở đây là gì, sao không tự viết API?**
Supabase sinh sẵn REST API (PostgREST) cho mọi bảng hoặc view. Em tận dụng để tập trung vào phần socket là trọng tâm môn học, và vẫn có đủ một web service để truy vấn dữ liệu từ bên ngoài.

**3. Khóa công khai bị lộ thì sao?**
Khóa này được thiết kế để công khai. Bảo vệ dữ liệu nằm ở Row Level Security: bảng gốc bị khóa, chỉ view `leaderboard` không có cột mật khẩu mới đọc được. Em đã kiểm chứng: dùng chính khóa đó đọc bảng Users thì bị 401. Khóa quản trị (secret) không bao giờ đưa vào Client.

**4. Mật khẩu lưu thế nào?**
BCrypt, tự thêm muối và có hệ số chi phí, nên chống được bảng tra ngược. Ngay trong Table Editor cô cũng thấy chỉ có chuỗi băm.

**5. Chống dò mật khẩu ra sao?**
Sai 5 lần thì khóa 60 giây, tính theo cả IP và theo tài khoản, và khóa cả khi nhập đúng trong thời gian khóa. Thông báo lỗi chung chung và luôn chạy BCrypt để không lộ tên nào tồn tại.

**6. Dữ liệu truyền qua mạng có an toàn không?**
Gói tin TCP được mã hóa AES-128 (CBC, IV ngẫu nhiên mỗi gói). Hạn chế đã biết: khóa đang nằm trong mã nguồn (phù hợp đồ án, thực tế nên trao đổi khóa hoặc dùng TLS), và CBC không có kiểm tra toàn vẹn nên hướng nâng cấp là AES-GCM.
*(Chỉ nói câu này khi `hoang/aes-encryption` đã được merge.)*

**7. Mất mạng thì game còn chạy không?**
Còn, đặt biến môi trường sang SQLite thì dữ liệu lưu cục bộ, không cần sửa code.

**8. Sao vẫn dùng TCP socket mà không dùng HTTP hết?**
Socket là nội dung cốt lõi của môn (framing, async, handshake). Game cần kết nối giữ lâu và đẩy sự kiện hai chiều tới người chơi, điều mà request đáp ứng một chiều của HTTP làm không tự nhiên.

**9. Xử lý hai người cùng đăng ký một tên thế nào?**
Có ràng buộc UNIQUE ở cơ sở dữ liệu và bắt lỗi trùng, nên một trong hai bị từ chối, không crash. Tên cũng so sánh không phân biệt hoa thường.

**10. Có xử lý đa luồng không?**
Server dùng async/await, mỗi client một Task, `ConcurrentDictionary` cho danh sách session, khóa riêng cho từng phòng và khóa gửi để hai Task không ghi chen byte vào cùng một stream.

**11. Còn thiếu gì so với kế hoạch?**
Trả lời thật: "Phần mạng, tài khoản, bảo mật và dữ liệu cloud đã xong. Phần gameplay theo lượt, xếp tàu, sương mù và kiểm thử tải còn lại của các giai đoạn sau, nhóm đang làm tiếp."

**12. Ai làm phần nào?** → Slide 21.

---

## 7. NẾU CÓ SỰ CỐ KHI DEMO

| Sự cố | Xử lý |
|---|---|
| Server báo lỗi kết nối DB | "Mạng đang chập chờn." Đặt `BATTLESHIP_DB=sqlite`, chạy lại, tiếp tục bước B (bỏ phần Supabase), rồi phát video đoạn Supabase |
| REST không trả kết quả quá 5 giây | Dừng, nói "em xin chuyển sang video", bấm phát |
| Client không mở / treo | Mở lại, nếu vẫn lỗi thì chuyển video |
| Cổng 8000 bị chiếm | `Get-NetTCPConnection -LocalPort 8000` rồi tắt tiến trình `Battleship.Server` cũ |
| Quên mật khẩu `demo01` | Đăng ký tài khoản mới `demo02` |
| Cô hỏi điều không biết | "Phần này em chưa kiểm chứng nên em xin không trả lời sai, em sẽ tìm hiểu và bổ sung" |
