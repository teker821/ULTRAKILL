using UnityEngine;
using UnityEngine.Rendering;

public class GLCoreBandaid : MonoBehaviour
{
	public GameObject optionsToHide;

	public GameObject dialogToShow;

	private void OnEnable()
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore)
		{
			optionsToHide.SetActive(value: false);
			dialogToShow.SetActive(value: true);
		}
		else
		{
			optionsToHide.SetActive(value: true);
			dialogToShow.SetActive(value: false);
		}
	}
}
