using UnityEngine;

public class PendingVibration
{
	public TimeSince timeSinceStart;

	public string key;

	public float intensityMultiplier = 1f;

	public bool isTracking;

	public GameObject trackedObject;

	public float Duration => MonoSingleton<RumbleManager>.Instance.ResolveDuration(key);

	public float Intensity => Mathf.Clamp01(MonoSingleton<RumbleManager>.Instance.ResolveIntensity(key) * intensityMultiplier);

	public bool IsFinished => (float)timeSinceStart >= Duration;
}
