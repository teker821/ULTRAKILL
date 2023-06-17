using UnityEngine;
using UnityEngine.InputSystem;

public class ActivateOnController : MonoBehaviour
{
	public bool oneTime;

	private bool doneOnce;

	public UltrakillEvent onController;

	public UltrakillEvent onNoController;

	private bool activated;

	private void Start()
	{
		Check();
	}

	private void Update()
	{
		Check();
	}

	private void Check()
	{
		if (!oneTime || !doneOnce)
		{
			if ((!doneOnce || activated) && (Gamepad.current == null || !Gamepad.current.enabled))
			{
				activated = false;
				onController?.Revert();
				onNoController?.Invoke();
			}
			else if (!activated && Gamepad.current != null && Gamepad.current.enabled)
			{
				activated = true;
				onController?.Invoke();
				onNoController?.Revert();
			}
			doneOnce = true;
		}
	}
}
