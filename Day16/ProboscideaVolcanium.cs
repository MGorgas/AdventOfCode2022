namespace Day16;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using static Day16.ProboscideaVolcanium;

public class ProboscideaVolcanium
{
	public static string filepath = @"C:\Users\Martin\source\repos\AdventOfCode2022\Day16\input.txt";
	public static void StartDay16()
	{

		ValveGraph graph = ValveGraph.Parse(File.ReadAllLines(filepath));

		Valve start = graph["AA"];

		// The current graph contains valves with zero flow rates, to remove them weighted edges need to be generated
		// and these valves can be ignored. This reduces the amount of possible paths one can take

		HashSet<Edge<Valve>> newEdges = new();

		var calculateShortestPathToStart = graph.CreateGetShortestPathFunction(start);
		foreach (Valve valve in graph.Nodes.Where(n => n.FlowRate > 0))
		{
			newEdges.Add(new Edge<Valve>(start, valve, calculateShortestPathToStart(valve)));
		}

		var valvesWithFlowRateGreaterZero = graph.Nodes.Where(n => n.FlowRate > 0).ToList();
		foreach (Valve fromValve in valvesWithFlowRateGreaterZero)
		{
			var calculateShortestPathToValve = graph.CreateGetShortestPathFunction(fromValve);

			foreach (Valve toValve in valvesWithFlowRateGreaterZero)
			{
				if (toValve == fromValve) continue;
				newEdges.Add(new Edge<Valve>(fromValve, toValve, calculateShortestPathToValve(toValve)));
			}
		}

		// reset the graph and add the nodes that have a flowrate (including the start valve with a flowrate of zero) and the resulting edges
		graph.ClearGraph();
		graph.AddNode(start);
		graph.AddNodes(valvesWithFlowRateGreaterZero);
		graph.AddEdges(newEdges);

		Stopwatch stopwatch= Stopwatch.StartNew();
		Console.WriteLine($"Part 1: {Algorithms.Solve(graph, start, 30)}");
		stopwatch.Stop();
		Console.WriteLine(stopwatch.Elapsed.ToString());

		stopwatch.Restart();
		Console.WriteLine($"Part 2: {Algorithms.Solve(graph, start, 26, true)}");
		stopwatch.Stop();
		Console.WriteLine(stopwatch.Elapsed.ToString());

	}

	[DebuggerDisplay("{Code}:{FlowRate}")]
	public class Valve
	{
		public Valve(string Code, int FlowRate)
		{
			this.Code = Code;
			this.FlowRate = FlowRate;
		}

		public string Code { get; }
		public int FlowRate { get; }

		public override bool Equals(object? obj)
		{
			if (obj is Valve other)
			{
				return this.Code.Equals(other.Code) && this.FlowRate == other.FlowRate;
			}
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return this.Code.GetHashCode() + FlowRate;
		}

		private string GetDebuggerDisplay()
		{
			return this.ToString();
		}
	}

	public class ValveGraph : Graph<Valve>
	{
		private const string VALVE = "valve";
		private const string VALVES = "valves";
		private const string FLOW_RATE = "flowrate";
		private const string PATTERN = $@"Valve (?<{VALVE}>[A-Z]{{2}}) has flow rate=(?<{FLOW_RATE}>\d+); tunnels? leads? to valves? (?<{VALVES}>.*)";

		public ValveGraph()
		{
		}

		public ValveGraph(IEnumerable<Valve> nodes, IEnumerable<Edge<Valve>> edges) : base(nodes, edges)
		{
		}

		public Valve this[string code]
		{
			get
			{
				return base.AdjacencyList.Keys.Single(k => k.Code== code);
			}
		}

		public static ValveGraph Parse(string text)
		{
			return Parse(text.Split(Environment.NewLine));
		}

		public static ValveGraph Parse(string[] lines)
		{


			Dictionary<Valve, string[]> valveAndNeighbors = new();
			foreach (string line in lines)
			{
				Match match = Regex.Match(line, PATTERN);
				string code = match.Groups[VALVE].Value;
				int flowRate = int.Parse(match.Groups[FLOW_RATE].Value);
				string[] otherValves = match.Groups[VALVES].Value.Split(',', StringSplitOptions.TrimEntries);

				valveAndNeighbors.Add(new Valve(code, flowRate), otherValves);
			}

			ValveGraph graph = new ValveGraph();

			List<Edge<Valve>> edges = new();
			foreach (var valveAndNeighbor in valveAndNeighbors)
			{
				edges.AddRange(valveAndNeighbors.Where(kv => kv.Value.Contains(valveAndNeighbor.Key.Code)).Select(kv => new Edge<Valve>(valveAndNeighbor.Key, kv.Key, 1)));
			}

			graph.AddNodes(valveAndNeighbors.Keys);
			graph.AddEdges(edges);

			return graph;
		}
	}

	[DebuggerDisplay("{From}:{To}={Weight}")]
	public class Edge<T>
	{
		public Edge([DisallowNull] T from, [DisallowNull] T to, int weight)
		{
			this.From = from;
			this.To = to;
			this.Weight = weight;
		}

		public T From { get; }
		public T To { get; set; }

		public int Weight { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj is Edge<T> edge)
			{
				return this.From.Equals(edge.From) && this.To.Equals(edge.To) && this.Weight == edge.Weight;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return this.From.GetHashCode() + this.To.GetHashCode() + this.Weight.GetHashCode();
		}
		public Edge<T> Reverse()
		{
			return new Edge<T>(this.To, this.From, this.Weight);
		}
	}

	public abstract class Graph<T>
	{
		public Graph()
		{
		}
		public Graph(IEnumerable<T> nodes, IEnumerable<Edge<T>> edges)
		{
			this.AddNodes(nodes);

			this.AddEdges(edges);
		}

		public Dictionary<T, HashSet<Edge<T>>> AdjacencyList { get; } = new();

		/// <summary>
		/// Gets the <see cref="List{T}"/> with the specified key.
		/// </summary>
		/// <value>
		/// The <see cref="List{T}"/>.
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public List<T> this[T key]
		{
			get
			{
				return this.AdjacencyList[key].Select(e => e.To).ToList();
			}
		}

		/// <summary>
		/// Gets the nodes.
		/// </summary>
		/// <value>
		/// The nodes.
		/// </value>
		public IEnumerable<T> Nodes
		{
			get
			{
				return this.AdjacencyList.Keys;
			}
		}

		/// <summary>
		/// Gets the edges.
		/// </summary>
		/// <value>
		/// The edges.
		/// </value>
		public IEnumerable<Edge<T>> Edges 
		{
			get
			{
				return this.AdjacencyList.Values.SelectMany(v => v);
			}
		}

		/// <summary>
		/// Removes all nodes and edges.
		/// </summary>
		public void ClearGraph()
		{
			this.AdjacencyList.Clear();
		}

		/// <summary>
		/// Adds the node.
		/// </summary>
		/// <param name="node">The node.</param>
		public void AddNode(T node)
		{
			this.AdjacencyList[node] = new HashSet<Edge<T>>();
		}

		/// <summary>
		/// Adds the nodes.
		/// </summary>
		/// <param name="nodes">The nodes.</param>
		public void AddNodes(IEnumerable<T> nodes)
		{
			foreach (T node in nodes)
			{
				this.AddNode(node);
			}
		}

		/// <summary>
		/// Clears the edges.
		/// </summary>
		public void ClearEdges()
		{
			foreach (var entry in this.AdjacencyList)
			{
				entry.Value.Clear();
			}
		}

		/// <summary>
		/// Gets the edge weight.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <returns>
		/// The weight of the edge as an integer
		/// </returns>
		public int GetEdgeWeight(T from, T to)
		{
			return this.AdjacencyList[from].First(e => e.To.Equals(to)).Weight;
		}

		/// <summary>
		/// Adds the edges if the connections are possible.
		/// (A connection is possible if the graph contains both nodes referenced in an edge)
		/// </summary>
		/// <param name="edges">The edges.</param>
		public void AddEdges(IEnumerable<Edge<T>> edges)
		{
			foreach (Edge<T> edge in edges)
			{
				this.AddEdge(edge);
			}
		}


		/// <summary>
		/// Adds the edge, if the connection is possible.
		/// (A connection is possible if the graph contains both nodes referenced in this edge)
		/// </summary>
		/// <param name="edge">The edge.</param>
		public void AddEdge(Edge<T> edge)
		{
			if (this.AdjacencyList.ContainsKey(edge.From) && this.AdjacencyList.ContainsKey(edge.To))
			{
				this.AdjacencyList[edge.From].Add(edge);
				this.AdjacencyList[edge.To].Add(edge.Reverse());
			}
		}
	}

	public class Algorithms
	{
		/// <summary>
		/// Starts a breadth-first search.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="graph">The graph.</param>
		/// <param name="root">The root node/item.</param>
		/// <param name="preVisit">The pre visit function to execute when passing throug the graph for the currently visited node/item.</param>
		/// <returns>
		/// All reachable nodes
		/// </returns>
		public static HashSet<T> BFSearch<T>(Graph<T> graph, T root, Action<T>? preVisit = null)
		{
			HashSet<T> visited = new();

			if (!graph.AdjacencyList.ContainsKey(root))
			{
				return visited;
			}

			Queue<T> queue = new();
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				T node = queue.Dequeue();

				if (visited.Contains(node))
				{
					continue;
				}

				if (preVisit != null)
				{
					preVisit(node);
				}

				visited.Add(node);

				foreach (T other in graph[node])
				{
					if (!visited.Contains(other))
					{
						queue.Enqueue(other);
					}
				}
			}
			return visited;
		}

		/// <summary>
		/// Solves the puzzle.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="startValve">The start valve.</param>
		/// <param name="minutes">The minutes.</param>
		/// <param name="partTwo">if set to <c>true</c> [part two].</param>
		/// <returns></returns>
		public static long Solve(ValveGraph graph, Valve startValve, int minutes, bool partTwo = false)
		{

			List<List<Valve>> paths = GetValidPaths(graph, graph.Nodes.ToList(), startValve, minutes).Select(p => p.ToList()).OrderBy(p => p.Count()).Distinct().ToList();

			if (!partTwo)
			{
				Dictionary<List<Valve>, long> pathFlowRate = paths.Select(p => (Path: p, FlowRate: GetPreasureRelease(graph, minutes, p))).ToDictionary(k => k.Path, v => v.FlowRate);
				return pathFlowRate.Max(p => p.Value);
			}
			else
			{
				long currentMax = 0;

				var result = paths.Select((p, i) => (Path: string.Concat(p.Skip(1).Select(v => v.Code)), FlowRate: GetPreasureRelease(graph, minutes, p))).ToList();
				result = result.Distinct().ToList();

				Dictionary<string, long> pathLookup = result.ToDictionary(x => x.Path, x => x.FlowRate);
				Dictionary<string, long> adjacentPaths = new Dictionary<string, long>(pathLookup);


				foreach(var entry in pathLookup)
				{
					adjacentPaths.Remove(entry.Key);
					foreach(var other in adjacentPaths.Where(p => !entry.Key.Chunk(2).Select(c => new string(c)).Any(c => p.Key.Contains(c))))
					{
						long val = other.Value + entry.Value;
						if(val > currentMax)
						{
							currentMax = val;
						}
					}
				}

				return currentMax;


				// progressbar
				//long maxValue = 0;

				//Console.WriteLine("Starting Part 2");
				//Console.WriteLine("Searching possible path combinations, please wait...");

				//double percentage = 0;
				//Console.WriteLine();
				//Stopwatch sw = Stopwatch.StartNew();

				//for (int l = 0; l < pathFlowRatesLeft.Count; l++)
				//{

				//	Console.CursorTop--;
				//	Console.CursorLeft = 0;
				//	percentage = ((l + 1D) / pathFlowRatesLeft.Count) * 100;
				//	Console.WriteLine(string.Format("[{0}]", string.Concat(new string('#', (int)percentage), new string('-', 100 - (int)percentage))));
				//	Console.Write($"{l + 1}/{pathFlowRatesLeft.Count} ({percentage:N2}) [estimated time left: {(l > 0 ? (sw.Elapsed / l) * (pathFlowRatesLeft.Count - l) : "??")}]");
				//}
				//sw.Stop();
				//Console.WriteLine($"Done after {sw.Elapsed}");

				//return maxValue;
			}
		}

		/// <summary>
		/// Gets the pressure release.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="minutes">The minutes.</param>
		/// <param name="path">The path.</param>
		/// <returns>
		/// The pressure release for this path
		/// </returns>
		private static long GetPreasureRelease(ValveGraph graph, int minutes, List<Valve> path)
		{
			List<(int flowrate, int startminute)> ratestarts = new();
			int minutesleft = minutes;

			for (int i = 0; i < path.Count - 1; i++)
			{
				Valve from = path[i];
				Valve to = path[i + 1];

				int weight = graph.GetEdgeWeight(from, to);
				minutesleft -= weight;
				ratestarts.Add((to.FlowRate, minutesleft));
				if (minutesleft < 0) break;
			}

			return ratestarts.Select(rs => rs.startminute * rs.flowrate).Sum();
		}

		/// <summary>
		/// Gets the valid paths.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="valves">The valves that are possible for this branch.</param>
		/// <param name="valve">The current valve as starting point.</param>
		/// <param name="minutes">The minutes left.</param>
		/// <returns>
		/// An enumeration of all possible paths. A path consists of valves in the order they are visited.
		/// </returns>
		private static IEnumerable<IEnumerable<Valve>> GetValidPaths(ValveGraph graph, IList<Valve> valves, Valve valve, int minutes)
		{
			// create a new list for this iteration
			IList<Valve> otherValves = valves.ToList();
			// remove the current valve from the list, so it is "already visitied and part of this branch"
			otherValves.Remove(valve);
			int currentReleaseValue = (valve.FlowRate * minutes);

			if (valves.Count == 1)
			{
				yield return new Valve[] { valve };
			}
			else
			{
				// get all the edges from the graph where the time will not be exceeded and where the valve is not already part of the path of this branch
				var edges = graph.AdjacencyList[valve].Where(e => minutes - e.Weight > 0).Where(e => otherValves.Contains(e.To)).ToList();

				if (edges.Any())
				{
					foreach (Edge<Valve> edge in edges)
					{
						// search the children of this itereation of the branch for valid branches
						var result = GetValidPaths(graph, otherValves, edge.To, minutes - edge.Weight).ToList();

						// return possible branches
						foreach (IEnumerable<Valve> path in result)
						{
							yield return new[] { valve }.Concat(path);
						}


						yield return new[] { valve }.Concat(new[] { edge.To });

					}
				}
			}
		}
	}
}

public static class GraphExtensions
{
	/// <summary>
	/// Creates the get shortest path function.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="graph">The graph.</param>
	/// <param name="start">The start node/item.</param>
	/// <returns>
	/// A function to calculate the shortest path from the <c>start</c> node to the then passed destination node in the function.
	/// </returns>
	public static Func<T, int> CreateGetShortestPathFunction<T>(this Graph<T> graph, T start)
	{
		Dictionary<T, T> previous = new();

		Queue<T> queue = new();
		queue.Enqueue(start);

		while (queue.Count > 0)
		{
			T node = queue.Dequeue();
			foreach (Edge<T> other in graph.AdjacencyList[node])
			{
				if (previous.ContainsKey(other.To))
				{
					continue;
				}
				previous[other.To] = node;
				queue.Enqueue(other.To);
			}
		}

		Func<T, int> shortestPath = (node) =>
		{
			List<T> path = new();

			T currentNode = node;
			while (!currentNode.Equals(start))
			{
				path.Add(currentNode);
				currentNode = previous[currentNode];
			}

			path.Add(start);
			path.Reverse();
			return path.Count();

		};
		return shortestPath;
	}
}

public static class EnumerableExtensions
{
	/// <summary>
	/// Allows the exception of specific items.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="values">The values.</param>
	/// <param name="exceptValues">The except values.</param>
	/// <returns></returns>
	public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, params T[] values)
	{
		return enumerable.Except(values.ToList());
	}

	/// <summary>
	/// Rotates the sequence to the right by putting the last item at the front.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="sequence">The sequence.</param>
	/// <param name="count">The count.</param>
	private static void RotateRight<T>(ref IList<T> sequence, int count)
	{
		T tmp = sequence[count - 1];
		sequence.RemoveAt(count - 1);
		sequence.Insert(0, tmp);
	}

	/// <summary>
	/// Gets the permutations.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="items">The items.</param>
	/// <returns>
	/// All possible permutations
	/// </returns>
	public static IEnumerable<IList<T>> GetPermutations<T>(this IList<T> items)
	{
		return items.GetPermutations<T>(items.Count());
	}

	private static IEnumerable<IList<T>> GetPermutations<T>(this IList<T> items, int count)
	{
		if (count == 1)
		{
			yield return items;
		}
		else
		{
			for (int i = 0; i < count; i++)
			{
				foreach (var permutation in items.GetPermutations(count - 1))
				{
					yield return permutation;
				}
				RotateRight<T>(ref items, count);
			}
		}
	}

	/// <summary>
	/// Gets the permutations.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="items">The items.</param>
	/// <param name="startItem">The start item.</param>
	/// <returns>
	/// All possible permutations that are starting with the start item
	/// </returns>
	public static IEnumerable<IList<T>> GetPermutations<T>(this IList<T> items, T startItem)
	{
		return items.GetPermutations<T>(startItem, items.Count());
	}

	private static IEnumerable<IList<T>> GetPermutations<T>(this IList<T> items, T startItem, int count)
	{
		if (count == 1 && items[0].Equals(startItem))
		{
			yield return items;
		}
		else
		{
			for(int i = 0; i < count; i++)
			{
				foreach (var permutation in items.GetPermutations(startItem, count - 1))
				{
					yield return permutation;
				}
				RotateRight<T>(ref items, count);
			}
		}

	}
}