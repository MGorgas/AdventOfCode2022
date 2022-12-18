namespace AdventOfCode2022.App;

using System;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;

public class Day11
{
	public static void MonkeyInTheMiddle()
	{
		int reliefValue = 3;
		int rounds = 20;

		bool isPart1 = false;

		string filepath = "...";

		string[] monkeyTexts = File.ReadAllText(filepath).Split(Environment.NewLine + Environment.NewLine);

		List<Monkey> monkeys = new List<Monkey>();

		for (int i = 0; i < monkeyTexts.Length; i++)
		{
			Monkey monkey = Monkey.Parse(i, monkeyTexts[i]);
			monkeys.Add(monkey);
		}

		if (!isPart1)
		{
			reliefValue = monkeys.Multiply(m => m.TestValue);
			rounds = 10000;
		}

		for (int round = 1; round <= rounds; round++)
		{
			foreach (Monkey monkey in monkeys)
			{
				while (monkey.Items.Count > 0)
				{
					MonkeyInspectionResult inspectionResult = monkey.Inspect(reliefValue, !isPart1);
					monkeys[inspectionResult.TargetMonkeyId].Items.Add(inspectionResult.ItemWorryLevel);
				}

			}

			if (round == 1 || round == 20 || round % 1000 == 0)
			{
				Console.WriteLine($"After round {round}, the monkeys are holding items with these worry levels: {round / 1000}");
				WriteMonkeysInspectionTimes(monkeys);
				//WriteMonkeysItems(monkeys);
			}

		}

		var orderedMonkeys = monkeys.OrderByDescending(monkey => monkey.InspectedItems);
		long monkeyBusinessLevel = orderedMonkeys.Take(2).Multiply((m) => m.InspectedItems);
		Console.WriteLine($"Monkey business level: {monkeyBusinessLevel}");
	}

	public static void WriteMonkeysItems(IEnumerable<Monkey> monkeys)
	{
		foreach (var monkey in monkeys)
		{
			Console.WriteLine($"Monkey {monkey.Id}: {string.Join(", ", monkey.Items.Select(i => i.ToString()))}");
		}
	}

	public static void WriteMonkeysInspectionTimes(IEnumerable<Monkey> monkeys)
	{
		foreach (var monkey in monkeys)
		{
			Console.WriteLine($"Monkey {monkey.Id} inspected items {monkey.InspectedItems} times");
		}
	}
}

public static class IEnumerableExtensions
{
	public static long Multiply<T>(this IEnumerable<T> items, Func<T, long> itemValue)
	{
		if (!items.Any()) return 0;

		long value = 1;
		foreach (T item in items)
		{
			value *= itemValue(item);
		}
		return value;
	}

	public static int Multiply<T>(this IEnumerable<T> items, Func<T, int> itemValue)
	{
		if (!items.Any()) return 0;

		int value = 1;
		foreach (T item in items)
		{
			value *= itemValue(item);
		}
		return value;
	}
}

public record MonkeyInspectionResult(int TargetMonkeyId, long ItemWorryLevel);

public class Monkey
{
	public int Id { get; set; }

	public long InspectedItems { get; set; }

	public List<long> Items { get; set; }

	public Func<long, long> Operation { get; set; }

	public Func<long, int> Test { get; set; }

	public int TestValue { get; set; }

	public static Monkey Parse(int id, string monkeyText)
	{
		Monkey monkey = new Monkey();

		string[] lines = monkeyText.Split(Environment.NewLine);

		monkey.Id = id;
		monkey.Items = lines[1].Split(": ")[1].Split(", ").Select(e => long.Parse(e)).ToList();


		string operationString = lines[2].Split('=')[1];
		monkey.Operation = CreateOperationFunction(operationString);


		int divisibleBy = int.Parse(lines[3].Split(' ').Last());
		monkey.TestValue = divisibleBy;
		int targetMonkeyTrue = int.Parse(lines[4].Split(' ').Last());
		int targetMonkeyFalse = int.Parse(lines[5].Split(' ').Last());
		monkey.Test = CreateTestFunction(divisibleBy, targetMonkeyTrue, targetMonkeyFalse);
		return monkey;
	}

	public static Func<long, long> CreateOperationFunction(string expression)
	{
		string[] expresstionParts = expression.Trim().Split(' ');
		var firstValue = expresstionParts[0];
		var operation = expresstionParts[1];
		var secondValue = expresstionParts[2];

		Func<long, long, long> innerOperation;

		innerOperation = operation switch
		{
			"+" => new Func<long, long, long>((a, b) => a + b),
			"-" => new Func<long, long, long>((a, b) => a - b),
			"*" => new Func<long, long, long>((a, b) => a * b),
			"/" => new Func<long, long, long>((a, b) => a / b),
			_ => throw new InvalidDataException()
		};

		Func<long, long> operationFunction;

		if (firstValue.Equals("old") && secondValue.Equals("old"))
		{
			operationFunction = new Func<long, long>((i) => innerOperation(i, i));
		}
		else if (firstValue.Equals("old") && long.TryParse(secondValue, out long i2))
		{
			operationFunction = new Func<long, long>((i) => innerOperation(i, i2));
		}
		else if (long.TryParse(firstValue, out long i3) && secondValue.Equals("old"))
		{
			operationFunction = new Func<long, long>((i) => innerOperation(i3, i));
		}
		else if (long.TryParse(firstValue, out long i4) && long.TryParse(secondValue, out long i5))
		{
			operationFunction = new Func<long, long>((i) => innerOperation(i4, i5));
		}
		else
		{
			throw new InvalidDataException();
		}

		return operationFunction;
	}

	public static Func<long, int> CreateTestFunction(long divisableByValue, int targetMonkeyTrueValue, int targetMonkeyFalseValue)
	{
		return new Func<long, int>(i => i % divisableByValue == 0 ? targetMonkeyTrueValue : targetMonkeyFalseValue);
	}

	public MonkeyInspectionResult Inspect(int reliefValue, bool useModulo = false)
	{
		long itemWorryLevel = this.Items.First();
		long newItemWorryLevel = this.Operation(itemWorryLevel);
		newItemWorryLevel = useModulo ? newItemWorryLevel % reliefValue : newItemWorryLevel / reliefValue;
		int targetMonkeyId = this.Test(newItemWorryLevel);

		this.InspectedItems++;
		this.Items.Remove(this.Items.First());

		return new MonkeyInspectionResult(targetMonkeyId, newItemWorryLevel);
	}
}

