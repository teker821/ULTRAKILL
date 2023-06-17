using UnityEngine;
using UnityEngine.Serialization;

public class PlayerLoadout : MonoBehaviour, IPlaceholdableComponent
{
	[FormerlySerializedAs("forceLoadout")]
	public bool forceStartLoadout;

	public ForcedLoadout loadout;

	public void WillReplace(GameObject oldObject, GameObject newObject, bool isSelfBeingReplaced)
	{
		if (isSelfBeingReplaced && forceStartLoadout)
		{
			PlayerLoadoutTarget component = newObject.GetComponent<PlayerLoadoutTarget>();
			if ((bool)component)
			{
				component.CommitLoadout(loadout);
			}
		}
	}

	public void SetLoadout()
	{
		MonoSingleton<GunSetter>.Instance.forcedLoadout = loadout;
		MonoSingleton<GunSetter>.Instance.ResetWeapons();
		MonoSingleton<FistControl>.Instance.forcedLoadout = loadout;
		MonoSingleton<FistControl>.Instance.ResetFists();
	}
}
