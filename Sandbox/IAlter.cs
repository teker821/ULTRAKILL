namespace Sandbox;

public interface IAlter
{
	bool allowOnlyOne { get; }

	string alterKey { get; }

	string alterCategoryName { get; }
}
