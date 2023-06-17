using UnityEngine;

public static class PrefsManagerHelper
{
	public static void SetPref(string key, object value, bool noSteamCloud)
	{
		if (value is float content)
		{
			if (noSteamCloud)
			{
				MonoSingleton<PrefsManager>.Instance.SetFloatLocal(key, content);
			}
			else
			{
				MonoSingleton<PrefsManager>.Instance.SetFloat(key, content);
			}
		}
		else if (value is int content2)
		{
			if (noSteamCloud)
			{
				MonoSingleton<PrefsManager>.Instance.SetIntLocal(key, content2);
			}
			else
			{
				MonoSingleton<PrefsManager>.Instance.SetInt(key, content2);
			}
		}
		else if (value is string content3)
		{
			if (noSteamCloud)
			{
				MonoSingleton<PrefsManager>.Instance.SetStringLocal(key, content3);
			}
			else
			{
				MonoSingleton<PrefsManager>.Instance.SetString(key, content3);
			}
		}
		else if (value is bool content4)
		{
			if (noSteamCloud)
			{
				MonoSingleton<PrefsManager>.Instance.SetBoolLocal(key, content4);
			}
			else
			{
				MonoSingleton<PrefsManager>.Instance.SetBool(key, content4);
			}
		}
		else
		{
			Debug.LogError("Unsupported type for PrefsManagerHelper.SetPref");
		}
	}
}
