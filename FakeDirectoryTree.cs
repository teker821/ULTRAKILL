using System.Collections.Generic;
using System.Linq;

public class FakeDirectoryTree<T> : IDirectoryTree<T>, IDirectoryTree
{
	public string name { get; private set; }

	public IEnumerable<IDirectoryTree<T>> children { get; private set; }

	public IEnumerable<T> files { get; private set; }

	public IDirectoryTree<T> parent { get; set; }

	public FakeDirectoryTree(string name, IEnumerable<T> files = null, IEnumerable<IDirectoryTree<T>> children = null, IDirectoryTree<T> parent = null)
	{
		this.name = name;
		this.children = children ?? new List<IDirectoryTree<T>>();
		this.files = files ?? new List<T>();
		this.parent = parent;
	}

	public FakeDirectoryTree(string name, List<T> files = null, List<FakeDirectoryTree<T>> children = null, IDirectoryTree<T> parent = null)
	{
		this.name = name;
		this.children = children ?? new List<FakeDirectoryTree<T>>();
		this.files = files ?? new List<T>();
		this.parent = parent;
		foreach (IDirectoryTree<T> child in this.children)
		{
			child.parent = this;
		}
	}

	public void Refresh()
	{
	}

	public IEnumerable<T> GetFilesRecursive()
	{
		return children.SelectMany((IDirectoryTree<T> child) => child.GetFilesRecursive()).Concat(files);
	}
}
