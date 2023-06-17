using UnityEngine;
using UnityEngine.UI;

public class AspectRatioChanger : MonoBehaviour
{
	private AspectRatioFitter arf;

	public float targetRatio = 1.7778f;

	public float speed = 1f;

	private void Start()
	{
		arf = GetComponent<AspectRatioFitter>();
		if (targetRatio == 0f)
		{
			targetRatio = arf.aspectRatio;
		}
	}

	private void Update()
	{
		if ((bool)arf && arf.aspectRatio != targetRatio)
		{
			arf.aspectRatio = Mathf.MoveTowards(arf.aspectRatio, targetRatio, Time.deltaTime * speed);
		}
	}

	public void ChangeRatio(float ratio)
	{
		targetRatio = ratio;
	}

	public void ChangeSpeed(float newSpeed)
	{
		speed = newSpeed;
	}
}
