using UnityEngine;

public class SpatialMusic : MonoBehaviour
{
	public float minDistance;

	public float maxDistance;

	private AudioHighPassFilter hiPass;

	private float hiPassDefaultFrequency;

	private AudioSource aud;

	private Transform target;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		hiPass = GetComponent<AudioHighPassFilter>();
		target = MonoSingleton<CameraController>.Instance.transform;
		if ((bool)hiPass)
		{
			hiPassDefaultFrequency = hiPass.cutoffFrequency;
		}
	}

	private void Update()
	{
		float num = (Mathf.Clamp(Vector3.Distance(base.transform.position, target.position), minDistance, maxDistance) - minDistance) / (maxDistance - minDistance);
		aud.spatialBlend = num;
		if ((bool)hiPass)
		{
			hiPass.cutoffFrequency = Mathf.Lerp(0f, hiPassDefaultFrequency, num);
		}
	}
}
