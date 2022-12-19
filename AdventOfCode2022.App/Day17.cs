namespace AdventOfCode2022.App;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static AdventOfCode2022.App.Day17;

public class Day17
{
	public static string filepath = @"C:\Users\Martin\source\repos\AdventOfCode2022\AdventOfCode2022.App\day18_t.txt";

	public static void PyroclasticFlow()
	{
		int[] directions = File.ReadAllText(filepath).Select(c => c == '>' ? 1 : -1).ToArray();


		RockShape[] rockOrder = { RockShape.BarHorizontal, RockShape.Plus, RockShape.FlippedL, RockShape.BarVertical, RockShape.Block };

		Stopwatch stopwatch = Stopwatch.StartNew();
		Console.WriteLine($"Part 1: {SimulateCave(2022, directions, rockOrder)}");
		stopwatch.Stop();
		Console.WriteLine($"Time needed: {stopwatch.Elapsed}{Environment.NewLine}");
		stopwatch.Restart();
		Console.WriteLine($"Part 2: {SimulateCave(1000000000000, directions, rockOrder)}");
		stopwatch.Stop();
		Console.WriteLine($"Time needed: {stopwatch.Elapsed}");

	}

	private static long SimulateCave(long amountRocksToFall, int[] directions, RockShape[] rockOrder, bool visualizeSimulation = false)
	{
		long result = 0;
		List<int> cave = new();
		
		// store informations to find out when the pattern repeats
		int firstRow = 0;
		HashSet<State> stateLookup = new(new StateComparer());
		List<State> stones = new();
		bool firstSetOfStonesFallen = false;
		// -------------------------------------------------------

		int blowDirection = 0;
		for (long rockNr = 0; rockNr < amountRocksToFall; rockNr++)
		{
			RockShape rockShape = rockOrder[rockNr % rockOrder.Length];

			Rock current = RockFactory.CreateRock(rockShape, 2, cave.Count);

			AddNewEmptyRows(ref cave, current.Shape.GetLength(0) + 3);

			current.Y = cave.Count - 1;
			bool added = false;
			bool fallen = true;

			while (!current.CantFall)
			{
				if (!added)
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
				if (visualizeSimulation)
				{
					VisualizeCave(cave, fps: 60);
				}
			}

			if(!firstSetOfStonesFallen && rockShape == rockOrder.Last())
			{
				firstSetOfStonesFallen = true;

			}
			// remove all empty spaces above the current structure for simplicity
			TrimEmptySpace(ref cave);

			if (!firstSetOfStonesFallen)
			{
				firstRow = cave[0];
			}

			State state = new State() { RockNr = rockNr+1, CaveHeight = cave.Count(), Shape = rockShape, DirectionNr = blowDirection - 1 };
			

			if (cave.Last() == firstRow)
			{
				if(rockNr != 0 && !stateLookup.Add(state))
				{

					//the pattern seems to repeat from now on and the result can be calculated
					State endOfOfRepeatingPattern = stones[stones.Count - 1];
					State firstAppearenceOfRepeatingPattern = stateLookup.First(s => s.Shape == rockShape && s.DirectionNr == blowDirection - 1);
					State lastStoneBeforePattern = stones.First(s => s.RockNr == firstAppearenceOfRepeatingPattern.RockNr - 1);


					long amountRocksInRepeatingPattern = (endOfOfRepeatingPattern.RockNr - firstAppearenceOfRepeatingPattern.RockNr) + 1; // include the current stone > +1
					long patternCaveHeight = endOfOfRepeatingPattern.CaveHeight - lastStoneBeforePattern.CaveHeight;


					long amountPatterns = (amountRocksToFall / amountRocksInRepeatingPattern);

					long restAmount = amountRocksToFall - (amountRocksInRepeatingPattern * amountPatterns + lastStoneBeforePattern.RockNr);

					// the rest amount needs to be the stone AFTER the detected pattern beginns
					State rest = stones[(int)(firstAppearenceOfRepeatingPattern.RockNr - 1 + restAmount - 1)];
					


					long height = (amountPatterns * patternCaveHeight) + lastStoneBeforePattern.CaveHeight + (rest.CaveHeight - lastStoneBeforePattern.CaveHeight);
					return height;
				}
			}
			stones.Add(state);

		}
		return result;
	}

	[DebuggerDisplay("RNr.:{RockNr} CH:{CaveHeight}")]
	public class State
	{
		public long RockNr;
		public long CaveHeight;
		public int DirectionNr;
		public RockShape Shape;
	}

	public class StateComparer : IEqualityComparer<State>
	{
		public bool Equals(State? x, State? y)
		{
			return x.DirectionNr == y.DirectionNr && x.Shape == y.Shape;
		}

		public int GetHashCode([DisallowNull] State obj)
		{
			return obj.DirectionNr.GetHashCode() + obj.Shape.GetHashCode();
		}
	}

	private static void UpdateRockInCave(ref List<int> cave, ref Rock rock, int horizontalDirection = 0, int downDirection = 0)
	{
		if (horizontalDirection != 0 && downDirection == 0)
		{
			// the rock gets blown into a direction
			ToggleRock(ref cave, ref rock);


			Rock preserved = rock.Clone();
			rock.X += horizontalDirection;

			UpdateRockShapeX(ref rock, horizontalDirection);
			if(CollidesX(ref cave, ref rock))
			{
				rock = preserved;
			}
			ToggleRock(ref cave, ref rock);
		}
		else if (horizontalDirection == 0 && downDirection == 1)
		{
			// the rock falls

			ToggleRock(ref cave, ref rock);
			rock.Y--;
			if (CollidesY(ref cave, ref rock))
			{
				rock.Y++;
				rock.CantFall = true;
			}

			ToggleRock(ref cave, ref rock);
		}
		else
		{
			// add the rock
			InitializeRock(ref rock);
			ToggleRock(ref cave, ref rock);
		}
	}

	private static void TrimEmptySpace(ref List<int> cave)
	{
		cave = cave.TakeWhile(r => r > 0).ToList();
	}

	private static List<int> TrimCave(ref List<int> cave)
	{
		// trim the cave to a point where is no possibility for rocks to fall through
		List<int> pattern = new();
		int trimAmount = 0;
		for(int i = 0; i < cave.Count - 2; i++)
		{

			if ((cave[i] | cave[i+1] | cave[i + 2]) == 127)
			{
				trimAmount = i - 1; // the last value before the "pattern" was found
			}

			if(trimAmount > 0)
			{
				break;
			}
		}

		if(trimAmount > 0)
		{
			pattern.AddRange(cave.Take(trimAmount));
			cave = cave.Skip(trimAmount).ToList();
		}

		return pattern;
	}

	private static void InitializeRock(ref Rock rock)
	{
		int r = rock.Width;
		for (int shapeLine = 0; shapeLine < rock.Shape.Length; shapeLine++)
		{
			rock.Shape[shapeLine] = (int)((int)rock.Shape[shapeLine] << 7 - r - rock.X);
		}
	}

	private static void UpdateRockShapeX(ref Rock rock, int direction = 0)
	{
		for (int shapeLine = 0; shapeLine < rock.Shape.Length; shapeLine++)
		{
			if(direction < 0)
			{ 
				rock.Shape[shapeLine] = (int)((int)rock.Shape[shapeLine] << Math.Abs(direction));
			}
			else if(direction > 0)
			{
				rock.Shape[shapeLine] = (int)((int)rock.Shape[shapeLine] >> Math.Abs(direction));
			}
		}

	}

	private static bool CollidesY(ref List<int> cave, ref Rock rock)
	{
		for (int shapeScanLine = 0; shapeScanLine < rock.Shape.Length; shapeScanLine++)
		{
			// check if the rock is below the bottom of the cave;
			if (rock.Y - shapeScanLine < 0)
			{
				return true;
			}

			if (Overlaps(rock.Shape[shapeScanLine], cave[rock.Y - shapeScanLine]))
			{
				return true;
			}
		}
		return false;

	}

	private static bool CollidesX(ref List<int> cave, ref Rock rock)
	{
		if (rock.X < 0)
		{
			return true;
		}

		if (rock.X + rock.Width > 7)
		{
			return true;
		}

		for (int shapeScanLine = 0; shapeScanLine < rock.Shape.Length; shapeScanLine++)
		{
			if (Overlaps(rock.Shape[shapeScanLine], cave[rock.Y - shapeScanLine]))
			{
				return true;
			}
		}

		return false;
	}

	private static bool Overlaps(int left, int right)
	{
		/* >> check overlapping/collision with existing cave structures
		 * 
		 *	OR			XOR
		 *	 01110		 01110
		 *	|00110		^00110
		 *	=01110		=00100	the results differ, so there is an overlap and this is not allowed
		 *	
		 *	 11100		 11100
		 *	|00011		^00011
		 *	=11111		=11111	the result are equal so there is NO overlap and the process can continue
		 *
		*/

		int or = left | right;
		int xor = left ^ right;
		return or != xor;
	}

	private static int GetOffsetX(Rock rock)
	{
		int r = 1;
		int rockLargestX = rock.Shape.Max();
		while ((rockLargestX >>= 1) != 0)
		{
			r++;
		}

		return r;
	}

	/// <summary>
	/// Toggles the rock. Removes it when it is in cave or Adds it when it is not in the cave.
	/// </summary>
	/// <param name="cave">The cave.</param>
	/// <param name="rock">The rock.</param>
	private static void ToggleRock(ref List<int> cave, ref Rock rock)
	{
		for (int shapeScanLine = 0; shapeScanLine < rock.Shape.Length; shapeScanLine++)
		{
			cave[rock.Y - shapeScanLine] ^= rock.Shape[shapeScanLine];
		}
	}


	public static void VisualizeCave(List<int> cave, int amountTopCaveRowsToDisplay = 20, int fps = 30)
	{
		Thread.Sleep(1000 / fps);
		Console.Clear();
		bool overflow = cave.Count - 1 > amountTopCaveRowsToDisplay;
		for (int y = cave.Count - 1; y >= 0 && y >= cave.Count - 1 - amountTopCaveRowsToDisplay; y--)
		{
			string row = Convert.ToString(cave[y], 2);
			row = row.Replace('1', '#').Replace('0', ' ');
			row = row.PadLeft(7, ' ');
			Console.WriteLine($"{y+1:D2}|{row}|");
		}
		if (!overflow)
		{
			Console.WriteLine("+-------+");
		}
		else
		{
			Console.WriteLine("  +Vv. .vV+");
		}
	}

	public static void AddNewEmptyRows(ref List<int> cave, int amount)
	{
		for (int i = 0; i < amount; i++)
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
					return new Rock((int[])BarHorizontalShape.Clone(), xPosition, yPosition);

				case RockShape.Plus:
					return new Rock((int[])PlusShape.Clone(), xPosition, yPosition);

				case RockShape.FlippedL:
					return new Rock((int[])FlippedLShape.Clone(), xPosition, yPosition);

				case RockShape.BarVertical:
					return new Rock((int[])BarVerticalShape.Clone(), xPosition, yPosition);

				case RockShape.Block:
					return new Rock((int[])BlockShape.Clone(), xPosition, yPosition);

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
			this.Width = CalculateWidth();
			this.Height = shape.Length;
		}

		public Rock Clone()
		{
			return new Rock((int[])Shape.Clone(), X, Y) { Width = this.Width, Height = this.Height };
		}

		// using the bits of a byte
		public int[] Shape { get; init; }

		public int X { get; set; }

		public int Y { get; set; }

		public int Width { get; private set; }

		public int Height { get; private set; }

		public bool CantFall { get; set; }

		private int CalculateWidth()
		{
			int r = 1;
			int rockLargestX = this.Shape.Max();
			while ((rockLargestX >>= 1) != 0)
			{
				r++;
			}

			return r;
		}
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