using UnityEngine;
using UnityEngine.UI;

public class MenuAlphaPop : MonoBehaviour
{
	[SerializeField]
	private bool animateOnEnable = true;

	[SerializeField]
	private bool canvasGroupInsteadOfImage;

	[SerializeField]
	private float initialAlpha = 1f;

	[SerializeField]
	private float finalAlpha;

	[SerializeField]
	private float animationDuration = 1f;

	private Image targetImage;

	private CanvasGroup targetGroup;

	private bool isAnimating;

	private float progress;

	private void Awake()
	{
		targetImage = GetComponent<Image>();
		targetGroup = GetComponent<CanvasGroup>();
	}

	private void OnEnable()
	{
		if (animateOnEnable)
		{
			Animate();
		}
	}

	private void Update()
	{
		if (isAnimating)
		{
			progress += Time.deltaTime / animationDuration;
			if (progress >= 1f)
			{
				isAnimating = false;
			}
			if (canvasGroupInsteadOfImage)
			{
				targetGroup.alpha = Mathf.Lerp(initialAlpha, finalAlpha, progress);
				return;
			}
			Color color = targetImage.color;
			color.a = Mathf.Lerp(initialAlpha, finalAlpha, progress);
			targetImage.color = color;
		}
	}

	public void Animate()
	{
		isAnimating = true;
		progress = 0f;
	}
}
