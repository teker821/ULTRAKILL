using UnityEngine;

public class ActivateOnSoundEnd : MonoBehaviour
{
	private AudioSource aud;

	private bool hasStarted;

	[SerializeField]
	private UltrakillEvent events;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (aud.isPlaying)
		{
			hasStarted = true;
		}
		if (hasStarted && !aud.isPlaying && aud.time == 0f)
		{
			hasStarted = false;
			events.Invoke();
		}
	}
}
