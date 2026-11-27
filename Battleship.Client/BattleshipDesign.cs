using Battleship.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

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
        private static readonly Color PlacementPurple = Color.FromArgb(168, 85, 247);
        private Color _playerNameBorderColor = Theme.Cyan;
        private Color _enemyNameBorderColor = Theme.Cyan;
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

        private TextBox boxChat;
        private bool _Chat = false;

        public BattleshipDesign(MatchFoundDto matchInfo = null)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
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

        private void Applytheme()
        {
            this.BackColor = Theme.BgBottom;
            tableLayoutPanel1.BackColor = Color.Transparent;
            tableLayoutPanel2.BackColor = Color.Transparent;

            panel1.BackColor = Theme.PanelBg;
            panel2.BackColor = Theme.PanelBg;
            panel9.BackColor = Theme.PanelBg;
            pnlPlayerName.BackColor = Theme.PanelBg;
            pnlPlayerName.Padding = new Padding(4);
            pnlEnemyName.BackColor = Theme.PanelBg;
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
            _secondsLeft = durationSeconds > 0 ? durationSeconds : 30;
            if (TxtTimer != null) TxtTimer.Text = $"{_secondsLeft}s";

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
            _secondsLeft--;
            if (TxtTimer != null) TxtTimer.Text = $"{Math.Max(0, _secondsLeft)}s";

            if (_secondsLeft <= 0)
            {
                _turnTimer.Stop();

                if (IsOnlineMode)
                {
                    // Online: chỉ hiển thị 0s và chờ Server gửi TurnChange (Reason=Timeout), không tự đổi lượt.
                    return;
                }

                // Chế độ Test: luân chuyển MyTurn <-> EnemyTurn khi hết giờ để test timer
                if (CurrentState == GameTurnState.MyTurn)
                {
                    AddChatMessage("Test", "Hết thời gian MyTurn -> chuyển sang đếm giờ EnemyTurn.");
                    SetTurnState(GameTurnState.EnemyTurn);
                }
                else if (CurrentState == GameTurnState.EnemyTurn)
                {
                    AddChatMessage("Test", "Hết thời gian EnemyTurn -> quay lại MyTurn.");
                    SetTurnState(GameTurnState.MyTurn);
                }
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
                    StopTurnTimer($"{TurnDurationSeconds}s");
                    _yellowShotsCount = 0;
                    UpdateShotsUI(_yellowShotsCount);
                    UpdateAllShipsConditionUI(playerBoard.PlayerShips, isEnemy: false);
                    UpdateAllShipsConditionUI(EnemyBoard.PlayerShips, isEnemy: true);
                    break;

                case GameTurnState.Waiting:
                    _playerNameBorderColor = Theme.Cyan;
                    _enemyNameBorderColor = Theme.Cyan;

                    playerBoard.IsPlacementPhase = false;
                    if (lblShots != null) lblShots.Text = "Waiting...";
                    StopTurnTimer("--");
                    _yellowShotsCount = 0;
                    UpdateShotsUI(_yellowShotsCount);
                    break;

                case GameTurnState.MyTurn:
                    _playerNameBorderColor = Theme.Gold;
                    _enemyNameBorderColor = Theme.Cyan;

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

                    _playerNameBorderColor = Theme.Cyan;
                    _enemyNameBorderColor = Theme.Gold;

                    if (lblShots != null) lblShots.Text = "Enemy's turn";
                    StartTurnTimer(displaySeconds);
                    _yellowShotsCount = 0;
                    UpdateShotsUI(_yellowShotsCount);
                    break;

                case GameTurnState.GameOver:
                    _playerNameBorderColor = Theme.Cyan;
                    _enemyNameBorderColor = Theme.Cyan;

                    playerBoard.IsPlacementPhase = false;
                    if (EnemyBoard.IsMyTurn) EnemyBoard.EndTurn();
                    if (lblShots != null) lblShots.Text = "End of game";
                    StopTurnTimer("0s");
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

                            AddChatMessage(chatMsg.Sender, chatMsg.Content);
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
                        DrawSocketPip(g, outerRect, Theme.InputBg, Color.FromArgb(140, Theme.InputBorder), Color.Transparent);
                        break;
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

        private void pnlOutlinePaint(Control ctrl, PaintEventArgs e, Color? customBorder = null)
        {
            using (Pen shadowPen = new Pen(Theme.Shadow, 1))
            using (Pen borderPen = new Pen(customBorder ?? Outline, 3))
            {
                e.Graphics.DrawRectangle(shadowPen, 0, 0, ctrl.Width - 1, ctrl.Height - 1);
                e.Graphics.DrawRectangle(borderPen, 2, 2, ctrl.Width - 5, ctrl.Height - 5);
            }
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

        private void panel2_Paint(object sender, PaintEventArgs e) => pnlOutlinePaint(panel2, e);
        private void panel9_Paint(object sender, PaintEventArgs e) => pnlOutlinePaint(panel9, e);

        public void CreateTransparentChatBox()
        {
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
                        Content = messageText
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
        private void pnlPlayerName_Paint(object sender, PaintEventArgs e) => pnlOutlinePaint(pnlPlayerName, e, _playerNameBorderColor);
        private void pnlEnemyName_Paint(object sender, PaintEventArgs e) => pnlOutlinePaint(pnlEnemyName, e, _enemyNameBorderColor);
        private void pnlEnemyName_Paint_1(object sender, PaintEventArgs e) => pnlOutlinePaint(pnlEnemyName, e, _enemyNameBorderColor);

        private void btnChat_Click(object sender, EventArgs e)
        {
            _Chat = !_Chat;

            if (_Chat)
            {
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

        //----------------------------- CÁC SỰ KIỆN TRỐNG BÊN DƯỚI GIỮ NGUYÊN -------------------------------------------------------
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void panel8_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionCarrier_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionSubmarine_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionBattleship_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionCruiser_Paint(object sender, PaintEventArgs e) { }
        private void tablePlayerConditionDestroyer_Paint(object sender, PaintEventArgs e) { }
        private void TxtTimer_TextChanged(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) => pnlOutlinePaint(panel1, e);
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