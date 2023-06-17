using UnityEngine;

public class WebButton : MonoBehaviour
{
	public string url;

	public void OpenURL()
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			Application.OpenURL(url);
		}
	}
}
