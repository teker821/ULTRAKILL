using UnityEngine;

public class DiscChange : MonoBehaviour
{
	public string targetLevelName;

	private bool ready;

	private bool done;

	[SerializeField]
	private GameObject discSound;

	private void Update()
	{
		if (ready && !done)
		{
			done = true;
			PlayerPrefs.SetInt("DisCha", 1);
			discSound.transform.parent = null;
			Object.DontDestroyOnLoad(discSound);
			if (discSound.TryGetComponent<FadeOut>(out var component))
			{
				component.BeginFade();
			}
			SceneHelper.LoadScene(targetLevelName);
		}
	}

	public void ReadyToChangeLevel()
	{
		ready = true;
	}
}
