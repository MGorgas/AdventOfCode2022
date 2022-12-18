namespace AdventOfCode2022.App;

using System.Runtime.CompilerServices;

public class Day01
{

	public static void CalorieCounting()
	{
		string filepath = "...";
		int topX = 3;
		var output = File.ReadAllText(filepath)
						 .Split($"{Environment.NewLine}{Environment.NewLine}")
						 .Select(caloriesSet => caloriesSet.Split(Environment.NewLine)
														   .Select(calories => int.Parse(calories))
														   .Sum())
						 .OrderByDescending(sum => sum)
						 .Take(topX)
						 .ToArray()
						 .Sum();


		Console.WriteLine($"Total of {output} calories.");
	}


}
