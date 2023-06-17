using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameConsole;

internal class CommandBranch
{
	public Delegate action;

	public Dictionary<string, CommandBranch> children = new Dictionary<string, CommandBranch>();

	public bool requireCheats;

	public CommandBranch(Delegate action, bool requireCheats)
	{
		this.action = action;
		this.requireCheats = requireCheats;
	}

	public CommandBranch()
	{
	}

	public bool Execute(string fullName, string currentName, Console con, Queue<string> args)
	{
		if (args.Count > 0)
		{
			if (requireCheats && con.CheatBlocker())
			{
				return false;
			}
			if (children.TryGetValue(args.Peek(), out var value))
			{
				return value.Execute((fullName != "") ? (fullName + " " + currentName) : currentName, args.Dequeue(), con, args);
			}
		}
		return PerformAction(fullName, currentName, con, args);
	}

	private bool PerformAction(string fullName, string name, Console con, Queue<string> args)
	{
		if ((object)action == null)
		{
			PrintUsage(fullName, con);
			return false;
		}
		if (requireCheats && con.CheatBlocker())
		{
			return false;
		}
		ParameterInfo[] parameters = action.Method.GetParameters();
		object[] array = new object[parameters.Length];
		if (args.Count != array.Length && (args.Count <= array.Length || !(parameters.Last().ParameterType == typeof(string[]))))
		{
			PrintUsage(fullName, con);
			throw new ArgumentException($"<b>'{name}'</b> has <i>{array.Length}</i> parameters, but <i>{args.Count}</i> arguments were given");
		}
		for (int i = 0; i < array.Length; i++)
		{
			Type parameterType = parameters[i].ParameterType;
			string text = args.Dequeue();
			if (parameterType == typeof(bool))
			{
				array[i] = bool.Parse(text);
				continue;
			}
			if (parameterType == typeof(int))
			{
				array[i] = int.Parse(text);
				continue;
			}
			if (parameterType == typeof(float))
			{
				array[i] = float.Parse(text);
				continue;
			}
			if (parameterType == typeof(string))
			{
				array[i] = text;
				continue;
			}
			if (parameterType == typeof(string[]))
			{
				List<string> list = new List<string> { text };
				list.AddRange(args);
				array[i] = list.ToArray();
				break;
			}
			if (parameterType.IsSubclassOf(typeof(Enum)))
			{
				array[i] = Enum.Parse(parameterType, text);
				continue;
			}
			throw new ArgumentException($"{name} has an unsupported parameter type: {parameterType}");
		}
		action.DynamicInvoke(array);
		return true;
	}

	public void PrintUsage(string name, Console con)
	{
		if ((object)action != null)
		{
			ParameterInfo[] parameters = action.Method.GetParameters();
			con.PrintLine("Usage: " + name + " " + string.Join(" ", parameters.Select((ParameterInfo p) => "<" + p.Name + ": " + p.ParameterType.Name + ">")));
		}
		con.PrintLine("Usage: " + name + " <subcommand>");
		if (children.Count <= 0)
		{
			return;
		}
		con.PrintLine("Subcommands:");
		foreach (KeyValuePair<string, CommandBranch> child in children)
		{
			con.PrintLine(" - " + child.Key);
		}
	}
}
