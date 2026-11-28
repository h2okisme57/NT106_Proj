using Battleship.Shared;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        public bool IsShot { get; set; } = false;            // Đã bị bắn chưa
        public bool IsPending { get; private set; } = false; // Đang gửi lên Server, chờ kết quả
        public bool HasShip { get; set; } = false;           // Có tàu không

        protected override bool ShowFocusCues => false;

        public CellButton()
        {
            SetStyle(
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);

            this.BackColor = Color.Transparent;
            this.UseVisualStyleBackColor = false;
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.FlatAppearance.MouseOverBackColor = Color.Transparent;
            this.FlatAppearance.MouseDownBackColor = Color.Transparent;
            this.TabStop = false;
        }

        public void MarkPendingShot()
        {
            if (IsShot || IsPending) return;
            IsPending = true;
            this.Cursor = Cursors.WaitCursor;
            this.Invalidate();
        }

        // Hoàn tác nếu Server trả ShotResultDto.IsValid == false hoặc PacketType.Error
        public void CancelPendingShot()
        {
            if (!IsPending || IsShot) return;
            IsPending = false;
            this.Cursor = Cursors.Hand;
            this.Invalidate();
        }

        public void SetShotState(bool isHit)
        {
            if (IsShot) return;

            IsPending = false;
            IsShot = true;
            HasShip = isHit;
            this.Cursor = Cursors.No;
            this.Invalidate();
        }

        public void ResetState()
        {
            IsPending = false;
            IsShot = false;
            HasShip = false;
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            // Vẽ lại phần nền của tableGame (biển, lưới, tàu) -> nút vô hình khi chưa bắn
            base.OnPaintBackground(pevent);

            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (IsPending)
            {
                float pad = Math.Max(3f, Math.Min(Width, Height) * 0.20f);
                using (Pen pendingPen = new Pen(Color.FromArgb(210, 255, 225, 90), 2f))
                {
                    g.DrawEllipse(pendingPen, pad, pad, Width - pad * 2f, Height - pad * 2f);
                }
                return;
            }

            if (!IsShot) return;

            if (HasShip)
            {
                // Ô bắn trúng: hiển thị vụ nổ
                DrawExplosion(g, Width, Height);
            }
            else
            {
                // Ô bắn hụt: phủ lớp màu xám khói trong suốt
                using (SolidBrush smokeBrush = new SolidBrush(Color.FromArgb(120, 120, 120, 120)))
                {
                    g.FillRectangle(smokeBrush, 0, 0, Width, Height);
                }
            }
        }

        private static void DrawExplosion(Graphics g, int width, int height)
        {
            float cx = width / 2f;
            float cy = height / 2f;
            float maxR = Math.Min(width, height) * 0.46f;

            // Quầng khói đỏ nhẹ phía sau
            float glowR = maxR * 0.92f;
            using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(95, 200, 40, 20)))
            {
                g.FillEllipse(glowBrush, cx - glowR, cy - glowR, glowR * 2f, glowR * 2f);
            }

            // Lớp tia nổ ngoài (đỏ cam)
            PointF[] outerBurst = CreateBurstPoints(cx, cy, maxR, maxR * 0.50f, spikes: 10, angleOffset: -Math.PI / 2);
            using (SolidBrush outerBrush = new SolidBrush(Color.FromArgb(245, 232, 62, 24)))
            {
                g.FillPolygon(outerBrush, outerBurst);
            }

            // Lớp tia nổ giữa (vàng cam)
            PointF[] midBurst = CreateBurstPoints(cx, cy, maxR * 0.72f, maxR * 0.34f, spikes: 8, angleOffset: -Math.PI / 6);
            using (SolidBrush midBrush = new SolidBrush(Color.FromArgb(250, 255, 168, 24)))
            {
                g.FillPolygon(midBrush, midBurst);
            }

            // Tâm vụ nổ sáng chói (vàng trắng)
            float coreR = maxR * 0.28f;
            using (SolidBrush coreBrush = new SolidBrush(Color.FromArgb(255, 255, 246, 190)))
            {
                g.FillEllipse(coreBrush, cx - coreR, cy - coreR, coreR * 2f, coreR * 2f);
            }
        }

        private static PointF[] CreateBurstPoints(float cx, float cy, float outerR, float innerR, int spikes, double angleOffset)
        {
            PointF[] points = new PointF[spikes * 2];
            double step = Math.PI / spikes;

            for (int i = 0; i < points.Length; i++)
            {
                float r = (i % 2 == 0) ? outerR : innerR;
                double angle = angleOffset + i * step;
                points[i] = new PointF(
                    cx + (float)(Math.Cos(angle) * r),
                    cy + (float)(Math.Sin(angle) * r));
            }
            return points;
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
        // Luật: bắn từng phát. Trúng thì được bắn tiếp, hụt thì đổi lượt (Server quyết định).
        public const int ShotsPerTurn = 1;
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

        private void RefreshBoardVisuals()
        {
            tableGame.Invalidate(true);
        }

        private void InitializeTurnTimer()
        {
            turnTimer = new Timer();
            turnTimer.Interval = 1000;
            turnTimer.Tick += TurnTimer_Tick;
        }

        private void TurnTimer_Tick(object sender, EventArgs e)
        {
            if (IsOnlineMode)
            {
                turnTimer.Stop();
                return;
            }

            secondsRemaining--;
            OnTimerTick?.Invoke(secondsRemaining);

            if (secondsRemaining <= 0)
            {
                turnTimer.Stop();
                EndTurn(isTimeout: true);
            }
        }

        public void StartTurn(int durationSeconds = 30)
        {
            IsMyTurn = true;
            IsWaitingShotResult = false;
            RemainingShots = ShotsPerTurn;
            secondsRemaining = durationSeconds > 0 ? durationSeconds : 30;

            OnRemainingShotsChanged?.Invoke(RemainingShots);
            OnTimerTick?.Invoke(secondsRemaining);

            turnTimer.Stop();
            if (!IsOnlineMode)
            {
                turnTimer.Start();
            }
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
                        Tag = new Point(col, row),
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0)
                    };

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
            RefreshBoardVisuals();
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
                if (hitShip.IsSunk)
                {
                    RefreshBoardVisuals();
                }
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
                        RefreshBoardVisuals();
                        MessageBox.Show("Đã bố trí xong hạm đội 5 tàu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OnAllShipsPlaced?.Invoke();
                    }
                }

                OnCellClicked?.Invoke(col, row);
                RefreshBoardVisuals();
            }
        }

        private void Cell_MouseEnter(object sender, EventArgs e)
        {
            if (IsEnemyBoard || !IsPlacementPhase || SelectedShip == null || SelectedShip.IsPlaced) return;
            CellButton btn = sender as CellButton;
            if (btn == null) return;
            currentHover = (Point)btn.Tag;
            RefreshBoardVisuals();
        }

        private void Cell_MouseLeave(object sender, EventArgs e)
        {
            if (IsEnemyBoard || !IsPlacementPhase || SelectedShip == null || SelectedShip.IsPlaced) return;
            currentHover = new Point(-1, -1);
            RefreshBoardVisuals();
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
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = tableGame.Width;
            int h = tableGame.Height;
            if (w <= 0 || h <= 0) return;

            float cellW = w / 10f;
            float cellH = h / 10f;

            // 1. VẼ ĐƯỜNG LƯỚI VIỀN XANH
            Color outline = Color.FromArgb(88, 160, 200);
            using (Pen pen = new Pen(outline, 2))
            {
                for (int i = 0; i <= 10; i++)
                {
                    g.DrawLine(pen, i * cellW, 0, i * cellW, h);
                    g.DrawLine(pen, 0, i * cellH, w, i * cellH);
                }
            }

            // 2. VẼ HÌNH TÀU TƯƠNG ỨNG (Chỉ vẽ trên bảng mình)
            if (!IsEnemyBoard)
            {
                foreach (var ship in PlayerShips)
                {
                    if (!ship.IsPlaced || ship.OccupiedCells.Count == 0) continue;

                    int minX = int.MaxValue, minY = int.MaxValue;
                    int maxX = int.MinValue, maxY = int.MinValue;

                    foreach (Point pt in ship.OccupiedCells)
                    {
                        if (pt.X < minX) minX = pt.X;
                        if (pt.Y < minY) minY = pt.Y;
                        if (pt.X > maxX) maxX = pt.X;
                        if (pt.Y > maxY) maxY = pt.Y;
                    }

                    bool isShipHorizontal = (maxX - minX) >= (maxY - minY);
                    RectangleF shipBounds = new RectangleF(
                        minX * cellW,
                        minY * cellH,
                        (maxX - minX + 1) * cellW,
                        (maxY - minY + 1) * cellH);

                    DrawShipGraphic(g, ship.Name, shipBounds, isShipHorizontal, isPreview: false, isValidPreview: true, isSunk: ship.IsSunk);
                }
            }

            // 3. VẼ BÓNG MỜ ĐÚNG HÌNH TÀU TRƯỚC KHI ĐẶT
            if (!IsEnemyBoard && IsPlacementPhase && SelectedShip != null && !SelectedShip.IsPlaced && currentHover.X >= 0)
            {
                bool isValid = CanPlaceShip(currentHover.X, currentHover.Y, SelectedShip.Size, IsHorizontal);
                Color hoverColor = isValid ? Color.FromArgb(95, 46, 204, 113) : Color.FromArgb(110, 231, 76, 60);

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

                RectangleF previewBounds = new RectangleF(
                    currentHover.X * cellW,
                    currentHover.Y * cellH,
                    (IsHorizontal ? SelectedShip.Size : 1) * cellW,
                    (IsHorizontal ? 1 : SelectedShip.Size) * cellH);

                DrawShipGraphic(g, SelectedShip.Name, previewBounds, IsHorizontal, isPreview: true, isValidPreview: isValid, isSunk: false);
            }
        }

        // ============================================================
        // HỆ THỐNG VẼ TỪNG LOẠI TÀU (CARRIER, BATTLESHIP, CRUISER, SUBMARINE, DESTROYER)
        // ============================================================
        private void DrawShipGraphic(Graphics g, string shipName, RectangleF bounds, bool isHorizontal, bool isPreview, bool isValidPreview, bool isSunk)
        {
            if (bounds.Width <= 2f || bounds.Height <= 2f) return;

            GraphicsState state = g.Save();
            try
            {
                float len;
                float thick;

                if (isHorizontal)
                {
                    g.TranslateTransform(bounds.X, bounds.Y);
                    len = bounds.Width;
                    thick = bounds.Height;
                }
                else
                {
                    // Xoay 90 độ theo chiều kim đồng hồ để dùng chung tọa độ vẽ ngang
                    g.TranslateTransform(bounds.X + bounds.Width, bounds.Y);
                    g.RotateTransform(90f);
                    len = bounds.Height;
                    thick = bounds.Width;
                }

                int alpha = isPreview ? 155 : 245;
                Color hullBase;
                Color deckColor;
                Color detailColor;
                Color borderColor;

                if (isPreview)
                {
                    hullBase = isValidPreview ? Color.FromArgb(alpha, 55, 165, 110) : Color.FromArgb(alpha, 195, 65, 65);
                    deckColor = isValidPreview ? Color.FromArgb(alpha, 95, 200, 145) : Color.FromArgb(alpha, 225, 105, 105);
                    detailColor = Color.FromArgb(alpha, 230, 245, 250);
                    borderColor = Color.FromArgb(alpha, 240, 255, 255);
                }
                else if (isSunk)
                {
                    hullBase = Color.FromArgb(220, 48, 52, 58);
                    deckColor = Color.FromArgb(220, 68, 72, 78);
                    detailColor = Color.FromArgb(200, 110, 115, 120);
                    borderColor = Color.FromArgb(220, 150, 90, 90);
                }
                else
                {
                    hullBase = Color.FromArgb(alpha, 66, 82, 98);
                    deckColor = Color.FromArgb(alpha, 96, 116, 134);
                    detailColor = Color.FromArgb(alpha, 165, 184, 202);
                    borderColor = Color.FromArgb(alpha, 208, 226, 242);
                }

                // Bóng đổ nhẹ dưới mặt nước
                if (!isPreview)
                {
                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(55, 5, 18, 32)))
                    {
                        g.FillEllipse(shadowBrush, thick * 0.12f, thick * 0.22f, len - thick * 0.16f, thick * 0.68f);
                    }
                }

                switch (shipName?.Trim().ToLowerInvariant())
                {
                    case "carrier":
                        DrawCarrier(g, len, thick, hullBase, deckColor, detailColor, borderColor, alpha);
                        break;
                    case "battleship":
                        DrawBattleship(g, len, thick, hullBase, deckColor, detailColor, borderColor, alpha);
                        break;
                    case "cruiser":
                        DrawCruiser(g, len, thick, hullBase, deckColor, detailColor, borderColor, alpha);
                        break;
                    case "submarine":
                        DrawSubmarine(g, len, thick, hullBase, deckColor, detailColor, borderColor, alpha);
                        break;
                    case "destroyer":
                    default:
                        DrawDestroyer(g, len, thick, hullBase, deckColor, detailColor, borderColor, alpha);
                        break;
                }
            }
            finally
            {
                g.Restore(state);
            }
        }

        private void DrawCarrier(Graphics g, float len, float thick, Color hullBase, Color deckColor, Color detailColor, Color borderColor, int alpha)
        {
            float px = thick * 0.08f;
            float py = thick * 0.10f;
            float x0 = px, x1 = len - px, w = x1 - x0;
            float y0 = py, y1 = thick - py, h = y1 - y0;

            PointF[] deckPoly =
            {
                new PointF(x0, y0 + h * 0.18f),
                new PointF(x0 + w * 0.12f, y0 + h * 0.18f),
                new PointF(x0 + w * 0.18f, y0),
                new PointF(x0 + w * 0.66f, y0),
                new PointF(x0 + w * 0.74f, y0 + h * 0.16f),
                new PointF(x1, y0 + h * 0.22f),
                new PointF(x1, y1 - h * 0.22f),
                new PointF(x0 + w * 0.84f, y1 - h * 0.12f),
                new PointF(x0, y1 - h * 0.12f)
            };

            using (GraphicsPath path = new GraphicsPath())
            using (SolidBrush deckBrush = new SolidBrush(Color.FromArgb(alpha, Math.Max(0, hullBase.R - 12), Math.Max(0, hullBase.G - 10), Math.Max(0, hullBase.B - 6))))
            using (Pen borderPen = new Pen(borderColor, 1.4f))
            {
                path.AddPolygon(deckPoly);
                g.FillPath(deckBrush, path);
                g.DrawPath(borderPen, path);
            }

            // Vạch đường băng chính + đường băng chéo
            using (Pen runwayPen = new Pen(Color.FromArgb(alpha, 235, 240, 245), 1.4f))
            {
                runwayPen.DashStyle = DashStyle.Dash;
                g.DrawLine(runwayPen, x0 + w * 0.06f, y0 + h * 0.56f, x0 + w * 0.94f, y0 + h * 0.52f);
            }
            using (Pen angledPen = new Pen(Color.FromArgb(alpha, 245, 210, 95), 1.3f))
            {
                angledPen.DashStyle = DashStyle.Dash;
                g.DrawLine(angledPen, x0 + w * 0.12f, y0 + h * 0.44f, x0 + w * 0.62f, y0 + h * 0.14f);
            }

            // Vạch sơn đuôi sàn bay
            using (Pen stripePen = new Pen(Color.FromArgb(alpha, 230, 235, 240), 1.6f))
            {
                g.DrawLine(stripePen, x0 + w * 0.03f, y0 + h * 0.30f, x0 + w * 0.03f, y1 - h * 0.24f);
            }

            // Đài chỉ huy (Island) bên mạn phải
            RectangleF island = new RectangleF(x0 + w * 0.46f, y1 - h * 0.36f, w * 0.17f, h * 0.21f);
            using (SolidBrush islandBrush = new SolidBrush(detailColor))
            using (Pen islandPen = new Pen(borderColor, 1f))
            {
                g.FillRectangle(islandBrush, island);
                g.DrawRectangle(islandPen, island.X, island.Y, island.Width, island.Height);
                g.FillEllipse(islandBrush, island.X + island.Width * 0.35f, island.Y + island.Height * 0.2f, island.Height * 0.6f, island.Height * 0.6f);
            }
        }

        private void DrawBattleship(Graphics g, float len, float thick, Color hullBase, Color deckColor, Color detailColor, Color borderColor, int alpha)
        {
            float px = thick * 0.08f;
            float py = thick * 0.14f;
            float x0 = px, x1 = len - px, w = x1 - x0;
            float y0 = py, y1 = thick - py, h = y1 - y0;
            float cy = thick / 2f;

            PointF[] hull =
            {
                new PointF(x0 + w * 0.04f, y0 + h * 0.22f),
                new PointF(x0 + w * 0.22f, y0 + h * 0.04f),
                new PointF(x0 + w * 0.68f, y0 + h * 0.04f),
                new PointF(x0 + w * 0.88f, y0 + h * 0.22f),
                new PointF(x1, cy),
                new PointF(x0 + w * 0.88f, y1 - h * 0.22f),
                new PointF(x0 + w * 0.68f, y1 - h * 0.04f),
                new PointF(x0 + w * 0.22f, y1 - h * 0.04f),
                new PointF(x0 + w * 0.04f, y1 - h * 0.22f),
                new PointF(x0, cy)
            };

            using (GraphicsPath path = new GraphicsPath())
            using (SolidBrush hullBrush = new SolidBrush(hullBase))
            using (Pen borderPen = new Pen(borderColor, 1.4f))
            {
                path.AddPolygon(hull);
                g.FillPath(hullBrush, path);
                g.DrawPath(borderPen, path);
            }

            // Boong tàu giữa
            RectangleF super = new RectangleF(x0 + w * 0.32f, y0 + h * 0.20f, w * 0.28f, h * 0.60f);
            RectangleF bridge = new RectangleF(x0 + w * 0.40f, y0 + h * 0.28f, w * 0.13f, h * 0.44f);
            using (SolidBrush deckBrush = new SolidBrush(deckColor))
            using (SolidBrush detailBrush = new SolidBrush(detailColor))
            using (Pen thinPen = new Pen(borderColor, 1f))
            {
                g.FillRectangle(deckBrush, super);
                g.DrawRectangle(thinPen, super.X, super.Y, super.Width, super.Height);
                g.FillRectangle(detailBrush, bridge);
                g.DrawRectangle(thinPen, bridge.X, bridge.Y, bridge.Width, bridge.Height);
            }

            // 3 Tháp pháo chính (2 trước, 1 sau)
            float turretR = h * 0.18f;
            DrawTurret(g, x0 + w * 0.18f, cy, turretR, barrelsRight: false, detailColor, borderColor);
            DrawTurret(g, x0 + w * 0.67f, cy, turretR, barrelsRight: true, detailColor, borderColor);
            DrawTurret(g, x0 + w * 0.80f, cy, turretR * 0.95f, barrelsRight: true, detailColor, borderColor);
        }

        private void DrawCruiser(Graphics g, float len, float thick, Color hullBase, Color deckColor, Color detailColor, Color borderColor, int alpha)
        {
            float px = thick * 0.09f;
            float py = thick * 0.18f;
            float x0 = px, x1 = len - px, w = x1 - x0;
            float y0 = py, y1 = thick - py, h = y1 - y0;
            float cy = thick / 2f;

            PointF[] hull =
            {
                new PointF(x0 + w * 0.04f, y0 + h * 0.26f),
                new PointF(x0 + w * 0.24f, y0 + h * 0.06f),
                new PointF(x0 + w * 0.68f, y0 + h * 0.08f),
                new PointF(x1, cy),
                new PointF(x0 + w * 0.68f, y1 - h * 0.08f),
                new PointF(x0 + w * 0.24f, y1 - h * 0.06f),
                new PointF(x0 + w * 0.04f, y1 - h * 0.26f),
                new PointF(x0, cy)
            };

            using (GraphicsPath path = new GraphicsPath())
            using (SolidBrush hullBrush = new SolidBrush(hullBase))
            using (Pen borderPen = new Pen(borderColor, 1.4f))
            {
                path.AddPolygon(hull);
                g.FillPath(hullBrush, path);
                g.DrawPath(borderPen, path);
            }

            // Cấu trúc thượng tầng + đài radar
            RectangleF super = new RectangleF(x0 + w * 0.34f, y0 + h * 0.22f, w * 0.28f, h * 0.56f);
            using (SolidBrush deckBrush = new SolidBrush(deckColor))
            using (SolidBrush detailBrush = new SolidBrush(detailColor))
            using (Pen thinPen = new Pen(borderColor, 1f))
            {
                g.FillRectangle(deckBrush, super);
                g.DrawRectangle(thinPen, super.X, super.Y, super.Width, super.Height);

                float radarR = h * 0.17f;
                g.FillEllipse(detailBrush, x0 + w * 0.48f - radarR, cy - radarR, radarR * 2f, radarR * 2f);
            }

            // 2 Tháp pháo (1 trước, 1 sau)
            float turretR = h * 0.17f;
            DrawTurret(g, x0 + w * 0.20f, cy, turretR, barrelsRight: false, detailColor, borderColor);
            DrawTurret(g, x0 + w * 0.72f, cy, turretR, barrelsRight: true, detailColor, borderColor);
        }

        private void DrawSubmarine(Graphics g, float len, float thick, Color hullBase, Color deckColor, Color detailColor, Color borderColor, int alpha)
        {
            float px = thick * 0.10f;
            float py = thick * 0.20f;
            float x0 = px, x1 = len - px, w = x1 - x0;
            float y0 = py, y1 = thick - py, h = y1 - y0;
            float cy = thick / 2f;

            Color subHull = Color.FromArgb(alpha, Math.Max(0, hullBase.R - 18), Math.Max(0, hullBase.G - 16), Math.Max(0, hullBase.B - 12));

            // Cánh lái đuôi và vây lặn mũi
            using (SolidBrush finBrush = new SolidBrush(deckColor))
            using (Pen borderPen = new Pen(borderColor, 1.2f))
            {
                g.FillRectangle(finBrush, x0 + w * 0.08f, y0 - h * 0.08f, w * 0.07f, h * 1.16f);
                g.FillRectangle(finBrush, x0 + w * 0.54f, y0 - h * 0.10f, w * 0.07f, h * 1.20f);

                // Thân tàu ngầm dạng thuôn tròn
                RectangleF bodyRect = new RectangleF(x0 + w * 0.04f, y0, w * 0.96f, h);
                using (SolidBrush bodyBrush = new SolidBrush(subHull))
                {
                    g.FillEllipse(bodyBrush, bodyRect);
                    g.DrawEllipse(borderPen, bodyRect);
                }

                // Đường gờ sống lưng tàu ngầm
                using (Pen spinePen = new Pen(Color.FromArgb(alpha, 145, 165, 185), 1.4f))
                {
                    g.DrawLine(spinePen, x0 + w * 0.14f, cy, x0 + w * 0.86f, cy);
                }

                // Tháp chỉ huy (Conning tower)
                RectangleF tower = new RectangleF(x0 + w * 0.48f, cy - h * 0.22f, w * 0.20f, h * 0.44f);
                using (SolidBrush towerBrush = new SolidBrush(detailColor))
                {
                    g.FillEllipse(towerBrush, tower);
                    g.DrawEllipse(borderPen, tower);
                }

                // Ống kính tiềm vọng
                float hatchR = h * 0.09f;
                g.FillEllipse(Brushes.WhiteSmoke, x0 + w * 0.58f - hatchR, cy - hatchR, hatchR * 2f, hatchR * 2f);
            }
        }

        private void DrawDestroyer(Graphics g, float len, float thick, Color hullBase, Color deckColor, Color detailColor, Color borderColor, int alpha)
        {
            float px = thick * 0.10f;
            float py = thick * 0.20f;
            float x0 = px, x1 = len - px, w = x1 - x0;
            float y0 = py, y1 = thick - py, h = y1 - y0;
            float cy = thick / 2f;

            PointF[] hull =
            {
                new PointF(x0, y0 + h * 0.18f),
                new PointF(x0 + w * 0.24f, y0 + h * 0.06f),
                new PointF(x0 + w * 0.64f, y0 + h * 0.10f),
                new PointF(x1, cy),
                new PointF(x0 + w * 0.64f, y1 - h * 0.10f),
                new PointF(x0 + w * 0.24f, y1 - h * 0.06f),
                new PointF(x0, y1 - h * 0.18f)
            };

            using (GraphicsPath path = new GraphicsPath())
            using (SolidBrush hullBrush = new SolidBrush(hullBase))
            using (Pen borderPen = new Pen(borderColor, 1.4f))
            {
                path.AddPolygon(hull);
                g.FillPath(hullBrush, path);
                g.DrawPath(borderPen, path);
            }

            // Đài chỉ huy nhỏ gọn
            RectangleF bridge = new RectangleF(x0 + w * 0.34f, y0 + h * 0.22f, w * 0.24f, h * 0.56f);
            using (SolidBrush deckBrush = new SolidBrush(deckColor))
            using (Pen thinPen = new Pen(borderColor, 1f))
            {
                g.FillRectangle(deckBrush, bridge);
                g.DrawRectangle(thinPen, bridge.X, bridge.Y, bridge.Width, bridge.Height);
            }

            // Sân đáp trực thăng phía đuôi
            float heliR = h * 0.22f;
            using (Pen heliPen = new Pen(Color.FromArgb(alpha, 230, 235, 240), 1.1f))
            {
                g.DrawEllipse(heliPen, x0 + w * 0.16f - heliR, cy - heliR, heliR * 2f, heliR * 2f);
            }

            // 1 Pháo mũi
            DrawTurret(g, x0 + w * 0.70f, cy, h * 0.16f, barrelsRight: true, detailColor, borderColor);
        }

        private void DrawTurret(Graphics g, float cx, float cy, float radius, bool barrelsRight, Color fillColor, Color borderColor)
        {
            float dir = barrelsRight ? 1f : -1f;
            float barrelLen = radius * 1.9f;
            float offset = radius * 0.35f;

            using (Pen barrelPen = new Pen(borderColor, Math.Max(1.4f, radius * 0.28f)))
            using (SolidBrush turretBrush = new SolidBrush(fillColor))
            using (Pen turretBorder = new Pen(borderColor, 1f))
            {
                g.DrawLine(barrelPen, cx, cy - offset, cx + dir * barrelLen, cy - offset);
                g.DrawLine(barrelPen, cx, cy + offset, cx + dir * barrelLen, cy + offset);

                g.FillEllipse(turretBrush, cx - radius, cy - radius, radius * 2f, radius * 2f);
                g.DrawEllipse(turretBorder, cx - radius, cy - radius, radius * 2f, radius * 2f);
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
            RefreshBoardVisuals();
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
            RefreshBoardVisuals();
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

            var readyDto = new PlayerReadyDto { RoomId = roomId };
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