using UnityEngine;

public class PlayerLoadoutTarget : MonoBehaviour
{
	public void CommitLoadout(ForcedLoadout loadout)
	{
		Debug.Log($"Setting loadout on {loadout}");
		MonoSingleton<GunSetter>.Instance.forcedLoadout = loadout;
		MonoSingleton<GunSetter>.Instance.ResetWeapons();
		MonoSingleton<FistControl>.Instance.forcedLoadout = loadout;
		MonoSingleton<FistControl>.Instance.ResetFists();
	}
}
