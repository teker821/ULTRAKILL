using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FileDirectoryTree : IDirectoryTree<FileInfo>, IDirectoryTree
{
	public string name { get; private set; }

	public DirectoryInfo realDirectory { get; private set; }

	public IDirectoryTree<FileInfo> parent { get; set; }

	public IEnumerable<IDirectoryTree<FileInfo>> children { get; private set; }

	public IEnumerable<FileInfo> files { get; private set; }

	public FileDirectoryTree(string path, IDirectoryTree<FileInfo> parent = null)
	{
		realDirectory = new DirectoryInfo(path);
		this.parent = parent;
		Refresh();
	}

	public FileDirectoryTree(DirectoryInfo realDirectory, IDirectoryTree<FileInfo> parent = null)
	{
		this.realDirectory = realDirectory;
		this.parent = parent;
		Refresh();
	}

	public void Refresh()
	{
		realDirectory.Create();
		name = realDirectory.Name;
		children = from dir in realDirectory.GetDirectories()
			select new FileDirectoryTree(dir, this);
		files = realDirectory.GetFiles();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return realDirectory.FullName.ToUpperInvariant() == (obj as DirectoryInfo).FullName.ToUpperInvariant();
	}

	public override int GetHashCode()
	{
		return realDirectory.GetHashCode();
	}

	public IEnumerable<FileInfo> GetFilesRecursive()
	{
		return children.SelectMany((IDirectoryTree<FileInfo> child) => child.GetFilesRecursive()).Concat(files);
	}
}
