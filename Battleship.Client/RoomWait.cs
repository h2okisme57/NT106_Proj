using Battleship.Shared;
using System;
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;

namespace Battleship.Client
{
    public partial class RoomWait : UserControl
    {
        // Mặc định cả 2 đều KHÔNG phải chủ phòng
        private bool _isHost = false;
        private bool _isHostSlot = false; // Vị trí Player 1 (true) hay Player 2 (false)
        private bool _roleLocked = false; // Khóa vai trò sau khi xác định luồng vào phòng

        private bool _isReady = false;
        private bool _hostReady = false;
        private bool _guestReady = false;
        private bool _isEnteringGame = false;
        private bool _isSubscribed = false;

        private string _currentRoomId = string.Empty;
        private string _hostName = string.Empty;
        private string _guestName = string.Empty;
        private RoomInfoDto? _pendingRoom;

        public event Action<MatchFoundDto>? OnMatchStarted;

        public RoomWait()
        {
            InitializeComponent();
            btnKick.Visible = false;

            // Đặt mặc định Player 2 là "---" thay vì "Waiting..." trong Designer
            lblPlayer2Conddition.Text = "---";
            lblPlayer2Conddition.ForeColor = Color.White;

            this.Load += RoomWait_Load;
            SubscribeNetwork();
        }

        private void SubscribeNetwork()
        {
            if (!_isSubscribed && NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnPacketReceived += NetworkManager_OnPacketReceived;
                _isSubscribed = true;
            }
        }

        private void UnsubscribeNetwork()
        {
            if (_isSubscribed && NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnPacketReceived -= NetworkManager_OnPacketReceived;
                _isSubscribed = false;
            }
        }

        // Gọi từ btnCreateRoom_Click -> Người tạo phòng có quyền Host
        public void SetAsHost()
        {
            _isHost = true;
            _isHostSlot = true;
            _roleLocked = true;

            _hostName = PlayerProfile.Username ?? "Player 1";
            lblPlayer1Username.Text = _hostName;
            lblPlayer2Username.Text = "Đang chờ người chơi...";
            lblPlayer2Conddition.Text = "---";
            lblPlayer2Conddition.ForeColor = Color.White;
            btnKick.Visible = false;
        }

        // Gọi từ btnFindRoom_Click -> Ghép trận ngẫu nhiên (cả 2 đều không có quyền Host)
        public void SetAsMatchmaking()
        {
            _isHost = false;
            _isHostSlot = false;
            _roleLocked = true;

            lblLogo.Text = "Finding Opponent...";
            lblPlayer1Username.Text = PlayerProfile.Username ?? "Player 1";
            lblPlayer2Username.Text = "Đang tìm đối thủ...";
            lblPlayer2Conddition.Text = "---";
            lblPlayer2Conddition.ForeColor = Color.White;
            btnKick.Visible = false;
        }

        // Gọi từ joinRoom.cs -> Mặc định asHost = false, người vào bằng JoinRoom luôn là Guest
        public void ApplyRoom(RoomInfoDto room, bool asHost = false)
        {
            if (!_roleLocked)
            {
                _isHost = asHost;
                _isHostSlot = asHost;
                _roleLocked = true;
            }

            _pendingRoom = room;
            if (IsHandleCreated) UpdateRoomUI(room);
        }

        private void RoomWait_Load(object sender, EventArgs e)
        {
            SubscribeNetwork();

            if (_pendingRoom != null)
            {
                UpdateRoomUI(_pendingRoom);
            }
            else if (PlayerProfile.CurrentUser != null && string.IsNullOrEmpty(_hostName))
            {
                lblPlayer1Username.Text = PlayerProfile.Username;
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            UnsubscribeNetwork();
            base.OnHandleDestroyed(e);
        }

        // =========================================================
        // XỬ LÝ PACKET TỪ SERVER
        // =========================================================
        private void NetworkManager_OnPacketReceived(Packet packet)
        {
            if (this.IsDisposed) return;

            // 1. Phản hồi Join phòng
            if (packet.Type == PacketType.JoinRoomResponse)
            {
                try
                {
                    var response = packet.DeserializePayload<JoinRoomResponseDto>();
                    if (response != null && response.Success && response.Room != null)
                    {
                        ApplyRoom(response.Room, asHost: false);
                    }
                }
                catch { }
            }
            // 2. Cập nhật phòng (người vào / thoát / bị đuổi)
            else if (packet.Type == PacketType.RoomUpdate)
            {
                try
                {
                    var update = packet.DeserializePayload<RoomUpdateDto>();
                    if (update != null && update.Room != null)
                    {
                        UpdateRoomUI(update.Room);
                    }
                }
                catch { }
            }
            // 3. Tìm thấy trận từ Matchmaking (btnFindRoom_Click)
            else if (packet.Type == PacketType.MatchFound)
            {
                try
                {
                    var match = packet.DeserializePayload<MatchFoundDto>();
                    if (match != null)
                    {
                        HandleMatchFound(match);
                    }
                }
                catch { }
            }
            // 4. Nhận đồng bộ trạng thái Ready từ người chơi bên kia
            else if (packet.Type == PacketType.ChatMessage)
            {
                try
                {
                    var chat = packet.DeserializePayload<ChatMessageDto>();
                    if (chat != null)
                    {
                        HandleSyncReadyMessage(chat);
                    }
                }
                catch { }
            }
            // 5. Bị chủ phòng đuổi khỏi phòng
            else if (packet.Type == PacketType.KickedFromRoom)
            {
                string reason = ExtractKickReason(packet.Payload);
                RunOnUI(() =>
                {
                    MessageBox.Show(reason, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CloseRoomWait();
                });
            }
            // 6. Tín hiệu bắt đầu trận từ Server (nếu Server chủ động gửi)
            else if (packet.Type == PacketType.GameStart)
            {
                RunOnUI(() => EnterMatchScreen());
            }
            // 7. Thông báo lỗi từ Server
            else if (packet.Type == PacketType.Error)
            {
                try
                {
                    var err = packet.DeserializePayload<ErrorDto>();
                    if (err != null && !string.IsNullOrWhiteSpace(err.Message))
                    {
                        RunOnUI(() =>
                        {
                            btnKick.Enabled = true;
                            MessageBox.Show(err.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        });
                    }
                }
                catch { }
            }
        }

        private void HandleMatchFound(MatchFoundDto match)
        {
            string myName = PlayerProfile.Username ?? string.Empty;

            // Trong Matchmaking: giữ _isHost = false cho cả 2, chỉ dùng match.IsHost để xếp vị trí P1/P2
            _isHost = false;
            _isHostSlot = match.IsHost;
            _roleLocked = true;

            var room = new RoomInfoDto
            {
                RoomId = match.RoomId,
                RoomName = "Quick Match",
                HostName = match.IsHost ? myName : match.OpponentName,
                GuestName = match.IsHost ? match.OpponentName : myName,
                State = RoomState.Waiting
            };

            _pendingRoom = room;
            if (IsHandleCreated) UpdateRoomUI(room);
        }

        private void UpdateRoomUI(RoomInfoDto room)
        {
            _pendingRoom = room;
            if (!this.IsHandleCreated || this.IsDisposed) return;

            RunOnUI(() =>
            {
                string previousGuest = _guestName;
                string myName = PlayerProfile.Username ?? string.Empty;

                _currentRoomId = room.RoomId;
                _hostName = room.HostName ?? string.Empty;
                _guestName = room.GuestName ?? string.Empty;

                // Nếu chưa bị khóa vai trò trước đó thì mặc định false (Guest)
                if (!_roleLocked)
                {
                    _isHost = false;
                    _isHostSlot = false;
                }

                // Nếu mình là Guest nhưng Server báo phòng không còn tên mình -> đã bị kick hoặc rời phòng
                if (!_isHostSlot && !string.IsNullOrEmpty(myName) &&
                    !string.Equals(_guestName, myName, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(previousGuest))
                {
                    CloseRoomWait();
                    return;
                }

                lblLogo.Text = string.IsNullOrWhiteSpace(room.RoomName)
                    ? "Waiting Room"
                    : $"Waiting Room - {room.RoomName}";

                lblPlayer1Username.Text = string.IsNullOrEmpty(_hostName) ? "Waiting..." : _hostName;

                if (!string.IsNullOrEmpty(_guestName))
                {
                    lblPlayer2Username.Text = _guestName;

                    // Khi có người chơi thứ 2 mới vào -> reset trạng thái Ready của 2 bên
                    if (!string.Equals(previousGuest, _guestName, StringComparison.OrdinalIgnoreCase))
                    {
                        _isReady = false;
                        _hostReady = false;
                        _guestReady = false;
                        RefreshReadyLabels();
                    }
                }
                else
                {
                    lblPlayer2Username.Text = "Đang chờ người chơi...";
                    _isReady = false;
                    _hostReady = false;
                    _guestReady = false;
                    RefreshReadyLabels();
                    lblPlayer2Conddition.Text = "---";
                    lblPlayer2Conddition.ForeColor = Color.White;
                }

                // Chỉ chủ phòng (tạo bằng CreateRoom) mới thấy nút KICK khi đã có Guest
                btnKick.Visible = _isHost && !string.IsNullOrEmpty(_guestName);

                if (room.State == RoomState.Playing)
                {
                    EnterMatchScreen();
                }
            });
        }

        // =========================================================
        // ĐỒNG BỘ READY QUA TIN NHẮN HỆ THỐNG
        // =========================================================
        private void HandleSyncReadyMessage(ChatMessageDto chat)
        {
            if (string.IsNullOrEmpty(_currentRoomId) || string.IsNullOrEmpty(chat.Content)) return;

            string prefix = $"[SYS_READY:{_currentRoomId}]:";
            if (!chat.Content.StartsWith(prefix, StringComparison.Ordinal)) return;

            string statePart = chat.Content.Substring(prefix.Length).Trim();
            bool readyState = statePart == "1";

            RunOnUI(() =>
            {
                if (string.Equals(chat.Sender, _hostName, StringComparison.OrdinalIgnoreCase))
                    _hostReady = readyState;
                else if (string.Equals(chat.Sender, _guestName, StringComparison.OrdinalIgnoreCase))
                    _guestReady = readyState;

                RefreshReadyLabels();

                if (!string.IsNullOrEmpty(_guestName) && _hostReady && _guestReady)
                {
                    EnterMatchScreen();
                }
            });
        }

        private void RefreshReadyLabels()
        {
            lblPlayer1Condition.Text = _hostReady ? "Ready" : "Waiting...";
            lblPlayer1Condition.ForeColor = _hostReady ? Color.LimeGreen : Color.Red;

            if (!string.IsNullOrEmpty(_guestName))
            {
                lblPlayer2Conddition.Text = _guestReady ? "Ready" : "Waiting...";
                lblPlayer2Conddition.ForeColor = _guestReady ? Color.LimeGreen : Color.Red;
            }
            else
            {
                lblPlayer2Conddition.Text = "---";
                lblPlayer2Conddition.ForeColor = Color.White;
            }

            _isReady = _isHostSlot ? _hostReady : _guestReady;
            btnReady.Text = _isReady ? "CANCEL" : "READY";
        }

        // =========================================================
        // SỰ KIỆN NÚT BẤM
        // =========================================================
        private void btnReady_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_guestName) || string.IsNullOrEmpty(_currentRoomId))
            {
                MessageBox.Show("Cần đủ 2 người chơi mới có thể sẵn sàng!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _isReady = !_isReady;
            if (_isHostSlot) _hostReady = _isReady;
            else _guestReady = _isReady;

            RefreshReadyLabels();

            if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
            {
                var syncChat = new ChatMessageDto
                {
                    Sender = PlayerProfile.Username ?? string.Empty,
                    Content = $"[SYS_READY:{_currentRoomId}]:{(_isReady ? "1" : "0")}"
                };
                _ = NetworkManager.Instance.SendAsync(Packet.Create(PacketType.ChatMessage, syncChat));
            }

            if (_hostReady && _guestReady)
            {
                EnterMatchScreen();
            }
        }

        private void btnKick_Click(object sender, EventArgs e)
        {
            // Chỉ người tạo phòng (_isHost == true) mới được phép kick
            if (!_isHost || string.IsNullOrEmpty(_guestName)) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn đuổi {_guestName} khỏi phòng?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
            {
                btnKick.Enabled = false;
                var kickDto = new KickPlayerDto
                {
                    RoomId = _currentRoomId,
                    TargetUsername = _guestName
                };
                _ = NetworkManager.Instance.SendAsync(Packet.Create(PacketType.KickPlayerRequest, kickDto));
            }
        }

        private void btnOut_Click(object sender, EventArgs e)
        {
            CloseRoomWait();
            if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
            {
                if (string.IsNullOrEmpty(_currentRoomId))
                {
                    _ = NetworkManager.Instance.SendAsync(Packet.Create(PacketType.MatchmakingCancelRequest, new { }));
                    return;
                }

                var leaveDto = new LeaveRoomDto { RoomId = _currentRoomId };
                _ = NetworkManager.Instance.SendAsync(Packet.Create(PacketType.LeaveRoom, leaveDto));
            }
        }

        private void EnterMatchScreen()
        {
            if (_isEnteringGame) return;
            _isEnteringGame = true;

            UnsubscribeNetwork();

            string opponentName = _isHostSlot ? _guestName : _hostName;
            var matchInfo = new MatchFoundDto
            {
                RoomId = _currentRoomId,
                OpponentName = opponentName,
                IsHost = _isHostSlot
            };

            OnMatchStarted?.Invoke(matchInfo);

            Control? parent = this.Parent;
            if (parent != null)
            {
                frmMain.SwitchToGameScreen();
                parent.Controls.Remove(this);
            }

            this.Dispose();
        }

        private void CloseRoomWait()
        {
            UnsubscribeNetwork();
            this.Parent?.Controls.Remove(this);
            this.Dispose();
        }

        private void RunOnUI(Action action)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (this.InvokeRequired)
                this.BeginInvoke(action);
            else
                action();
        }

        private static string ExtractKickReason(string payload)
        {
            const string defaultMsg = "Bạn đã bị chủ phòng mời ra khỏi phòng.";
            if (string.IsNullOrWhiteSpace(payload)) return defaultMsg;

            try
            {
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.String)
                    return string.IsNullOrWhiteSpace(root.GetString()) ? defaultMsg : root.GetString()!;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in root.EnumerateObject())
                    {
                        if ((string.Equals(prop.Name, "Reason", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(prop.Name, "Message", StringComparison.OrdinalIgnoreCase)) &&
                            prop.Value.ValueKind == JsonValueKind.String &&
                            !string.IsNullOrWhiteSpace(prop.Value.GetString()))
                        {
                            return prop.Value.GetString()!;
                        }
                    }
                }
            }
            catch { }

            return defaultMsg;
        }

        private void lblPlayer1Username_Click(object sender, EventArgs e) { }
        private void lblPlayer2Username_Click(object sender, EventArgs e) { }
        private void lblPlayer1Condition_Click(object sender, EventArgs e) { }
        private void lblPlayer2Conddition_Click(object sender, EventArgs e) { }
    }
}