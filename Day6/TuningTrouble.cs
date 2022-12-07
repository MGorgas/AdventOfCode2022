namespace Day6;

public class TuningTrouble
{
	public static void StartDay6()
	{
		string filePath = "...";

		var part1 = File.ReadAllLines(filePath)
			.Select(
				line => {
					string firstMatch = new string(line.SkipWhile((c, idx) => line.Substring(idx, 14).Distinct().Count() < 14).Take(14).ToArray());
					return line.IndexOf(firstMatch) + 14;
					}
			).ToList();

		foreach(var lineresult in part1)
		{
			Console.WriteLine(lineresult);
		}


	}



}