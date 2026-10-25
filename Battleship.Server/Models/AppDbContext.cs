using Battleship.Client.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Server.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchDetail> MatchDetails { get; set; }

        // Chọn loại DB bằng biến môi trường BATTLESHIP_DB:
        //   "sqlite"   (mặc định) -> file battleship.db ở máy
        //   "supabase" -> PostgreSQL trên Supabase, chuỗi kết nối lấy từ SUPABASE_DB_CONNECTION
        // Mất mạng thì chỉ cần đổi lại thành sqlite là chạy offline, không phải sửa code.
        public static bool UseSupabase =>
            string.Equals(Environment.GetEnvironmentVariable("BATTLESHIP_DB"), "supabase",
                          StringComparison.OrdinalIgnoreCase);

        public static string ProviderName => UseSupabase ? "Supabase (PostgreSQL)" : "SQLite (battleship.db)";

        static AppDbContext()
        {
            // Npgsql mặc định chỉ nhận DateTime kiểu UTC, mà model đang dùng DateTime.Now.
            // Bật chế độ cũ để khỏi bị lỗi khi lưu thời gian trận đấu.
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (UseSupabase)
            {
                string? conn = Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION");
                if (string.IsNullOrWhiteSpace(conn))
                {
                    throw new InvalidOperationException(
                        "Đang chọn Supabase nhưng chưa đặt biến môi trường SUPABASE_DB_CONNECTION.");
                }
                optionsBuilder.UseNpgsql(conn);
            }
            else
            {
                optionsBuilder.UseSqlite("Data Source=battleship.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Player1).WithMany().HasForeignKey(m => m.Player1Id).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Player2).WithMany().HasForeignKey(m => m.Player2Id).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Winner).WithMany().HasForeignKey(m => m.WinnerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MatchDetail>()
                .HasOne(md => md.Player).WithMany(u => u.MatchDetails).HasForeignKey(md => md.PlayerId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
