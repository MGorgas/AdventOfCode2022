namespace AdventOfCode2022.App;

using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

public class Day19
{
	public static string filepath = @"";

	public static void NotEnoughMinerals()
	{
		var blueprints = File.ReadAllLines(filepath);


		int result = 0;
		for (int i = 1; i <= blueprints.Length; i++)
		{
			int bpr = FindBestResultForBlueprint2(blueprints[i-1], 24); // for the testdata it only works for the first blueprint
			Console.WriteLine($"Blueprint no.{i}: {bpr}");
			result += i * bpr;
		}
        Console.WriteLine($"Part 1: {result}");


		result = 1;
        for (int i = 1; i <= 3; i++) 
		{
            int bpr = FindBestResultForBlueprint2(blueprints[i - 1], 32); // for the testdata it only works for the first blueprint
            Console.WriteLine($"Blueprint no.{i}: {bpr}");
            result *= bpr;
        }
		Console.WriteLine($"Part 2: {result}");

	}

	public static int FindBestResultForBlueprint(string blueprintInput, int minutes)
	{
		Blueprint blueprint = Blueprint.Parse(blueprintInput);
		State initialState = new State(0, 1, 0, 0, 0, 0, 0, 0, 0);

		State result = initialState;

		Queue<State> states = new();
		HashSet<State> visited = new();

		states.Enqueue(initialState);
		visited.Add(initialState);

		while(states.Count > 0)
		{
			State state = states.Dequeue();

			if (state.minute > minutes) continue;
			result = state.OpenedGeodes > result.OpenedGeodes ? state : result;

			Material[] possibleRobots = blueprint.BuildableRobots(state);

			State[] newStates = new State[possibleRobots.Length+1];

			for(int i = 0; i < possibleRobots.Length; i++)
			{
				RobotBuildCosts rbs = blueprint.GetBuildCostsFor(possibleRobots[i]);

				State createRobotState = new State(state.minute + 1,
									possibleRobots[i] == Material.Ore ? state.OreRobots + 1 : state.OreRobots,
									possibleRobots[i] == Material.Clay ? state.ClayRobots + 1 : state.ClayRobots,
									possibleRobots[i] == Material.Obsidian ? state.ObsidianRobots + 1 : state.ObsidianRobots,
									possibleRobots[i] == Material.Geode ? state.GeodeRobots + 1 : state.GeodeRobots,
									state.AmountOre + state.OreRobots - rbs.OreCosts,
									state.AmountClay + state.ClayRobots - rbs.ClayCosts,
									state.AmountObsidian + state.ObsidianRobots - rbs.ObsidianCosts,
									state.OpenedGeodes + state.GeodeRobots);

				newStates[i] = createRobotState;
			}

			newStates[newStates.Length - 1] = new State(state.minute + 1,
							state.OreRobots,
							state.ClayRobots,
							state.ObsidianRobots,
							state.GeodeRobots,
							state.AmountOre + state.OreRobots,
							state.AmountClay + state.ClayRobots,
							state.AmountObsidian + state.ObsidianRobots,
							state.OpenedGeodes + state.GeodeRobots);
			
			if(newStates.Any(s => s.GeodeRobots > state.GeodeRobots))
			{
				var filteredNextStates = newStates.Where(s => s.GeodeRobots > state.GeodeRobots);

				foreach (State nextState in filteredNextStates)
				{
                    if (visited.Add(nextState))
                    {
                        states.Enqueue(nextState);
                    }
                }
			}
			else
			{
				foreach(State newState in newStates)
				{
					if (visited.Add(newState))
					{
						states.Enqueue(newState);
					}
				}
			}

		}
		Console.WriteLine(result);
		return result.OpenedGeodes;
	}

    public static int FindBestResultForBlueprint2(string blueprintInput, int minutes)
    {
        Blueprint blueprint = Blueprint.Parse(blueprintInput);
        State initialState = new State(0, 1, 0, 0, 0, 0, 0, 0, 0);

        HashSet<State> visited = new();
        //State result = initialState;

        Queue<State> queue = new Queue<State>();
        queue.Enqueue(initialState);
        visited.Add(initialState);

		bool hasGeodeRobot = false;
		int currentMaxGeodeRobots = 0;
        for (int minute = 1; minute <= minutes; minute++)
		{
			Queue<State> currentMinuteStates = new();

            while (queue.Count > 0)
            {
                State state = queue.Dequeue();

                if (state.minute > minutes) continue;
                //result = state.OpenedGeodes > result.OpenedGeodes ? state : result;

                Material[] possibleRobots = blueprint.BuildableRobots(state);

                State[] newStates = new State[possibleRobots.Length + 1];

                for (int i = 0; i < possibleRobots.Length; i++)
                {
                    RobotBuildCosts rbs = blueprint.GetBuildCostsFor(possibleRobots[i]);

                    State createRobotState = new State(minute,
                                        possibleRobots[i] == Material.Ore ? state.OreRobots + 1 : state.OreRobots,
                                        possibleRobots[i] == Material.Clay ? state.ClayRobots + 1 : state.ClayRobots,
                                        possibleRobots[i] == Material.Obsidian ? state.ObsidianRobots + 1 : state.ObsidianRobots,
                                        possibleRobots[i] == Material.Geode ? state.GeodeRobots + 1 : state.GeodeRobots,
                                        state.AmountOre + state.OreRobots - rbs.OreCosts,
                                        state.AmountClay + state.ClayRobots - rbs.ClayCosts,
                                        state.AmountObsidian + state.ObsidianRobots - rbs.ObsidianCosts,
                                        state.OpenedGeodes + state.GeodeRobots);

                    newStates[i] = createRobotState;
                }

                newStates[newStates.Length - 1] = new State(minute,
                                state.OreRobots,
                                state.ClayRobots,
                                state.ObsidianRobots,
                                state.GeodeRobots,
                                state.AmountOre + state.OreRobots,
                                state.AmountClay + state.ClayRobots,
                                state.AmountObsidian + state.ObsidianRobots,
                                state.OpenedGeodes + state.GeodeRobots);

                if (newStates.Any(s => s.GeodeRobots > state.GeodeRobots))
                {
                    var filteredNextStates = newStates.Where(s => s.GeodeRobots > state.GeodeRobots);

                    foreach (State nextState in filteredNextStates)
                    {
                        if (visited.Add(nextState))
                        {
                            currentMinuteStates.Enqueue(nextState);
                        }
                    }
                }
                else
                {
                    foreach (State newState in newStates)
                    {
                        if (visited.Add(newState))
                        {
                            currentMinuteStates.Enqueue(newState);
                        }
                    }
                }

            }

			IEnumerable<State> toEnqueue;

			if(!hasGeodeRobot && currentMinuteStates.Any(s => s.GeodeRobots > 0))
			{
                //remove all branches where no geode robot is yet created
                toEnqueue = currentMinuteStates.Where(s => s.GeodeRobots > 0);
				currentMaxGeodeRobots++;
                hasGeodeRobot = true;
            }
			//else if (hasGeodeRobot)
			//{
			//	currentMaxGeodeRobots++;
   //             toEnqueue = currentMinuteStates;
   //         }
            else
			{
				// there is nothing "interesting" happening for now
				toEnqueue = currentMinuteStates;
			}

			foreach(State s in toEnqueue)
			{
				queue.Enqueue(s);
			}
        }

        return queue.Max(s => s.OpenedGeodes);

    }

    public record State(int minute, int OreRobots, int ClayRobots, int ObsidianRobots, int GeodeRobots, int AmountOre, int AmountClay, int AmountObsidian, int OpenedGeodes);


	public class Blueprint
	{
		private List<RobotBuildCosts> robotBuildCosts;

		private Blueprint(List<RobotBuildCosts> robotBuildCosts)
		{
			this.robotBuildCosts = robotBuildCosts;
		}

		public IReadOnlyList<RobotBuildCosts> RobotsBuildCosts { get { return robotBuildCosts; } }

		public RobotBuildCosts GetBuildCostsFor(Material material)
		{
			return this.robotBuildCosts.First(r => r.RobotMaterialCollectionType == material);
		}

		public static Blueprint Parse(string blueprint)
		{
			var values = Regex.Matches(blueprint, @"[0-9]+").Select(m => int.Parse(m.Value)).ToArray();
			
			HashSet<RobotBuildCosts> robotBuildCosts = new();

			robotBuildCosts.Add(new RobotBuildCosts(Material.Geode, values[5], 0, values[6]));
			robotBuildCosts.Add(new RobotBuildCosts(Material.Obsidian, values[3], values[4], 0));
			robotBuildCosts.Add(new RobotBuildCosts(Material.Clay, values[2], 0, 0));
			robotBuildCosts.Add(new RobotBuildCosts(Material.Ore, values[1], 0, 0));

			return new Blueprint(robotBuildCosts.ToList());
		}

		public Material[] BuildableRobots(State state)
		{
			return this.RobotsBuildCosts.Where(r => r.OreCosts <= state.AmountOre && r.ClayCosts <= state.AmountClay && r.ObsidianCosts <= state.AmountObsidian).Select(r => r.RobotMaterialCollectionType).ToArray();
		}
	}

	public record RobotBuildCosts(Material RobotMaterialCollectionType, int OreCosts, int ClayCosts, int ObsidianCosts);

	public enum Material
	{
		Ore,
		Clay,
		Obsidian,
		Geode
	}
}
