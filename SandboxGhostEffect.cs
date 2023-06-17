using UnityEngine;

public class SandboxGhostEffect : MonoBehaviour
{
	public Collider targetCollider;

	private const float scaleMulti = 1.3f;

	private const float duration = 0.2f;

	private Vector3 originalScale;

	private TimeSince timeSinceStart;

	private void Awake()
	{
		originalScale = base.transform.localScale;
		base.transform.localScale *= 1.3f;
		timeSinceStart = 0f;
	}

	private void Update()
	{
		base.transform.localScale = Vector3.Lerp(originalScale * 1.3f, originalScale, (float)timeSinceStart / 0.2f);
		if ((float)timeSinceStart >= 0.2f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
