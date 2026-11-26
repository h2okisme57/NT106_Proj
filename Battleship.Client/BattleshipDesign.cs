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
        MyTurn,          // Lượt của mình
        EnemyTurn,       // Lượt của đối thủ (khóa bảng, chờ địch bắn)
        GameOver         // Kết thúc trận đấu
    }

    public partial class BattleshipDesign : UserControl
    {
        Color Outline = Color.FromArgb(88, 160, 200);
        private BoardGame playerBoard;
        public BoardGame EnemyBoard;
        private MatchFoundDto _currentMatch;
        private bool _myShipsPlaced = false;
        private bool _enemyShipsPlaced = false;
        private bool _matchStarted = false;

        private bool _isCleanedUp = false;
        private bool _isGameOverHandled = false;
        private bool _wasConnected = false;

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

            this.Load += BattleshipDesign_Load;
            this.Disposed += BattleshipDesign_Disposed;

            _currentMatch = matchInfo ?? new MatchFoundDto
            {
                RoomId = "Room_Test_999",
                OpponentName = "Waiting for opponent...",
                IsHost = true
            };

            SetupPlayerNames();
            SetupPlayerBoard();

            if (NetworkManager.Instance != null)
            {
                _wasConnected = NetworkManager.Instance.IsConnected;
                NetworkManager.Instance.OnPacketReceived -= NetworkManager_OnPacketReceived;
                NetworkManager.Instance.OnPacketReceived += NetworkManager_OnPacketReceived;
            }
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

            InitTurnTimer();
            SetDefaultGrayColors();
            CreateTransparentChatBox();
            SetTurnState(GameTurnState.PlacingShips);
        }

        private void InitTurnTimer()
        {
            _turnTimer = new Timer();
            _turnTimer.Interval = 1000;
            _turnTimer.Tick += TurnTimer_Tick;

            _connectionCheckTimer = new Timer();
            _connectionCheckTimer.Interval = 1000;
            _connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
            _connectionCheckTimer.Start();
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
            if (_turnTimer == null) return;
            _turnTimer.Stop();
            _secondsLeft = durationSeconds > 0 ? durationSeconds : 30;
            if (TxtTimer != null) TxtTimer.Text = $"{_secondsLeft}s";
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

                // Hết giờ: khóa tạm bảng địch trong khi chờ Server gửi PacketType.TurnChange
                if (CurrentState == GameTurnState.MyTurn && EnemyBoard.IsMyTurn)
                {
                    EnemyBoard.EndTurn();
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

            // Bật chế độ Online cho cả 2 bảng
            playerBoard.IsOnlineMode = true;
            EnemyBoard.IsOnlineMode = true;
            EnemyBoard.IsEnemyBoard = true;

            // 1. Vừa đặt xong con tàu nào -> bảng tình trạng tàu đó chuyển sang xanh lá ngay
            playerBoard.OnShipPlacedSuccessfully += (placedShip) =>
            {
                TableLayoutPanel targetTable = GetShipConditionTable(placedShip.Name, isEnemy: false);
                UpdateSingleShipCondition(targetTable, placedShip.IsPlaced, placedShip.HitCount);
            };

            // 2. Khi xếp xong đủ 5 tàu trên bảng của mình
            playerBoard.OnAllShipsPlaced += async () =>
            {
                if (_isGameOverHandled) return;

                _myShipsPlaced = true;
                SetTurnState(GameTurnState.Waiting);

                try
                {
                    // Gửi đội hình 5 tàu lên Server
                    await playerBoard.SendMyShipsToServerAsync(_currentMatch.RoomId);

                    if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
                    {
                        // Gửi kèm PlayerReadyDto để Server kích hoạt GameStart
                        var readyDto = new PlayerReadyDto
                        {
                            RoomId = _currentMatch.RoomId,
                            IsReady = true
                        };
                        await NetworkManager.Instance.SendAsync(Packet.Create(PacketType.PlayerReady, readyDto));

                        // Gửi tín hiệu đồng bộ cho Client đối thủ (dự phòng trường hợp Server không broadcast GameStart)
                        var syncPlaced = new ChatMessageDto
                        {
                            Sender = PlayerProfile.Username ?? string.Empty,
                            Content = $"[SYS_SHIPS_READY:{_currentMatch.RoomId}]:1"
                        };
                        await NetworkManager.Instance.SendAsync(Packet.Create(PacketType.ChatMessage, syncPlaced));
                    }

                    AddChatMessage("Hệ thống", "Đã gửi đội hình tàu. Đang chờ đối thủ xếp tàu...");

                    // Nếu đối thủ đã xếp xong trước đó -> vào trận ngay
                    if (_enemyShipsPlaced)
                    {
                        StartMatchLocally();
                    }
                }
                catch (Exception ex)
                {
                    AddChatMessage("Hệ thống", $"Lỗi gửi đội hình tàu: {ex.Message}");
                }
            };

            // 3. Cập nhật tình trạng tàu khi có tàu bị bắn trúng / đánh chìm
            playerBoard.OnShipConditionUpdated += (ships) => UpdateAllShipsConditionUI(ships, isEnemy: false);
            EnemyBoard.OnShipConditionUpdated += (ships) => UpdateAllShipsConditionUI(ships, isEnemy: true);

            // 4. Cập nhật số viên đạn còn lại lên TableShots
            EnemyBoard.OnRemainingShotsChanged += (remainingShots) =>
            {
                UpdateShotsUI(remainingShots);
            };

            // 5. Chỉ tự đổi lượt cục bộ khi chơi Offline (Online do Server điều phối)
            EnemyBoard.OnTurnEnded += () =>
            {
                if (!EnemyBoard.IsOnlineMode && CurrentState == GameTurnState.MyTurn)
                {
                    SetTurnState(GameTurnState.EnemyTurn);
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

        public void SetTurnState(GameTurnState newState)
        {
            if (_isGameOverHandled && newState != GameTurnState.GameOver) return;

            CurrentState = newState;

            switch (newState)
            {
                case GameTurnState.PlacingShips:
                    playerBoard.IsPlacementPhase = true;
                    if (lblShots != null) lblShots.Text = "Place Ships";
                    StopTurnTimer($"{TurnDurationSeconds}s");
                    UpdateShotsUI(0);
                    UpdateAllShipsConditionUI(playerBoard.PlayerShips, isEnemy: false);
                    UpdateAllShipsConditionUI(EnemyBoard.PlayerShips, isEnemy: true);
                    break;

                case GameTurnState.Waiting:
                    playerBoard.IsPlacementPhase = false;
                    if (lblShots != null) lblShots.Text = "Waiting...";
                    StopTurnTimer("--");
                    UpdateShotsUI(0);
                    break;

                case GameTurnState.MyTurn:
                    pnlPlayerName.ForeColor = Color.LimeGreen;
                    pnlEnemyName.ForeColor = Color.White;

                    if (lblShots != null) lblShots.Text = "Your turn";
                    StartTurnTimer(TurnDurationSeconds);

                    EnemyBoard.StartTurn();
                    UpdateShotsUI(1);
                    UpdateAllShipsConditionUI(EnemyBoard.PlayerShips, isEnemy: true);
                    break;

                case GameTurnState.EnemyTurn:
                    if (EnemyBoard.IsMyTurn) EnemyBoard.EndTurn();

                    pnlPlayerName.ForeColor = Color.White;
                    pnlEnemyName.ForeColor = Color.OrangeRed;

                    if (lblShots != null) lblShots.Text = "Enemy's turn";
                    StartTurnTimer(TurnDurationSeconds);
                    UpdateShotsUI(0);
                    break;

                case GameTurnState.GameOver:
                    playerBoard.IsPlacementPhase = false;
                    if (EnemyBoard.IsMyTurn) EnemyBoard.EndTurn();
                    if (lblShots != null) lblShots.Text = "End of game";
                    StopTurnTimer("0s");
                    UpdateShotsUI(0);
                    break;
            }
        }

        // =========================================================
        // NHẬN PACKET TRẬN ĐẤU TỪ SERVER
        // =========================================================
        private void NetworkManager_OnPacketReceived(Packet packet)
        {
            if (packet == null || this.IsDisposed) return;

            switch (packet.Type)
            {
                // 1. Trận đấu bắt đầu sau khi cả 2 xếp tàu xong
                case PacketType.GameStart:
                    {
                        var gameStart = packet.DeserializePayload<GameStartDto>();
                        if (gameStart == null || !IsCurrentRoom(gameStart.RoomId)) return;

                        SafeInvokeUI(() =>
                        {
                            if (_isGameOverHandled) return;

                            TurnDurationSeconds = gameStart.TimeLimitSeconds > 0 ? gameStart.TimeLimitSeconds : 30;
                            EnemyBoard.MarkAllShipsReady();

                            bool isFirstTurn = string.Equals(
                                gameStart.FirstTurnUsername,
                                PlayerProfile.Username,
                                StringComparison.OrdinalIgnoreCase);

                            AddChatMessage("Hệ thống", isFirstTurn
                                ? "Trận đấu bắt đầu! Bạn đi trước."
                                : $"Trận đấu bắt đầu! {gameStart.FirstTurnUsername} đi trước.");

                            SetTurnState(isFirstTurn ? GameTurnState.MyTurn : GameTurnState.EnemyTurn);
                        });
                        break;
                    }

                // 2. Kết quả từng phát bắn (của mình hoặc đối thủ)
                case PacketType.ShotResult:
                    {
                        var result = packet.DeserializePayload<ShotResultDto>();
                        if (result == null || !IsCurrentRoom(result.RoomId)) return;

                        SafeInvokeUI(() => HandleShotResult(result));
                        break;
                    }

                // 3. Server thông báo đổi lượt (bắn hụt hoặc hết giờ)
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

                            SetTurnState(isMyTurn ? GameTurnState.MyTurn : GameTurnState.EnemyTurn);
                        });
                        break;
                    }

                // 4. Kết thúc trận đấu (thắng / thua)
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

                // 5. Đối thủ mất kết nối / thoát giữa trận
                case PacketType.OpponentDisconnected:
                    {
                        SafeInvokeUI(() =>
                        {
                            HandleGameOver(true, "Đối thủ đã mất kết nối mạng hoặc thoát khỏi trận đấu!");
                        });
                        break;
                    }

                // 6. Chat trong trận (lọc bỏ gói đồng bộ Ready của phòng chờ)
                case PacketType.ChatMessage:
                    {
                        try
                        {
                            var chatMsg = packet.DeserializePayload<ChatMessageDto>();
                            if (chatMsg != null &&
                                !string.IsNullOrEmpty(chatMsg.Content) &&
                                !chatMsg.Content.StartsWith("[SYS_READY:", StringComparison.Ordinal))
                            {
                                AddChatMessage(chatMsg.Sender, chatMsg.Content);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi giải mã tin nhắn: {ex.Message}");
                        }
                        break;
                    }

                // 7. Thông báo lỗi từ Server
                case PacketType.Error:
                    {
                        var err = packet.DeserializePayload<ErrorDto>();
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

                // Kiểm tra thắng ngay khi 5 tàu địch đều đã chìm
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

                // Kiểm tra thua ngay khi 5 tàu của mình đều đã chìm
                if (playerBoard.PlayerShips.Count > 0 && playerBoard.PlayerShips.All(s => s.IsPlaced && s.HitCount >= s.Size))
                {
                    HandleGameOver(false, "Toàn bộ hạm đội của bạn đã bị đánh chìm!");
                    return;
                }
            }

            // Bắn trúng -> bắn tiếp; Bắn hụt -> đổi lượt theo NextTurnUsername từ Server
            if (!string.IsNullOrWhiteSpace(result.NextTurnUsername))
            {
                bool nextIsMyTurn = string.Equals(
                    result.NextTurnUsername,
                    PlayerProfile.Username,
                    StringComparison.OrdinalIgnoreCase);

                SetTurnState(nextIsMyTurn ? GameTurnState.MyTurn : GameTurnState.EnemyTurn);
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

        private void HandleGameOver(bool isWinner, string reason)
        {
            if (_isGameOverHandled) return;
            _isGameOverHandled = true;

            _connectionCheckTimer?.Stop();
            SetTurnState(GameTurnState.GameOver);

            pnlPlayerName.ForeColor = isWinner ? Color.LimeGreen : Color.OrangeRed;
            pnlEnemyName.ForeColor = isWinner ? Color.OrangeRed : Color.LimeGreen;

            if (lblShots != null)
            {
                lblShots.Text = isWinner ? "VICTORY!" : "DEFEAT!";
            }

            string title = isWinner ? "CHIẾN THẮNG" : "THẤT BẠI";
            string message = isWinner
                ? $"Chúc mừng! Bạn đã giành chiến thắng.\nLý do: {reason}"
                : $"Bạn đã thua trận.\nLý do: {reason}";

            AddChatMessage("Hệ thống", $"{title} - {reason}");
            MessageBox.Show(message, title, MessageBoxButtons.OK,
                isWinner ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
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

        private void UpdateShotsUI(int remainingShots)
        {
            if (TableShots == null) return;

            var orderedPics = TableShots.Controls.OfType<PictureBox>()
                                        .OrderBy(p => TableShots.GetColumn(p))
                                        .ThenBy(p => TableShots.GetRow(p))
                                        .ToList();

            for (int i = 0; i < orderedPics.Count; i++)
            {
                string status = (i < remainingShots) ? "Shoots" : "Used";
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

            // Tàu của mình: chưa đặt -> xám, đã đặt -> xanh lá, trúng đạn -> đỏ từ trái sang phải
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

        private void UpdateSingleShipCondition(TableLayoutPanel table, bool isPlaced, int hitCount)
        {
            if (table == null) return;

            var orderedPics = table.Controls.OfType<PictureBox>()
                                   .OrderBy(p => table.GetColumn(p))
                                   .ThenBy(p => table.GetRow(p))
                                   .ToList();

            for (int i = 0; i < orderedPics.Count; i++)
            {
                string status;
                if (!isPlaced)
                {
                    status = "Used";
                }
                else
                {
                    status = (i < hitCount) ? "Damaged" : "Undamaged";
                }

                UpdateCellStatus(orderedPics[i], status);
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
                msgLabel.ForeColor = Color.White;
                msgLabel.AutoSize = true;
                msgLabel.MaximumSize = new Size(370, 0);

                chatPanel.Controls.Add(msgLabel);
                chatPanel.Visible = true;
                chatPanel.ScrollControlIntoView(msgLabel);
            }));
        }

        private void SetupPlayerNames()
        {
            string myName = PlayerProfile.Username ?? string.Empty;
            string enemyName = _currentMatch?.OpponentName ?? "Waiting for opponent...";

            pnlPlayerName.Text = myName;
            pnlEnemyName.Text = enemyName;
        }

        private Image CreateCircleImage(Color color, int size)
        {
            if (size <= 0) size = 20;

            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                int margin = 2;
                int drawSize = size - (margin * 2);

                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, margin, margin, drawSize, drawSize);
                }
            }
            return bmp;
        }

        public void UpdateCellStatus(PictureBox targetCell, string status)
        {
            int size = Math.Min(targetCell.Width, targetCell.Height);
            if (size <= 0) size = 20;

            switch (status.ToLower())
            {
                case "damaged":
                    targetCell.Image = CreateCircleImage(Color.Red, size);
                    break;
                case "undamaged":
                    targetCell.Image = CreateCircleImage(Color.LimeGreen, size);
                    break;
                case "shoots":
                    targetCell.Image = CreateCircleImage(Color.Yellow, size);
                    break;
                case "used":
                    targetCell.Image = CreateCircleImage(Color.Gray, size);
                    break;
                default:
                    targetCell.Image = null;
                    break;
            }
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

            // Bảng đạn & bảng tàu của mình: khởi tạo chấm xám
            foreach (TableLayoutPanel table in playerAndShotTables)
            {
                InitStatusTableCells(table, "Used");
            }

            // Bảng tàu của địch: không hiển thị gì cả cho đến khi tàu chìm
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

        private void pnlOutlinePaint(Control ctrl, PaintEventArgs e)
        {
            int border_thickness = 4;
            using (Pen pen = new Pen(Outline, border_thickness))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, ctrl.Width - 1, ctrl.Height - 1);
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
                chatPanel.BackColor = Color.FromArgb(180, 10, 20, 40);
                chatPanel.FlowDirection = FlowDirection.TopDown;
                chatPanel.WrapContents = false;
                chatPanel.Visible = false;
                chatPanel.BringToFront();
            }

            if (boxChat != null)
            {
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
        private void pnlPlayerName_Paint(object sender, PaintEventArgs e) => pnlOutlinePaint(pnlPlayerName, e);
        private void pnlEnemyName_Paint(object sender, PaintEventArgs e) => pnlOutlinePaint(pnlEnemyName, e);
        private void pnlEnemyName_Paint_1(object sender, PaintEventArgs e) => pnlOutlinePaint(pnlEnemyName, e);

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

            if (NetworkManager.Instance != null)
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

        private void StartMatchLocally(string? serverFirstTurn = null, int? serverTimeLimit = null)
        {
            if (_isGameOverHandled) return;

            if (serverTimeLimit.HasValue && serverTimeLimit.Value > 0)
                TurnDurationSeconds = serverTimeLimit.Value;

            EnemyBoard.MarkAllShipsReady();

            bool isFirstTurn;
            if (!string.IsNullOrWhiteSpace(serverFirstTurn))
            {
                isFirstTurn = string.Equals(serverFirstTurn, PlayerProfile.Username, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                // Mặc định Host đi trước nếu khởi động đồng bộ từ Client
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
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void TableShots_Paint(object sender, PaintEventArgs e) { }
        private void lblShots_Click(object sender, EventArgs e) { }
        private void tableEnemyConditionCarrier_Paint(object sender, PaintEventArgs e) { }
        private void tableEnemyConditionSubmarine_Paint(object sender, PaintEventArgs e) { }
        private void tableEnemyConditionBattleship_Paint(object sender, PaintEventArgs e) { }
        private void tableEnemyConditionCrusier_Paint(object sender, PaintEventArgs e) { }
        private void tableEnemyConditionDestroyer_Paint(object sender, PaintEventArgs e) { }
        private void pnlPlayerBoard_Paint(object sender, PaintEventArgs e) { }
        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e) { }
        private void label21_Click(object sender, PaintEventArgs e) { }
        private void label22_Click(object sender, PaintEventArgs e) { }
        private void pnlPlayerFrameBoard_Paint(object sender, PaintEventArgs e) { }
        private void lblPlayerName_Click(object sender, PaintEventArgs e) { }
        private void button1_Click(object sender, EventArgs e) { }
        private void boxChat_TextChanged(object sender, EventArgs e) { }
    }
}