namespace Day8;

using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using static Day8.TreetopTreeHouse;

public class TreetopTreeHouse
{
	public struct Tree
	{
		public Tree(int height, bool isVisible) {
			this.Height = height;
			this.IsVisible = isVisible;
			this.ScenicScore = 0;
		}

		public int Height { get; }
		public bool IsVisible { get; set; }

		public int ScenicScore { get; set; }

		public override string ToString()
		{
			return $"H: {this.Height} SC:{this.ScenicScore} {(this.IsVisible ? "visible": "invisible")}";
		}
	}

	public static void StartDay8()
	{
		string filePath = "...";

		var output = File.ReadAllLines(filePath);

		Tree[,] grid = new Tree[output.Length, output[0].Length];

		for (int y = 0; y < grid.GetLength(0); y++)
		{
			for (int x = 0; x < grid.GetLength(1); x++)
			{
				bool isVisible = false;
				if (y == 0 || y == grid.GetLength(0) - 1 || x == 0 || x == grid.GetLength(1) - 1)
				{
					isVisible = true;
				}

				grid[y,x] = new Tree(output[y][x] - '0', isVisible);
			}
		}

		// part 1

		for (int y = 1; y < grid.GetLength(0) - 1; y++)
		{
			for (int x = 1; x < grid.GetLength(1) - 1; x++)
			{
				Tree currentTree = grid[y, x];


				bool hasHigherLeft = false, hasHigherRight = false, hasHigherTop = false, hasHigherBottom = false;
				int visibleTreesLeft = 0, visibleTreesRight = 0, visibleTreesTop = 0, visibleTreesBottom = 0;

				for (int stx = x-1; stx >= 0; stx--)
				{
					//scenicScore
					visibleTreesLeft++;

					//visible from left
					if (grid[y, stx].Height >= currentTree.Height)
					{
						hasHigherLeft = true;
						break;
					}
				}

				for (int stx = x + 1; stx < grid.GetLength(1); stx++)
				{
					//scenicScore
					visibleTreesRight++;

					//visible from right
					if (grid[y, stx].Height >= currentTree.Height)
					{
						hasHigherRight = true;
						break;
					}
				}

				for (int sty = y - 1; sty >= 0; sty--)
				{
					//scenicScore
					visibleTreesTop++;

					//visible from top
					if (grid[sty, x].Height >= currentTree.Height)
					{
						hasHigherTop = true;
						break;
					}
				}

				for (int sty = y + 1; sty < grid.GetLength(0); sty++)
				{
					//scenicScore
					visibleTreesBottom++;

					//visible from bottom
					if (grid[sty, x].Height >= currentTree.Height)
					{
						hasHigherBottom = true;
						break;
					}
				}
				grid[y, x].ScenicScore = visibleTreesLeft * visibleTreesRight * visibleTreesTop * visibleTreesBottom;
				grid[y, x].IsVisible = !hasHigherLeft || !hasHigherRight || !hasHigherTop || !hasHigherBottom;
			}
		}

		//grid.Visualize((tree) => tree.Height);
		//Console.WriteLine();
		//grid.Visualize((tree) => tree.ScenicScore);
		//Console.WriteLine();

		//grid.Visualize((tree) => tree.IsVisible ? "X" : "O");
		//Console.WriteLine();


		var flattened = grid.Flatten();
		int visibleTrees = flattened.Count(t => t.IsVisible );
		int highesScenicScore = flattened.Max(t => t.ScenicScore);

		Console.WriteLine($"Part 1 Amount of Visible Trees: {visibleTrees}");
		Console.WriteLine($"The highest Scenic Score is: {highesScenicScore}");

	}
	
}

public static class ArrayExtensions
{
	public static T[] Flatten<T>(this T[,] array2d)
	{
		T[] result = new T[array2d.GetLength(0) * array2d.GetLength(1)];
		int idx = 0;
		for(int i = 0; i < array2d.GetLength(0); i++)
		{
			for (int j = 0; j < array2d.GetLength(1); j++)
			{
				result[idx++] = array2d[i, j];
			}
		}
		return result;
	}

	public static void Visualize<T>(this T[,] array2d, Func<T, object> func)
	{
		for (int y = 0; y < array2d.GetLength(0); y++)
		{
			for (int x = 0; x < array2d.GetLength(1); x++)
			{
				Console.Write(func(array2d[y, x]));
			}
			Console.WriteLine();
		}
	}
}