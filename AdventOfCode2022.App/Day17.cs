namespace AdventOfCode2022.App;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static AdventOfCode2022.App.Day17;

public class Day17
{
	public static string filepath = @"C:\Users\Martin\source\repos\AdventOfCode2022\AdventOfCode2022.App\t_day17.txt";

	public static void PyroclasticFlow()
	{
		int[] directions = File.ReadAllText(filepath).Select(c => c == '>' ? 1 : -1).ToArray();

		/*
		- using bits
		- create rock
		- put rock in highest point in cave
		- rock falls to bottom or at least lowest possible
		- insert a new rock three rows higher than the highest point of the current structure in the cave
		- after all rocks are fallen, remove all empty lines in the list and count the amount of rows in the list

		>> OPTIMIZATION
		If there is a path which connects the left to the right path
		delete everything below the lowest point of this path to reduce the size of the list
		BUT store the "deleted height" in a separate value for later addition to the rest
		*/

		RockShape[] rockOrder = { RockShape.BarHorizontal, RockShape.Plus, RockShape.FlippedL, RockShape.BarVertical, RockShape.Block};

		List<int> cave = new List<int>();

		int amountRocksToFall = 2022;

		int blowDirection = 0;
		for(int rockNr = 0; rockNr < amountRocksToFall; rockNr++)
		{
			Rock current = RockFactory.CreateRock(rockOrder[rockNr % rockOrder.Length], 2, cave.Count);

			AddNewEmptyRows(ref cave, current.Shape.GetLength(0) + 3);

			current.Y = cave.Count - 1;
			bool added = false;
			bool fallen = true;


			while (!current.CantFall)
			{
				if(!added)
				{
					// add the rock
					UpdateRockInCave(ref cave, ref current);
					added = true;
				}
				else if (fallen)
				{
					// move rock to left or right
					UpdateRockInCave(ref cave, ref current, horizontalDirection: directions[blowDirection]);
					blowDirection++;

					if (blowDirection == directions.Length)
					{
						blowDirection = 0;
					}

					fallen = false;
				}
				else
				{
					// move rock downwards
					UpdateRockInCave(ref cave, ref current, downDirection: 1);
					fallen = true;
				}

				VisualizeCave(cave);
			}

			// remove all empty spaces above the current structure for simplicity
			cave = cave.TakeWhile(r => r > 0).ToList();
		}
	}

	private static void UpdateRockInCave(ref List<int> cave, ref Rock rock, int horizontalDirection = 0, int downDirection = 0)
	{
		if(horizontalDirection != 0 && downDirection == 0)
		{
			// the rock gets blown into a direction

		}
		else if (horizontalDirection == 0 && downDirection == 1)
		{
			// the rock falls



			ToggleRock(ref cave, ref rock);
			rock.Y--;

			UpdateRockShape(ref rock);
			ToggleRock(ref cave, ref rock);
		}
		else
		{
			// add the rock
			UpdateRockShape(ref rock);
			ToggleRock(ref cave, ref rock);
		}
	}

	private static void UpdateRockShape(ref Rock rock)
	{
		int r = 1;
		int rockLargestX = rock.Shape.Max();
		while ((rockLargestX >>= 1) != 0)
		{
			r++;
		}

		for(int shapeLine = 0; shapeLine < rock.Shape.Length; shapeLine++)
		{
			rock.Shape[shapeLine] = (int)((int)rock.Shape[shapeLine] << 7 - r);
		}

	}


	private static void ToggleRock(ref List<int> cave, ref Rock rock)
	{
		for (int shapeScanLine = 0; shapeScanLine < rock.Shape.Length; shapeScanLine++)
		{
			// check here for collisions
			// >> check overlapping

			/*
			 *	 01110		 01110
			 *	|00110		^00110
			 *	=01110		=00100	the results differ, so there is an overlap and this is not allowed
			 *	
			 *	 11100		 11100
			 *	|00011		^00011
			 *	=11111		=11111	the result are equal so there is NO overlap and the process can continue
			 *
			*/


			if (rock.Y - shapeScanLine == 0)
			{
				// rock is below at the bottom of the cave
				// set the rock as "is fixed"
				rock.CantFall = true;
				cave[rock.Y - shapeScanLine] ^= rock.Shape[shapeScanLine];


			}
			else
			{
				cave[rock.Y - shapeScanLine] ^= rock.Shape[shapeScanLine];
			}
		}
	}


	public static void VisualizeCave(List<int> cave)
	{
		Console.Clear();
		for(int y = cave.Count - 1; y >= 0;y--)
		{
			string row = Convert.ToString(cave[y], 2);
			row = row.Replace('1', '#').Replace('0', ' ');
			row = row.PadLeft(7, ' ');
			Console.WriteLine($"|{row}|");
		}
		Console.WriteLine("+-------+");
		Thread.Sleep(60);
	}

	public static void AddNewEmptyRows(ref List<int> cave, int amount)
	{
		for(int i = 0; i < amount; i++)
		{
			cave.Add(0);
		}
	}

	public static class RockFactory
	{
		public static Rock CreateRock(RockShape shape, int xPosition, int yPosition)
		{
			switch (shape)
			{
				case RockShape.BarHorizontal:
					return new Rock(BarHorizontalShape, xPosition, yPosition);

				case RockShape.Plus:
					return new Rock(PlusShape,			xPosition, yPosition);

				case RockShape.FlippedL:
					return new Rock(FlippedLShape,		xPosition, yPosition);

				case RockShape.BarVertical:
					return new Rock(BarVerticalShape,	xPosition, yPosition);

				case RockShape.Block:
					return new Rock(BlockShape,			xPosition, yPosition);

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		// each number represents the bits in the row of the shape 15 = 1111 -> #### represents the horizontal line and so one
		private static int[] BarHorizontalShape = { 15 };

		private static int[] PlusShape = { 2, 7, 2 };

		private static int[] FlippedLShape = { 1, 1, 7 };

		private static int[] BarVerticalShape = { 1, 1, 1, 1 };

		private static int[] BlockShape = { 3, 3 };
	}

	public class Rock
	{
		public Rock(int[] shape, int xPosition, int yPosition)
		{
			this.Shape = shape;
			this.X = xPosition;
			this.Y = yPosition;
		}

		// using the bits of a byte
		public int[] Shape { get; init; }

		public int X { get; set;}

		public int Y { get; set;}	

		public bool CantFall { get; set; }
	}

	public enum RockShape
	{
		BarHorizontal,
		BarVertical,
		FlippedL,
		Block,
		Plus
	}
}