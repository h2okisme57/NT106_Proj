using System.Text.Json;
using System.Text.Json.Serialization;

namespace Battleship.Shared;

// Cấu hình JSON dùng chung toàn hệ thống (Client + Server).
// Quan trọng: JsonStringEnumConverter khiến enum (PacketType, trạng thái...) được
// serialize theo TÊN thay vì theo SỐ. Nhờ vậy việc thêm / chèn giá trị enum về sau
// KHÔNG còn làm lệch gói tin giữa Client và Server.
public static class JsonConfig
{
    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };
}
