using UnityEngine;

public class DeletePlayerPref : MonoBehaviour
{
	public string playerPref;

	private void Start()
	{
		string text = playerPref;
		if (text == "cg_custom_pool")
		{
			playerPref = "cyberGrind.customPool";
		}
		MonoSingleton<PrefsManager>.Instance.DeleteKey(playerPref);
	}
}
