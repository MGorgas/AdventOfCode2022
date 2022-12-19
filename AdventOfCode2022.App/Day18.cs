namespace AdventOfCode2022.App;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public class Day18
{

	public static string filepath = @"C:\Users\Martin\source\repos\AdventOfCode2022\AdventOfCode2022.App\day18.txt";
	public static void BuilingBoulders()
	{
		int[][] droplets = File.ReadLines(filepath).Select(l => l.Split(',')).Select(r => new[] { int.Parse(r[0]), int.Parse(r[1]), int.Parse(r[2]) }).ToArray();

		/* - imagine a 3d object formed from those droplets
		 * - slice the object
		 * - iterate throug the sclices, slice by z(depth)
		 * 
		 * for a plane just take two coordinates of a droplet and put it in the plane
		 * use only the droplets for the specific depths
		 * 
		 * when going throug the object, do it for all of the three dimensions and count the visible faces of this slice
		 * 
		 * ^			^
		 * |   111		|  111
		 * | 111111		| 111 
		 * |  11 11		|  11 
		 * |			|
		 * +-------->	+-------->
		 * Left			Front			...
		 * 
		 * just count the droplets to get the surfaces sizes of a side
		 * sum the sizes to get the whole surface
		 *  
		 */



		// rotate the slices 3 times to iterate throug z, than x and than y

		Console.WriteLine($"Part 1: {CountFaces(droplets)}");
		Console.WriteLine($"Part 2: {CountFaces(droplets, true)}");
	}

	private static int CountFaces(int[][] droplets, bool ignoreHollowAreas = false)
	{
		int total = 0;

		int directions = 3;
		int dimensionSize = droplets.SelectMany(v => v).Max() + 1;
		int[][] slices = new int[dimensionSize][];

		for (int dir = 0; dir < directions; dir++)
		{
			for (int dim = 0; dim < dimensionSize; dim++)
			{

				int[][] sliceDroplets = droplets.Where(d => d[dir] == dim).ToArray();

				int[] slize = new int[dimensionSize];

				foreach (int[] droplet in sliceDroplets)
				{
					int x = droplet[(dir + 1) % directions];
					int y = droplet[(dir + 2) % directions];
					slize[y] = slize[y] | (1 << x);
				}
				slices[dim] = slize;
			}

			//for (int d = 0; d < slices.Length; d++)
			//{
			//	Visualize(slices[d], 20);
			//	Console.WriteLine($"Z={d}");
			//}


			int countD = 0;
			for (int d = 0; d < slices.Length; d++)
			{
				int[] current = slices[d];
				if (d == 0)
				{
					// check the first slice

					int[] behind = slices[d + 1];

					// count all faces of this layer because they all will be visible from this side
					countD += CountVisible(current);

					// check if there are faces visible from behind and count them
					int[] checkBehind = XOr(And(current, behind), current);
					countD += CountVisible(checkBehind);

				}
				else if (d > 0 && d < slices.Length - 1)
				{
					// the object between the first and the last slice

					int[] before = slices[d - 1];
					int[] behind = slices[d + 1];

					// count the faces that are not covered from the layer before
					int[] checkBefore = XOr(And(current, behind), current);
					countD += CountVisible(checkBefore);

					// count the faces that are not covered from the layer before
					int[] checkBehind = XOr(And(current, behind), current);
					countD += CountVisible(checkBehind);
				}
				else
				{
					// check the last slice
					int[] before = slices[d - 1];

					// check if there are faces visible from before and count them
					int[] checkBehind = XOr(And(current, before), current);
					countD += CountVisible(checkBehind);

					// count all faces because they are all visible from behind
					countD += CountVisible(current);
				}

				if(ignoreHollowAreas && d < slices.Length-1)
				{
					// prepare the next

					int[] prepare = slices[d+1];
					int[] not = Not(prepare);
					int[] notOrCurrent =  Or(not, current);
					int[] notNotAndCurrent = Not(notOrCurrent);
					prepare = Or(current, notNotAndCurrent);

					slices[d + 1] = prepare;
				}

				Visualize(current, fps: 5, clear: true) ;
			}

			Console.WriteLine($"Counted in {(dir == 0 ? "X" : dir == 1 ? "Y" : "Z")}-dimension: {countD}");
			total += countD;
		}

		return total;
	}

	public static void Visualize(int[] map, int? fps = null, bool clear = false)
	{
		if(fps != null) Thread.Sleep(1000 / fps.Value);
		if(clear) Console.Clear();
		Console.WriteLine("▲ Y ");
		for (int y = map.Length - 1; y >= 0 ; y--)
		{
			string row = Convert.ToString(map[y], 2);
			row = row.Replace('1', '█').Replace('0', ' ');
			row = row.PadLeft(map.Length, ' ');
			Console.WriteLine($"│{row}");
		}
		Console.WriteLine($"└{string.Concat(Enumerable.Repeat('─', map.Length))}► X");
	}

	public static int[] And(int[] a, int[] b)
	{
		return ArrayOperations(a, b, (va, vb) => va & vb);
	}

	public static int[] Or(int[] a, int[] b)
	{
		return ArrayOperations(a, b, (va, vb) => va | vb);
	}

	public static int[] XOr(int[] a, int[] b)
	{
		return ArrayOperations(a, b, (va, vb) => va ^ vb);
	}

	public static int[] Not(int[] a)
	{
		return ArrayOperations(a, (va) => ~va);
	}

	public static int[] ArrayOperations(int[] a, Func<int, int> operation)
	{
		int[] result = new int[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			result[i] = operation(a[i]);
		}
		return result;
	}

	public static int[] ArrayOperations(int[] a, int[] b, Func<int, int, int> operation)
	{
		if (a.Length != b.Length)
		{
			throw new ArgumentException("The arrays have to have the same length!");
		}

		int[] result = new int[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			result[i] = operation(a[i],b[i]);
		}

		return result;
	}

	public static int CountVisible(int[] slice)
	{
		int count = 0;
		for(int i = 0; i < slice.Length; i++)
		{
			int current = slice[i];

			while(current > 0 )
			{
				if((current & 1) == 1)
				{
					count++;
				}
				current = current >> 1;
			}
		}
		return count;
	}
}
