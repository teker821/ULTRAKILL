using UnityEngine;

public class SandboxIconSwitcher : MonoBehaviour
{
	public int iconPack;

	public void SwitchIconPack()
	{
		MonoSingleton<IconManager>.Instance.SetIconPack(iconPack);
		MonoSingleton<IconManager>.Instance.Reload();
	}
}
