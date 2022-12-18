namespace AdventOfCode2022.App;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using static global::AdventOfCode2022.App.Day15;

public class Day15
{
	public static string filepath = @"";
	public static string filepathtest = @"";
	public static void BeaconExclusiveZone()
	{
		bool isTestData = false;
		List<Sensor> sensors = File.ReadAllLines(isTestData ? filepathtest : filepath).Select(l => Sensor.Parse(l)).ToList();

		int rowY = isTestData ? 10 : 2000000;

		Console.WriteLine($"Part 1: {Search(sensors, rowY).Count()}");


		int searchAreaMin = 0;
		int searchAreaMax = isTestData ? 20 : 4_000_000;
		long multiplier = 4_000_000;

		Coordinate result = Search(sensors, searchAreaMin, searchAreaMax);

		Console.WriteLine($"Part 2: {result.X * multiplier + result.Y}");
	}


	public static Coordinate Search(List<Sensor> sensors, int lower, int upper)
	{
		IntervalX range = new IntervalX(lower, upper);
		Coordinate result = null;

		for (int y = lower; y <= upper && result == null; y++)
		{
			IntervallXList intervalls = new IntervallXList();

			foreach (Sensor s in sensors.Where(s => Math.Abs(y - s.Coordinate.Y) - s.ManhattenDistance <= 0))
			{
				intervalls.Merge(s.GetInterval(y));
				if (intervalls.Cover(range))
				{
					break;
				}
			}
			if (!intervalls.Cover(range) && intervalls.Intervals.Count == 2)
			{
				result = new Coordinate(intervalls.Intervals.First().To + 1, y);
			}
		}
		return result;
	}

	private static HashSet<Coordinate> Search(List<Sensor> sensors, int rowY, bool includeBeacon = false)
	{
		HashSet<Coordinate> found = new HashSet<Coordinate>();
		foreach (var sensor in sensors)
		{
			int manhattenDistance = sensor.ManhattenDistance;

			int rowDistanceToSensorRow = sensor.Coordinate.Y - rowY;

			if (Math.Abs(rowDistanceToSensorRow) <= manhattenDistance)
			{
				int distanceRest = manhattenDistance - Math.Abs(rowDistanceToSensorRow);
				for (int x = sensor.Coordinate.X - distanceRest; x <= sensor.Coordinate.X + distanceRest; x++)
				{
					if (includeBeacon || sensor.Beacon.Coordinate.Y != rowY || sensor.Beacon.Coordinate.Y == rowY && x != sensor.Beacon.Coordinate.X)
					{
						found.Add(new Coordinate(x, rowY));
					}
				}
			}

		}
		return found;
	}

	public record Coordinate(int X, int Y)
	{
		public static Coordinate Parse(string input)
		{
			int[] coordinates = string.Concat(input.Where(c => char.IsDigit(c) || c == ',' || c == '-')).Split(',').Select(v => int.Parse(v)).ToArray();
			return new Coordinate(coordinates[0], coordinates[1]);
		}

	};

	public record Beacon(Coordinate Coordinate)
	{
		public static Beacon Parse(string input)
		{
			return new Beacon(Coordinate.Parse(input));
		}
	}

	public record IntervalX(int From, int To)
	{
		public override string ToString()
		{
			return $"{this.From}|{this.To}";
		}

		public bool Overlap(IntervalX other)
		{
			bool overlap = this.From <= other.From && this.To >= other.From;
			bool overlapv2 = this.From <= other.To && this.To >= other.To;
			bool overlapv3 = this.From <= other.From && this.To >= other.To;
			bool overlapv4 = other.From <= this.From && other.To >= this.To;
			return overlap | overlapv2 | overlapv3 | overlapv4;
		}

		public bool Touch(IntervalX other)
		{
			bool touch = this.To == other.From - 1;
			bool touch2 = this.From == other.To + 1;
			return touch | touch2;
		}
	}

	public class IntervallXList
	{
		private List<IntervalX> intervals = new();
		public IntervallXList()
		{
		}

		public void Merge(IntervalX interval)
		{
			var foundInterval = this.intervals.FirstOrDefault(iv => iv.Overlap(interval) || iv.Touch(interval));

			if (foundInterval == null)
			{
				this.intervals.Add(interval);

			}
			else
			{
				this.intervals.Remove(foundInterval);
				int mostleftX = interval.From <= foundInterval.From ? interval.From : foundInterval.From;
				int mostRightX = interval.To >= foundInterval.To ? interval.To : foundInterval.To;
				this.Merge(new IntervalX(mostleftX, mostRightX));
			}

			this.intervals = this.intervals.OrderBy(i => i.From).ToList();
		}

		public bool Cover(IntervalX interval)
		{
			return this.intervals.Any(i => i.From <= interval.From && i.To >= interval.To);
		}

		public List<IntervalX> Intervals => this.intervals;

		public override string ToString()
		{
			return string.Join(',', this.intervals.Select(i => i.ToString()));
		}
	}

	public record Sensor(Coordinate Coordinate, Beacon Beacon, int ManhattenDistance)
	{
		public static Sensor Parse(string input)
		{
			string[] parts = input.Split(':');

			Coordinate coordinate = Coordinate.Parse(parts[0]);
			Beacon beacon = Beacon.Parse(parts[1]);
			Sensor sensor = new Sensor(coordinate, beacon, ManhattenDistanceBetween(coordinate, beacon.Coordinate));

			return sensor;
		}

		private static int ManhattenDistanceBetween(Coordinate s, Coordinate b)
		{
			return Math.Abs(b.X - s.X) + Math.Abs(b.Y - s.Y);
		}

		public IntervalX GetInterval(int y)
		{
			int rowDistanceToSensorRow = Math.Abs(this.Coordinate.Y - y);
			int mostLeftX = this.Coordinate.X - (ManhattenDistance - rowDistanceToSensorRow);
			int amount = (ManhattenDistance - rowDistanceToSensorRow) * 2;
			return new IntervalX(mostLeftX, mostLeftX + amount);
		}
	}


}