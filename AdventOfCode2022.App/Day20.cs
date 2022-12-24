namespace AdventOfCode2022.App;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

public class Day20
{
    public static string filepath = @"C:\Users\Martin\source\repos\AdventOfCode2022\AdventOfCode2022.App\day20_t.txt";

    public static void GrovePositioningSystem()
    {
        List<(int Index, long Value)> message = File.ReadAllLines(filepath).Select((number, index) => (index, long.Parse(number))).ToList();


        long result = 0;

        result = GetMixResult(message);
        Console.WriteLine($"Part 1: {result}");


        long decryptionKey = 811589153;
        List<(int Index, long Value)> part2 = new(message);
        result = GetMixResult(message,10, value => value * decryptionKey);
        Console.WriteLine($"Part 2: {result}"); // 5449009573242 to high
                                                // 3760092545849

    }

    private static long GetMixResult(List<(int Index, long Value)> message, int mixingTimes = 1, Func<long, long> applyDecryptionMethod = null)
    {
        List<(int Index, long Value)> copy = new(message);

        if (applyDecryptionMethod != null)
        {
            copy = copy.Select(i => (i.Index, applyDecryptionMethod(i.Value))).ToList();
        }

        long result = 0;

        for(int time = 0; time < mixingTimes; time++)
        {
            for (int i = 0; i < copy.Count; i++)
            {
                (int Index, long Value) currentItem = copy.First(item => item.Index == i);
                int currentIndex = copy.IndexOf(currentItem);
                copy.Remove(currentItem);
                currentIndex += (int)(currentItem.Value % copy.Count);
                if (currentIndex <= 0)
                {
                    currentIndex = copy.Count + currentIndex;
                }
                else if (currentIndex > copy.Count - 1)
                {
                    currentIndex = currentIndex % copy.Count;
                }
                copy.Insert(currentIndex, currentItem);
            }
        }

        int[] numbersAfterZero = { 1000, 2000, 3000 };
        int zeroIndex = copy.IndexOfFirstWhere(i => i.Value == 0);
        foreach (int number in numbersAfterZero)
        {
            int index = (number + zeroIndex) % copy.Count;
            result += copy[index].Value;
        }

        return result;
    }
}

public static partial class ListExtensions
{
    public static int IndexOfFirstWhere<T>(this List<T> list, Func<T, bool> predicate)
    {
        T item = list.FirstOrDefault(i => predicate(i));
        if (item == null) return -1;
        return list.IndexOf(item);
    }
}