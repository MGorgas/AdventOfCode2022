namespace AdventOfCode2022.App;

public class Day04
{
	public static void CampCleanup()
	{
		string filepath = "...";

		var part1 = File.ReadAllLines(filepath)
						.Select(line => line.Split(','))
						.Select(entry => new { first = entry.First().Split('-'), second = entry.Last().Split('-') })
						.Select(stringEntry => new
						{
							firstStart = int.Parse(stringEntry.first[0]),
							firstEnd = int.Parse(stringEntry.first[1]),
							secondStart = int.Parse(stringEntry.second[0]),
							secondEnd = int.Parse(stringEntry.second[1])
						})
						.Where(assignment => assignment.firstStart >= assignment.secondStart && assignment.firstEnd <= assignment.secondEnd ||
										   assignment.secondStart >= assignment.firstStart && assignment.secondEnd <= assignment.firstEnd)
						.Count();

		Console.WriteLine(part1);

		var part2 = File.ReadAllLines(filepath)
				.Select(line => line.Split(','))
				.Select(entry => new { first = entry.First().Split('-'), second = entry.Last().Split('-') })
				.Select(stringEntry => new
				{
					firstStart = int.Parse(stringEntry.first[0]),
					firstEnd = int.Parse(stringEntry.first[1]),
					secondStart = int.Parse(stringEntry.second[0]),
					secondEnd = int.Parse(stringEntry.second[1])
				})
				.Where(assignment => assignment.firstStart >= assignment.secondStart && assignment.firstStart <= assignment.secondEnd ||
								   assignment.secondStart >= assignment.firstStart && assignment.secondStart <= assignment.firstEnd ||
								   assignment.firstEnd >= assignment.secondStart && assignment.firstEnd <= assignment.secondEnd ||
								   assignment.secondEnd >= assignment.firstStart && assignment.secondEnd <= assignment.firstEnd)

				.Count();

		Console.WriteLine(part2);

	}
}