using Battleship.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Battleship.Client
{
    public enum GameTurnState
    {
        PlacingShips,    // Giai đoạn tự xếp 5 tàu
        WaitingOpponent, // Đã xếp xong, chờ đối thủ xếp tàu
        MyTurn,          // Lượt của mình (được bắn 5 viên, đếm ngược 60s)
        EnemyTurn,       // Lượt của đối thủ (khóa bảng, chờ địch bắn)
        GameOver         // Kết thúc trận đấu
    }

    public partial class BattleshipDesign : UserControl
    {
        Color Outline = Color.FromArgb(88, 160, 200);
        private BoardGame playerBoard;
        public BoardGame EnemyBoard;
        private MatchFoundDto _currentMatch;

        public GameTurnState CurrentState { get; private set; } = GameTurnState.PlacingShips;

        public int TurnDurationSeconds { get; set; } = 60;
        private Timer _turnTimer;
        private int _secondsLeft;

        private TextBox boxChat;
        private bool _Chat = false;

        public BattleshipDesign()
        {
            InitializeComponent();

            this.Load += BattleshipDesign_Load;

            _currentMatch = new MatchFoundDto
            {
                RoomId = "Room_Test_999",
                OpponentName = "Kẻ Hủy Diệt",
                IsHost = true
            };

            SetupPlayerNames();
            SetupPlayerBoard();

            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnPacketReceived += NetworkManager_OnPacketReceived;
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnPacketReceived -= NetworkManager_OnPacketReceived;
            }
            base.OnHandleDestroyed(e);
            _turnTimer?.Dispose();
        }

        private void BattleshipDesign_Load(object sender, EventArgs e)
        {
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
        }

        private void StartTurnTimer(int durationSeconds)
        {
            _turnTimer.Stop();
            _secondsLeft = durationSeconds;
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
            if (TxtTimer != null) TxtTimer.Text = $"{_secondsLeft}s";

            if (_secondsLeft <= 0)
            {
                _turnTimer.Stop();

                if (CurrentState == GameTurnState.MyTurn)
                {
                    SetTurnState(GameTurnState.EnemyTurn);
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

            EnemyBoard.IsEnemyBoard = true;

            // 1. Vừa đặt xong con tàu nào -> bảng tình trạng tàu đó chuyển sang xanh lá ngay
            playerBoard.OnShipPlacedSuccessfully += (placedShip) =>
            {
                TableLayoutPanel targetTable = GetShipConditionTable(placedShip.Name, isEnemy: false);
                UpdateSingleShipCondition(targetTable, placedShip.IsPlaced, placedShip.HitCount);
            };

            // 2. Khi xếp xong đủ 5 tàu trên bảng của mình
            playerBoard.OnAllShipsPlaced += () =>
            {
                SetTurnState(GameTurnState.WaitingOpponent);

                // --- CHẾ ĐỘ TEST TẠM KHI CHƯA CÓ SERVER ---
                EnemyBoard.SetupDummyEnemyShips();
                SetTurnState(GameTurnState.MyTurn);
                // ------------------------------------------
            };

            // 3. Cập nhật tình trạng tàu khi có tàu bị bắn trúng (đỏ từ trái sang phải)
            playerBoard.OnShipConditionUpdated += (ships) => UpdateAllShipsConditionUI(ships, isEnemy: false);
            EnemyBoard.OnShipConditionUpdated += (ships) => UpdateAllShipsConditionUI(ships, isEnemy: true);

            // 4. Cập nhật số viên đạn còn lại lên TableShots mỗi lần bắn
            EnemyBoard.OnRemainingShotsChanged += (remainingShots) =>
            {
                UpdateShotsUI(remainingShots);
            };

            // 5. Khi bắn hết 5 phát hoặc hết 60s -> Tự chuyển sang lượt đối thủ
            EnemyBoard.OnTurnEnded += () =>
            {
                SetTurnState(GameTurnState.EnemyTurn);
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

                case GameTurnState.WaitingOpponent:
                    playerBoard.IsPlacementPhase = false;
                    if (lblShots != null) lblShots.Text = "Waiting for opponent...";
                    StopTurnTimer("--");
                    UpdateShotsUI(0);
                    break;

                case GameTurnState.MyTurn:
                    pnlPlayerName.ForeColor = Color.LimeGreen;
                    pnlEnemyName.ForeColor = Color.White;

                    if (lblShots != null) lblShots.Text = "Your turn";
                    StartTurnTimer(TurnDurationSeconds);

                    EnemyBoard.StartTurn();
                    UpdateShotsUI(5);
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
                    if (EnemyBoard.IsMyTurn) EnemyBoard.EndTurn();
                    if (lblShots != null) lblShots.Text = "End of game";
                    StopTurnTimer("0s");
                    UpdateShotsUI(0);
                    break;
            }
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
                string status = (i < remainingShots) ? "miss" : "gray";
                UpdateCellStatus(orderedPics[i], status);
            }
        }

        private void UpdateAllShipsConditionUI(List<ShipInfo> ships, bool isEnemy)
        {
            if (ships == null) return;

            foreach (var ship in ships)
            {
                TableLayoutPanel targetTable = GetShipConditionTable(ship.Name, isEnemy);
                UpdateSingleShipCondition(targetTable, ship.IsPlaced, ship.HitCount);
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

            // Sắp xếp PictureBox từ trái sang phải theo cột
            var orderedPics = table.Controls.OfType<PictureBox>()
                                   .OrderBy(p => table.GetColumn(p))
                                   .ThenBy(p => table.GetRow(p))
                                   .ToList();

            for (int i = 0; i < orderedPics.Count; i++)
            {
                string status;
                if (!isPlaced)
                {
                    status = "gray"; // Chưa đặt tàu -> xám
                }
                else
                {
                    // Đã đặt tàu -> bị bắn trúng tô đỏ từ trái sang phải, còn lại xanh lá
                    status = (i < hitCount) ? "red" : "miss";
                }

                UpdateCellStatus(orderedPics[i], status);
            }
        }

        private void AddChatMessage(string sender, string content)
        {
            if (chatPanel.IsDisposed || !chatPanel.IsHandleCreated) return;

            chatPanel.Invoke(new Action(() =>
            {
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

        private void NetworkManager_OnPacketReceived(Packet packet)
        {
            if (packet.Type == PacketType.GameStart)
            {
                var gameStart = packet.DeserializePayload<GameStartDto>();
                if (gameStart == null) return;

                this.Invoke((MethodInvoker)delegate
                {
                    bool isFirstTurn = string.Equals(gameStart.FirstTurnUsername, PlayerProfile.Username, StringComparison.OrdinalIgnoreCase);

                    if (isFirstTurn)
                        SetTurnState(GameTurnState.MyTurn);
                    else
                        SetTurnState(GameTurnState.EnemyTurn);
                });
                return;
            }

            if (packet.Type == PacketType.ChatMessage)
            {
                try
                {
                    var chatMsg = packet.DeserializePayload<ChatMessageDto>();
                    if (chatMsg != null)
                    {
                        AddChatMessage(chatMsg.Sender, chatMsg.Content);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi giải mã tin nhắn: {ex.Message}");
                }
            }
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
                case "red":
                    targetCell.Image = CreateCircleImage(Color.Red, size);
                    break;
                case "miss":
                    targetCell.Image = CreateCircleImage(Color.LimeGreen, size);
                    break;
                case "destroyed":
                    targetCell.Image = CreateCircleImage(Color.Yellow, size);
                    break;
                case "gray":
                    targetCell.Image = CreateCircleImage(Color.Gray, size);
                    break;
                default:
                    targetCell.Image = null;
                    break;
            }
        }

        private void SetDefaultGrayColors()
        {
            TableLayoutPanel[] allStatusTables = new TableLayoutPanel[]
            {
                TableShots,
                tablePlayerConditionCarrier,
                tablePlayerConditionSubmarine,
                tablePlayerConditionBattleship,
                tablePlayerConditionCruiser,
                tablePlayerConditionDestroyer,
                tableEnemyConditionCarrier,
                tableEnemyConditionSubmarine,
                tableEnemyConditionBattleship,
                tableEnemyConditionCrusier,
                tableEnemyConditionDestroyer
            };

            foreach (TableLayoutPanel table in allStatusTables)
            {
                if (table == null) continue;

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
                        UpdateCellStatus(picBox, "gray");
                    }
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

                if (!NetworkManager.Instance.IsConnected)
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
        private void label22_Click(object sender, EventArgs e) { }
        private void pnlPlayerFrameBoard_Paint(object sender, PaintEventArgs e) { }
        private void lblPlayerName_Click(object sender, PaintEventArgs e) { }
        private void button1_Click(object sender, EventArgs e) { }
        private void boxChat_TextChanged(object sender, EventArgs e) { }
    }
}