namespace GameConsole.Commands;

public class Clear : ICommand
{
	public string Name => "Clear";

	public string Description => "Clears the console.";

	public string Command => "clear";

	public void Execute(Console con, string[] args)
	{
		con.Clear();
	}
}
