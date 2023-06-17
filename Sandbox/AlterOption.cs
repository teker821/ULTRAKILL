using System;

namespace Sandbox;

[Serializable]
public class AlterOption<T>
{
	public string name;

	public string key;

	public T value;

	public Action<T> callback;

	public IConstraints constraints;
}
