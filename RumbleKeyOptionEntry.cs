using UnityEngine;
using UnityEngine.UI;

public class RumbleKeyOptionEntry : MonoBehaviour
{
	public string key;

	public Text keyName;

	public Slider intensitySlider;

	public Slider durationSlider;

	public Button intensityWrapper;

	public Button durationWrapper;

	public GameObject durationContainer;

	public void ResetIntensity()
	{
		intensitySlider.SetValueWithoutNotify(MonoSingleton<RumbleManager>.Instance.ResolveDefaultIntensity(key));
		MonoSingleton<PrefsManager>.Instance.DeleteKey(key + ".intensity");
	}

	public void ResetDuration()
	{
		durationSlider.SetValueWithoutNotify(MonoSingleton<RumbleManager>.Instance.ResolveDefaultDuration(key));
		MonoSingleton<PrefsManager>.Instance.DeleteKey(key + ".duration");
	}

	public void SetIntensity(float value)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat(key + ".intensity", value);
	}

	public void SetDuration(float value)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat(key + ".duration", value);
	}
}
