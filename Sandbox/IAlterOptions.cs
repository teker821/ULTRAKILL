namespace Sandbox;

public interface IAlterOptions<T>
{
	AlterOption<T>[] options { get; }
}
