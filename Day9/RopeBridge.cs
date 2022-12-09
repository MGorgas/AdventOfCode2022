using System.Security.Cryptography.X509Certificates;

namespace Day9;

public class RopeBridge
{
	public class Knot : IEquatable<Knot>
	{
		public Knot(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public Knot? LeadingKnot { get; set; }

		public int X { get; set; }
		public int Y { get; set; }

		public bool Equals(Knot other)
		{
			return this.X == other.X && this.Y == other.Y;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}
	}
	public static void StartDay9()
	{
		string filepath = "...";
		string[] commands = File.ReadAllLines(filepath);

		int amountTailKnots = 9;

		Knot head = new(0,0);
		List<Knot> tailKnots = new List<Knot>();
		

		for(int k = 0; k < amountTailKnots; k++)
		{
			Knot knot = new(0,0);
			knot.LeadingKnot = k == 0 ? head : tailKnots[k-1];
			tailKnots.Add(knot);
		}

		Knot tail = tailKnots.Last();
		HashSet<Knot> tailVisitedPositions = new HashSet<Knot>();

		foreach(string cmd in commands)
		{
			string[] args = cmd.Split(' ');

			for(int i = 0; i < int.Parse(args[1]); i++)
			{
				switch (args[0])
				{
					case "R": head.X++; break;
					case "L": head.X--;	break;
					case "D": head.Y++; break;
					case "U": head.Y--;	break;
				}

				foreach(Knot knot in tailKnots)
				{
					int distY = knot.LeadingKnot.Y - knot.Y;
					int distX = knot.LeadingKnot.X - knot.X;

					if(Math.Abs(distX) > 1 || Math.Abs(distY) > 1)
					{
						knot.X += Math.Sign(distX);
						knot.Y += Math.Sign(distY);
					}

				}

				tailVisitedPositions.Add(new Knot(tail.X, tail.Y));

			}
		}

		Console.WriteLine(tailVisitedPositions.Count());
	}
}