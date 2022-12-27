namespace AdventOfCode2022.App;

using System.Diagnostics;
using System.Linq.Expressions;

public class Day21
{
    public static string filepath = @"C:\Users\Martin\source\repos\AdventOfCode2022\AdventOfCode2022.App\day21.txt";

    public static void MonkeyMath()
    {
        // part 1
        Dictionary<string, Monkey> monkeys = File.ReadAllLines(filepath).Select(l => Monkey.Parse(l)).ToDictionary(m => m.Name, m => m);
        
        long result = GetResult(monkeys, monkeys["root"]);
        Console.WriteLine($"Part 1: {result}");

        Monkey root = monkeys["root"];
        root.Operation = "-";
        root.Number = 0;
        Monkey humn = GetHumanValue(monkeys, root, "humn");

        Console.WriteLine($"Part 2: {humn.Number.Value}");
    }


    [DebuggerDisplay("M:{Name} N:{Number}")]
    private class Monkey
    {
        public Monkey()
        {
        }

        public string Name { get; set; }

        public long? Number { get; set; }

        public bool HasOperation { get; set; }

        public string Operation { get; set; }

        public string LeftOperationMonkeyName { get; set; }

        public string RightOperationMonkeyName { get; set; }


        public static Monkey Parse(string input)
        {
            string[] nameAndNumberOrOperation = input.Split(':', StringSplitOptions.TrimEntries);
            string[] numberOrOperation = nameAndNumberOrOperation[1].Split(' ');

            string name = nameAndNumberOrOperation[0];
            bool hasOperation = numberOrOperation.Length > 1;

            long? number = null;
            string leftOperationMonkeyName = string.Empty;
            string operation = string.Empty;
            string rightOperationMonkeyName = string.Empty;

            if (hasOperation)
            {
                leftOperationMonkeyName = numberOrOperation[0];
                operation = numberOrOperation[1];
                rightOperationMonkeyName = numberOrOperation[2];
            }
            else
            {
                number = long.Parse(numberOrOperation[0]);
            }
            Monkey monkey = new Monkey()
            {
                Name = name,
                Number = number,
                HasOperation = hasOperation,
                Operation = operation,
                LeftOperationMonkeyName = leftOperationMonkeyName,
                RightOperationMonkeyName = rightOperationMonkeyName
            };

            return monkey;
        }
    }

    private static long GetResult(Dictionary<string, Monkey> monkeys, Monkey currentMonkey)
    {
        if (currentMonkey.HasOperation)
        {
            Monkey left = monkeys[currentMonkey.LeftOperationMonkeyName];
            Monkey right = monkeys[currentMonkey.RightOperationMonkeyName];

            return GetOperation(currentMonkey.Operation)(GetResult(monkeys, left), GetResult(monkeys, right));
        }

        return currentMonkey.Number.Value;
    }

    private static bool DependsOnTarget(Dictionary<string, Monkey> monkeys, Monkey currentMonkey, string targetName)
    {
        if (currentMonkey.Name.Equals(targetName))
        {
            return true;
        }
        if (currentMonkey.HasOperation)
        {
            Monkey left = monkeys[currentMonkey.LeftOperationMonkeyName];
            Monkey right = monkeys[currentMonkey.RightOperationMonkeyName];

            return DependsOnTarget(monkeys, left, targetName) || DependsOnTarget(monkeys, right, targetName);
        }

        return false;
    }


    private static Monkey GetHumanValue(Dictionary<string, Monkey> monkeys, Monkey currentMonkey, string targetName)
    {
        if (currentMonkey.HasOperation && !currentMonkey.Name.Equals(targetName))
        {
            Func<long, long, long> inverseOperation = GetInverseOperation(currentMonkey.Operation);

            Monkey left = monkeys[currentMonkey.LeftOperationMonkeyName];
            Monkey right = monkeys[currentMonkey.RightOperationMonkeyName];
            //need to inverse the operations but take care of commutative properties
            if (DependsOnTarget(monkeys, left, targetName))
            {
                var number = GetResult(monkeys, right);
                switch (currentMonkey.Operation)
                {
                    case "+":
                        left.Number = currentMonkey.Number - number;
                        break;
                    case "-":
                        left.Number = currentMonkey.Number + number;
                        break;
                    case "*":
                        left.Number = currentMonkey.Number / number;
                        break;
                    case "/":
                        left.Number = currentMonkey.Number * number;
                        break;
                    default:
                        throw new InvalidDataException();
                }
                return GetHumanValue(monkeys, left, targetName);
            }
            else
            {
                var number = GetResult(monkeys, left);
                switch (currentMonkey.Operation)
                {
                    case "+":
                        right.Number = currentMonkey.Number - number;
                        break;
                    case "-":
                        right.Number = number - currentMonkey.Number;
                        break;
                    case "*":
                        right.Number = currentMonkey.Number / number;
                        break;
                    case "/":
                        right.Number = number / currentMonkey.Number;
                        break;
                    default:
                        throw new InvalidDataException();
                }
                return GetHumanValue(monkeys, right, targetName);
            }
        }
        return currentMonkey;
    }

    private static Func<long, long, long> GetInverseOperation(string operationSymbol)
    {
        return GetOperation(InvertOperation(operationSymbol));
    }

    private static Func<long, long ,long> GetOperation(string operationSymbol)
    {
        return operationSymbol switch
        {
            "+" => new Func<long, long, long>((a, b) => a + b),
            "-" => new Func<long, long, long>((a, b) => a - b),
            "*" => new Func<long, long, long>((a, b) => a * b),
            "/" => new Func<long, long, long>((a, b) => a / b),
            _ => throw new InvalidDataException()
        };
    }

    private static string InvertOperation(string operationSymbol)
    {
        return operationSymbol switch
        {
            "+" => "-",
            "-" => "+",
            "*" => "/",
            "/" => "*",
            _ => throw new InvalidDataException()
        };
    }
}
