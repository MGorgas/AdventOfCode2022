namespace AdventOfCode2022.App;

public class Day03
{
	public static void RucksackRecognition()
	{
		string filepath = "...";

		var part1 = File.ReadAllLines(filepath)
						 .Select(line => line.Chunk(line.Length / 2).ToArray())
						 .Select(rs => rs[0].Intersect(rs[1]).First())
						 .Select(letter => char.IsLower(letter) ? letter - 96 : letter - 38)
						 .Sum();

		Console.WriteLine(part1);

		var part2 = File.ReadAllLines(filepath)
						  .Chunk(3)
						  .Select(g => g[0].Intersect(g[1]).Intersect(g[2]).First())
						  .Select(letter => char.IsLower(letter) ? letter - 96 : letter - 38)
						  .Sum();

		Console.WriteLine(part2);
	}
}