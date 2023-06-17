using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Subtitle : MonoBehaviour
{
	public AudioSource distanceCheckObject;

	public Subtitle nextInChain;

	[SerializeField]
	private float fadeInSpeed = 0.001f;

	[SerializeField]
	private float holdForBase = 2f;

	[SerializeField]
	private float holdForPerChar = 0.1f;

	[SerializeField]
	private float fadeOutSpeed = 0.0001f;

	[SerializeField]
	private float paddingHorizontal;

	[SerializeField]
	private Text uiText;

	private CanvasGroup group;

	private float currentAlpha;

	private bool isFadingIn;

	private bool chainContinue;

	private float holdFor;

	private bool isFadingOut;

	private TimeSince holdingSince;

	private RectTransform rectTransform;

	private float baseAlpha = 1f;

	private void Awake()
	{
		group = GetComponent<CanvasGroup>();
		rectTransform = GetComponent<RectTransform>();
	}

	private void OnEnable()
	{
		Fit();
		string text = new Regex("<[^>]*>").Replace(uiText.text, "");
		holdFor = holdForBase + (float)text.Length * holdForPerChar;
		currentAlpha = 0f;
		isFadingIn = true;
	}

	public void ContinueChain()
	{
		chainContinue = true;
	}

	private void Update()
	{
		if (distanceCheckObject == null)
		{
			baseAlpha = 1f;
		}
		else
		{
			float num = Vector3.Distance(MonoSingleton<CameraController>.Instance.transform.position, distanceCheckObject.transform.position);
			float num2 = distanceCheckObject.minDistance + (distanceCheckObject.maxDistance - distanceCheckObject.minDistance) * 0.5f;
			if (num <= num2)
			{
				baseAlpha = 1f;
			}
			else
			{
				float num3 = num - num2;
				float num4 = distanceCheckObject.maxDistance - num2;
				float num5 = Mathf.Clamp01(num3 / num4);
				switch (distanceCheckObject.rolloffMode)
				{
				case AudioRolloffMode.Custom:
					baseAlpha = distanceCheckObject.GetCustomCurve(AudioSourceCurveType.CustomRolloff).Evaluate(num5);
					break;
				case AudioRolloffMode.Linear:
					baseAlpha = 1f - num5;
					break;
				case AudioRolloffMode.Logarithmic:
					baseAlpha = 1f - Mathf.Clamp01(Mathf.Log10(num3) / Mathf.Log10(num4));
					break;
				}
			}
		}
		if (isFadingIn)
		{
			currentAlpha += fadeInSpeed * Time.deltaTime;
			if (currentAlpha >= 1f)
			{
				currentAlpha = 1f;
				isFadingIn = false;
				holdingSince = 0f;
			}
			group.alpha = currentAlpha * baseAlpha;
			return;
		}
		if (isFadingOut)
		{
			currentAlpha -= fadeOutSpeed * Time.deltaTime;
			if (currentAlpha <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
			group.alpha = currentAlpha * baseAlpha;
			return;
		}
		if (distanceCheckObject != null)
		{
			group.alpha = currentAlpha * baseAlpha;
		}
		if ((float)holdingSince > holdFor && chainContinue)
		{
			isFadingOut = true;
			MonoSingleton<SubtitleController>.Instance.NotifyHoldEnd(this);
			if ((bool)nextInChain)
			{
				nextInChain.ContinueChain();
			}
		}
	}

	private void Fit()
	{
		StartCoroutine(FitAsync());
	}

	private IEnumerator FitAsync()
	{
		yield return new WaitForFixedUpdate();
		float preferredSize = LayoutUtility.GetPreferredSize(uiText.rectTransform, 0);
		uiText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize + paddingHorizontal * 2f);
	}
}
