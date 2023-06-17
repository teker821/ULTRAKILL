using System.Collections.Generic;
using System.Linq;

public static class UnlockConditionExtensions
{
	public static bool AllMet(this List<UnlockCondition> list)
	{
		return list.Aggregate(seed: true, (bool acc, UnlockCondition cond) => acc && cond.conditionMet);
	}

	public static string DescribeAll(this List<UnlockCondition> list)
	{
		if (list.Count == 0)
		{
			return "";
		}
		if (list.Count == 1)
		{
			return list.First().description;
		}
		return list.Skip(1).Aggregate(list.First().description, (string desc, UnlockCondition cond) => desc + ", " + cond.description);
	}
}
