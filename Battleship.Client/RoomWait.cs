using Battleship.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Battleship.Client
{
    public partial class RoomWait : UserControl
    {
        private bool _isReady = false;
        private bool _isHost = false;
        private string _currentRoomId = string.Empty;

        public RoomWait()
        {
            InitializeComponent();
            this.Load += RoomWait_Load;
        }

        private void RoomWait_Load(object sender, EventArgs e)
        {
            if (PlayerProfile.CurrentUser != null)
            {
                lblPlayer1Username.Text = PlayerProfile.Username;
            }

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
        }

        // XỬ LÝ DỮ LIỆU TỪ SERVER TRẢ VỀ
        private void NetworkManager_OnPacketReceived(Packet packet)
        {
            // 1. Nhận phản hồi khi Join phòng (JoinRoomResponseDto)
            if (packet.Type == PacketType.JoinRoomResponse)
            {
                try
                {
                    var response = packet.DeserializePayload<JoinRoomResponseDto>();
                    if (response != null && response.Success && response.Room != null)
                    {
                        UpdateRoomUI(response.Room);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi giải mã JoinRoomResponseDto: {ex.Message}");
                }
            }
            // 2. Nhận cập nhật trạng thái phòng (RoomUpdateDto)
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi giải mã RoomUpdateDto: {ex.Message}");
                }
            }
            // 3. Tín hiệu bắt đầu trận đấu (GameStartDto)
            else if (packet.Type == PacketType.GameStart)
            {
                try
                {
                    var gameStartInfo = packet.DeserializePayload<GameStartDto>();
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show($"Bắt đầu trận đấu! Người đi trước: {gameStartInfo?.FirstTurnPlayer}");
                        // Chuyển sang PlacementPhase (Màn hình xếp tàu) tại đây
                    }));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi giải mã GameStartDto: {ex.Message}");
                }
            }
        }

        // Cập nhật giao diện phòng từ RoomInfoDto
        private void UpdateRoomUI(RoomInfoDto room)
        {
            this.Invoke(new Action(() =>
            {
                _currentRoomId = room.RoomId;
                string currentUsername = PlayerProfile.Username;
                _isHost = (room.HostName == currentUsername);

                // Cập nhật Player 1 (Host)
                lblPlayer1Username.Text = room.HostName;

                // Cập nhật Player 2 (Guest)
                if (!string.IsNullOrEmpty(room.GuestName))
                {
                    lblPlayer2Username.Text = room.GuestName;
                }
                else
                {
                    lblPlayer2Username.Text = "Đang chờ người chơi...";
                    lblPlayer2Conddition.Text = "---";
                    lblPlayer2Conddition.ForeColor = Color.White;
                }
            }));
        }

        // NÚT BẤM READY
        private void btnReady_Click(object sender, EventArgs e)
        {
            _isReady = !_isReady;

            // Cập nhật giao diện cá nhân ngay lập tức
            if (_isHost)
            {
                lblPlayer1Condition.Text = _isReady ? "Ready" : "Waiting...";
                lblPlayer1Condition.ForeColor = _isReady ? Color.LimeGreen : Color.Red;
            }
            else
            {
                lblPlayer2Conddition.Text = _isReady ? "Ready" : "Waiting...";
                lblPlayer2Conddition.ForeColor = _isReady ? Color.LimeGreen : Color.Red;
            }

            btnReady.Text = _isReady ? "CANCEL" : "READY";

            // Gửi PlayerReadyDto chuẩn theo Shared (chứa thuộc tính Ready)
            if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
            {
                var readyDto = new PlayerReadyDto { Ready = _isReady };
                var packet = Packet.Create(PacketType.PlayerReady, readyDto);
                _ = NetworkManager.Instance.SendAsync(packet);
            }
        }

        // NÚT BẤM THOÁT PHÒNG
        private void btnOut_Click(object sender, EventArgs e)
        {
            if (NetworkManager.Instance != null && NetworkManager.Instance.IsConnected)
            {
                var leaveDto = new LeaveRoomDto { RoomId = _currentRoomId };
                var packet = Packet.Create(PacketType.LeaveRoom, leaveDto);
                _ = NetworkManager.Instance.SendAsync(packet);
            }
        }

        private void lblPlayer1Username_Click(object sender, EventArgs e) { }
        private void lblPlayer2Username_Click(object sender, EventArgs e) { }
        private void lblPlayer1Condition_Click(object sender, EventArgs e) { }
        private void lblPlayer2Conddition_Click(object sender, EventArgs e) { }
    }
}