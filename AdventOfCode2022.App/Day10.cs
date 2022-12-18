namespace AdventOfCode2022.App;

using System.Text;

public class Day10
{
	public static void CathodeRayTube()
	{
		string filePath = "...";
		var programLines = File.ReadAllLines(filePath).AsEnumerable();

		int registerX = 1;
		int cycles = 1;
		int sumOfSignalStrength = 0;

		StringBuilder screenContent = new StringBuilder();

		IEnumerator<string> enumerator = programLines.GetEnumerator();

		bool programEnd = false;
		while (!programEnd)
		{
			if (!enumerator.MoveNext())
			{
				programEnd = true;
				continue;
			};
			string[] current = enumerator.Current.Split(' ');
			int operationCycles = 0;
			bool isNoop = false;
			switch (current[0])
			{
				case "noop":
					operationCycles = 1;
					isNoop = true;
					break;
				case "addx":
					operationCycles = 2;
					break;
				default:
					throw new InvalidDataException();
			}

			for (int i = 0; i < operationCycles; i++, cycles++)
			{


				//part2
				int currentPixelX = cycles % 40;
				screenContent.Append(currentPixelX > registerX - 1 && currentPixelX <= registerX + 2 ? "#" : ".");


				if (cycles % 40 - 20 == 0)
				{
					//part 1
					WriteCycle(registerX, currentPixelX, enumerator.Current);
					sumOfSignalStrength += registerX * cycles;
				}

				if (i == 1)
				{
					registerX += int.Parse(current[1]);
				}

			}
		}
		Console.WriteLine($"Part 1 SignalStrengthsSum: {sumOfSignalStrength}");

		Draw(screenContent);

	}

	public static void WriteCycle(int registerX, int cycle, string prefix = "")
	{
		Console.WriteLine(string.Format("{0,10} C:{1,4}  register-X:{2,3}   multiple:{3,5}", prefix, cycle, registerX, registerX * cycle));
	}

	public static void Draw(StringBuilder stringBuilder)
	{
		var screenLines = stringBuilder.ToString().Chunk(40).Select(chunk => new string(chunk.ToArray()));
		foreach (var line in screenLines)
		{
			Console.WriteLine(line);
		}
	}
}