using UnityEngine;

public class HudOpenEffect : MonoBehaviour
{
	private RectTransform tran;

	private float originalWidth;

	private float originalHeight;

	private bool gotValues;

	private bool animating;

	public bool skip;

	private void Awake()
	{
		if (!gotValues)
		{
			tran = GetComponent<RectTransform>();
			originalWidth = tran.localScale.x;
			originalHeight = tran.localScale.y;
			gotValues = true;
		}
	}

	private void OnEnable()
	{
		if (!gotValues)
		{
			tran = GetComponent<RectTransform>();
			originalWidth = tran.localScale.x;
			originalHeight = tran.localScale.y;
			gotValues = true;
		}
		if (!skip)
		{
			tran.localScale = new Vector3(0.05f, 0.05f, tran.localScale.z);
			animating = true;
		}
	}

	private void Update()
	{
		if (!animating)
		{
			return;
		}
		float num = tran.localScale.x;
		float num2 = tran.localScale.y;
		if (!skip)
		{
			if (num != originalWidth)
			{
				num = Mathf.MoveTowards(num, originalWidth, Time.unscaledDeltaTime * ((originalWidth - num + 0.1f) * 30f));
			}
			else if (num2 != originalHeight)
			{
				num2 = Mathf.MoveTowards(num2, originalHeight, Time.unscaledDeltaTime * ((originalHeight - num2 + 0.1f) * 30f));
			}
		}
		else
		{
			num = originalWidth;
			num2 = originalHeight;
		}
		tran.localScale = new Vector3(num, num2, tran.localScale.z);
		if (num == originalWidth && num2 == originalHeight)
		{
			animating = false;
		}
	}

	public void Force()
	{
		if (!gotValues)
		{
			tran = GetComponent<RectTransform>();
			originalWidth = tran.localScale.x;
			originalHeight = tran.localScale.y;
			gotValues = true;
		}
	}
}
