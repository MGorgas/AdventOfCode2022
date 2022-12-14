namespace Day14;

public class RegolithReservoir
{
	public static string filepath = "C:\\Users\\Martin\\source\\repos\\AdventOfCode2022\\Day14\\input.txt";
	public static void StartDay14(bool isPartTwo = false)
	{
		List<List<Coordinate>> caveScanData = File.ReadAllLines(filepath).Select(line => line.Split(" -> ").Select(c => c.Split(',')).Select(sr => new Coordinate(int.Parse(sr[0]), int.Parse(sr[1]))).ToList()).ToList();

		List<Coordinate> sandSourcesCoordinates = new() { new Coordinate(500, 0) };

		int largestX = caveScanData.SelectMany(l => l).Max(i => i.X);
		int largestY = caveScanData.SelectMany(l => l).Max(i => i.Y);

		int part2Bottom = largestY + 2;


		Material[,] cave = !isPartTwo ? new Material[largestY + 1, largestX + 1] : new Material[part2Bottom + 1, largestX * 2];

		foreach (List<Coordinate> dataSet in caveScanData)
		{
			if (dataSet.Count == 1)
			{
				cave[dataSet[0].Y, dataSet[0].X] = Material.Rock;
			}
			else if (dataSet.Count > 1)
			{
				for (int i = 0; i < dataSet.Count - 1; i++)
				{
					Coordinate first = dataSet[i];
					Coordinate second = dataSet[i + 1];

					int directionY = Math.Sign(second.Y - first.Y);
					int directionX = Math.Sign(second.X - first.X);

					if (first.X == second.X)
					{
						for (int y = first.Y; y != second.Y + directionY; y += directionY)
						{
							cave[y, first.X] = Material.Rock;
						}
					}
					else if (first.Y == second.Y)
					{
						for (int x = first.X; x != second.X + directionX; x += directionX)
						{
							cave[first.Y, x] = Material.Rock;
						}
					}
					else
					{
						throw new InvalidDataException();
					}

				}
			}
			else
			{
				throw new InvalidDataException();
			}
		}

		if(isPartTwo)
		{
			for(int x = 0; x < cave.GetLength(1); x++)
			{
				cave[part2Bottom,x] = Material.Rock;
			}
		}


		int amountSandUnits = 0;
		foreach(Coordinate sandSource in sandSourcesCoordinates)
		{
			bool stopFalling = false;

			while (!stopFalling) 
			{
				int sandX = sandSource.X;
				int sandY = sandSource.Y;

				bool canFall = true;

				while (canFall && !stopFalling)
				{
					try
					{
						if (cave[sandY + 1, sandX] == Material.Air && (isPartTwo && sandY + 1 != part2Bottom))
						{
							sandY++;
						}
						else if (cave[sandY + 1, sandX - 1] == Material.Air && (isPartTwo && sandY + 1 != part2Bottom))
						{
							sandY++;
							sandX--;
						}
						else if (cave[sandY + 1, sandX + 1] == Material.Air && (isPartTwo && sandY + 1 != part2Bottom))
						{
							sandY++;
							sandX++;
						}
						else
						{
							canFall= false;
							amountSandUnits++;
							cave[sandY, sandX] = Material.RestingSand;
							if(sandY == 0 && sandX == 500)
							{
								stopFalling = true;
							}
						}

					}
					catch
					{
						stopFalling = true;
					}

				}
			}

		}

		DrawCave(cave);

		Console.WriteLine(amountSandUnits);

	}

	private static void DrawCave(Material[,] cave)
	{
		for (int y = 0; y < cave.GetLength(0); y++)
		{
			for (int x = 0; x < cave.GetLength(1); x++)
			{
				Console.Write(cave[y, x] switch { Material.Rock => '█', Material.Air => ' ', Material.RestingSand => 'O' });
			}
			Console.WriteLine();
		}
	}
}

public record Coordinate(int X, int Y);

public enum Material
{
	Air,
	Rock,
	RestingSand
}