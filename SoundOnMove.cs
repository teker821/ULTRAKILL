using UnityEngine;

public class SoundOnMove : MonoBehaviour
{
	private AudioSource aud;

	private Rigidbody rb;

	public float minSpeed;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (!aud.isPlaying && rb.velocity.magnitude > minSpeed)
		{
			aud.Play();
		}
		else if (aud.isPlaying && rb.velocity.magnitude <= minSpeed)
		{
			aud.Stop();
		}
	}
}
