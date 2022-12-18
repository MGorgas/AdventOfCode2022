namespace AdventOfCode2022.App;

using System.Security;

public class Day05
{
	public static void SupplyStacks()
	{
		string filepath = "...";

		var data = File.ReadAllLines(filepath);
		var table = data.TakeWhile(lines => !string.IsNullOrWhiteSpace(lines)).ToList();
		var instructions = data.Skip(table.Count + 1).Take(data.Count() - (table.Count + 1)).ToList();

		var amountStacks = table.Last().Count(s => s != ' ');

		List<Stack<char>> stacks = new List<Stack<char>>();
		for (int i = 0; i < amountStacks; i++)
		{
			stacks.Add(new Stack<char>());
		}


		for (var row = table.Count - 2; row >= 0; row--) // -1 to exclude the last line
		{
			for (var currentStack = 0; currentStack < amountStacks; currentStack++)
			{
				var value = new string(table[row].Skip(currentStack * 3 + currentStack).Take(3).ToArray());
				if (!string.IsNullOrWhiteSpace(value))
				{
					stacks[currentStack].Push(value.First(c => char.IsLetter(c)));
				}
			}
		}

		foreach (var instruction in instructions)
		{
			var task = ReadInstruction(instruction);

			Stack<char> load = new Stack<char>();

			//for part 1 just push directly from one stack to the other without the "load" stack

			for (int i = 0; i < task.Amount; i++)
			{

				load.Push(stacks[task.From - 1].Pop());
			}

			for (int i = 0; i < task.Amount; i++)
			{
				stacks[task.To - 1].Push(load.Pop());
			}

		}

		string result = "";
		foreach (Stack<char> stack in stacks)
		{
			result += stack.Pop();
		}

		Console.WriteLine(result);

	}

	public static (int Amount, int From, int To) ReadInstruction(string instruction)
	{
		string[] s = instruction.Split(' ');
		return (int.Parse(s[1]), int.Parse(s[3]), int.Parse(s[5]));
	}

}