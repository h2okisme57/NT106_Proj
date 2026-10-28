# HƯỚNG DẪN LÀM SLIDE (template Beamer)

Bộ slide dùng **LaTeX Beamer** với template xanh của nhóm. Mỗi người chỉ sửa **file phần của mình** trong `parts/`.
Không biết LaTeX cũng làm được: chỉ cần **sửa chữ trong các mẫu có sẵn** và bỏ ảnh vào thư mục `images/`.

## 1. Cấu trúc thư mục
```
docs/slides/
├── slide_demo.tex          <- file ghép (KHÔNG sửa)
├── parts/
│   ├── 01_tongquan.tex     Hoàng
│   ├── 02_server_duc.tex   ĐỨC   <- Đức sửa file này
│   ├── 03_csdl_dung.tex    DŨNG  <- Dũng sửa file này
│   ├── 04_cloud_hoang.tex  Hoàng
│   ├── 05_demo.tex         Hoàng
│   └── 06_nhom.tex         Hoàng
└── images/                 <- bỏ ảnh vào đây (xem images/README.md)
```
Thứ tự slide: Tổng quan → **Đức** → **Dũng** → Cloud → Demo → Nhóm.

## 2. Ba cách xem slide (chọn một)

**Cách A: Overleaf (không cần cài gì, khuyên dùng):**
1. Vào **overleaf.com**, đăng ký miễn phí.
2. Nén thư mục `docs/slides` thành `.zip` → **New Project → Upload Project**.
3. **Menu (góc trên trái) → Compiler → chọn `XeLaTeX`** (bắt buộc, mặc định là pdfLaTeX sẽ lỗi).
4. Bấm **Recompile**. Xem PDF bên phải.

**Cách B: Cài MiKTeX trên máy:** mở terminal trong `docs/slides` rồi chạy **2 lần**:
```powershell
xelatex slide_demo.tex
xelatex slide_demo.tex
```
(Chạy 2 lần để mục lục và số trang đúng.)

**Cách C: Không biên dịch:** chỉ sửa file `.tex` và ảnh rồi nộp PR. Hoàng sẽ biên dịch và gửi lại PDF để xem.

> Không có thư mục `fonts/` và `assets/` thì slide tự dùng font mặc định, **vẫn biên dịch được**. Không cần lo.

## 3. Các mẫu hay dùng (copy rồi sửa chữ)

### Một slide có khối chữ
```latex
\begin{frame}{Tiêu Đề Slide}
  \begin{block}{Tên khối}
    \begin{itemize}
      \item Ý thứ nhất.
      \item Ý thứ hai, chữ \textbf{in đậm} một vài từ.
      \item Tên hàm hay tên file thì bọc \texttt{RoomManager.cs}.
    \end{itemize}
  \end{block}
\end{frame}
```
Có 3 loại khối: `block` (xanh đậm), `alertblock` (xanh dương, dùng cho vấn đề/lưu ý), `exampleblock` (vàng, dùng cho kết luận/ví dụ).

### Hai cột: chữ bên trái, ảnh bên phải
```latex
\begin{frame}{Tiêu Đề}
  \begin{columns}[T]
    \begin{column}{0.50\textwidth}
      \begin{block}{Ý chính}
        \begin{itemize}
          \item Nội dung.
        \end{itemize}
      \end{block}
    \end{column}
    \begin{column}{0.46\textwidth}
      \centering
      \slideimg[\textwidth]{ten-file.png}{Mô tả ảnh này chụp gì}
    \end{column}
  \end{columns}
\end{frame}
```

### Chèn ảnh
```latex
\slideimg[0.8\textwidth]{05-client-lobby.png}{Màn hình Lobby}
```
- `[0.8\textwidth]` là chiều rộng (80% bề rộng slide). Bỏ trống thì mặc định 90%.
- Chưa có file ảnh thì hiện **khung xám** ghi tên file cần bỏ vào. Có rồi thì tự hiện ảnh.
- **Ảnh không dùng nữa thì xóa luôn dòng `\slideimg` đó** (đừng để khung xám trong bản nộp cuối).

### Bảng
```latex
\begin{tabularx}{\textwidth}{@{}Y Y@{}}
  \toprule
  \textbf{Cột 1} & \textbf{Cột 2} \\
  \midrule
  Dòng 1 ô 1 & Dòng 1 ô 2 \\
  Dòng 2 ô 1 & Dòng 2 ô 2 \\
  \bottomrule
\end{tabularx}
```
Muốn 3 cột thì viết `{@{}Y Y Y@{}}`.

### Xóa một slide
Xóa từ dòng `\begin{frame}` đến dòng `\end{frame}` tương ứng.

## 4. Lỗi hay gặp khi gõ LaTeX
Mấy ký tự này **phải thêm dấu `\` phía trước**, nếu không biên dịch sẽ lỗi:

| Muốn hiện | Phải gõ |
|---|---|
| `_` (gạch dưới) | `\_`  (ví dụ `X\_Coordinate`) |
| `%` | `\%` |
| `&` | `\&` |
| `#` | `\#` |
| `$` | `\$` |
| `{` `}` | `\{` `\}` |
| `C#` | `C\#` |

Ngoài ra:
- Mỗi `\begin{...}` phải có `\end{...}` tương ứng. Thiếu là lỗi.
- Dòng bắt đầu bằng `%` là **ghi chú**, LaTeX bỏ qua (dùng để nhắn nhau).
- Lỗi đỏ khó hiểu: xem **dòng đầu tiên** báo lỗi trong log, nó chỉ đúng chỗ. Gửi Hoàng nếu kẹt.

## 5. Nguyên tắc nội dung slide
- **3 đến 5 ý mỗi slide**, mỗi ý một dòng ngắn. Slide không phải bài văn.
- Người nghe đọc slide trong lúc Hoàng nói, nên **chữ ít, ảnh nhiều**.
- Mọi ảnh phải **đọc được từ xa** (cắt gọn, chụp to).
- Chỉ ghi điều **đã làm thật**.
