using UnityEngine;
using UnityEngine.AddressableAssets;

public class Bootstrap : MonoBehaviour
{
	private void Start()
	{
		Debug.Log(Addressables.RuntimePath);
		if (!GameProgressSaver.GetTutorial() || !GameProgressSaver.GetIntro())
		{
			MonoSingleton<PrefsManager>.Instance.SetInt("weapon.arm0", 1);
			SceneHelper.LoadScene("Tutorial", noBlocker: true);
		}
		else
		{
			SceneHelper.LoadScene("Intro", noBlocker: true);
		}
	}
}
