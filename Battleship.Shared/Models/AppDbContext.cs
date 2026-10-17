using Battleship.Client.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Server.Models
{
    internal class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchDetail> MatchDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=battleship.db");
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
