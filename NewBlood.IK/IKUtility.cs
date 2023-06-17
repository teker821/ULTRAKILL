using UnityEngine;

namespace NewBlood.IK;

internal static class IKUtility
{
	public static bool IsDescendantOf(Transform transform, Transform ancestor)
	{
		transform = transform.parent;
		while (transform != null)
		{
			if (transform == ancestor)
			{
				return true;
			}
			transform = transform.parent;
		}
		return false;
	}

	public static bool IsDescendantOf(Transform transform, Transform ancestor, int ancestorCount)
	{
		transform = transform.parent;
		for (int i = 0; i < ancestorCount; i++)
		{
			if (transform == null)
			{
				break;
			}
			if (transform == ancestor)
			{
				return true;
			}
			transform = transform.parent;
		}
		return false;
	}

	public static bool AncestorCountAtLeast(Transform transform, int count)
	{
		for (int i = 0; i < count; i++)
		{
			if (transform.parent == null)
			{
				return false;
			}
			transform = transform.parent;
		}
		return true;
	}

	public static int GetAncestorCount(Transform transform)
	{
		int num = 0;
		while (transform.parent != null)
		{
			num++;
			transform = transform.parent;
		}
		return num;
	}

	public static int GetMaxChainCount(IKChain3D chain)
	{
		if (chain.effector != null)
		{
			return GetAncestorCount(chain.effector) + 1;
		}
		return 0;
	}
}
