namespace Day13;

using System.Drawing;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

public class DistressSignal
{
	public static string filepath = "C:\\Users\\Martin\\source\\repos\\AdventOfCode2022\\Day13\\data.txt";

	public static void StartDay13()
	{
		List<string> packages = File.ReadAllText(filepath).Split(Environment.NewLine + Environment.NewLine).ToList();

		int sumOfValidIndicies = 0;
		for (int i = 0; i < packages.Count; i++)
		{
			string[] data = packages[i].Split(Environment.NewLine);

			Console.WriteLine($"== Pair {i + 1} ==");
			Console.WriteLine($"Compare {data[0]} vs {data[1]}");

			string[] leftData = Unpack(data[0]);
			string[] rightData = Unpack(data[1]);

			var intResult = Validate(leftData, rightData);
			
			if (intResult < 0)
			{
				Console.WriteLine($"Current valid index: {i + 1}");
				sumOfValidIndicies += i + 1;
			}

			Console.WriteLine();

		}

		Console.WriteLine($"Part1: Sum of indices with right order: {sumOfValidIndicies}");
		
		packages = File.ReadAllText(filepath).Split(Environment.NewLine).Where(l => !string.IsNullOrEmpty(l)).ToList();
		packages.Add("[[2]]");
		packages.Add("[[6]]");

		packages.Sort(Compare);

		packages.ForEach(e => Console.WriteLine(e));

		int part2Result = (packages.IndexOf("[[2]]")+1) * (packages.IndexOf("[[6]]")+1);
		Console.WriteLine($"The decoder key for the distress signal is: {part2Result}");
	}

	public static void StartDay13_2()
	{
		// learned about JsonNode/JsonArray and Enumerable.Zip from @https://github.com/encse
		IEnumerable<JsonNode[]> packageSet = File.ReadAllText(filepath).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(i => JsonNode.Parse(i)).Chunk(2);

		var part1result = packageSet.Select((p, i) => ComparePackages(p[0], p[1]) < 1 ? i+1 : -1).Where(i => i > -1).Sum();

		JsonNode del1 = JsonNode.Parse("[[2]]");
		JsonNode del2 = JsonNode.Parse("[[6]]");
		var part2packageSet = packageSet.SelectMany(i => i).Concat(new[] {del1, del2}).ToList();

		part2packageSet.Sort(ComparePackages);


		Console.WriteLine(part1result);
		Console.WriteLine((part2packageSet.IndexOf(del1) + 1) * (part2packageSet.IndexOf(del2) + 1));
	}

	private static int ComparePackages(JsonNode left, JsonNode? right)
	{
		if(left is JsonValue && right is JsonValue)
		{
			return Math.Sign((int)left - (int)right);
		}
		else
		{
			JsonArray leftArray = left as JsonArray ?? new JsonArray((int)left);
			JsonArray rightArray = right as JsonArray ?? new JsonArray((int)right);

			return  Enumerable.Zip(leftArray, rightArray)
							  .Select(e => ComparePackages(e.First, e.Second))
							  .FirstOrDefault(c => c != 0, Math.Sign(leftArray.Count - rightArray.Count));
		}

	}

	private static int ComparePackages(string left, string right)
	{
		var leftNode = JsonNode.Parse(left);
		var rightNode = JsonNode.Parse(right);

		if(leftNode is JsonValue && rightNode is JsonValue)
		{
			return Math.Sign((int)leftNode - (int)rightNode);
		}
		return 0;
	}
	public static string[] Unpack(string data)
	{
		string current = data;
		List<string> segments = new();
		if (data.StartsWith('[') && data.EndsWith(']'))
		{
			current = data[1..^1];
		}

		while (current.Length > 0)
		{
			if (current.StartsWith(','))
			{
				current = current[1..^0];
			}
			if (current.StartsWith('['))
			{
				int bracketCounter = 1;
				int index = 1;
				while (bracketCounter > 0)
				{
					if (current[index] == '[') { bracketCounter++; }
					else if (current[index] == ']') { bracketCounter--; }
					index++;
				}

				segments.Add(current[0..index]);

				current = current.Substring(index);
			}
			else
			{
				int end = current.IndexOf(',');
				if (end > 0)
				{
					segments.Add(current[0..end]);
					current = current.Substring(end + 1);
				}
				else
				{
					segments.Add(current);
					current = string.Empty;
				}
			}
		}


		return segments.ToArray();
	}

	public static int Compare(string left, string right)
	{
		return Validate(Unpack(left), Unpack(right));
	}

	public static int Validate(string[] leftData, string[] rightData)
	{
		int returnValue = 0;
		if(leftData.Length == rightData.Length && leftData.Length == 0)
		{
			return 0;
		}
		else if (leftData.Length == 0)
		{
			Console.WriteLine("Left side ran out of items, so inputs are in the right order");
			return -1;
		}
		else if (rightData.Length == 0)
		{
			Console.WriteLine("Right side ran out of items, so inputs are not in the right order");
			return 1;
		}
		else
		{
			for (int i = 0; i < leftData.Length; i++)
			{
				bool leftIsNumber = int.TryParse(leftData[i], out int leftIntValue);

				if (i < rightData.Length)
				{
					Console.WriteLine($"Compare {leftData[i]} vs {rightData[i]}");

					bool rightIsNumber = int.TryParse(rightData[i], out int rightIntValue);

					if (leftIsNumber && rightIsNumber)
					{
						if (leftIntValue < rightIntValue)
						{
							Console.WriteLine("Left side is smaller, so inputs are in the right order");
							returnValue = -1;
							break;
						}
						else if (leftIntValue > rightIntValue)
						{
							Console.WriteLine("Right side is smaller, so inputs are not in the right order");
							returnValue = 1;
							break;
						}
					}

					if (leftIsNumber && !rightIsNumber)
					{
						returnValue = Validate(new string[] { leftData[i] }, Unpack(rightData[i]));
					}

					if (!leftIsNumber && rightIsNumber)
					{
						returnValue = Validate(Unpack(leftData[i]), new string[] { rightData[i] });
					}

					if (!leftIsNumber && !rightIsNumber)
					{
						returnValue = Validate(Unpack(leftData[i]), Unpack(rightData[i]));
					}

					if (returnValue != 0)
					{
						break;
					}
				}
				else
				{
					Console.WriteLine("Right side ran out of items, so inputs are not in the right order");
					returnValue = 1;
					break;
				}
			}
		}

		if(returnValue == 0 && leftData.Length < rightData.Length)
		{
			Console.WriteLine("Left side ran out of items, so inputs are in the right order");
			returnValue = -1;
		}

		return returnValue;
	}
}

public static class EnumerableExtensions
{
	public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, params T[] items)
	{
		return enumerable.Concat(items);
	}
}
