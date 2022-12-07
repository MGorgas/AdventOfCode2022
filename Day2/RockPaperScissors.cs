namespace Day2;

using System;


public class RockPaperScissorsGame
{                              // Rock	Paper  Scissors
	private static bool?[,] WinningTable = {
								{ null,  true,  false}, // Rock
								{ false, null,  true}, // Paper
								{ true,  false, null} //Scissors
							};

	public static void StartDay2()
	{
		string filepath = "...";

		int[] output = File.ReadAllText(filepath)
						 .Split(Environment.NewLine)
						 .Select(roundText =>
						 {
							 string[] split = roundText.Split(' ');
							 Possibility shape = ParseShape(split[0]);
							 bool? outcome = ParseOutcome(split[1]);
							 return new { Shape = shape, Outcome = outcome };
						 })
						 .Select(round => GetResult(round.Shape, round.Outcome))
						 .ToArray();

		long resultSum = 0;
		foreach (int result in output)
		{
			resultSum += result;
			Console.WriteLine(result);
		}

		Console.WriteLine($"My final score: {resultSum}");
		Console.ReadLine();
	}

	private static Possibility ParseShape(string input)
	{
		switch (input)
		{
			case "A":
				//case "X":
				return Possibility.Rock;
			case "B":
				//case "Y":
				return Possibility.Paper;
			case "C":
				//case "Z":
				return Possibility.Scissors;
			default:
				throw new Exception("No translation for input found");
		}
	}

	private static bool? ParseOutcome(string input)
	{
		switch (input)
		{
			case "X":
				return false;
			case "Y":
				return null;

			case "Z":
				return true;
			default:
				throw new Exception("No translation for input found");
		}
	}

	private static int GetResult(Possibility opponent, Possibility self)
	{
		int score = 0;
		switch (WinningTable[(int)opponent, (int)self])
		{
			case null:
				score += 3;
				break;
			case true:
				score += 6;
				break;
			case false:
				break;
		}
		score += (int)self + 1;

		return score;
	}

	private static int GetResult(Possibility opponent, bool? outcome)
	{
		bool?[] row = WinningTable.GetRow((int)opponent);
		Possibility shape = (Possibility)Array.IndexOf(row, outcome);

		return GetResult(opponent, shape);
	}

}

public static class ArrayExtensions
{
	public static T[] GetRow<T>(this T[,] array, int row)
	{
		return Enumerable.Range(0, array.GetLength(1)).Select(column => array[row, column]).ToArray();
	}
}

public enum Possibility
{
	Rock,
	Paper,
	Scissors
}