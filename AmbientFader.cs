using UnityEngine;

public class AmbientFader : MonoBehaviour
{
	private float fadeAmount = 1f;

	public float target = 1f;

	public float time;

	private bool activated;

	public bool onEnable;

	private void Start()
	{
		fadeAmount = Shader.GetGlobalFloat("_AmbientStrength");
		target = fadeAmount;
		if (onEnable)
		{
			activated = true;
		}
	}

	private void Update()
	{
		if (activated)
		{
			fadeAmount = Mathf.MoveTowards(fadeAmount, target, 1f / time * Time.deltaTime);
			Shader.SetGlobalFloat("_AmbientStrength", fadeAmount);
			if (fadeAmount == target)
			{
				activated = false;
			}
		}
	}

	public void FadeTo(float newTarget)
	{
		target = newTarget;
		if (time == 0f)
		{
			fadeAmount = target;
			Shader.SetGlobalFloat("_AmbientStrength", fadeAmount);
		}
		else
		{
			activated = true;
		}
	}
}
