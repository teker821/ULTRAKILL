using UnityEngine;

public class FadeOutBars : MonoBehaviour
{
	private bool fadeOut;

	public float fadeOutTime;

	private SliderToFillAmount[] slids;

	private void Start()
	{
		CheckState();
		slids = GetComponentsInChildren<SliderToFillAmount>();
		SliderToFillAmount[] array = slids;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].mama = this;
		}
	}

	private void Update()
	{
		if (fadeOut)
		{
			fadeOutTime = Mathf.MoveTowards(fadeOutTime, 0f, Time.unscaledDeltaTime);
		}
	}

	public void CheckState()
	{
		fadeOut = MonoSingleton<PrefsManager>.Instance.GetBool("crossHairHudFade");
		fadeOutTime = 2f;
	}
}
