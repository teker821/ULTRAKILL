using UnityEngine;
using UnityEngine.UI;

public class RumbleSettingsMenu : MonoBehaviour
{
	[SerializeField]
	private RumbleKeyOptionEntry optionTemplate;

	[SerializeField]
	private Transform container;

	[SerializeField]
	private Button totalWrapper;

	[SerializeField]
	private Button quitButton;

	[SerializeField]
	private Slider totalSlider;

	private void Start()
	{
		optionTemplate.gameObject.SetActive(value: false);
		Button button = totalWrapper;
		string[] all = RumbleProperties.All;
		foreach (string key in all)
		{
			RumbleKeyOptionEntry option = Object.Instantiate(optionTemplate, container);
			option.gameObject.SetActive(value: true);
			option.key = key;
			option.keyName.text = MonoSingleton<RumbleManager>.Instance.ResolveFullName(key);
			float num = MonoSingleton<RumbleManager>.Instance.ResolveDuration(key);
			if (num >= float.PositiveInfinity)
			{
				option.durationContainer.gameObject.SetActive(value: false);
			}
			else
			{
				option.durationSlider.SetValueWithoutNotify(num);
				option.durationSlider.onValueChanged.AddListener(delegate(float value)
				{
					option.SetDuration(value);
				});
			}
			option.intensitySlider.SetValueWithoutNotify(MonoSingleton<RumbleManager>.Instance.ResolveIntensity(key));
			option.intensitySlider.onValueChanged.AddListener(delegate(float value)
			{
				option.SetIntensity(value);
			});
			Navigation navigation = option.intensityWrapper.navigation;
			navigation.selectOnUp = button;
			option.intensityWrapper.navigation = navigation;
			navigation = button.navigation;
			navigation.selectOnDown = option.intensityWrapper;
			button.navigation = navigation;
			button = (option.durationContainer.gameObject.activeSelf ? option.durationWrapper : option.intensityWrapper);
			if (button == null)
			{
				Debug.LogError("Previous button is null");
			}
		}
		Navigation navigation2 = button.navigation;
		navigation2.selectOnDown = quitButton;
		button.navigation = navigation2;
		Navigation navigation3 = quitButton.navigation;
		navigation3.selectOnUp = button;
		quitButton.navigation = navigation3;
		totalSlider.SetValueWithoutNotify(MonoSingleton<PrefsManager>.Instance.GetFloat("totalRumbleIntensity"));
	}

	public void ChangeMasterMulti(float value)
	{
		MonoSingleton<PrefsManager>.Instance.SetFloat("totalRumbleIntensity", value);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}
}
