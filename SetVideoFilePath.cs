using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class SetVideoFilePath : MonoBehaviour
{
	[SerializeField]
	private string videoName;

	private void OnEnable()
	{
		if (!SceneHelper.IsPlayingCustom)
		{
			GetComponent<VideoPlayer>().url = "file://" + Path.Combine(Application.streamingAssetsPath, "Videos", videoName);
		}
	}
}
