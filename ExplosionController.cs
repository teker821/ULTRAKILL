using UnityEngine;

public class ExplosionController : MonoBehaviour
{
	public GameObject[] toActivate;

	public string playerPref;

	private void Start()
	{
		string text = playerPref;
		if (!(text == "SimFir"))
		{
			if (text == "SimExp")
			{
				playerPref = "simpleExplosions";
			}
		}
		else
		{
			playerPref = "simpleFire";
		}
		if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal(playerPref))
		{
			return;
		}
		GameObject[] array = toActivate;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				gameObject.SetActive(value: true);
			}
		}
	}
}
