namespace GameConsole;

public interface ICommand
{
	string Name { get; }

	string Description { get; }

	string Command { get; }

	void Execute(Console con, string[] args);
}
