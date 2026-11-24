# GIAO THỨC GÓI TIN TRẬN ĐẤU (BATTLESHIP)

Tài liệu dùng cho Client ghép giao diện với Turn Engine trên Server.
Mọi gói tin đều bọc trong `Packet` (Framing 4-byte + AES-128-CBC).

## 1. Thứ tự gói tin chuẩn của một trận đấu

| Bước | Gói tin (`PacketType`) | DTO tương ứng | Chiều gửi | Ai nhận | Ghi chú |
|---:|---|---|---|---|---|
| 1 | `PlaceShipsRequest` | `PlaceShipsDto` | Client → Server | Server | Gửi đủ 5 tàu (size `5, 4, 3, 3, 2`). Không cần gửi `RoomId` (Server tự tìm phòng theo session). Hiện Server không gửi gói phản hồi riêng nếu hợp lệ; chỉ log nếu sai. |
| 2 | `PlayerReady` | `PlayerReadyDto` | Client → Server | Server | Báo đã chốt đội hình tàu. Yêu cầu đã gửi `PlaceShipsRequest` hợp lệ trước đó. |
| 3 | `GameStart` | `GameStartDto` | Server → Client | **Cả 2 bên** | Phát khi **cả 2 người** đã bấm `PlayerReady`. Client nhận gói này để mở/chuyển sang màn hình trận đấu. |
| 4 | `TurnChange` | `TurnChangedDto` | Server → Client | **Cả 2 bên** | Phát ngay cùng `GameStart` (`Reason = "GameStart"`). Chủ phòng (`Host`) đi lượt đầu, đồng hồ `30` giây. |
| 5 | `FireShotRequest` | `FireShotDto` | Client → Server | Server | Chỉ người đang giữ lượt gửi tọa độ `(X, Y)` với `0 <= X, Y <= 9`. |
| 6 | `ShotResult` | `ShotResultDto` | Server → Client | **Cả 2 bên** | Báo kết quả phát bắn (`IsHit`, `IsSunk`, `SunkShipName`, `NextTurnUsername`). Bắn trúng (`IsHit = true`) giữ nguyên lượt; bắn hụt (`IsHit = false`) đổi lượt. |
| 7 | `TurnChange` | `TurnChangedDto` | Server → Client | **Cả 2 bên** | Chỉ phát khi **đổi lượt**: do bắn hụt (`Reason = "Miss"`) hoặc hết 30 giây (`Reason = "Timeout"`). |
| 8 | `GameOver` | `GameOverDto` | Server → Client | **Cả 2 bên** | Phát khi 1 bên chìm hết 5 tàu (`17` ô) hoặc đối thủ mất kết nối. |

> **Lưu ý cho Client về đồng hồ 30s:** Bộ đếm 30 giây trên UI chỉ để hiển thị cho người chơi. Server mới là bên chốt hết giờ và tự động gửi `TurnChange` (`Reason = "Timeout"`).

## 2. Xử lý các tình huống lỗi và ngắt kết nối

| Tình huống | Phản ứng của Server | Gói trả về & Người nhận |
|---|---|---|
| Đặt sai tàu (thiếu/thừa tàu, sai size `5,4,3,3,2`, tràn biên, đè tàu) | Không lưu bàn cờ; bấm `PlayerReady` sau đó sẽ bị từ chối. | Hiện chưa có gói lỗi riêng (Server ghi log). Client nên chặn trước ở UI. |
| Bắn khi chưa tới lượt | Bỏ qua yêu cầu, không đổi lượt. | Không trả gói (bị từ chối im lặng). |
| Bắn ngoài biên (`<0` hoặc `>9`) hoặc bắn trùng ô đã bắn | Giữ nguyên lượt người bắn. | Trả `ShotResult` cho **riêng người bắn** với `IsValid = false`. |
| Quá 30 giây không bắn | Server tự chuyển lượt sang đối thủ và reset 30s. | Gửi `TurnChange` cho **cả 2 bên** với `Reason = "Timeout"`. |
| Một bên tắt app hoặc mất mạng giữa trận | Dừng timer, xử thắng cho người ở lại, đóng phòng. | Gửi `GameOver` cho **người ở lại** với `Reason = "OpponentDisconnected"`. |

## 3. Ví dụ JSON payload từng gói

### `PlaceShipsRequest` (Client → Server)
```json
{
  "RoomId": "A1B2C3",
  "Ships": [
    { "ShipName": "Carrier", "Size": 5, "StartX": 0, "StartY": 0, "IsHorizontal": true },
    { "ShipName": "Battleship", "Size": 4, "StartX": 0, "StartY": 2, "IsHorizontal": true },
    { "ShipName": "Cruiser", "Size": 3, "StartX": 0, "StartY": 4, "IsHorizontal": true },
    { "ShipName": "Submarine", "Size": 3, "StartX": 0, "StartY": 6, "IsHorizontal": true },
    { "ShipName": "Destroyer", "Size": 2, "StartX": 0, "StartY": 8, "IsHorizontal": true }
  ]
}
```

### `PlayerReady` (Client → Server)
```json
{
  "RoomId": "A1B2C3",
  "IsReady": true
}
```

### `GameStart` (Server → Cả 2 Client)
```json
{
  "RoomId": "A1B2C3",
  "FirstTurnUsername": "player1",
  "TimeLimitSeconds": 30
}
```

### `TurnChange` (Server → Cả 2 Client)
```json
{
  "RoomId": "A1B2C3",
  "CurrentTurnUsername": "player1",
  "TimeLimitSeconds": 30,
  "Reason": "GameStart"
}
```
*(Giá trị `Reason`: `"GameStart"`, `"Miss"`, `"Timeout"`)*

### `FireShotRequest` (Client → Server)
```json
{
  "RoomId": "A1B2C3",
  "X": 4,
  "Y": 2
}
```

### `ShotResult` (Server → Cả 2 Client)
```json
{
  "RoomId": "A1B2C3",
  "ShooterUsername": "player1",
  "X": 4,
  "Y": 2,
  "IsValid": true,
  "IsHit": true,
  "IsSunk": false,
  "SunkShipName": null,
  "NextTurnUsername": "player1"
}
```

### `GameOver` (Server → Cả 2 Client)
```json
{
  "RoomId": "A1B2C3",
  "WinnerUsername": "player1",
  "LoserUsername": "player2",
  "Reason": "AllShipsSunk"
}
```
*(Giá trị `Reason`: `"AllShipsSunk"`, `"OpponentDisconnected"`, `"Surrender"`, `"Timeout"`)*
