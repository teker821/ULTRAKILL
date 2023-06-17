using UnityEngine;

public class LightDistanceFade : MonoBehaviour
{
	private Transform player;

	private Light lit;

	private float maxIntensity;

	public float minDistance;

	public float maxDistance;

	private void Start()
	{
		player = MonoSingleton<CameraController>.Instance.transform;
		lit = GetComponent<Light>();
		maxIntensity = lit.intensity;
	}

	private void Update()
	{
		if (!lit || !player)
		{
			return;
		}
		float num = Vector3.Distance(base.transform.position, player.position);
		if (num >= maxDistance)
		{
			lit.enabled = false;
			return;
		}
		lit.enabled = true;
		if (num <= minDistance)
		{
			lit.intensity = maxIntensity;
			return;
		}
		float f = maxDistance - minDistance;
		float f2 = num - minDistance;
		lit.intensity = Mathf.Pow((Mathf.Sqrt(f) - Mathf.Sqrt(f2)) / Mathf.Sqrt(f), 2f) * maxIntensity;
	}
}
