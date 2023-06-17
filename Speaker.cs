using UnityEngine;

public class Speaker : MonoBehaviour
{
	public AudioClip[] sounds;

	private void Start()
	{
		AudioSource component = GetComponent<AudioSource>();
		component.clip = sounds[Random.Range(0, sounds.Length)];
		component.Play();
		component.time = Random.Range(0f, component.clip.length);
	}
}
