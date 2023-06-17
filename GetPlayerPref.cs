using UnityEngine;
using UnityEngine.Events;

public class GetPlayerPref : MonoBehaviour
{
	public string pref;

	public int valueToCheckFor;

	public UnityEvent onCheckSuccess;

	public UnityEvent onCheckFail;

	private void Awake()
	{
		string text = pref;
		if (!(text == "DisCha"))
		{
			if (text == "ShoUseTut")
			{
				pref = "hideShotgunPopup";
				if (MonoSingleton<PrefsManager>.Instance.GetBool(pref))
				{
					onCheckSuccess?.Invoke();
				}
				else
				{
					onCheckFail?.Invoke();
				}
			}
			else
			{
				pref = "weapon." + pref;
				if (MonoSingleton<PrefsManager>.Instance.GetInt(pref, 1) == valueToCheckFor)
				{
					onCheckSuccess?.Invoke();
				}
				else
				{
					onCheckFail?.Invoke();
				}
			}
		}
		else if (PlayerPrefs.GetInt(pref, 0) == valueToCheckFor)
		{
			onCheckSuccess?.Invoke();
		}
		else
		{
			onCheckFail?.Invoke();
		}
	}
}
