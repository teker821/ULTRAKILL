using System.Globalization;
using UnityEngine;

public struct TimeSince
{
	private float time;

	public static implicit operator float(TimeSince ts)
	{
		return Time.time - ts.time;
	}

	public static implicit operator TimeSince(float ts)
	{
		TimeSince result = default(TimeSince);
		result.time = Time.time - ts;
		return result;
	}

	public new string ToString()
	{
		return ((float)this).ToString(CultureInfo.InvariantCulture);
	}
}
