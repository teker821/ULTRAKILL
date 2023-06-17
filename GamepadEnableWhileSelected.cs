using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

internal sealed class GamepadEnableWhileSelected : MonoBehaviour
{
	public GameObject[] GameObjects;

	public GameObject[] Disable;

	public bool DisableWhenDeselected;

	private void Update()
	{
		if (!(MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad))
		{
			return;
		}
		if (EventSystem.current.currentSelectedGameObject == base.gameObject)
		{
			if (GameObjects != null)
			{
				GameObject[] gameObjects = GameObjects;
				for (int i = 0; i < gameObjects.Length; i++)
				{
					gameObjects[i].SetActive(value: true);
				}
			}
			if (Disable != null)
			{
				GameObject[] gameObjects = Disable;
				for (int i = 0; i < gameObjects.Length; i++)
				{
					gameObjects[i]?.SetActive(value: false);
				}
			}
		}
		else if (DisableWhenDeselected && GameObjects != null)
		{
			GameObject[] gameObjects = GameObjects;
			for (int i = 0; i < gameObjects.Length; i++)
			{
				gameObjects[i].SetActive(value: false);
			}
		}
	}
}
