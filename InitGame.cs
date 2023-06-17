using UnityEngine;

public class InitGame : MonoBehaviour
{
	public static bool hasInitialized;

	private void Awake()
	{
		if (!hasInitialized)
		{
			int intLocal = MonoSingleton<PrefsManager>.Instance.GetIntLocal("resolutionWidth", -1);
			int intLocal2 = MonoSingleton<PrefsManager>.Instance.GetIntLocal("resolutionHeight", -1);
			bool boolLocal = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("fullscreen");
			if (intLocal != -1 && intLocal2 != -1)
			{
				Screen.SetResolution(intLocal, intLocal2, boolLocal);
			}
			else
			{
				Resolution currentResolution = Screen.currentResolution;
				Screen.SetResolution(currentResolution.width, currentResolution.height, boolLocal);
			}
			hasInitialized = true;
		}
	}
}
