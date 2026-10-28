# GIAO VIỆC NGÀY MAI (29/10): ĐỨC VÀ DŨNG

> Báo cáo là **thứ Sáu 30/10**. Ngày mai (29/10) là ngày **chốt slide + tập dượt**.
> **Hoàng là người thuyết trình chính và KHÔNG làm slide nữa.** Slide phần của Đức và Dũng do **chính hai bạn** hoàn thiện (nội dung + ảnh). Hoàng chỉ ghép, biên dịch và tập nói.
> Nhờ vậy cả 3 người đều hiểu phần của nhau khi cô hỏi.

---

## 0. QUY TẮC SỐNG CÒN (đọc trước khi làm)
1. **Mỗi người chỉ sửa file phần của mình.** Đức: `docs/slides/parts/02_server_duc.tex`. Dũng: `docs/slides/parts/03_csdl_dung.tex`. **Không sửa file của người khác**, không sửa `slide_demo.tex`. Nhờ vậy không bao giờ bị conflict.
2. **Chỉ ghi điều ĐÃ LÀM THẬT.** Cái nào chưa làm thì ghi "đang phát triển". Cô hỏi sâu vào chỗ mình nói khống là mất điểm.
3. **Không để lộ bí mật trong ảnh**: mật khẩu DB, chuỗi kết nối, `sb_secret_...`. (Chi tiết ở `docs/slides/images/README.md`.)
4. **Mỗi slide ~40 giây khi nói.** Chữ ngắn, mỗi ý một dòng.
5. **Kẹt quá 15 phút thì nhắn Hoàng ngay**, đừng im lặng tới tối.

---

## 1. MỞ ĐẦU (cả hai làm trong 20 phút đầu, trước khi làm việc riêng)

```bash
git checkout main
git pull origin main
```
Sau đó **đọc 3 file** (mỗi file 5 phút):
- `docs/HuongDan_Supabase_WebService.md`: hiểu Supabase và web service (đọc **Phần A** và **Phần D**).
- `docs/KichBan_Thuyet_Trinh.md`: biết Hoàng sẽ nói gì và khi nào đến phần của mình.
- `docs/slides/HUONG_DAN_LAM_SLIDE.md`: cách sửa slide.

**Chạy thử hệ thống ở chế độ OFFLINE (không cần mật khẩu Supabase)**, để chụp ảnh giao diện:
```powershell
cd <thư-mục-repo>
$env:BATTLESHIP_DB = "sqlite"
dotnet run --project Battleship.Server
```
Mở **cửa sổ PowerShell khác**:
```powershell
cd <thư-mục-repo>
dotnet run --project Battleship.Client
```
(Chi tiết ở `HuongDan_Supabase_WebService.md` mục D.)

---

## 2. VIỆC CỦA ĐỨC

**Branch:** `duc/slides-phan-duc` · **File:** `docs/slides/parts/02_server_duc.tex`

| # | Việc | Hạn | Xong khi |
|---|---|---|---|
| Đ1 | Đọc bản nháp 4 slide trong file, **sửa cho đúng với code của mình** (chỗ có dấu `%%% ĐỨC KIỂM TRA`) | 12:00 | Không còn ý nào sai so với `RoomManager.cs`, `RoomHandler.cs` |
| Đ2 | Chụp **`11-room-list.png`**: màn hình *Join Room* ở Client | 14:00 | File nằm trong `docs/slides/images/` |
| Đ3 | Lưu **`12-state-machine.png`**: hình State Machine của Đức (đang ở README trên GitHub: chuột phải vào hình → Lưu ảnh) | 14:00 | File nằm trong `images/` |
| Đ4 | *(Tùy chọn)* **`13-wireshark-aes.png`**: Wireshark bắt gói đăng nhập, dữ liệu là byte lộn xộn. Cần Npcap Loopback. Không làm được thì **xóa slide 4** hoặc bỏ qua | 15:00 | |
| Đ5 | **Quay video dự phòng phần OFFLINE**: chạy Server bằng SQLite + 1 đến 2 Client, đăng ký, đăng nhập, xem danh sách phòng. Quay màn hình bằng **`Win + Alt + R`** (Xbox Game Bar). Có thuyết minh ngắn | 16:00 | Có file `.mp4`, gửi Hoàng |
| Đ6 | Mở **Pull Request** vào `main` | **17:00** | PR hiện đúng 1 file `.tex` + vài ảnh |

> Slide 1 có ảnh `10-server-log.png` (cửa sổ Server nối Supabase): **Hoàng chụp**, vì cần mật khẩu Supabase. Đức bỏ qua ảnh này, khung xám vẫn nằm đó đến khi Hoàng bỏ ảnh vào.

**Câu hỏi cô có thể hỏi Đức (chuẩn bị trả lời):**
1. `RoomManager` chống hai người vào cùng một phòng cùng lúc như thế nào? *(khóa riêng từng phòng + `ConcurrentDictionary`)*
2. State Machine của phòng gồm những trạng thái nào, cái nào đã code, cái nào chưa?
3. Vì sao server phải là bên quyết định Hit/Miss mà không để client tự tính?
4. Một client đang ở trong phòng mà mất kết nối đột ngột thì chuyện gì xảy ra?

---

## 3. VIỆC CỦA DŨNG

**Branch:** `dung/slides-phan-dung` · **File:** `docs/slides/parts/03_csdl_dung.tex`

| # | Việc | Hạn | Xong khi |
|---|---|---|---|
| D1 | **Gắn nút "Bảng xếp hạng" ở Lobby**: bấm thì `new frmLeaderboard().ShowDialog();` (form đã có trong `main`, tự tải dữ liệu). Làm ở branch **riêng** `dung/lobby-leaderboard-button`, PR riêng | 12:00 | Bấm nút thấy danh sách |
| D2 | **Kiểm tra thông báo khóa ở Client**: đăng nhập sai 5 lần liên tiếp thì Client có hiện *"Đăng nhập sai quá nhiều lần. Thử lại sau 60 giây."* không? Nếu Client chỉ báo chung chung "thất bại" thì **sửa để hiện đúng thông báo server trả về** hoặc báo Hoàng | 13:00 | Thấy đúng thông báo |
| D3 | Đọc bản nháp 4 slide, **sửa cho đúng với Models** (chỗ có dấu `%%% DŨNG KIỂM TRA`) | 14:00 | ERD khớp `Models/*.cs` |
| D4 | Chụp **`04-client-login.png`**, **`05-client-lobby.png`**, **`07-login-lockout.png`** (và **`06-client-leaderboard.png`** nếu D1 xong) | 15:00 | 3 đến 4 file trong `images/` |
| D5 | Chụp **`01-supabase-tables.png`**, **`02-rest-blocked.png`**, **`03-rest-json.png`**. Cần được **mời vào project Supabase** (Hoàng làm sáng nay, kiểm tra email). Ảnh 02 và 03 chụp từ script `docs\demo\demo_rest.ps1` | 16:00 | 3 file trong `images/` |
| D6 | Mở **Pull Request** slide vào `main` | **17:00** | PR hiện đúng 1 file `.tex` + vài ảnh |

> Bảng xếp hạng đang **trống**. Trước khi chụp `01`, `03`, `06`, Hoàng sẽ chạy dữ liệu mẫu trên Supabase sáng nay. Nếu thấy vẫn trống thì nhắn Hoàng.

**Câu hỏi cô có thể hỏi Dũng (chuẩn bị trả lời):**
1. Có mấy bảng? Quan hệ giữa chúng là gì? Vì sao `MatchDetails` tách riêng?
2. Mật khẩu lưu thế nào? BCrypt khác băm thường (MD5, SHA) chỗ nào? *(có muối, cố tình chậm)*
3. Vì sao bảng `Users` có `Id` kiểu `string`? *(Guid, tự sinh ở code)*
4. Tại sao phải bật RLS và tạo view `leaderboard`?

---

## 4. VIỆC CHUNG
| Việc | Hạn |
|---|---|
| Cả hai **đọc kịch bản** (`docs/KichBan_Thuyet_Trinh.md`), biết khi nào Hoàng chuyển sang phần mình để **thao tác máy hoặc bổ sung** | 17:00 |
| Có mặt **tập dượt lần 1** (online hoặc trực tiếp) | **20:00** |
| Có mặt **tập dượt lần 2** + chốt | **22:00** |

---

## 5. CÁCH NỘP (làm đúng từng dòng)

**Đức:**
```bash
git checkout main
git pull origin main
git checkout -b duc/slides-phan-duc
# ... sửa docs/slides/parts/02_server_duc.tex, bỏ ảnh vào docs/slides/images/ ...
git add docs/slides/parts/02_server_duc.tex docs/slides/images
git commit -m "Docs: slide phan kien truc server va game"
git push -u origin duc/slides-phan-duc
```
**Dũng:** giống hệt, thay branch thành `dung/slides-phan-dung` và file thành `docs/slides/parts/03_csdl_dung.tex`.

Rồi lên GitHub mở **Pull Request** vào `main` (GitHub hiện link ngay sau khi push). **Hoàng sẽ merge**, hai bạn không tự merge.

> ⚠️ **Chỉ `git add` đúng file mình sửa** như trên, **không** `git add .` (dễ kéo theo file rác).
> ⚠️ Tên ảnh **phải đúng** như trong `images/README.md`, vì slide tìm ảnh theo tên.

---

## 6. HOÀNG LÀM GÌ (để hai bạn biết mà phối hợp)
| Việc | Hạn |
|---|---|
| Merge PR tài liệu/slide gốc để hai bạn `git pull` được | tối nay |
| **Mời Dũng (và Đức) vào project Supabase**, chạy dữ liệu mẫu | sáng 29/10 |
| Chụp ảnh `10-server-log.png` | sáng 29/10 |
| **Review và merge** PR của hai bạn, biên dịch slide | 17:00 đến 19:00 |
| **Quay video dự phòng phần Supabase + REST** | 21:00 |
| Tập dượt, dẫn buổi tập | 20:00 và 22:00 |

---

## 7. CHECKLIST CUỐI NGÀY (ai cũng tự tick)
- [ ] `git pull origin main` rồi `dotnet build BattleshipNetwork.slnx` ra **0 error**.
- [ ] Đã mở PR slide phần mình.
- [ ] Mọi ảnh đã nằm đúng tên trong `docs/slides/images/` (hoặc đã xóa khung ảnh không dùng).
- [ ] Đã đọc kịch bản và biết khi nào mình thao tác máy.
- [ ] Trả lời được 4 câu hỏi của phần mình mà không nhìn tài liệu.
