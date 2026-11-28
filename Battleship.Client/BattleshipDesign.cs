using Battleship.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using System.Reflection;
namespace Battleship.Client
{
    public enum GameTurnState
    {
        PlacingShips,    // Giai đoạn tự xếp 5 tàu
        Waiting,         // Đã xếp xong, chờ đối thủ xếp tàu
        MyTurn,          // Lượt của mình (bắn 1 phát, đếm ngược)
        EnemyTurn,       // Lượt của đối thủ
        GameOver         // Kết thúc trận đấu
    }

    public partial class BattleshipDesign : UserControl
    {
        Color Outline = Theme.PanelBorder;
        private static readonly Color PanelSurface = Color.FromArgb(22, 56, 94);       // Màu panel2 & panel9 (nổi lên trên)
        private static readonly Color PanelRecessed = Color.FromArgb(14, 38, 66);      // Màu panel1 ở giữa (bị đè xuống)
        private static readonly Color BoardBackdrop = Color.FromArgb(10, 28, 50);      // Nền tableLayoutPanel1
        private static readonly Color SoftBorder = Color.FromArgb(88, 138, 178);
        private static readonly Color EdgeHighlight = Color.FromArgb(132, 192, 232);   // Viền sáng ở mép đè lên panel1
        private static readonly Color SoftCyan = Color.FromArgb(74, 158, 206);
        private static readonly Color SoftGold = Color.FromArgb(212, 172, 74);
        private static readonly Color PlacementPurple = Color.FromArgb(148, 102, 204);
        private Color _playerNameBorderColor = SoftCyan;
        private Color _enemyNameBorderColor = SoftCyan;
        private BoardGame playerBoard;
        public BoardGame EnemyBoard;
        private MatchFoundDto _currentMatch;

        public bool IsOnlineMode { get; private set; } = true;

        private bool _isCleanedUp = false;
        private bool _isGameOverHandled = false;
        private bool _wasConnected = false;
        private bool _matchStarted = false;
        private int _yellowShotsCount = 0;

        public GameTurnState CurrentState { get; private set; } = GameTurnState.PlacingShips;

        public int TurnDurationSeconds { get; set; } = 30;
        private Timer _turnTimer;
        private Timer _connectionCheckTimer;
        private int _secondsLeft;
        private DateTime _turnStartTime;        // Thời điểm bắt đầu đếm ngược (để tránh drift)
        private int _turnDurationForDisplay;    // Tổng giây đếm ngược của lượt hiện tại

        private TextBox boxChat;
        private bool _Chat = false;
        private Timer _chatAutoHideTimer;

        public BattleshipDesign(MatchFoundDto matchInfo = null)
        {
            InitializeComponent();
            EnableDoubleBuffer(this);
            EnableDoubleBuffer(tableLayoutPanel1);
            EnableDoubleBuffer(panel1);
            EnableDoubleBuffer(panel2);
            EnableDoubleBuffer(panel9);

            // Gắn sự kiện vẽ viền nổi cho panel2 (trái) và panel9 (phải)
            panel2.Paint -= panel2_Paint;
            panel2.Paint += panel2_Paint;
            panel9.Paint -= panel9_Paint;
            panel9.Paint += panel9_Paint;

            // Vẽ lại mượt khi đổi kích thước cửa sổ
            tableLayoutPanel1.SizeChanged += (s, e) => tableLayoutPanel1.Invalidate();
            panel1.SizeChanged += (s, e) => panel1.Invalidate();
            panel2.SizeChanged += (s, e) => panel2.Invalidate();
            panel9.SizeChanged += (s, e) => panel9.Invalidate();
            Applytheme();
            Theme.ApplyRetroFont(this);

            this.Load += BattleshipDesign_Load;
            this.Disposed += BattleshipDesign_Disposed;

            // matchInfo == null -> Chế độ Test cục bộ | matchInfo != null -> Chế độ Online
            IsOnlineMode = (matchInfo != null);

            _currentMatch = matchInfo ?? new MatchFoundDto
            {
                RoomId = "TEST_ROOM_01",
                OpponentName = "Test Target",
                IsHost = true
            };

            SetupPlayerNames();
            SetupPlayerBoard();

            if (IsOnlineMode && NetworkManager.Instance != null)
            {
                _wasConnected = NetworkManager.Instance.IsConnected;
                NetworkManager.Instance.OnPacketReceived -= NetworkManager_OnPacketReceived;
                NetworkManager.Instance.OnPacketReceived += NetworkManager_OnPacketReceived;
            }

        }

        private static void EnableDoubleBuffer(Control ctrl)
        {
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                           ?.SetValue(ctrl, true, null);
        }


        private void Applytheme()
        {
            this.BackColor = Theme.BgBottom;
            tableLayoutPanel1.BackColor = BoardBackdrop;
            tableLayoutPanel2.BackColor = Color.Transparent;

            panel1.BackColor = PanelRecessed; // Tối hơn để có cảm giác nằm dưới panel2 và panel9
            panel2.BackColor = PanelSurface;
            panel9.BackColor = PanelSurface;
            pnlPlayerName.BackColor = PanelSurface;
            pnlPlayerName.Padding = new Padding(4);
            pnlEnemyName.BackColor = PanelSurface;
            pnlEnemyName.Padding = new Padding(4);

            Label[] shipLabels = { label1, label2, label3, label4, label6, label5, label7, label8, label9, label10 };
            foreach (var lbl in shipLabels)
            {
                if (lbl == null) continue;
                lbl.ForeColor = Theme.Text;
                lbl.Font = FontManager.GetFont(8.5f, FontStyle.Bold);
            }

            TableLayoutPanel[] coordPanels = { tableLayoutPanel3, tableLayoutPanel4, tableLayoutPanel5, tableLayoutPanel6 };
            foreach (var tbl in coordPanels)
            {
                if (tbl == null) continue;
                tbl.BackColor = Color.Transparent;
                foreach (Label lbl in tbl.Controls.OfType<Label>())
                {
                    lbl.ForeColor = Theme.Cyan;
                    lbl.Font = FontManager.GetFont(8f, FontStyle.Bold);
                }
            }

            if (lblPlayerName != null)
            {
                lblPlayerName.Dock = DockStyle.Fill;
                lblPlayerName.TextAlign = ContentAlignment.MiddleCenter;
                lblPlayerName.Font = FontManager.GetFont(11f, FontStyle.Bold);
            }
            if (lblEnemyName != null)
            {
                lblEnemyName.Dock = DockStyle.Fill;
                lblEnemyName.TextAlign = ContentAlignment.MiddleCenter;
                lblEnemyName.Font = FontManager.GetFont(11f, FontStyle.Bold);
            }
            if (lblShots != null)
            {
                lblShots.ForeColor = Theme.Gold;
                lblShots.Font = FontManager.GetFont(13f, FontStyle.Bold);
                lblShots.Location = new Point(6, 124);
                lblShots.Size = new Size(panel1.Width - 12, 70);
                lblShots.TextAlign = ContentAlignment.MiddleCenter;
            }
            if (TxtTimer != null)
            {
                TxtTimer.ReadOnly = true;
                TxtTimer.TabStop = false;
                TxtTimer.BackColor = Theme.InputBg;
                TxtTimer.ForeColor = Theme.Green;
                TxtTimer.Font = FontManager.GetFont(13f, FontStyle.Bold);
                TxtTimer.BorderStyle = BorderStyle.FixedSingle;
                TxtTimer.Size = new Size(132, 38);
                TxtTimer.Location = new Point((panel1.Width - TxtTimer.Width) / 2, 16);
            }
            if (TableShots != null)
            {
                TableShots.BackColor = Color.Transparent;
                TableShots.Size = new Size(210, 42);
                TableShots.Location = new Point((panel1.Width - TableShots.Width) / 2, 68);
            }
            if (btnChat != null)
            {
                btnChat.Location = new Point(panel9.Width - 118, 6);
                btnChat.Size = new Size(110, 32);
                Theme.StyleButton(btnChat, Theme.Cyan, Theme.CyanDark, 9f);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Theme.DrawCrtBackground(e.Graphics, this.ClientRectangle, withOuterFrame: false);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (this.Disposing)
            {
                CleanupAndLeaveRoom();
            }
            base.OnHandleDestroyed(e);
        }

        private void BattleshipDesign_Load(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.FormClosing -= ParentForm_FormClosing;
                parentForm.FormClosing += ParentForm_FormClosing;
            }

            SetupPlayerNames();
            InitTurnTimer();
            SetDefaultGrayColors();
            CreateTransparentChatBox();

            if (!IsOnlineMode)
            {
                // Chế độ Test: đặt cố định 5 tàu (true = hàng ngang 1 từ X=0, false = cột dọc 1 từ Y=0)
                bool placeHorizontally = true;
                playerBoard.PlaceFixedTestShips(placeHorizontally);
                EnemyBoard.PlaceFixedTestShips(placeHorizontally);

                UpdateAllShipsConditionUI(playerBoard.PlayerShips, isEnemy: false);
                UpdateAllShipsConditionUI(EnemyBoard.PlayerShips, isEnemy: true);

                AddChatMessage("Test", "Chế độ Test: Đã đặt cố định 5 tàu. Bắt đầu thử nghiệm bắn & bộ đếm giờ!");
                SetTurnState(GameTurnState.MyTurn);
                return;
            }

            // TODO (Server/Shared): Hiện Battleship.Shared và Battleship.Server chưa có PacketType riêng
            // để yêu cầu/nhận timer giai đoạn đặt tàu (Server chỉ mới chạy StartOrResetTurnTimer sau khi cả 2 PlayerReady).
            // Khi Server bổ sung (ví dụ PlacementTimerRequest/PlacementTimerStart), bỏ comment đoạn dưới để đồng bộ từ Server:
            // if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
            // {
            //     _ = NetworkManager.Instance.SendAsync(Packet.Create(PacketType.PlacementTimerRequest, new { RoomId = _currentMatch.RoomId }));
            // }

            SetTurnState(GameTurnState.PlacingShips);
        }

        private void InitTurnTimer()
        {
            _turnTimer = new Timer();
            _turnTimer.Interval = 1000;
            _turnTimer.Tick += TurnTimer_Tick;

            if (IsOnlineMode)
            {
                _connectionCheckTimer = new Timer();
                _connectionCheckTimer.Interval = 1000;
                _connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
                _connectionCheckTimer.Start();
            }
        }

        private void ConnectionCheckTimer_Tick(object sender, EventArgs e)
        {
            if (_isGameOverHandled || CurrentState == GameTurnState.GameOver)
            {
                _connectionCheckTimer?.Stop();
                return;
            }

            bool isConnected = NetworkManager.Instance != null && NetworkManager.Instance.IsConnected;
            if (isConnected)
            {
                _wasConnected = true;
                return;
            }

            if (_wasConnected)
            {
                _connectionCheckTimer?.Stop();
                HandleGameOver(false, "Bạn đã mất kết nối mạng với máy chủ và bị xử thua!");
            }
        }

        private void StartTurnTimer(int durationSeconds)
        {
            // Dùng DateTime để tránh drift: ghi nhớ thời điểm bắt đầu và tổng giây
            _turnDurationForDisplay = durationSeconds > 0 ? durationSeconds : 30;
            _turnStartTime = DateTime.UtcNow;
            _secondsLeft = _turnDurationForDisplay;
            if (TxtTimer != null) TxtTimer.Text = $"{_secondsLeft}";

            if (_turnTimer == null) return;
            _turnTimer.Stop();
            _turnTimer.Start();
        }

        private void StopTurnTimer(string displayText = "--")
        {
            _turnTimer?.Stop();
            if (TxtTimer != null) TxtTimer.Text = displayText;
        }

        private void TurnTimer_Tick(object sender, EventArgs e)
        {
            // Tính thời gian còn lại dựa trên DateTime thực để không bị drift
            int elapsed = (int)(DateTime.UtcNow - _turnStartTime).TotalSeconds;
            int remaining = Math.Max(0, _turnDurationForDisplay - elapsed);
            if (TxtTimer != null) TxtTimer.Text = $"{remaining}";

            if (remaining <= 0)
            {
                _turnTimer.Stop();

                // Cả Online lẫn Test: chuyển lượt ngay tại client khi hết giờ
                if (CurrentState == GameTurnState.MyTurn)
                {
                    SetTurnState(GameTurnState.EnemyTurn, TurnDurationSeconds);
                }
                else if (CurrentState == GameTurnState.EnemyTurn)
                {
                    SetTurnState(GameTurnState.MyTurn, TurnDurationSeconds);
                }
                // PlacingShips: giữ trạng thái, Server sẽ gửi TurnChange khi cả hai xong
            }
        }

        private void SetupPlayerBoard()
        {
            pnlPlayerBoard.Controls.Clear();
            pnlEnemyBoard.Controls.Clear();

            playerBoard = new BoardGame();
            EnemyBoard = new BoardGame();

            playerBoard.Dock = DockStyle.Fill;
            EnemyBoard.Dock = DockStyle.Fill;

            pnlPlayerBoard.Controls.Add(playerBoard);
            pnlEnemyBoard.Controls.Add(EnemyBoard);

            playerBoard.IsOnlineMode = IsOnlineMode;
            playerBoard.CurrentRoomId = _currentMatch.RoomId;

            EnemyBoard.IsOnlineMode = IsOnlineMode;
            EnemyBoard.CurrentRoomId = _currentMatch.RoomId;
            EnemyBoard.IsEnemyBoard = true;
            EnemyBoard.IsPlacementPhase = false;

            // 1. Vừa đặt xong con tàu nào -> bảng tình trạng tàu đó chuyển sang xanh lá ngay
            playerBoard.OnShipPlacedSuccessfully += (placedShip) =>
            {
                TableLayoutPanel targetTable = GetShipConditionTable(placedShip.Name, isEnemy: false);
                UpdateSingleShipCondition(targetTable, placedShip, isEnemy: false);
            };

            // 2. Khi xếp xong đủ 5 tàu trên bảng của mình
            playerBoard.OnAllShipsPlaced += async () =>
            {
                if (_isGameOverHandled) return;

                if (!IsOnlineMode)
                {
                    SetTurnState(GameTurnState.MyTurn);
                    return;
                }

                SetTurnState(GameTurnState.Waiting);

                try
                {
                    await playerBoard.SendMyShipsToServerAsync(_currentMatch.RoomId);

                    if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
                    {
                        var syncPlaced = new ChatMessageDto
                        {
                            Sender = PlayerProfile.Username ?? string.Empty,
                            Content = $"[SYS_SHIPS_READY:{_currentMatch.RoomId}]:1"
                        };
                        await NetworkManager.Instance.SendAsync(Packet.Create(PacketType.ChatMessage, syncPlaced));
                    }

                    AddChatMessage("Hệ thống", "Đã gửi đội hình tàu. Đang chờ đối thủ xếp tàu...");
                }
                catch (Exception ex)
                {
                    AddChatMessage("Hệ thống", $"Lỗi gửi đội hình tàu: {ex.Message}");
                }
            };

            // 3. Cập nhật tình trạng tàu khi có tàu bị bắn trúng / đánh chìm
            playerBoard.OnShipConditionUpdated += (ships) => UpdateAllShipsConditionUI(ships, isEnemy: false);
            EnemyBoard.OnShipConditionUpdated += (ships) =>
            {
                UpdateAllShipsConditionUI(ships, isEnemy: true);

                if (!IsOnlineMode && !_isGameOverHandled && ships != null && ships.Count > 0 &&
                    ships.All(s => s.Size > 0 && s.HitCount >= s.Size))
                {
                    HandleGameOver(true, "Đã bắn chìm toàn bộ 5 tàu mục tiêu trong chế độ Test!");
                }
            };

            // 4. Cập nhật số viên đạn còn lại lên TableShots
            EnemyBoard.OnRemainingShotsChanged += (remainingShots) =>
            {
                UpdateShotsUI(remainingShots);
            };

            // 5. Chế độ Test: bắn xong một phát thì nạp lại để bắn tiếp
            EnemyBoard.OnTurnEnded += () =>
            {
                if (!IsOnlineMode && !_isGameOverHandled && CurrentState == GameTurnState.MyTurn)
                {
                    AddChatMessage("Test", "Đã bắn xong, nạp lại phát tiếp theo.");
                    SetTurnState(GameTurnState.MyTurn);
                }
            };

            // Tắt khung lưới mặc định của WinForms
            tableLayoutPanel3.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel4.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel5.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanel6.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;

            tableLayoutPanel4.CellPaint += tableLayoutPanel4_CellPaint;
            tableLayoutPanel3.CellPaint += tableLayoutPanel3_CellPaint;
            tableLayoutPanel5.CellPaint += tableLayoutPanel5_CellPaint;
            tableLayoutPanel6.CellPaint += tableLayoutPanel6_CellPaint;
        }

        public void SetTurnState(GameTurnState newState, int? timerSeconds = null, bool isExtraTurn = false)
        {
            if (_isGameOverHandled && newState != GameTurnState.GameOver) return;

            CurrentState = newState;
            int displaySeconds = timerSeconds.HasValue && timerSeconds.Value >= 0
                ? timerSeconds.Value
                : TurnDurationSeconds;

            switch (newState)
            {
                case GameTurnState.PlacingShips:
                    _playerNameBorderColor = PlacementPurple;
                    _enemyNameBorderColor = PlacementPurple;

                    playerBoard.IsPlacementPhase = true;
                    if (lblShots != null) lblShots.Text = "Place Ships";
                    // Đếm ngược thời gian đặt tàu trên Client để đồng hồ không bị đứng yên
                    // (Server hiện chưa gửi gói tin đếm giờ riêng cho giai đoạn PlacingShips)
                    StartTurnTimer(displaySeconds);
                    _yellowShotsCount = 0;
                    UpdateShotsUI(_yellowShotsCount);
                    UpdateAllShipsConditionUI(playerBoard.PlayerShips, isEnemy: false);
                    UpdateAllShipsConditionUI(EnemyBoard.PlayerShips, isEnemy: true);
                    break;

                case GameTurnState.Waiting:
                    _playerNameBorderColor = SoftCyan;
                    _enemyNameBorderColor = SoftCyan;

                    playerBoard.IsPlacementPhase = false;
                    if (lblShots != null) lblShots.Text = "Waiting...";
                    StopTurnTimer("--");
                    _yellowShotsCount = 0;
                    UpdateShotsUI(_yellowShotsCount);
                    break;

                case GameTurnState.MyTurn:
                    _playerNameBorderColor = SoftGold;
                    _enemyNameBorderColor = SoftCyan;

                    if (lblShots != null) lblShots.Text = "Your turn";
                    StartTurnTimer(displaySeconds);

                    _yellowShotsCount = isExtraTurn
                        ? Math.Min(5, _yellowShotsCount + 1)
                        : 1;

                    EnemyBoard.IsPlacementPhase = false;
                    EnemyBoard.StartTurn(displaySeconds);
                    UpdateShotsUI(_yellowShotsCount);
                    UpdateAllShipsConditionUI(EnemyBoard.PlayerShips, isEnemy: true);
                    break;

                case GameTurnState.EnemyTurn:
                    if (EnemyBoard.IsMyTurn) EnemyBoard.EndTurn();

                    _playerNameBorderColor = SoftCyan;
                    _enemyNameBorderColor = SoftGold;

                    if (lblShots != null) lblShots.Text = "Enemy's turn";
                    StartTurnTimer(displaySeconds);
                    _yellowShotsCount = 0;
                    UpdateShotsUI(_yellowShotsCount);
                    break;

                case GameTurnState.GameOver:
                    _playerNameBorderColor = SoftCyan;
                    _enemyNameBorderColor = SoftCyan;

                    playerBoard.IsPlacementPhase = false;
                    if (EnemyBoard.IsMyTurn) EnemyBoard.EndTurn();
                    if (lblShots != null) lblShots.Text = "End of game";
                    StopTurnTimer("0");
                    _yellowShotsCount = 0;
                    UpdateShotsUI(_yellowShotsCount);
                    break;
            }

            pnlPlayerName?.Invalidate();
            pnlEnemyName?.Invalidate();
        }

        private void StartMatchLocally(string serverFirstTurn = null, int? serverTimeLimit = null)
        {
            if (_isGameOverHandled) return;

            if (serverTimeLimit.HasValue && serverTimeLimit.Value > 0)
                TurnDurationSeconds = serverTimeLimit.Value;

            bool isFirstTurn;
            if (!string.IsNullOrWhiteSpace(serverFirstTurn))
            {
                isFirstTurn = string.Equals(serverFirstTurn, PlayerProfile.Username, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                isFirstTurn = _currentMatch != null && _currentMatch.IsHost;
            }

            if (!_matchStarted)
            {
                _matchStarted = true;
                AddChatMessage("Hệ thống", isFirstTurn
                    ? "Trận đấu bắt đầu! Bạn đi trước."
                    : "Trận đấu bắt đầu! Lượt của đối thủ.");
            }

            SetTurnState(isFirstTurn ? GameTurnState.MyTurn : GameTurnState.EnemyTurn);
        }

        // =========================================================
        // NHẬN PACKET TRẬN ĐẤU TỪ SERVER
        // =========================================================
        private void NetworkManager_OnPacketReceived(Packet packet)
        {
            if (!IsOnlineMode || packet == null || this.IsDisposed) return;

            switch (packet.Type)
            {
                case PacketType.GameStart:
                    {
                        var gameStart = packet.DeserializePayload<GameStartDto>();
                        if (gameStart == null || !IsCurrentRoom(gameStart.RoomId)) return;

                        SafeInvokeUI(() =>
                        {
                            if (!string.IsNullOrWhiteSpace(gameStart.FirstTurnUsername) &&
                                !string.Equals(gameStart.FirstTurnUsername, PlayerProfile.Username, StringComparison.OrdinalIgnoreCase) &&
                                (string.IsNullOrWhiteSpace(_currentMatch.OpponentName) || _currentMatch.OpponentName == "Waiting for opponent..."))
                            {
                                _currentMatch.OpponentName = gameStart.FirstTurnUsername;
                                if (lblEnemyName != null) lblEnemyName.Text = gameStart.FirstTurnUsername;
                            }

                            StartMatchLocally(gameStart.FirstTurnUsername, gameStart.TimeLimitSeconds);
                        });
                        break;
                    }

                case PacketType.ShotResult:
                    {
                        var result = packet.DeserializePayload<ShotResultDto>();
                        if (result == null || !IsCurrentRoom(result.RoomId)) return;

                        SafeInvokeUI(() => HandleShotResult(result));
                        break;
                    }

                case PacketType.TurnChange:
                    {
                        var turnChanged = packet.DeserializePayload<TurnChangedDto>();
                        if (turnChanged == null || !IsCurrentRoom(turnChanged.RoomId)) return;

                        SafeInvokeUI(() =>
                        {
                            if (_isGameOverHandled) return;

                            TurnDurationSeconds = turnChanged.TimeLimitSeconds > 0 ? turnChanged.TimeLimitSeconds : 30;

                            // Hỗ trợ sẵn nếu Server gửi TurnChange với Reason = "Placement" để đồng bộ timer đặt tàu
                            if (string.Equals(turnChanged.Reason, "Placement", StringComparison.OrdinalIgnoreCase))
                            {
                                SetTurnState(GameTurnState.PlacingShips, TurnDurationSeconds);
                                return;
                            }

                            bool isMyTurn = string.Equals(
                                turnChanged.CurrentTurnUsername,
                                PlayerProfile.Username,
                                StringComparison.OrdinalIgnoreCase);

                            if (string.Equals(turnChanged.Reason, "Timeout", StringComparison.OrdinalIgnoreCase))
                            {
                                AddChatMessage("Hệ thống", isMyTurn
                                    ? "Đối thủ hết giờ! Đến lượt bạn."
                                    : "Hết giờ! Chuyển lượt sang đối thủ.");
                            }

                            SetTurnState(isMyTurn ? GameTurnState.MyTurn : GameTurnState.EnemyTurn, TurnDurationSeconds);
                        });
                        break;
                    }

                case PacketType.GameOver:
                    {
                        var gameOver = packet.DeserializePayload<GameOverDto>();
                        if (gameOver == null || !IsCurrentRoom(gameOver.RoomId)) return;

                        SafeInvokeUI(() =>
                        {
                            bool isWinner = string.Equals(
                                gameOver.WinnerUsername,
                                PlayerProfile.Username,
                                StringComparison.OrdinalIgnoreCase);

                            HandleGameOver(isWinner, FormatGameOverReason(gameOver.Reason, isWinner));
                        });
                        break;
                    }

                case PacketType.OpponentDisconnected:
                    {
                        SafeInvokeUI(() =>
                        {
                            HandleGameOver(true, "Đối thủ đã mất kết nối mạng hoặc thoát khỏi trận đấu!");
                        });
                        break;
                    }

                case PacketType.RoomUpdate:
                    {
                        var update = packet.DeserializePayload<RoomUpdateDto>();
                        if (update?.Room == null || !IsCurrentRoom(update.Room.RoomId)) return;

                        string myName = PlayerProfile.Username ?? string.Empty;
                        string opponent = string.Equals(update.Room.HostName, myName, StringComparison.OrdinalIgnoreCase)
                            ? update.Room.GuestName
                            : update.Room.HostName;

                        if (!string.IsNullOrWhiteSpace(opponent))
                        {
                            _currentMatch.OpponentName = opponent;
                            SafeInvokeUI(() =>
                            {
                                if (lblEnemyName != null) lblEnemyName.Text = opponent;
                            });
                        }
                        break;
                    }

                case PacketType.ChatMessage:
                    {
                        try
                        {
                            var chatMsg = packet.DeserializePayload<ChatMessageDto>();
                            if (chatMsg == null || string.IsNullOrEmpty(chatMsg.Content)) break;

                            if (chatMsg.Content.StartsWith("[SYS_READY:", StringComparison.Ordinal))
                                break;

                            string shipPrefix = $"[SYS_SHIPS_READY:{_currentMatch?.RoomId}]:";
                            if (chatMsg.Content.StartsWith(shipPrefix, StringComparison.Ordinal))
                            {
                                if (!string.Equals(chatMsg.Sender, PlayerProfile.Username, StringComparison.OrdinalIgnoreCase))
                                {
                                    SafeInvokeUI(() =>
                                    {
                                        AddChatMessage("Hệ thống", "Đối thủ đã xếp xong đội hình tàu.");
                                    });
                                }
                                break;
                            }

                            // Chỉ nhận tin nhắn thuộc đúng trận đấu này (bỏ qua tin nhắn từ Lobby / phòng khác)
                            string matchPrefix = $"[MATCH_CHAT:{_currentMatch?.RoomId}]:";
                            if (!chatMsg.Content.StartsWith(matchPrefix, StringComparison.Ordinal))
                            {
                                break;
                            }

                            string actualContent = chatMsg.Content.Substring(matchPrefix.Length);
                            AddChatMessage(chatMsg.Sender, actualContent);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi giải mã tin nhắn: {ex.Message}");
                        }
                        break;
                    }

                case PacketType.Error:
                    {
                        var err = packet.DeserializePayload<ErrorDto>();
                        SafeInvokeUI(() => EnemyBoard.ClearAllPendingShots());
                        if (err != null && !string.IsNullOrWhiteSpace(err.Message))
                        {
                            AddChatMessage("Hệ thống", err.Message);
                        }
                        break;
                    }
            }
        }

        private void HandleShotResult(ShotResultDto result)
        {
            if (_isGameOverHandled) return;

            bool isMyShot = string.Equals(
                result.ShooterUsername,
                PlayerProfile.Username,
                StringComparison.OrdinalIgnoreCase);

            if (!result.IsValid)
            {
                if (isMyShot)
                {
                    EnemyBoard.CancelPendingShot(result.X, result.Y);
                    AddChatMessage("Hệ thống", "Tọa độ bắn không hợp lệ, hãy chọn ô khác.");
                }
                return;
            }

            if (isMyShot)
            {
                EnemyBoard.UpdateCellShot(result.X, result.Y, result.IsHit);

                if (result.IsSunk && !string.IsNullOrWhiteSpace(result.SunkShipName))
                {
                    EnemyBoard.ApplyEnemyShipHit(result.SunkShipName, isSunk: true);
                    MarkEnemyShipSunkUI(result.SunkShipName);
                    AddChatMessage("Hệ thống", $"Bạn đã đánh chìm tàu {result.SunkShipName} của đối thủ!");
                }

                if (EnemyBoard.PlayerShips.Count > 0 && EnemyBoard.PlayerShips.All(s => s.HitCount >= s.Size))
                {
                    HandleGameOver(true, "Bạn đã đánh chìm toàn bộ hạm đội của đối thủ!");
                    return;
                }
            }
            else
            {
                playerBoard.UpdateCellShot(result.X, result.Y, result.IsHit);

                if (result.IsSunk && !string.IsNullOrWhiteSpace(result.SunkShipName))
                {
                    AddChatMessage("Hệ thống", $"Tàu {result.SunkShipName} của bạn đã bị đánh chìm!");
                }

                if (playerBoard.PlayerShips.Count > 0 && playerBoard.PlayerShips.All(s => s.IsPlaced && s.HitCount >= s.Size))
                {
                    HandleGameOver(false, "Toàn bộ hạm đội của bạn đã bị đánh chìm!");
                    return;
                }
            }

            // Khi bắn trúng (chưa hết trận), Server không gửi TurnChange mà giữ lượt qua ShotResult.NextTurnUsername
            // và đặt lại timer 30s; khi bắn hụt, Server gửi TurnChange (Reason=Miss) ngay sau đó.
            if (result.IsHit && !string.IsNullOrWhiteSpace(result.NextTurnUsername))
            {
                bool nextIsMyTurn = string.Equals(
                    result.NextTurnUsername,
                    PlayerProfile.Username,
                    StringComparison.OrdinalIgnoreCase);

                SetTurnState(
                    nextIsMyTurn ? GameTurnState.MyTurn : GameTurnState.EnemyTurn,
                    TurnDurationSeconds,
                    isExtraTurn: nextIsMyTurn && isMyShot);
            }
        }

        private void HandleGameOver(bool isWinner, string reason)
        {
            if (_isGameOverHandled) return;
            _isGameOverHandled = true;

            _connectionCheckTimer?.Stop();
            SetTurnState(GameTurnState.GameOver);

            if (lblShots != null)
            {
                lblShots.Text = isWinner ? "VICTORY!" : "DEFEAT!";
            }

            if (IsOnlineMode && PlayerProfile.CurrentUser != null)
            {
                if (isWinner)
                {
                    PlayerProfile.CurrentUser.Wins += 1;
                    PlayerProfile.CurrentUser.Score += 10;
                }
                else
                {
                    PlayerProfile.CurrentUser.Losses += 1;
                }

                // TODO (Server/Shared): Hiện tại Battleship.Shared chưa có PacketType/DTO để Client gửi yêu cầu tăng số trận & thắng/thua,
                // và Battleship.Server (TcpServer.cs) chưa đăng ký RoomManager.OnMatchFinished để cập nhật Users.Wins/Losses/Matches vào DB.
                // Khi Server/Shared bổ sung, cần xử lý cập nhật DB tại đây (hoặc tự động trên Server khi GameOver).
            }

            string title = isWinner ? "CHIẾN THẮNG" : "THẤT BẠI";
            string message = isWinner
                ? $"Chúc mừng! Bạn đã giành chiến thắng.\nLý do: {reason}"
                : $"Bạn đã thua trận.\nLý do: {reason}";

            AddChatMessage("Hệ thống", $"{title} - {reason}");
            MessageBox.Show(message, title, MessageBoxButtons.OK,
                isWinner ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            CleanupAndLeaveRoom();
            frmMain.SwitchToLobby();
        }

        private string FormatGameOverReason(string reason, bool isWinner)
        {
            switch (reason)
            {
                case "AllShipsSunk":
                    return isWinner
                        ? "Bạn đã đánh chìm toàn bộ hạm đội của đối thủ!"
                        : "Toàn bộ hạm đội của bạn đã bị đánh chìm!";
                case "OpponentDisconnected":
                    return isWinner
                        ? "Đối thủ đã mất kết nối mạng hoặc thoát trận."
                        : "Bạn đã mất kết nối mạng và bị xử thua.";
                case "Surrender":
                    return isWinner ? "Đối thủ đã đầu hàng." : "Bạn đã đầu hàng.";
                case "Timeout":
                    return isWinner ? "Đối thủ hết thời gian quy định." : "Bạn hết thời gian quy định.";
                default:
                    return string.IsNullOrWhiteSpace(reason) ? "Trận đấu kết thúc." : reason;
            }
        }

        private bool IsCurrentRoom(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(_currentMatch?.RoomId))
                return true;

            return string.Equals(roomId, _currentMatch.RoomId, StringComparison.OrdinalIgnoreCase);
        }

        private void SafeInvokeUI(Action action)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action();
        }

        private void UpdateShotsUI(int yellowShotsCount)
        {
            if (TableShots == null) return;

            var orderedPics = TableShots.Controls.OfType<PictureBox>()
                                        .OrderBy(p => TableShots.GetColumn(p))
                                        .ThenBy(p => TableShots.GetRow(p))
                                        .ToList();

            int clampedCount = Math.Max(0, Math.Min(orderedPics.Count, yellowShotsCount));
            for (int i = 0; i < orderedPics.Count; i++)
            {
                orderedPics[i].Visible = true;
                string status = (i < clampedCount) ? "Shoots" : "Used";
                UpdateCellStatus(orderedPics[i], status);
            }
        }

        private void UpdateAllShipsConditionUI(List<ShipInfo> ships, bool isEnemy)
        {
            if (ships == null) return;

            foreach (var ship in ships)
            {
                TableLayoutPanel targetTable = GetShipConditionTable(ship.Name, isEnemy);
                UpdateSingleShipCondition(targetTable, ship, isEnemy);
            }
        }

        private TableLayoutPanel GetShipConditionTable(string shipName, bool isEnemy)
        {
            if (isEnemy)
            {
                switch (shipName)
                {
                    case "Carrier": return tableEnemyConditionCarrier;
                    case "Battleship": return tableEnemyConditionBattleship;
                    case "Cruiser": return tableEnemyConditionCrusier;
                    case "Submarine": return tableEnemyConditionSubmarine;
                    case "Destroyer": return tableEnemyConditionDestroyer;
                }
            }
            else
            {
                switch (shipName)
                {
                    case "Carrier": return tablePlayerConditionCarrier;
                    case "Battleship": return tablePlayerConditionBattleship;
                    case "Cruiser": return tablePlayerConditionCruiser;
                    case "Submarine": return tablePlayerConditionSubmarine;
                    case "Destroyer": return tablePlayerConditionDestroyer;
                }
            }
            return null;
        }

        private void UpdateSingleShipCondition(TableLayoutPanel table, ShipInfo ship, bool isEnemy)
        {
            if (table == null || ship == null) return;

            var orderedPics = table.Controls.OfType<PictureBox>()
                                   .OrderBy(p => table.GetColumn(p))
                                   .ThenBy(p => table.GetRow(p))
                                   .ToList();

            if (isEnemy)
            {
                // Tàu địch: không hiển thị gì cho tới khi chìm hẳn mới hiện đỏ cả tàu
                bool isSunk = ship.Size > 0 && ship.HitCount >= ship.Size;
                string status = isSunk ? "Damaged" : "None";

                for (int i = 0; i < orderedPics.Count; i++)
                {
                    UpdateCellStatus(orderedPics[i], status);
                }
                return;
            }

            for (int i = 0; i < orderedPics.Count; i++)
            {
                string status;
                if (!ship.IsPlaced)
                {
                    status = "Used";
                }
                else
                {
                    status = (i < ship.HitCount) ? "Damaged" : "Undamaged";
                }

                UpdateCellStatus(orderedPics[i], status);
            }
        }

        private void MarkEnemyShipSunkUI(string shipName)
        {
            TableLayoutPanel table = GetShipConditionTable(shipName, isEnemy: true);
            if (table == null) return;

            var orderedPics = table.Controls.OfType<PictureBox>()
                                   .OrderBy(p => table.GetColumn(p))
                                   .ThenBy(p => table.GetRow(p))
                                   .ToList();

            for (int i = 0; i < orderedPics.Count; i++)
            {
                UpdateCellStatus(orderedPics[i], "Damaged");
            }
        }

        private void AddChatMessage(string sender, string content)
        {
            if (chatPanel == null || chatPanel.IsDisposed || !chatPanel.IsHandleCreated) return;

            chatPanel.BeginInvoke(new Action(() =>
            {
                if (chatPanel.IsDisposed) return;

                Label msgLabel = new Label();
                msgLabel.Text = $"[{sender}]: {content}";
                msgLabel.BackColor = Color.Transparent;
                msgLabel.ForeColor = Theme.Text;
                msgLabel.Font = FontManager.GetFont(8.5f, FontStyle.Regular);
                msgLabel.AutoSize = true;
                msgLabel.MaximumSize = new Size(370, 0);

                chatPanel.Controls.Add(msgLabel);
                chatPanel.Visible = true;
                chatPanel.ScrollControlIntoView(msgLabel);

                if (!_Chat && _chatAutoHideTimer != null)
                {
                    _chatAutoHideTimer.Stop();
                    _chatAutoHideTimer.Start();
                }
            }));
        }

        private void SetupPlayerNames()
        {
            string myName = !string.IsNullOrWhiteSpace(PlayerProfile.Username)
                ? PlayerProfile.Username
                : "Player 1";

            string enemyName = !string.IsNullOrWhiteSpace(_currentMatch?.OpponentName)
                ? _currentMatch.OpponentName
                : "Waiting for opponent...";

            if (lblPlayerName != null)
            {
                lblPlayerName.Text = myName;
            }

            if (lblEnemyName != null)
            {
                lblEnemyName.Text = enemyName;

            }
        }

        private Image CreateStatusPipImage(string status, int size)
        {
            if (size <= 0) size = 24;

            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                int margin = Math.Max(2, size / 10);
                int drawSize = size - (margin * 2);
                if (drawSize <= 4) drawSize = size - 2;

                Rectangle outerRect = new Rectangle(margin, margin, drawSize, drawSize);
                Rectangle innerRect = new Rectangle(margin + 3, margin + 3, Math.Max(2, drawSize - 6), Math.Max(2, drawSize - 6));

                switch (status)
                {
                    case "damaged":
                        DrawLitPip(g, outerRect, innerRect, Theme.Red, Theme.RedDark);
                        using (Pen xPen = new Pen(Theme.Text, Math.Max(1.8f, size / 11f)))
                        {
                            xPen.StartCap = LineCap.Round;
                            xPen.EndCap = LineCap.Round;
                            int pad = Math.Max(5, size / 4);
                            g.DrawLine(xPen, pad, pad, size - pad, size - pad);
                            g.DrawLine(xPen, size - pad, pad, pad, size - pad);
                        }
                        break;

                    case "undamaged":
                        DrawLitPip(g, outerRect, innerRect, Theme.Green, Color.FromArgb(24, 128, 72));
                        break;

                    case "shoots":
                        DrawLitPip(g, outerRect, innerRect, Theme.Gold, Color.FromArgb(168, 108, 16));
                        int coreSize = Math.Max(4, drawSize / 3);
                        Rectangle coreRect = new Rectangle(
                            (size - coreSize) / 2,
                            (size - coreSize) / 2,
                            coreSize,
                            coreSize);
                        using (SolidBrush coreBrush = new SolidBrush(Color.FromArgb(210, 255, 248, 210)))
                        {
                            g.FillEllipse(coreBrush, coreRect);
                        }
                        break;

                    case "used":
                        DrawSocketPip(g, outerRect, Theme.InputBg, Theme.InputBorder, Theme.Slate);
                        break;

                    default:
                        return null;
                }
            }
            return bmp;
        }

        private static void DrawLitPip(Graphics g, Rectangle outerRect, Rectangle innerRect, Color faceColor, Color rimColor)
        {
            using (SolidBrush shadowBrush = new SolidBrush(Theme.Shadow))
            using (SolidBrush rimBrush = new SolidBrush(rimColor))
            using (SolidBrush faceBrush = new SolidBrush(faceColor))
            using (Pen borderPen = new Pen(Theme.Shadow, 1.5f))
            using (Pen innerPen = new Pen(Color.FromArgb(110, 255, 255, 255), 1.2f))
            {
                g.FillEllipse(shadowBrush, outerRect.X + 1, outerRect.Y + 2, outerRect.Width, outerRect.Height);
                g.FillEllipse(rimBrush, outerRect);
                g.FillEllipse(faceBrush, outerRect.X, outerRect.Y, outerRect.Width, Math.Max(2, outerRect.Height - 2));
                g.DrawEllipse(innerPen, innerRect);
                g.DrawEllipse(borderPen, outerRect);
            }

            int glintSize = Math.Max(3, outerRect.Width / 4);
            using (SolidBrush glintBrush = new SolidBrush(Color.FromArgb(165, 255, 255, 255)))
            {
                g.FillEllipse(glintBrush, outerRect.X + outerRect.Width / 5, outerRect.Y + outerRect.Height / 6, glintSize, glintSize);
            }
        }

        private static void DrawSocketPip(Graphics g, Rectangle outerRect, Color fillColor, Color ringColor, Color dotColor)
        {
            using (SolidBrush fillBrush = new SolidBrush(fillColor))
            using (Pen ringPen = new Pen(ringColor, 1.5f))
            {
                g.FillEllipse(fillBrush, outerRect);
                g.DrawEllipse(ringPen, outerRect);
            }

            if (dotColor != Color.Transparent)
            {
                int dotSize = Math.Max(4, outerRect.Width / 3);
                Rectangle dotRect = new Rectangle(
                    outerRect.X + (outerRect.Width - dotSize) / 2,
                    outerRect.Y + (outerRect.Height - dotSize) / 2,
                    dotSize,
                    dotSize);
                using (SolidBrush dotBrush = new SolidBrush(dotColor))
                {
                    g.FillEllipse(dotBrush, dotRect);
                }
            }
        }

        public void UpdateCellStatus(PictureBox targetCell, string status)
        {
            int size = Math.Min(targetCell.Width, targetCell.Height);
            if (size <= 0) size = 24;

            string normalized = (status ?? string.Empty).ToLowerInvariant();
            if (string.IsNullOrEmpty(normalized) || normalized == "none")
            {
                targetCell.Image = null;
                return;
            }

            targetCell.Image = CreateStatusPipImage(normalized, size);
        }

        private void SetDefaultGrayColors()
        {
            TableLayoutPanel[] playerAndShotTables = new TableLayoutPanel[]
            {
                TableShots,
                tablePlayerConditionCarrier,
                tablePlayerConditionSubmarine,
                tablePlayerConditionBattleship,
                tablePlayerConditionCruiser,
                tablePlayerConditionDestroyer
            };

            TableLayoutPanel[] enemyConditionTables = new TableLayoutPanel[]
            {
                tableEnemyConditionCarrier,
                tableEnemyConditionSubmarine,
                tableEnemyConditionBattleship,
                tableEnemyConditionCrusier,
                tableEnemyConditionDestroyer
            };

            foreach (TableLayoutPanel table in playerAndShotTables)
            {
                InitStatusTableCells(table, "Used");
            }

            foreach (TableLayoutPanel table in enemyConditionTables)
            {
                InitStatusTableCells(table, "None");
            }
        }

        private void InitStatusTableCells(TableLayoutPanel table, string initialStatus)
        {
            if (table == null) return;

            table.Controls.Clear();

            int cols = table.ColumnCount > 0 ? table.ColumnCount : 1;
            int rows = table.RowCount > 0 ? table.RowCount : 1;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    PictureBox picBox = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(1),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.Transparent
                    };

                    table.Controls.Add(picBox, col, row);
                    UpdateCellStatus(picBox, initialStatus);
                }
            }
        }

        private void pnlOutlinePaint(Control ctrl, PaintEventArgs e, Color? customBorder = null, bool soft = false)
        {
            if (soft)
            {
                using (Pen borderPen = new Pen(customBorder ?? SoftBorder, 2))
                {
                    e.Graphics.DrawRectangle(borderPen, 1, 1, ctrl.Width - 3, ctrl.Height - 3);
                }
                return;
            }

            using (Pen shadowPen = new Pen(Theme.Shadow, 1))
            using (Pen borderPen = new Pen(customBorder ?? Outline, 2))
            {
                e.Graphics.DrawRectangle(shadowPen, 0, 0, ctrl.Width - 1, ctrl.Height - 1);
                e.Graphics.DrawRectangle(borderPen, 1, 1, ctrl.Width - 3, ctrl.Height - 3);
            }
        }




// Gắn vào sự kiện Paint của Form (hoặc Panel cha chứa các card)
        private void ParentContainer_Paint(object sender, PaintEventArgs e)
        {
            DrawSoftShadow(e.Graphics, panel2, cornerRadius: 8, blurSize: 16, offsetX: 0, offsetY: 6, maxAlpha: 110);
            DrawSoftShadow(e.Graphics, panel9, cornerRadius: 8, blurSize: 16, offsetX: 0, offsetY: 6, maxAlpha: 110);
        }

        private void DrawSoftShadow(
            Graphics g,
            Control targetCard,
            int cornerRadius = 8,
            int blurSize = 16,
            int offsetX = 0,
            int offsetY = 6,
            int maxAlpha = 110)
        {
            if (!targetCard.Visible || targetCard.Width < 4 || targetCard.Height < 4) return;

            GraphicsState state = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            RectangleF shadowRect = new RectangleF(
                targetCard.Left + offsetX - blurSize,
                targetCard.Top + offsetY - blurSize,
                targetCard.Width + blurSize * 2f,
                targetCard.Height + blurSize * 2f);

            using (GraphicsPath path = CreateRoundedRectPath(shadowRect, cornerRadius + blurSize))
            using (PathGradientBrush pgb = new PathGradientBrush(path))
            {
                float fx = Math.Max(0f, Math.Min(0.95f, (shadowRect.Width - blurSize * 2.2f) / shadowRect.Width));
                float fy = Math.Max(0f, Math.Min(0.95f, (shadowRect.Height - blurSize * 2.2f) / shadowRect.Height));
                pgb.FocusScales = new PointF(fx, fy);

                pgb.InterpolationColors = new ColorBlend
                {
                    Colors = new[]
                    {
                Color.FromArgb(0, 0, 8, 20),
                Color.FromArgb((int)(maxAlpha * 0.22f), 0, 8, 20),
                Color.FromArgb((int)(maxAlpha * 0.60f), 0, 8, 20),
                Color.FromArgb(maxAlpha, 0, 8, 20)
            },
                    Positions = new[] { 0f, 0.35f, 0.72f, 1f }
                };

                g.FillPath(pgb, path);
            }

            g.Restore(state);
        }

        private static GraphicsPath CreateRoundedRectPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0.5f)
            {
                path.AddRectangle(rect);
                return path;
            }

            float d = Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height));
            path.AddArc(rect.X, rect.Y, d, d, 180f, 90f);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270f, 90f);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0f, 90f);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        private Panel CreateLayeredBackdrop(Control card, int offsetX = 6, int offsetY = 6, Color? layerColor = null)
        {
            if (card?.Parent == null) return null;

            Control parent = card.Parent;
            Panel backdrop = new Panel
            {
                Size = card.Size,
                Location = new Point(card.Left + offsetX, card.Top + offsetY),
                BackColor = layerColor ?? Color.FromArgb(8, 18, 34), // Màu lớp nền nằm dưới
                Anchor = card.Anchor,
                Visible = card.Visible,
                Enabled = false
            };

            // Đặt backdrop nằm ngay phía sau card để card đè lên trên
            int cardIndex = parent.Controls.GetChildIndex(card);
            parent.Controls.Add(backdrop);
            parent.Controls.SetChildIndex(backdrop, cardIndex + 1);

            // Tự động đồng bộ vị trí, kích thước, ẩn/hiện khi card thay đổi
            card.LocationChanged += (s, e) => backdrop.Location = new Point(card.Left + offsetX, card.Top + offsetY);
            card.SizeChanged += (s, e) => backdrop.Size = card.Size;
            card.VisibleChanged += (s, e) => backdrop.Visible = card.Visible;
            card.Disposed += (s, e) => backdrop.Dispose();

            return backdrop;
        }

        private static Color ShiftColor(Color c, int delta)
        {
            return Color.FromArgb(
                c.A,
                Math.Max(0, Math.Min(255, c.R + delta)),
                Math.Max(0, Math.Min(255, c.G + delta)),
                Math.Max(0, Math.Min(255, c.B + delta)));
        }

        private void pnlOutlinePaintUP(TableLayoutPanel tbl, TableLayoutCellPaintEventArgs e)
        {
            using (Pen pen = new Pen(Outline, 1))
            {
                e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Top - 1, e.CellBounds.Right, e.CellBounds.Top - 1);
            }
        }

        private void pnlOutlinePaintleft(TableLayoutPanel tbl, TableLayoutCellPaintEventArgs e)
        {
            using (Pen pen = new Pen(Outline, 1))
            {
                e.Graphics.DrawLine(pen, e.CellBounds.Left - 1, e.CellBounds.Top, e.CellBounds.Left - 1, e.CellBounds.Bottom);
            }
        }



        public void CreateTransparentChatBox()
        {
            if (_chatAutoHideTimer == null)
            {
                _chatAutoHideTimer = new Timer();
                _chatAutoHideTimer.Interval = 5000;
                _chatAutoHideTimer.Tick += (s, ev) =>
                {
                    _chatAutoHideTimer.Stop();
                    if (!_Chat && chatPanel != null && !chatPanel.IsDisposed)
                    {
                        chatPanel.Visible = false;
                    }
                };
            }

            if (chatPanel != null)
            {
                chatPanel.AutoScroll = true;
                chatPanel.BackColor = Theme.InputBg;
                chatPanel.BorderStyle = BorderStyle.FixedSingle;
                chatPanel.Padding = new Padding(6);
                chatPanel.FlowDirection = FlowDirection.TopDown;
                chatPanel.WrapContents = false;
                chatPanel.Visible = false;
                chatPanel.BringToFront();
            }

            if (boxChat != null)
            {
                Theme.StyleTextBox(boxChat, 9f);
                boxChat.ForeColor = Theme.Text;
                boxChat.BorderStyle = BorderStyle.FixedSingle;
                boxChat.Visible = false;
                boxChat.BringToFront();
            }
        }

        private async void boxChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string messageText = boxChat.Text.Trim();

                if (string.IsNullOrWhiteSpace(messageText)) return;

                if (!IsOnlineMode)
                {
                    string localName = !string.IsNullOrWhiteSpace(PlayerProfile.Username) ? PlayerProfile.Username : "Tester";
                    AddChatMessage(localName, messageText);
                    boxChat.Clear();
                    return;
                }

                if (NetworkManager.Instance == null || !NetworkManager.Instance.IsConnected)
                {
                    AddChatMessage("Hệ thống", "Chưa kết nối đến máy chủ!");
                    return;
                }

                try
                {
                    var chatMsg = new ChatMessageDto
                    {
                        Sender = PlayerProfile.Username,
                        Content = $"[MATCH_CHAT:{_currentMatch?.RoomId}]:{messageText}"
                    };

                    var packet = Packet.Create(PacketType.ChatMessage, chatMsg);
                    boxChat.Clear();
                    await NetworkManager.Instance.SendAsync(packet);
                }
                catch (Exception ex)
                {
                    AddChatMessage("Hệ thống", $"Lỗi gửi tin nhắn: {ex.Message}");
                }
            }
        }

        private void tableLayoutPanel4_CellPaint(object sender, TableLayoutCellPaintEventArgs e) => pnlOutlinePaintUP(tableLayoutPanel4, e);
        private void tableLayoutPanel5_CellPaint(object sender, TableLayoutCellPaintEventArgs e) => pnlOutlinePaintleft(tableLayoutPanel5, e);
        private void tableLayoutPanel6_CellPaint(object sender, TableLayoutCellPaintEventArgs e) => pnlOutlinePaintUP(tableLayoutPanel6, e);
        private void tableLayoutPanel3_CellPaint(object sender, TableLayoutCellPaintEventArgs e) => pnlOutlinePaintleft(tableLayoutPanel3, e);
        private void pnlPlayerName_Paint(object sender, PaintEventArgs e) => pnlOutlinePaint(pnlPlayerName, e, _playerNameBorderColor, soft: true);
        private void pnlEnemyName_Paint(object sender, PaintEventArgs e) => pnlOutlinePaint(pnlEnemyName, e, _enemyNameBorderColor, soft: true);
        private void pnlEnemyName_Paint_1(object sender, PaintEventArgs e) => pnlOutlinePaint(pnlEnemyName, e, _enemyNameBorderColor, soft: true);

        private void btnChat_Click(object sender, EventArgs e)
        {
            _Chat = !_Chat;

            if (_Chat)
            {
                _chatAutoHideTimer?.Stop();
                btnChat.Text = "Cancel";
                if (chatPanel != null) chatPanel.Visible = true;
                if (boxChat != null)
                {
                    boxChat.Visible = true;
                    boxChat.Focus();
                }
            }
            else
            {
                _chatAutoHideTimer?.Stop();
                btnChat.Text = "Chat";
                if (chatPanel != null) chatPanel.Visible = false;
                if (boxChat != null)
                {
                    boxChat.Visible = false;
                    boxChat.Clear();
                }
            }
        }

        private void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CleanupAndLeaveRoom();
        }

        private void BattleshipDesign_Disposed(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.FormClosing -= ParentForm_FormClosing;
            }

            CleanupAndLeaveRoom();
        }

        private void CleanupAndLeaveRoom()
        {
            if (_isCleanedUp) return;
            _isCleanedUp = true;

            _turnTimer?.Stop();
            _turnTimer?.Dispose();

            _connectionCheckTimer?.Stop();
            _connectionCheckTimer?.Dispose();

            _chatAutoHideTimer?.Stop();
            _chatAutoHideTimer?.Dispose();

            if (IsOnlineMode && NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnPacketReceived -= NetworkManager_OnPacketReceived;

                if (NetworkManager.Instance.IsConnected && !string.IsNullOrEmpty(_currentMatch?.RoomId))
                {
                    try
                    {
                        var leaveDto = new LeaveRoomDto { RoomId = _currentMatch.RoomId };
                        _ = NetworkManager.Instance.SendAsync(Packet.Create(PacketType.LeaveRoom, leaveDto));
                    }
                    catch { }
                }
            }
        }


        // Viền nổi cho panel2 (nằm bên trái -> mép đè hướng sang phải)
        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            DrawElevatedSidePanelBorder(e.Graphics, panel2.ClientRectangle, overlapOnRight: true);
        }

        // Viền nổi cho panel9 (nằm bên phải -> mép đè hướng sang trái)
        private void panel9_Paint(object sender, PaintEventArgs e)
        {
            DrawElevatedSidePanelBorder(e.Graphics, panel9.ClientRectangle, overlapOnRight: false);
        }

        private void DrawElevatedSidePanelBorder(Graphics g, Rectangle rect, bool overlapOnRight)
        {
            if (rect.Width <= 4 || rect.Height <= 4) return;

            // Tắt AntiAlias khi vẽ đường thẳng 1px để viền sắc nét tuyệt đối, không bị lem pixel
            g.SmoothingMode = SmoothingMode.None;

            int r = rect.Width - 1;
            int b = rect.Height - 1;

            // 1. Khung viền chính 1px
            using (Pen basePen = new Pen(SoftBorder, 1))
            {
                g.DrawRectangle(basePen, 0, 0, r, b);
            }

            // 2. Đường bắt sáng mảnh ở cạnh trên tạo cảm giác tấm thẻ nổi
            using (Pen topLightPen = new Pen(Color.FromArgb(65, 190, 232, 255), 1))
            {
                g.DrawLine(topLightPen, 1, 1, r - 1, 1);
            }

            // 3. Nhấn mạnh mép đè lên panel1 (cạnh phải với panel2, cạnh trái với panel9)
            using (Pen overlapEdgePen = new Pen(EdgeHighlight, 1))
            using (Pen innerRimPen = new Pen(Color.FromArgb(45, 190, 232, 255), 1))
            using (Pen bottomRimPen = new Pen(Color.FromArgb(140, 6, 18, 34), 1))
            {
                if (overlapOnRight)
                {
                    // panel2 bên trái: cạnh phải sáng sắc nét đè sang panel1
                    g.DrawLine(overlapEdgePen, r, 0, r, b);
                    g.DrawLine(innerRimPen, r - 1, 1, r - 1, b - 1);
                }
                else
                {
                    // panel9 bên phải: cạnh trái sáng sắc nét đè sang panel1
                    g.DrawLine(overlapEdgePen, 0, 0, 0, b);
                    g.DrawLine(innerRimPen, 1, 1, 1, b - 1);
                }

                // Cạnh dưới hơi sẫm để tạo độ dày tấm panel
                g.DrawLine(bottomRimPen, 1, b - 1, r - 1, b - 1);
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            // Bóng đổ trên nền tableLayoutPanel1 (hiện rõ ở khoảng Margin xung quanh panel2 và panel9)
            // panel2 ở bên trái -> bóng lệch phải & xuống dưới
            if (panel2 != null && panel2.Visible)
            {
                DrawLayeredDropEdge(g, panel2.Bounds, offsetX: 5, offsetY: 5);
            }

            // panel9 ở bên phải -> bóng lệch trái & xuống dưới
            if (panel9 != null && panel9.Visible)
            {
                DrawLayeredDropEdge(g, panel9.Bounds, offsetX: -5, offsetY: 5);
            }
        }

        private static void DrawLayeredDropEdge(Graphics g, Rectangle bounds, int offsetX, int offsetY)
        {
            // Xếp 3 lớp bóng phẳng giảm dần độ đậm -> mềm mắt nhưng cạnh vẫn vuông sắc nét
            int[] alphas = { 115, 65, 30 };
            for (int i = 0; i < alphas.Length; i++)
            {
                int step = i + 1;
                int dx = offsetX > 0 ? Math.Min(offsetX, step * 2) : Math.Max(offsetX, -step * 2);
                int dy = Math.Min(offsetY, step * 2);

                using (SolidBrush b = new SolidBrush(Color.FromArgb(alphas[i], 3, 10, 22)))
                {
                    g.FillRectangle(b, bounds.X + dx, bounds.Y + dy, bounds.Width, bounds.Height);
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (panel1.Width <= 20 || panel1.Height <= 0) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            // Bóng đè mượt từ panel2 (mép trái) và panel9 (mép phải) hắt thẳng vào panel1
            int shadowWidth = 14;
            int maxAlpha = 165;

            for (int i = 0; i < shadowWidth; i++)
            {
                // Suy giảm bậc 2 (đậm sát mép panel2/panel9, tan mượt về phía giữa panel1)
                float t = 1f - (i / (float)shadowWidth);
                int alpha = (int)(maxAlpha * t * t);
                if (alpha <= 0) continue;

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, 2, 8, 18)))
                {
                    // Bóng từ panel2 (trái) đè sang
                    g.FillRectangle(brush, i, 0, 1, panel1.Height);

                    // Bóng từ panel9 (phải) đè sang
                    g.FillRectangle(brush, panel1.Width - 1 - i, 0, 1, panel1.Height);
                }
            }
        }
        //----------------------------- CÁC SỰ KIỆN TRỐNG BÊN DƯỚI GIỮ NGUYÊN -------------------------------------------------------
        private void label4_Click(object sender, EventArgs e) { }
        private void panel8_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionCarrier_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionSubmarine_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionBattleship_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionCruiser_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionDestroyer_Paint(object sender, PaintEventArgs e) { }
        private void TxtTimer_TextChanged(object sender, EventArgs e) { }

        private void TableShots_Paint(object sender, PaintEventArgs e) { }
        private void lblShots_Click(object sender, EventArgs e) { }
        private void tableEnemyConditionCarrier_Paint(object sender, PaintEventArgs e) { }
        private void tableEnemyConditionSubmarine_Paint(object sender, PaintEventArgs e) { }
        private void tableEnemyConditionBattleship_Paint(object sender, PaintEventArgs e) { }
        private void tableEnemyConditionCrusier_Paint(object sender, PaintEventArgs e) { }
        private void tableEnemyConditionDestroyer_Paint(object sender, PaintEventArgs e) { }
        private void pnlPlayerBoard_Paint(object sender, PaintEventArgs e) { }
        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e) { }
        private void label21_Click(object sender, EventArgs e) { }
        private void label22_Click(object sender, EventArgs e) { }
        private void pnlPlayerFrameBoard_Paint(object sender, PaintEventArgs e) { }
        private void lblPlayerName_Click(object sender, EventArgs e) { }
        private void button1_Click(object sender, EventArgs e) { }
        private void boxChat_TextChanged(object sender, EventArgs e) { }
    }
}