using UnityEngine;

public class GameEnd : MonoBehaviour
{
	public AudioSource aud;

	private AudioSource endingSong;

	private void Awake()
	{
		endingSong = GameObject.FindWithTag("EndingSong").GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (endingSong.volume < 1f)
		{
			endingSong.volume += Time.deltaTime / 2f;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			endingSong.volume = 0f;
			aud.volume = 0f;
			Invoke("EndGame", 0.1f);
		}
	}

	private void EndGame()
	{
		Application.Quit();
	}
}
