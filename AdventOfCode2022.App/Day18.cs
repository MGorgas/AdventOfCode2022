namespace AdventOfCode2022.App;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public class Day18
{

	public static string filepath = @"C:\Users\Martin\source\repos\AdventOfCode2022\AdventOfCode2022.App\day18.txt";

	public static void BuilingBoulders()
	{
		int[][] droplets = File.ReadLines(filepath).Select(l => l.Split(',')).Select(r => new[] { int.Parse(r[0]), int.Parse(r[1]), int.Parse(r[2]) }).ToArray();

		var dis = droplets.Distinct().ToList();

		/* - imagine a 3d object formed from those droplets
		 * - slice the object
		 * - iterate through the slices, slice by z(depth)
		 * 
		 * for a plane just take two coordinates of a droplet and put it in the plane
		 * use only the droplets for the specific depths
		 * 
		 * when going through the object, do it for all of the three dimensions and count the visible faces or edges of this slice
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

		int part1 = CountEdges(droplets);
		Console.WriteLine($"Part 1_Slices: {part1}");

		int part2 = CountEdges(droplets, true); // works only for the test data
		Console.WriteLine($"Part 2_Slices: {part2}"); // wrong results [to low] = 2367, 2370, 2392, 2462  [right should be 2492, maybe come back it later]



		// 2nd attempt after reading and learning from maaaaaany solutions

		int part1_2 = CountFaces3D(droplets);
		Console.WriteLine($"Part 1_3D: {part1_2}");

		int part2_2 = CountFaces3D(droplets, true);
		Console.WriteLine($"Part 2_3D: {part2_2}");
	}

	private static int CountFaces3D(int[][] droplets, bool isPart2 = false)
	{

		Point3D[] points = droplets.Select(d => new Point3D(X: d[0], Y: d[1], Z: d[2])).ToArray();

		if (!isPart2)
		{
			return points.SelectMany(d => GetPossibleNeigbors(d)).Where(n => !points.Contains(n)).Count();
		}
		else
		{

			return GetVisible2(points);


		}
	}

	private static IEnumerable<Point3D> GetPossibleNeigbors(Point3D d)
	{
		return new[] { -1, 1 }.Select(o => new[] {	new Point3D(d.X + o,   d.Y,        d.Z),
													new Point3D(d.X,       d.Y + o,    d.Z),
													new Point3D(d.X,       d.Y,        d.Z + o)
												}).SelectMany(p => p);
	}

	private static int GetVisible2(Point3D[] spread)
	{
		int result = 0;
		Boundaries boundaries = GetBoundaries(spread);

		Queue<Point3D> queue = new();
		HashSet<Point3D> visited = new();

		Point3D start = new Point3D(0, 0, 0);
		queue.Enqueue(start);

		while (queue.Count > 0)
		{
			Point3D point = queue.Dequeue();

			if (!visited.Add(point)) continue;

			foreach(Point3D neighbor in GetPossibleNeigbors(point))
			{
				if (spread.Contains(neighbor))
				{
					result++;
					continue;
				}
				if (boundaries.Contains(neighbor))
				{
					queue.Enqueue(neighbor);
				}
			}
			
		}

		return result;
	}

	public static Boundaries GetBoundaries(Point3D[] matrix)
	{
		int xMax = matrix.Select(d => d.X).Max();
		int xMin = matrix.Select(d => d.X).Min();

		int yMax = matrix.Select(d => d.Y).Max();
		int yMin = matrix.Select(d => d.Y).Min();

		int zMax = matrix.Select(d => d.Z).Max();
		int zMin = matrix.Select(d => d.Z).Min();

		return new Boundaries(xMin, xMax, yMin, yMax, zMin, zMax);

	}


	public record Boundaries(int xMin, int xMax, int yMin, int yMax, int zMin, int zMax)
	{
		public bool Contains(Point3D point)
		{
			return (xMin - 1 <= point.X && point.X <= xMax + 1) &&
				   (yMin - 1 <= point.Y && point.Y <= yMax + 1) &&
				   (zMin - 1 <= point.Z && point.Z <= zMax + 1);
		}
	}

	public record Point3D(int X, int Y, int Z);




	// My first approach in 2D... part 1 works for testdata and input data, part2 only working for testdata  -.-


	private static int CountEdges(int[][] droplets, bool onlyOutside = false)
	{
		int total = 0;

		int directions = 3;
		int dimensionSize = droplets.SelectMany(v => v).Max() + 1;

		for (int dir = 0; dir <= directions-1 ; dir++)
		{
			int[][] slices = new int[dimensionSize][];
			bool countedFirstSliceEdgesInTwoDirections = false;
			for (int dim = 0; dim < dimensionSize; dim++)
			{
				int[][] sliceDroplets = droplets.Where(d => d[(dir) % directions] == dim).ToArray();

				int[] slice = new int[dimensionSize];

				foreach (int[] droplet in sliceDroplets)
				{
					int x = droplet[(dir + 1) % directions];
					int y = droplet[(dir + 2) % directions];
					slice[y] = slice[y] | (1 << x);
				}
				if (onlyOutside)
				{
					slices[dim] = Not(FloodFill(slice));
				}
				else
				{
					slices[dim] = slice;
				}
			}

			//slices = TrimFrontAndBack(slices);

			
			
			int countD = slices.Select(s => CountHorizontalVisibleSides(s)).Sum();


			Console.WriteLine($"Counted in dimension-{dir + 1}: {countD}");

			total += countD;
		}
		return total;
	}

	public static int[] FloodFill(int[] slice)
	{
		int[] sliceCopy = (int[])slice.Clone();
		int[] outside = new int[sliceCopy.Length];

		int sizeX = sizeof(int) * 8;
		int sizeY = slice.Length;

		Queue<(int X, int Y)> queue = new();
		HashSet<(int X, int Y)> visited = new();
		// start filling from all the corners
		queue.Enqueue((0, 0));
		queue.Enqueue((sizeX - 1, 0));
		queue.Enqueue((0, sizeY - 1));
		queue.Enqueue((sizeX - 1, sizeY - 1));



		while (queue.Count > 0)
		{

			(int X, int Y) current = queue.Dequeue();

			if (!visited.Contains(current) && ((sliceCopy[current.Y] >> current.X) & 1) == 0)
			{
				if (current.X > 0)
				{
					var n = (current.X - 1, current.Y);
					if (!queue.Contains(n))
					{
						queue.Enqueue(n);
					}
				}

				if (current.X < sizeX - 1)
				{
					var n = (current.X + 1, current.Y);

					if (!queue.Contains(n))
					{
						queue.Enqueue(n);
					}
				}
				if (current.Y > 0)
				{
					var n = (current.X, current.Y - 1);

					if (!queue.Contains(n))
					{
						queue.Enqueue(n);
					}
				}
				if (current.Y < sizeY - 1)
				{
					var n = (current.X, current.Y + 1);

					if (!queue.Contains(n))
					{
						queue.Enqueue(n);
					}
				}

				sliceCopy[current.Y] = sliceCopy[current.Y] | (1 << current.X);
				outside[current.Y] = outside[current.Y] | (1 << current.X);

				visited.Add(current);
			}
			//Visualize(outside, fps: 500, true);

		}

		return outside;
	}

	public static int[][] RemoveEmptySlices(int[][] slices)
	{

		for (int si = 0; si < slices.Length; si++)
		{
			if (slices[si].Sum() == 0)
			{
				slices[si] = new int[0];
			}
		}

		return slices.Where(s => s.Length != 0).ToArray();
	}

	public static void Visualize(int[] map, int? fps = null, bool clear = false)
	{
		if (fps != null) Thread.Sleep(1000 / fps.Value);
		if (clear) Console.Clear();
		Console.WriteLine("▲ Y ");
		for (int y = map.Length - 1; y >= 0; y--)
		//for (int y = map.Length - 1; y >= 0 ; y--)
		{
			string row = Convert.ToString(map[y], 2);
			row = row.Replace('1', '█').Replace('0', ' ');
			row = row.PadLeft(sizeof(int) * 8, ' ');
			Console.WriteLine($"│{row}");
		}
		Console.WriteLine($"└{string.Concat(Enumerable.Repeat('─', sizeof(int) * 8))}► X");
	}

	public static int[] And(int[] a, int[] b)
	{
		return ArrayOperation(a, b, (va, vb) => va & vb);
	}

	public static int[] Or(int[] a, int[] b)
	{
		return ArrayOperation(a, b, (va, vb) => va | vb);
	}

	public static int[] XOr(int[] a, int[] b)
	{
		return ArrayOperation(a, b, (va, vb) => va ^ vb);
	}

	public static int[] Not(int[] a)
	{
		return ArrayOperation(a, (va) => ~va);
	}

	public static int[] ArrayOperation(int[] a, Func<int, int> operation)
	{
		int[] result = new int[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			result[i] = operation(a[i]);
		}
		return result;
	}

	public static int[] ArrayOperation(int[] a, int[] b, Func<int, int, int> operation)
	{
		if (a.Length != b.Length)
		{
			throw new ArgumentException("The arrays have to have the same length!");
		}

		int[] result = new int[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			result[i] = operation(a[i], b[i]);
		}

		return result;
	}

	public static int CountVisible(int[] slice)
	{
		int count = 0;
		for (int i = 0; i < slice.Length; i++)
		{
			int current = slice[i];

			while (current > 0)
			{
				if ((current & 1) == 1)
				{
					count++;
				}
				current = current >> 1;
			}
		}
		return count;
	}

	public static int CountHorizontalVisibleSides(int[] slice)
	{
		int sizeY = slice.Length; ;
		int sizeX = sizeof(int) * 8;

		int visibleSides = 0;
		for (int y = 0; y < sizeY; y++)
		{
			for (int x = 0; x < sizeX; x++)
			{
				if (((slice[y] >> x) & 1) == 0)
				{
					continue;
				}

				if (x > 0 && (((slice[y] >> x - 1) & 1) == 0))
				{
					visibleSides++;
				}

				if (x < sizeX - 1 && (((slice[y] >> x + 1) & 1) == 0))
				{
					visibleSides++;

				}


				if (x == 0 || x == sizeX - 1)
				{
					visibleSides++;
				}

			}
		}
		return visibleSides;
	}

	public static int CountVerticalVisibleSides(int[] slice)
	{
		int sizeY = slice.Length; ;
		int sizeX = sizeof(int) * 8;

		int visibleSides = 0;

		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				if (((slice[y] >> x) & 1) == 0)
				{
					continue;
				}

				if (y > 0 && (((slice[y - 1] >> x) & 1) == 0))
				{
					visibleSides++;
				}

				if (y < sizeY - 1 && (((slice[y + 1] >> x + 1) & 1) == 0))
				{
					visibleSides++;
				}


				if (y == 0 || y == sizeY - 1)
				{
					visibleSides++;
				}

			}
		}
		return visibleSides;
	}
}
