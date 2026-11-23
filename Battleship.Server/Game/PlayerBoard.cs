using System;
using System.Collections.Generic;
using System.Linq;
using Battleship.Shared;

namespace Battleship.Server
{
	public class ShipInstance
	{
		public string ShipName { get; set; } = string.Empty;
		public int Size { get; set; }
		public HashSet<(int X, int Y)> Coordinates { get; } = new();
		public HashSet<(int X, int Y)> Hits { get; } = new();
		public bool IsSunk => Coordinates.Count > 0 && Hits.Count >= Coordinates.Count;
	}

	public class PlayerBoard
	{
		public const int BoardSize = 10;
		public List<ShipInstance> Ships { get; } = new();
		public HashSet<(int X, int Y)> FiredShots { get; } = new();
		public bool IsReady { get; set; } = false;

		public bool AllShipsSunk => Ships.Count > 0 && Ships.All(s => s.IsSunk);

		// Kiểm tra tính hợp lệ và đặt đội hình tàu lên bàn cờ 10x10
		public bool TryPlaceShips(List<ShipPlacementDto>? placements, out string errorMessage)
		{
			errorMessage = string.Empty;
			if (placements == null || placements.Count == 0)
			{
				errorMessage = "Danh sách tàu trống.";
				return false;
			}

			// Luật: đúng 5 tàu với kích thước 5, 4, 3, 3, 2.
			var expectedSizes = new[] { 5, 4, 3, 3, 2 };
			if (!placements.Select(p => p.Size).OrderByDescending(x => x).SequenceEqual(expectedSizes))
			{
				errorMessage = "Cần đúng 5 tàu với kích thước 5, 4, 3, 3, 2.";
				return false;
			}

			var tempShips = new List<ShipInstance>();
			var occupiedCells = new HashSet<(int X, int Y)>();

			foreach (var dto in placements)
			{
				if (dto.Size <= 0 || dto.Size > BoardSize)
				{
					errorMessage = $"Kích thước tàu {dto.ShipName} không hợp lệ.";
					return false;
				}

				var ship = new ShipInstance
				{
					ShipName = string.IsNullOrWhiteSpace(dto.ShipName) ? $"Ship_{dto.Size}" : dto.ShipName,
					Size = dto.Size
				};

				for (int i = 0; i < dto.Size; i++)
				{
					int x = dto.IsHorizontal ? dto.StartX + i : dto.StartX;
					int y = dto.IsHorizontal ? dto.StartY : dto.StartY + i;

					// 1. Kiểm tra tràn biên 10x10
					if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize)
					{
						errorMessage = $"Tàu {ship.ShipName} vượt ra ngoài biên bàn cờ 10x10 tại ({x}, {y}).";
						return false;
					}

					// 2. Kiểm tra các tàu đè lên nhau
					if (!occupiedCells.Add((x, y)))
					{
						errorMessage = $"Tàu {ship.ShipName} bị đè lên tàu khác tại tọa độ ({x}, {y}).";
						return false;
					}

					ship.Coordinates.Add((x, y));
				}

				tempShips.Add(ship);
			}

			Ships.Clear();
			Ships.AddRange(tempShips);
			FiredShots.Clear();
			return true;
		}

		// Phục vụ sẵn cho Turn Engine Phần 2 (FireShot -> Hit/Miss/Sunk)
		public (bool IsValid, bool IsHit, bool IsSunk, string? SunkShipName) ProcessShot(int x, int y)
		{
			if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize)
				return (false, false, false, null);

			// Không cho bắn trùng ô đã bắn
			if (!FiredShots.Add((x, y)))
				return (false, false, false, null);

			foreach (var ship in Ships)
			{
				if (ship.Coordinates.Contains((x, y)))
				{
					ship.Hits.Add((x, y));
					return (true, true, ship.IsSunk, ship.IsSunk ? ship.ShipName : null);
				}
			}

			return (true, false, false, null);
		}
	}
}