using System.Collections.Generic;
using System.Linq;

namespace GameConsole.Commands;

public class ConsoleCmd : CommandRoot
{
	public override string Name => "Console";

	public override string Description => "Used for configuring the console";

	protected override void BuildTree(Console con)
	{
		BuildBoolMenu("hide_badge", () => con.errorBadge.hidden, delegate(bool value)
		{
			con.errorBadge.SetEnabled(value);
		});
		Leaf("change_bind", delegate(string bind, string key)
		{
			if (con.binds.defaultBinds.ContainsKey(bind.ToLower()))
			{
				con.binds.Rebind(bind.ToLower(), key);
			}
			else
			{
				con.PrintLine(bind.ToLower() + " is not a valid bind.\nListing valid binds:", ConsoleLogType.Error);
				ListDefaults(con);
			}
		});
		Leaf("list_binds", delegate
		{
			con.PrintLine("Listing binds:");
			foreach (KeyValuePair<string, InputActionState> registeredBind in con.binds.registeredBinds)
			{
				con.PrintLine(registeredBind.Key + "  -  " + registeredBind.Value.Action.bindings.First().path);
			}
		});
		Leaf("reset", delegate
		{
			MonoSingleton<Console>.Instance.consoleWindow.ResetWindow();
		});
	}

	private void ListDefaults(Console con)
	{
		foreach (KeyValuePair<string, string> defaultBind in con.binds.defaultBinds)
		{
			con.PrintLine(defaultBind.Key + "  -  " + defaultBind.Value);
		}
	}
}
