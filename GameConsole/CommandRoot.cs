using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;

namespace GameConsole;

public abstract class CommandRoot : ICommand
{
	public class PrefReference
	{
		public string Key;

		public Type Type;

		public bool Local;

		public string Default;
	}

	private CommandBranch rootNode;

	private CommandBranch currentNode;

	private const string KeyColor = "#db872c";

	private const string TypeColor = "#879fff";

	private const string ValueColor = "#4ac246";

	public abstract string Name { get; }

	public abstract string Description { get; }

	public virtual string Command => Name.Replace(' ', '_').ToLower();

	protected abstract void BuildTree(Console con);

	protected void Branch(string name, Action buildChildren, bool requireCheats = false)
	{
		CommandBranch value = new CommandBranch
		{
			requireCheats = requireCheats
		};
		CommandBranch commandBranch = currentNode;
		commandBranch.children.Add(name, value);
		currentNode = value;
		buildChildren?.Invoke();
		currentNode = commandBranch;
	}

	protected void Leaf(Action action, bool requireCheats = false)
	{
		currentNode.action = action;
		currentNode.requireCheats = requireCheats;
	}

	protected void Leaf(string name, Action overload, bool requireCheats = false)
	{
		currentNode.children.Add(name, new CommandBranch(overload, requireCheats));
		currentNode.requireCheats = requireCheats;
	}

	protected void Leaf<T1>(string name, Action<T1> overload, bool requireCheats = false)
	{
		currentNode.children.Add(name, new CommandBranch(overload, requireCheats));
		currentNode.requireCheats = requireCheats;
	}

	protected void Leaf<T1, T2>(string name, Action<T1, T2> overload, bool requireCheats = false)
	{
		currentNode.children.Add(name, new CommandBranch(overload, requireCheats));
		currentNode.requireCheats = requireCheats;
	}

	protected void Leaf<T1, T2, T3>(string name, Action<T1, T2, T3> overload, bool requireCheats = false)
	{
		currentNode.children.Add(name, new CommandBranch(overload, requireCheats));
		currentNode.requireCheats = requireCheats;
	}

	protected void Leaf<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> overload, bool requireCheats = false)
	{
		currentNode.children.Add(name, new CommandBranch(overload, requireCheats));
		currentNode.requireCheats = requireCheats;
	}

	public void Execute(Console con, string[] args)
	{
		if (rootNode == null)
		{
			rootNode = new CommandBranch();
			currentNode = rootNode;
			BuildTree(con);
		}
		Queue<string> args2 = new Queue<string>(args);
		rootNode.Execute("", Command, con, args2);
	}

	public void BuildPrefsEditor(List<PrefReference> pref)
	{
		Leaf("prefs", delegate
		{
			if (!MonoSingleton<Console>.Instance.CheatBlocker())
			{
				MonoSingleton<Console>.Instance.PrintLine("Available prefs:");
				foreach (PrefReference item in pref)
				{
					string text = (item.Local ? "<color=red>LOCAL</color>" : string.Empty);
					if (item.Type == typeof(int))
					{
						string text2 = (MonoSingleton<PrefsManager>.Instance.HasKey(item.Key) ? (item.Local ? MonoSingleton<PrefsManager>.Instance.GetIntLocal(item.Key) : MonoSingleton<PrefsManager>.Instance.GetInt(item.Key)).ToString() : (string.IsNullOrEmpty(item.Default) ? "<color=red>NOT SET</color>" : item.Default));
						MonoSingleton<Console>.Instance.PrintLine("- <color=#db872c>" + item.Key + "</color>: <color=#4ac246>" + text2 + "</color>   [<color=#879fff>int</color>] " + text);
					}
					else if (item.Type == typeof(float))
					{
						string text3 = (MonoSingleton<PrefsManager>.Instance.HasKey(item.Key) ? (item.Local ? MonoSingleton<PrefsManager>.Instance.GetFloatLocal(item.Key) : MonoSingleton<PrefsManager>.Instance.GetFloat(item.Key)).ToString(CultureInfo.InvariantCulture) : (string.IsNullOrEmpty(item.Default) ? "<color=red>NOT SET</color>" : item.Default));
						MonoSingleton<Console>.Instance.PrintLine("- <color=#db872c>" + item.Key + "</color>: <color=#4ac246>" + text3 + "</color>   [<color=#879fff>float</color>] " + text);
					}
					else if (item.Type == typeof(bool))
					{
						string text4 = (MonoSingleton<PrefsManager>.Instance.HasKey(item.Key) ? ((item.Local ? MonoSingleton<PrefsManager>.Instance.GetBoolLocal(item.Key) : MonoSingleton<PrefsManager>.Instance.GetBool(item.Key)) ? "True" : "False") : (string.IsNullOrEmpty(item.Default) ? "<color=red>NOT SET</color>" : item.Default));
						MonoSingleton<Console>.Instance.PrintLine("- <color=#db872c>" + item.Key + "</color>: <color=#4ac246>" + text4 + "</color>   [<color=#879fff>float</color>] " + text);
					}
					else if (item.Type == typeof(string))
					{
						string text5 = (item.Local ? MonoSingleton<PrefsManager>.Instance.GetStringLocal(item.Key) : MonoSingleton<PrefsManager>.Instance.GetString(item.Key));
						MonoSingleton<Console>.Instance.PrintLine("- <color=#db872c>" + item.Key + "</color>: <color=#4ac246>\"" + (string.IsNullOrEmpty(text5) ? item.Default : text5) + "\"</color>   [<color=#879fff>float</color>] " + text);
					}
					else
					{
						MonoSingleton<Console>.Instance.PrintLine("Pref " + item.Key + " is type " + item.Type.Name + " (Unrecognized)");
					}
				}
				MonoSingleton<Console>.Instance.PrintLine("You can use `<color=#7df59d>prefs set <type> <value></color>` to change a pref");
				MonoSingleton<Console>.Instance.PrintLine("or `<color=#7df59d>prefs set_local <type> <value></color>` to change a <color=#db872c>local</color> pref. (it matters)");
			}
		});
	}

	public void BuildBoolMenu(string commandKey, Func<bool> valueGetter, Action<bool> valueSetter, bool inverted = false)
	{
		BuildBoolMenu(commandKey, valueGetter, valueSetter, inverted, null);
	}

	public void BuildBoolMenu(string commandKey, Func<bool> valueGetter, Action<bool> valueSetter, [CanBeNull] Action continueBranch)
	{
		BuildBoolMenu(commandKey, valueGetter, valueSetter, inverted: false, continueBranch);
	}

	public void BuildBoolMenu(string commandKey, Func<bool> valueGetter, Action<bool> valueSetter, bool inverted, [CanBeNull] Action continueBranch)
	{
		Branch(commandKey, delegate
		{
			Leaf("toggle", delegate
			{
				bool flag = !valueGetter();
				valueSetter(flag);
				MonoSingleton<Console>.Instance.PrintLine("<color=#db872c>" + commandKey + "</color> is now <color=#4ac246>" + GetStateName(flag, inverted) + "</color>");
			}, requireCheats: true);
			Leaf("on", delegate
			{
				valueSetter(!inverted);
				MonoSingleton<Console>.Instance.PrintLine("<color=#db872c>" + commandKey + "</color> is now <color=#4ac246>" + GetStateName(!inverted, inverted) + "</color>");
			}, requireCheats: true);
			Leaf("off", delegate
			{
				valueSetter(inverted);
				MonoSingleton<Console>.Instance.PrintLine("<color=#db872c>" + commandKey + "</color> is now <color=#4ac246>" + GetStateName(inverted, inverted) + "</color>");
			}, requireCheats: true);
			Leaf("read", delegate
			{
				MonoSingleton<Console>.Instance.PrintLine("The current value is <color=#4ac246>" + GetStateName(valueGetter(), inverted) + "</color>");
			}, requireCheats: true);
			continueBranch?.Invoke();
		});
	}

	private string GetStateName(bool value, bool inverted)
	{
		if (!inverted)
		{
			if (!value)
			{
				return "off";
			}
			return "on";
		}
		if (!value)
		{
			return "on";
		}
		return "off";
	}
}
