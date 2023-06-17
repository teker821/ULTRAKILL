using System;

namespace Discord;

public class ResultException : Exception
{
	public readonly Result Result;

	public ResultException(Result result)
		: base(result.ToString())
	{
	}
}
