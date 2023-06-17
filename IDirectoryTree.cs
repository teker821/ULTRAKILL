using System.Collections.Generic;

public interface IDirectoryTree
{
}
public interface IDirectoryTree<T> : IDirectoryTree
{
	string name { get; }

	IDirectoryTree<T> parent { get; set; }

	IEnumerable<IDirectoryTree<T>> children { get; }

	IEnumerable<T> files { get; }

	IEnumerable<T> GetFilesRecursive();

	void Refresh();
}
