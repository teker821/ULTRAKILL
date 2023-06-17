using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class DebugUI : MonoSingleton<DebugUI>
{
	public static bool previewRumble;

	[SerializeField]
	private GameObject rumbleContainer;

	[SerializeField]
	private RectTransform rumbleIconTransform;

	[SerializeField]
	private Image rumbleImage;

	[SerializeField]
	private Slider rumbleIntensityBar;

	private void Update()
	{
		if (!previewRumble)
		{
			rumbleContainer.gameObject.SetActive(value: false);
			return;
		}
		float currentIntensity = MonoSingleton<RumbleManager>.Instance.currentIntensity;
		rumbleContainer.gameObject.SetActive(value: true);
		float x = Mathf.Sin(Time.time * 100f) * (currentIntensity * 6f);
		rumbleIconTransform.anchoredPosition = new Vector2(x, rumbleIconTransform.anchoredPosition.y);
		rumbleImage.color = new Color(1f, 1f, 1f, (currentIntensity == 0f) ? 0.3f : 1f);
		rumbleIntensityBar.gameObject.SetActive(currentIntensity > 0f);
		rumbleIntensityBar.value = currentIntensity;
	}
}
