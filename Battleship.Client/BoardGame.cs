using Battleship.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Battleship.Client
{
    public class ShipInfo
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public bool IsPlaced { get; set; } = false;
        public List<Point> OccupiedCells { get; set; } = new List<Point>();

        // Đếm số ô đã bị bắn trúng
        public int HitCount { get; set; } = 0;
        public bool IsSunk => HitCount >= Size;

        public ShipInfo(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }

    public class CellButton : Button
    {
        public bool IsShot { get; set; } = false;          // Đã bị bắn chưa
        public bool IsPending { get; private set; } = false; // Đang gửi lên Server, chờ kết quả
        public bool HasShip { get; set; } = false;         // Có tàu không

        public void MarkPendingShot()
        {
            if (IsShot || IsPending) return;
            IsPending = true;
            this.Cursor = Cursors.WaitCursor;
        }

        // Hoàn tác nếu Server trả ShotResultDto.IsValid == false hoặc PacketType.Error
        public void CancelPendingShot()
        {
            if (!IsPending || IsShot) return;
            IsPending = false;
            this.Cursor = Cursors.Hand;
        }

        public void SetShotState(bool isHit)
        {
            if (IsShot) return;

            IsPending = false;
            IsShot = true;
            HasShip = isHit;

            // Trúng tàu -> Đỏ | Bắn hụt -> Trắng xám
            Color resultColor = isHit
                ? Color.FromArgb(220, 53, 69)    // Đỏ
                : Color.FromArgb(215, 220, 225); // Trắng xám

            this.BackColor = resultColor;
            this.FlatAppearance.MouseOverBackColor = resultColor;
            this.FlatAppearance.MouseDownBackColor = resultColor;
            this.Cursor = Cursors.No;
        }

        public void ResetState()
        {
            IsPending = false;
            IsShot = false;
            HasShip = false;
            this.BackColor = Color.Transparent;
            this.FlatAppearance.MouseOverBackColor = Color.Transparent;
            this.FlatAppearance.MouseDownBackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
        }
    }

    public partial class BoardGame : UserControl
    {
        public bool IsOnlineMode { get; set; } = true;
        public bool IsWaitingShotResult { get; set; } = false;
        public string CurrentRoomId { get; set; } = string.Empty;

        private CellButton[,] cells = new CellButton[10, 10];
        private Image waterTile;
        private bool isResizing = false;
        private Point currentHover = new Point(-1, -1);

        private Timer turnTimer;
        private int secondsRemaining = 30;
        private bool _isEnemyBoard = false;

        public bool IsEnemyBoard
        {
            get => _isEnemyBoard;
            set
            {
                _isEnemyBoard = value;
                if (_isEnemyBoard)
                {
                    IsPlacementPhase = false;
                }
            }
        }

        public bool IsPlacementPhase { get; set; } = true;
        public bool IsHorizontal { get; set; } = true;
        public bool IsMyTurn { get; private set; } = false;
        public int RemainingShots { get; private set; } = 0;

        public ShipInfo SelectedShip { get; set; }
        public int[,] BoardGrid { get; private set; } = new int[10, 10];
        public List<ShipInfo> PlayerShips { get; private set; } = new List<ShipInfo>();

        public event Action<int, int> OnCellClicked;
        public event Action<ShipInfo> OnShipPlacedSuccessfully;
        public event Action OnAllShipsPlaced;
        public event Action<int> OnRemainingShotsChanged;
        public event Action<int> OnTimerTick;
        public event Action OnTurnEnded;
        public event Action<List<ShipInfo>> OnShipConditionUpdated;

        public BoardGame()
        {
            InitializeComponent();
            waterTile = Properties.Resources.OceanBox;

            typeof(TableLayoutPanel).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                                    ?.SetValue(tableGame, true, null);

            InitializeTurnTimer();
            InitializeDefaultShips();
            GenerateSingleBoard();

            tableGame.Paint += tableGame_Paint;
        }

        private void InitializeTurnTimer()
        {
            turnTimer = new Timer();
            turnTimer.Interval = 1000;
            turnTimer.Tick += TurnTimer_Tick;
        }

        private void TurnTimer_Tick(object sender, EventArgs e)
        {
            secondsRemaining--;
            OnTimerTick?.Invoke(secondsRemaining);

            if (secondsRemaining <= 0)
            {
                EndTurn(isTimeout: true);
            }
        }

        public void StartTurn(int durationSeconds = 30)
        {
            IsMyTurn = true;
            IsWaitingShotResult = false;
            RemainingShots = 5;
            secondsRemaining = durationSeconds > 0 ? durationSeconds : 30;

            OnRemainingShotsChanged?.Invoke(RemainingShots);
            OnTimerTick?.Invoke(secondsRemaining);

            turnTimer.Stop();
            turnTimer.Start();
        }

        public void EndTurn(bool isTimeout = false)
        {
            turnTimer.Stop();
            IsMyTurn = false;
            IsWaitingShotResult = false;
            RemainingShots = 0;

            OnRemainingShotsChanged?.Invoke(RemainingShots);
            OnTurnEnded?.Invoke();
        }

        private void InitializeDefaultShips()
        {
            PlayerShips = new List<ShipInfo>
            {
                new ShipInfo("Carrier", 5),
                new ShipInfo("Battleship", 4),
                new ShipInfo("Cruiser", 3),
                new ShipInfo("Submarine", 3),
                new ShipInfo("Destroyer", 2)
            };
            SelectedShip = PlayerShips[0];
        }

        private void GenerateSingleBoard()
        {
            tableGame.BackgroundImage = waterTile;
            tableGame.BackgroundImageLayout = ImageLayout.Stretch;
            tableGame.Dock = DockStyle.None;
            tableGame.Margin = new Padding(0);
            tableGame.Padding = new Padding(0);
            tableGame.RowCount = 10;
            tableGame.ColumnCount = 10;
            tableGame.RowStyles.Clear();
            tableGame.ColumnStyles.Clear();

            for (int i = 0; i < 10; i++)
            {
                tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
                tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            }

            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    CellButton btn = new CellButton()
                    {
                        Dock = DockStyle.Fill,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.Transparent,
                        Tag = new Point(col, row),
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0),
                        TabStop = false
                    };

                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

                    btn.MouseDown += Cell_MouseDown;
                    btn.MouseEnter += Cell_MouseEnter;
                    btn.MouseLeave += Cell_MouseLeave;

                    cells[col, row] = btn;
                    tableGame.Controls.Add(btn, col, row);
                }
            }
        }

        public void ToggleRotation()
        {
            IsHorizontal = !IsHorizontal;
            tableGame.Invalidate();
        }

        public bool CanPlaceShip(int startCol, int startRow, int size, bool isHorizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int c = startCol + (isHorizontal ? i : 0);
                int r = startRow + (isHorizontal ? 0 : i);

                if (c >= 10 || r >= 10) return false;
                if (BoardGrid[c, r] == 1) return false;
            }
            return true;
        }

        private void RegisterHitOnShip(int col, int row)
        {
            Point shotPoint = new Point(col, row);
            var hitShip = PlayerShips.FirstOrDefault(s => s.OccupiedCells.Contains(shotPoint));
            if (hitShip != null && hitShip.HitCount < hitShip.Size)
            {
                hitShip.HitCount++;
                OnShipConditionUpdated?.Invoke(PlayerShips);
            }
        }

        private async void Cell_MouseDown(object sender, MouseEventArgs e)
        {
            CellButton btn = sender as CellButton;
            if (btn == null) return;

            Point coords = (Point)btn.Tag;
            int col = coords.X;
            int row = coords.Y;

            // --- NHÁNH 1: BẢNG ĐỊCH (BẮN TÀU) ---
            if (IsEnemyBoard)
            {
                if (e.Button != MouseButtons.Left) return;
                if (btn.IsShot || btn.IsPending) return;
                if (IsPlacementPhase || !IsMyTurn) return;

                if (IsOnlineMode)
                {
                    if (IsWaitingShotResult || RemainingShots <= 0) return;

                    if (NetworkManager.Instance == null || !NetworkManager.Instance.IsConnected || string.IsNullOrWhiteSpace(CurrentRoomId))
                        return;

                    IsWaitingShotResult = true;
                    btn.MarkPendingShot();
                    OnCellClicked?.Invoke(col, row);

                    try
                    {
                        var shotDto = new FireShotDto
                        {
                            RoomId = CurrentRoomId,
                            X = col,
                            Y = row
                        };
                        await NetworkManager.Instance.SendAsync(Packet.Create(PacketType.FireShotRequest, shotDto));
                    }
                    catch
                    {
                        CancelPendingShot(col, row);
                    }
                    return;
                }

                // Chế độ Test offline
                if (RemainingShots <= 0) return;
                RemainingShots--;
                OnRemainingShotsChanged?.Invoke(RemainingShots);
                btn.SetShotState(btn.HasShip);
                if (btn.HasShip) RegisterHitOnShip(col, row);
                OnCellClicked?.Invoke(col, row);

                if (RemainingShots <= 0) EndTurn(isTimeout: false);
                return;
            }

            // --- NHÁNH 2: BẢNG MÌNH (ĐẶT TÀU) ---
            if (e.Button == MouseButtons.Right)
            {
                if (IsPlacementPhase) ToggleRotation();
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (IsPlacementPhase && SelectedShip != null && !SelectedShip.IsPlaced)
                {
                    if (!CanPlaceShip(col, row, SelectedShip.Size, IsHorizontal)) return;

                    SelectedShip.OccupiedCells.Clear();
                    for (int i = 0; i < SelectedShip.Size; i++)
                    {
                        int c = col + (IsHorizontal ? i : 0);
                        int r = row + (IsHorizontal ? 0 : i);

                        BoardGrid[c, r] = 1;
                        cells[c, r].HasShip = true;
                        SelectedShip.OccupiedCells.Add(new Point(c, r));
                    }

                    SelectedShip.IsPlaced = true;
                    OnShipPlacedSuccessfully?.Invoke(SelectedShip);

                    SelectedShip = PlayerShips.FirstOrDefault(s => !s.IsPlaced);
                    if (SelectedShip == null)
                    {
                        IsPlacementPhase = false;
                        tableGame.Invalidate();
                        MessageBox.Show("Đã bố trí xong hạm đội 5 tàu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OnAllShipsPlaced?.Invoke();
                    }
                }

                OnCellClicked?.Invoke(col, row);
                tableGame.Invalidate();
            }
        }

        private void Cell_MouseEnter(object sender, EventArgs e)
        {
            if (IsEnemyBoard) return;
            CellButton btn = sender as CellButton;
            if (btn == null) return;
            currentHover = (Point)btn.Tag;
            tableGame.Invalidate();
        }

        private void Cell_MouseLeave(object sender, EventArgs e)
        {
            if (IsEnemyBoard) return;
            currentHover = new Point(-1, -1);
            tableGame.Invalidate();
        }

        public void CancelPendingShot(int col, int row)
        {
            IsWaitingShotResult = false;
            if (col >= 0 && col < 10 && row >= 0 && row < 10)
            {
                cells[col, row]?.CancelPendingShot();
            }
        }

        public void ClearAllPendingShots()
        {
            IsWaitingShotResult = false;
            for (int col = 0; col < 10; col++)
            {
                for (int row = 0; row < 10; row++)
                {
                    cells[col, row]?.CancelPendingShot();
                }
            }
        }

        private void tableGame_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int w = tableGame.Width;
            int h = tableGame.Height;
            float cellW = w / 10f;
            float cellH = h / 10f;

            // 1. VẼ ĐƯỜNG LƯỚI VIỀN XANH
            Color Outline = Color.FromArgb(88, 160, 200);
            using (Pen pen = new Pen(Outline, 2))
            {
                for (int i = 0; i <= 10; i++)
                {
                    g.DrawLine(pen, i * cellW, 0, i * cellW, h);
                    g.DrawLine(pen, 0, i * cellH, w, i * cellH);
                }
            }

            // 2. VẼ TÀU (Chỉ vẽ trên bảng mình)
            if (!IsEnemyBoard)
            {
                using (SolidBrush shipBrush = new SolidBrush(Color.FromArgb(210, 80, 90, 100)))
                using (Pen shipBorderPen = new Pen(Color.FromArgb(255, 200, 220, 240), 1.5f))
                {
                    foreach (var ship in PlayerShips)
                    {
                        if (ship.IsPlaced && ship.OccupiedCells.Count > 0)
                        {
                            int minX = int.MaxValue, minY = int.MaxValue;
                            int maxX = int.MinValue, maxY = int.MinValue;

                            foreach (Point pt in ship.OccupiedCells)
                            {
                                if (pt.X < minX) minX = pt.X;
                                if (pt.Y < minY) minY = pt.Y;
                                if (pt.X > maxX) maxX = pt.X;
                                if (pt.Y > maxY) maxY = pt.Y;
                            }

                            float rectX = minX * cellW;
                            float rectY = minY * cellH;
                            float rectW = (maxX - minX + 1) * cellW;
                            float rectH = (maxY - minY + 1) * cellH;

                            g.FillRectangle(shipBrush, rectX, rectY, rectW, rectH);
                            g.DrawRectangle(shipBorderPen, rectX, rectY, rectW, rectH);
                        }
                    }
                }
            }

            // 3. VẼ BÓNG MỜ TRƯỚC KHI ĐẶT TÀU
            if (!IsEnemyBoard && IsPlacementPhase && SelectedShip != null && !SelectedShip.IsPlaced && currentHover.X >= 0)
            {
                bool isValid = CanPlaceShip(currentHover.X, currentHover.Y, SelectedShip.Size, IsHorizontal);
                Color hoverColor = isValid ? Color.FromArgb(140, 46, 204, 113) : Color.FromArgb(140, 231, 76, 60);

                using (SolidBrush hoverBrush = new SolidBrush(hoverColor))
                {
                    for (int i = 0; i < SelectedShip.Size; i++)
                    {
                        int c = currentHover.X + (IsHorizontal ? i : 0);
                        int r = currentHover.Y + (IsHorizontal ? 0 : i);

                        if (c < 10 && r < 10)
                        {
                            g.FillRectangle(hoverBrush, c * cellW, r * cellH, cellW, cellH);
                        }
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if (isResizing) return;
            isResizing = true;

            int baseSize = Math.Min(this.Width, this.Height);
            int perfectSize = (baseSize / 10) * 10;

            tableGame.Size = new Size(perfectSize, perfectSize);
            tableGame.Location = new Point((this.Width - perfectSize) / 2, (this.Height - perfectSize) / 2);

            isResizing = false;
            base.OnResize(e);
        }

        public void SetCellImage(int col, int row, Image img)
        {
            if (col >= 0 && col < 10 && row >= 0 && row < 10)
                cells[col, row].BackgroundImage = img;
        }

        public void ClearCellImage(int col, int row)
        {
            if (col >= 0 && col < 10 && row >= 0 && row < 10)
                cells[col, row].BackgroundImage = null;
        }

        // Gọi khi Server trả kết quả bắn (hoặc địch bắn sang bảng mình)
        public void UpdateCellShot(int col, int row, bool isHit)
        {
            IsWaitingShotResult = false;
            if (col < 0 || col >= 10 || row < 0 || row >= 10) return;

            if (!cells[col, row].IsShot)
            {
                cells[col, row].SetShotState(isHit);

                if (IsEnemyBoard && IsOnlineMode && RemainingShots > 0)
                {
                    RemainingShots--;
                    OnRemainingShotsChanged?.Invoke(RemainingShots);
                }

                if (isHit) RegisterHitOnShip(col, row);
            }
        }

        /// <summary>
        /// Đặt cố định 5 tàu để Test (không ngẫu nhiên):
        /// - horizontal = true : xếp ngang từ Cột 1 (X = 0), tàu đầu nằm trọn Hàng ngang 1 (Y = 0)
        /// - horizontal = false: xếp dọc từ Hàng 1 (Y = 0), tàu đầu nằm trọn Cột dọc 1 (X = 0)
        /// </summary>
        public void PlaceFixedTestShips(bool horizontal = true)
        {
            Array.Clear(BoardGrid, 0, BoardGrid.Length);
            for (int c = 0; c < 10; c++)
            {
                for (int r = 0; r < 10; r++)
                {
                    cells[c, r]?.ResetState();
                }
            }

            if (PlayerShips == null || PlayerShips.Count == 0)
            {
                InitializeDefaultShips();
            }

            for (int index = 0; index < PlayerShips.Count; index++)
            {
                var ship = PlayerShips[index];
                ship.OccupiedCells.Clear();
                ship.HitCount = 0;
                ship.IsPlaced = true;

                for (int step = 0; step < ship.Size; step++)
                {
                    int x = horizontal ? step : index;
                    int y = horizontal ? index : step;

                    BoardGrid[x, y] = 1;
                    cells[x, y].HasShip = true;
                    ship.OccupiedCells.Add(new Point(x, y));
                }
            }

            SelectedShip = null;
            IsPlacementPhase = false;
            tableGame.Invalidate();
        }

        public async Task SendMyShipsToServerAsync(string roomId)
        {
            CurrentRoomId = roomId;
            if (NetworkManager.Instance == null || !NetworkManager.Instance.IsConnected) return;
            var shipDtos = new List<ShipPlacementDto>();

            foreach (var ship in PlayerShips)
            {
                if (!ship.IsPlaced || ship.OccupiedCells.Count == 0) continue;

                var firstCell = ship.OccupiedCells[0];
                bool isHorizontal = true;

                if (ship.OccupiedCells.Count > 1)
                {
                    var secondCell = ship.OccupiedCells[1];
                    isHorizontal = (firstCell.Y == secondCell.Y);
                }

                shipDtos.Add(new ShipPlacementDto
                {
                    ShipName = ship.Name,
                    StartX = firstCell.X,
                    StartY = firstCell.Y,
                    Size = ship.Size,
                    IsHorizontal = isHorizontal
                });
            }

            var placeShipsDto = new PlaceShipsDto
            {
                RoomId = roomId,
                Ships = shipDtos
            };
            await NetworkManager.Instance.SendAsync(new Packet
            {
                Type = PacketType.PlaceShipsRequest,
                Payload = JsonSerializer.Serialize(placeShipsDto)
            });
            var readyDto = new PlayerReadyDto
            {
                RoomId = roomId,
                IsReady = true
            };
            await NetworkManager.Instance.SendAsync(new Packet
            {
                Type = PacketType.PlayerReady,
                Payload = JsonSerializer.Serialize(readyDto)
            });
        }

        public void ApplyEnemyShipHit(string shipName, bool isSunk = false)
        {
            if (string.IsNullOrEmpty(shipName)) return;

            var ship = PlayerShips.FirstOrDefault(s =>
                string.Equals(s.Name, shipName, StringComparison.OrdinalIgnoreCase));

            if (ship != null)
            {
                ship.IsPlaced = true;
                if (isSunk)
                    ship.HitCount = ship.Size;
                else if (ship.HitCount < ship.Size)
                    ship.HitCount++;

                OnShipConditionUpdated?.Invoke(PlayerShips);
            }
        }

        public void MarkAllShipsReady()
        {
            foreach (var ship in PlayerShips)
            {
                ship.IsPlaced = true;
                ship.HitCount = 0;
            }
            OnShipConditionUpdated?.Invoke(PlayerShips);
        }

        public void ApplyEnemyShipSunk(string sunkShipName)
        {
            if (string.IsNullOrWhiteSpace(sunkShipName)) return;

            var ship = PlayerShips.FirstOrDefault(s =>
                string.Equals(s.Name, sunkShipName, StringComparison.OrdinalIgnoreCase));

            if (ship != null)
            {
                ship.IsPlaced = true;
                ship.HitCount = ship.Size;
                OnShipConditionUpdated?.Invoke(PlayerShips);
            }
        }
    }
}