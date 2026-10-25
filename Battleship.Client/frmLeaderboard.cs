using Battleship.Client.Services;

namespace Battleship.Client;

// Form Bảng xếp hạng: lấy dữ liệu qua REST API của Supabase rồi hiện lên ListView.
// Viết bằng code (không dùng Designer) cho gọn và đỡ bị conflict file .resx.
// Cách mở từ chỗ khác (ví dụ nút ở Lobby):   new frmLeaderboard().ShowDialog();
public class frmLeaderboard : Form
{
    private readonly ListView listView;
    private readonly Button btnRefresh;
    private readonly Label lblStatus;
    private readonly LeaderboardService _service = new LeaderboardService();

    public frmLeaderboard()
    {
        Text = "Bảng xếp hạng";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(560, 420);

        btnRefresh = new Button
        {
            Text = "Làm mới",
            Location = new Point(12, 12),
            Size = new Size(100, 30)
        };
        btnRefresh.Click += async (s, e) => await LoadAsync();

        lblStatus = new Label
        {
            Location = new Point(124, 18),
            Size = new Size(420, 20),
            Text = ""
        };

        listView = new ListView
        {
            Location = new Point(12, 52),
            Size = new Size(536, 356),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };
        listView.Columns.Add("Hạng", 60);
        listView.Columns.Add("Tên người chơi", 220);
        listView.Columns.Add("Thắng", 70);
        listView.Columns.Add("Thua", 70);
        listView.Columns.Add("Điểm", 90);

        Controls.Add(btnRefresh);
        Controls.Add(lblStatus);
        Controls.Add(listView);

        // Mở form là tự tải luôn
        Shown += async (s, e) => await LoadAsync();
    }

    // Gọi REST rồi đổ dữ liệu vào ListView
    public async Task LoadAsync()
    {
        btnRefresh.Enabled = false;
        lblStatus.Text = "Đang tải...";

        try
        {
            var list = await _service.GetTopAsync(10);

            listView.Items.Clear();
            int rank = 1;
            foreach (var p in list)
            {
                var item = new ListViewItem(rank.ToString());
                item.SubItems.Add(p.Username);
                item.SubItems.Add(p.Wins.ToString());
                item.SubItems.Add(p.Losses.ToString());
                item.SubItems.Add(p.Score.ToString());
                listView.Items.Add(item);
                rank++;
            }

            lblStatus.Text = list.Count == 0 ? "Chưa có người chơi nào." : "Đã tải " + list.Count + " người chơi.";
        }
        catch (Exception ex)
        {
            // Mất mạng hoặc Supabase lỗi thì báo nhẹ nhàng, không cho app bị sập
            lblStatus.Text = "Không tải được (kiểm tra mạng): " + ex.Message;
        }
        finally
        {
            btnRefresh.Enabled = true;
        }
    }
}
