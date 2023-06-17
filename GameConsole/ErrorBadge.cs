using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameConsole;

public class ErrorBadge : MonoBehaviour
{
	[SerializeField]
	private GameObject badgeContainer;

	[SerializeField]
	private Text errorCountText;

	[SerializeField]
	private CanvasGroup flashGroup;

	[SerializeField]
	private CanvasGroup alertGroup;

	public bool hidden;

	private void OnEnable()
	{
		Console instance = MonoSingleton<Console>.Instance;
		instance.onError = (Action)Delegate.Combine(instance.onError, new Action(OnError));
	}

	private void OnDisable()
	{
		if (MonoSingleton<Console>.Instance != null)
		{
			Console instance = MonoSingleton<Console>.Instance;
			instance.onError = (Action)Delegate.Remove(instance.onError, new Action(OnError));
		}
	}

	private void OnError()
	{
		badgeContainer.SetActive(!hidden);
		Update();
		flashGroup.alpha = 0f;
		StopAllCoroutines();
		if (!hidden)
		{
			StartCoroutine(FlashBadge());
		}
	}

	private IEnumerator FlashBadge()
	{
		flashGroup.alpha = 0f;
		while (flashGroup.alpha < 1f)
		{
			flashGroup.alpha += 0.2f;
			if (alertGroup.alpha < flashGroup.alpha)
			{
				alertGroup.alpha = (Console.IsOpen ? 0f : flashGroup.alpha);
			}
			yield return new WaitForSecondsRealtime(0.03f);
		}
		flashGroup.alpha = 1f;
		alertGroup.alpha = (Console.IsOpen ? 0f : flashGroup.alpha);
		while (flashGroup.alpha > 0f)
		{
			flashGroup.alpha -= 0.1f;
			yield return new WaitForSecondsRealtime(0.03f);
		}
		flashGroup.alpha = 0f;
	}

	public void SetEnabled(bool enabled, bool hide = true)
	{
		hidden = !enabled;
		if (enabled)
		{
			if (MonoSingleton<Console>.Instance.errorCount > 0)
			{
				badgeContainer.SetActive(value: true);
				Update();
			}
		}
		else
		{
			badgeContainer.SetActive(value: false);
		}
	}

	public void Dismiss()
	{
		StopAllCoroutines();
		alertGroup.alpha = 0f;
	}

	private void Update()
	{
		if ((bool)MonoSingleton<Console>.Instance)
		{
			int errorCount = MonoSingleton<Console>.Instance.errorCount;
			errorCountText.text = string.Format("{0} error{1}", errorCount, (errorCount == 1) ? "" : "s");
		}
	}
}
