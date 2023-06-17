using GameConsole;
using UnityEngine;
using UnityEngine.UI;

public class PinConsole : MonoBehaviour
{
	private Image image;

	private bool pinned;

	private void Awake()
	{
		image = GetComponent<Image>();
	}

	public void TogglePin()
	{
		pinned = !pinned;
		MonoSingleton<Console>.Instance.pinned = pinned;
		image.color = (pinned ? Color.red : Color.white);
	}
}
