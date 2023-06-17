namespace GameConsole.Commands;

public class Echo : ICommand
{
	public string Name => "Echo";

	public string Description => "Echo the given text";

	public string Command => "echo";

	public void Execute(Console con, string[] args)
	{
		con.PrintLine(string.Join(" ", args));
	}
}
