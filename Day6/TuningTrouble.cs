namespace Day6;

public class TuningTrouble
{
	public static void StartDay6()
	{
		string filePath = "...";

		// length in part 1 is 4;
		int sequenceLength = 14;

		var part1 = File.ReadAllLines(filePath)
			.Select(
				line => {
					string firstMatch = new string(line.SkipWhile((c, idx) => line.Substring(idx, sequenceLength).Distinct().Count() < sequenceLength).Take(sequenceLength).ToArray());
					return line.IndexOf(firstMatch) + sequenceLength;
					}
			).ToList();

		foreach(var lineresult in part1)
		{
			Console.WriteLine(lineresult);
		}


	}



}