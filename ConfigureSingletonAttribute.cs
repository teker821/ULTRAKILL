using System;
using JetBrains.Annotations;

[BaseTypeRequired(typeof(MonoSingleton<>))]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ConfigureSingletonAttribute : Attribute
{
	public SingletonFlags Flags { get; }

	public ConfigureSingletonAttribute(SingletonFlags flags)
	{
		Flags = flags;
	}
}
