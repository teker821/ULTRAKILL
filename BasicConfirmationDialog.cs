using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BasicConfirmationDialog : MonoBehaviour
{
	[SerializeField]
	private GameObject blocker;

	[SerializeField]
	private UnityEvent onConfirm;

	public void ShowDialog()
	{
		base.gameObject.SetActive(value: true);
		blocker.SetActive(value: true);
	}

	private void Update()
	{
		if (MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame))
		{
			Cancel();
		}
	}

	public void Confirm()
	{
		base.gameObject.SetActive(value: false);
		blocker.SetActive(value: false);
		onConfirm.Invoke();
	}

	public void Cancel()
	{
		base.gameObject.SetActive(value: false);
		blocker.SetActive(value: false);
	}
}
