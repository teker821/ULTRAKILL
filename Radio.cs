using UnityEngine;

public class Radio : MonoBehaviour
{
	public AudioClip[] songs;

	private AudioSource aud;

	private int currentSong;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		currentSong = Random.Range(0, songs.Length);
		aud.clip = songs[currentSong];
		aud.Play();
		aud.time = Random.Range(0f, aud.clip.length);
	}

	private void Update()
	{
		if (aud.time >= aud.clip.length - 0.01f)
		{
			NextSong();
		}
	}

	public void NextSong()
	{
		currentSong++;
		if (currentSong >= songs.Length)
		{
			currentSong = 0;
		}
		aud.clip = songs[currentSong];
		aud.time = 0f;
		aud.Play();
	}
}
