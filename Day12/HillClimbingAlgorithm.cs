﻿namespace Day12;

using System.Xml.Linq;

public class HillClimbingAlgorithm
{
	public static void StartDay12()
	{
		string filepath = "C:\\Users\\Martin\\source\\repos\\AdventOfCode2022\\Day12\\data.txt";
		bool isPartOne = false;

		string[] lines = File.ReadAllLines(filepath);

		Waypoint[][] waypoints = new Waypoint[lines.Length][];
		Waypoint start = null;
		Waypoint end = null;

		for(int y = 0; y < lines.Length; y++)
		{
			waypoints[y] = new Waypoint[lines[y].Length];
			for(int x = 0; x < lines[y].Length; x++)
			{
				int height = lines[y][x] - 'a';

				Waypoint waypoint = new Waypoint(x,y,height);

				if (lines[y][x] == 'S')
				{
					waypoint.Height = 0;
					start = waypoint;
				}
				else if (lines[y][x] == 'E')
				{
					waypoint.Height = 'z' - 'a';
					end = waypoint;
				}

				waypoints[y][x] = waypoint;
			}
		}
		
		for (int y = 0; y < lines.Length; y++)
		{
			for (int x = 0; x < lines[y].Length; x++)
			{
				Waypoint current = waypoints[y][x];

				int heightDifference = 0;

				new List<Action<Action<Waypoint>>> { 
					{ (action) => {if (x - 1 >= 0)					action(waypoints[y][x - 1]); } }, 
					{ (action) => {if (x + 1 < waypoints[y].Length) action(waypoints[y][x + 1]); } }, 
					{ (action) => {if (y - 1 >= 0)					action(waypoints[y - 1][x]); } }, 
					{ (action) => {if (y + 1 < waypoints.Length)	action(waypoints[y + 1][x]); } }
				}
				.ForEach(action => action((neighborWaypoint) =>
									{
										heightDifference = neighborWaypoint.Height - current.Height;
										current.NeighborWaypoints.AddIf(() => heightDifference <= 1, neighborWaypoint);
									})
				); 

			}
		}

		List<Waypoint> walkedWaypoints = new List<Waypoint>();

		if (isPartOne)
		{
			walkedWaypoints.Add(start);
		}
		else
		{
			walkedWaypoints.AddRange(waypoints.SelectMany(i => i).Where(i => i.Height == 0));
		}

		while (!walkedWaypoints.Contains(end))
		{
			List<Waypoint> temporaryWaypoints = new();
			foreach (Waypoint waypoint in walkedWaypoints.Where(w => !w.Visited))
			{
				waypoint.Visited = true;
				foreach (Waypoint neighbor in waypoint.NeighborWaypoints.Where(w => !w.Visited))
				{
					if (temporaryWaypoints.Contains(neighbor) || walkedWaypoints.Contains(neighbor)) continue;
					neighbor.Counter = neighbor.Height == 0 ? 0 : waypoint.Counter + 1;
					temporaryWaypoints.Add(neighbor);
				}
			}

			walkedWaypoints.AddRange(temporaryWaypoints);
		}

		Console.WriteLine($"Shortest path has {end.Counter} steps");
	}
}

public static class ListExtensions
{
	public static void AddIf<T>(this List<T> enumerable, Func<bool> ifStatement, T item)
	{
		if (ifStatement())
		{
			enumerable.Add(item);
		}
	}
}
public class Waypoint
{

	public Waypoint(int x, int y, int height)
	{
		this.X = x;
		this.Y = y;
		this.Height = height;
		this.NeighborWaypoints = new();
		this.Counter = 0;
	}

	public int X { get; }

	public int Y { get; }

	public int Height { get; set; }

	public int Counter { get; set; }

	public bool Visited { get; set; }

	public List<Waypoint> NeighborWaypoints { get; set; }
}