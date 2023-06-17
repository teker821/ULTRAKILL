using UnityEngine;

public class UnlockMouse : MonoBehaviour
{
	public bool unlockOnEnable = true;

	public bool lockOnDisable;

	private bool wasEnabled;

	private void OnEnable()
	{
		if (unlockOnEnable)
		{
			Unlock();
		}
	}

	public void Unlock()
	{
		GameStateManager.Instance.RegisterState(new GameState("unlock-mouse-component", base.gameObject)
		{
			cursorLock = LockMode.Unlock
		});
		wasEnabled = true;
	}

	private void OnDisable()
	{
		if (lockOnDisable && wasEnabled && base.gameObject.scene.isLoaded)
		{
			Lock();
		}
	}

	public void Lock()
	{
		if (wasEnabled)
		{
			wasEnabled = false;
			GameStateManager.Instance.PopState("unlock-mouse-component");
		}
	}

	private void OnApplicationQuit()
	{
		lockOnDisable = false;
	}
}
