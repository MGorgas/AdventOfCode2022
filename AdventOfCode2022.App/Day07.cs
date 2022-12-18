namespace AdventOfCode2022.App;

public class Day07
{
	public static void NoSpaceLeftOnDevice()
	{
		string filepath = "...";

		var commandsAndOutputs = File.ReadAllLines(filepath);

		long overallsize = 70000000;
		long updateSize = 30000000;


		EDirectory root = new EDirectory("/");
		EDirectory currentDirectory = root;

		List<EDirectory> allDirectories = new();
		allDirectories.Add(root);

		string lastCommand = "";
		foreach (var commandOrOutput in commandsAndOutputs)
		{
			string[] parsed = commandOrOutput.Split(' ');
			switch (parsed[0])
			{
				case "$":
					// is command
					lastCommand = parsed[1];

					switch (lastCommand)
					{
						case "cd":
							switch (parsed[2])
							{
								case "..":
									currentDirectory = currentDirectory.ParentDirectory;
									break;
								default:
									currentDirectory = (EDirectory)currentDirectory.Contents.FirstOrDefault(e => e.Type == ElementType.Directory && e.Name == parsed[2]) ?? currentDirectory;
									break;
							}
							break;
						case "ls":
							break;
					}
					break;
				default:
					// is output

					if (lastCommand == "ls")
					{
						switch (parsed[0])
						{
							case "dir":
								EDirectory dir = new EDirectory(parsed[1]) { ParentDirectory = currentDirectory };
								currentDirectory.Contents.Add(dir);
								allDirectories.Add(dir);
								break;
							default:
								EFile file = new EFile(parsed[1], long.Parse(parsed[0]));
								currentDirectory.Contents.Add(file);
								break;
						}
					}

					break;
			}
		}

		// part1
		var summOfSmallDirectories = allDirectories.Where(d => d.Size <= 100000).Sum(d => d.Size);
		Console.WriteLine(summOfSmallDirectories);


		// part 2
		long currentlyAvailableSpace = overallsize - root.Size;
		long minimumFreeingSize = updateSize - currentlyAvailableSpace;

		Console.WriteLine($"{currentlyAvailableSpace} / {overallsize} available");
		Console.WriteLine($"Update size: {updateSize}");
		Console.WriteLine($"Minimum size to remove: {minimumFreeingSize}");

		var directoriesFiltered = allDirectories.Where(d => d.Size >= minimumFreeingSize).OrderBy(d => d.Size);

		foreach (var d in directoriesFiltered)
		{
			Console.WriteLine(string.Format("{0,10}{1,10}", d.Name, d.Size));
		}



	}

	public enum ElementType
	{
		Directory,
		File
	}

	public abstract class Element
	{
		public Element(ElementType type, string name)
		{
			this.Type = type;
			this.Name = name;
		}
		public string Name { get; }

		public ElementType Type { get; }

		public abstract long Size { get; }

		public override string ToString()
		{
			return $"{this.Name} ({this.Type})";
		}
	}

	public class EDirectory : Element
	{

		public EDirectory(string name) : base(ElementType.Directory, name)
		{
			Contents = new();
		}
		public EDirectory? ParentDirectory { get; set; }

		public List<Element> Contents { get; set; }

		public override long Size
		{
			get
			{
				long size = 0;

				foreach (var element in Contents)
				{
					size += element.Size;
				}

				return size;
			}

		}
	}

	public class EFile : Element
	{
		public EFile(string name, long size) : base(ElementType.File, name)
		{
			Size = size;
		}

		public override long Size { get; }
	}
}