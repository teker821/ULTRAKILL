using System.Collections.Generic;

namespace GameConsole.Commands;

public class Help : ICommand
{
	public string Name => "Help";

	public string Description => "Helps you with things, does helpful things, lists things maybe??? Just a helpful pal.";

	public string Command => "help";

	public void Execute(Console con, string[] args)
	{
		if (args.Length != 0)
		{
			if (con.recognizedCommands.ContainsKey(args[0].ToLower()))
			{
				con.PrintLine("<b>" + args[0].ToLower() + "</b> - " + con.recognizedCommands[args[0].ToLower()].Description);
			}
			else
			{
				con.PrintLine("Command not found.");
			}
			return;
		}
		con.PrintLine("Listing recognized commands:");
		foreach (KeyValuePair<string, ICommand> recognizedCommand in con.recognizedCommands)
		{
			con.PrintLine("<b>" + recognizedCommand.Key + "</b> - " + recognizedCommand.Value.Description);
		}
	}
}
