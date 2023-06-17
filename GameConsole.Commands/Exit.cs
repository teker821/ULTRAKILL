using UnityEngine;

namespace GameConsole.Commands;

public class Exit : ICommand
{
	public string Name => "Exit";

	public string Description => "Quits the game.";

	public string Command => Name.ToLower();

	public void Execute(Console con, string[] args)
	{
		con.PrintLine("Goodbye \ud83d\udc4b");
		Application.Quit();
	}
}
