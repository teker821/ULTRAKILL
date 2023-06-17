using UnityEngine;
using UnityEngine.Events;

public class PlayerInputHooks : MonoBehaviour
{
	[Header("Fire1")]
	[SerializeField]
	private UnityEvent onFire1Pressed;

	[SerializeField]
	private UnityEvent onFire1Released;

	[Space]
	[Header("Fire2")]
	[SerializeField]
	private UnityEvent onFire2Pressed;

	[SerializeField]
	private UnityEvent onFire2Released;

	[Space]
	[Header("Slide")]
	[SerializeField]
	private UnityEvent onSlideInputStart;

	[SerializeField]
	private UnityEvent onSlideInputEnd;

	[Space]
	[Header("Jump")]
	[SerializeField]
	private UnityEvent onJumpPressed;

	[SerializeField]
	private UnityEvent onJumpReleased;

	[Header("Dash")]
	[SerializeField]
	private UnityEvent onDashPressed;

	[SerializeField]
	private UnityEvent onDashReleased;

	private void Update()
	{
		if (!MonoSingleton<OptionsManager>.Instance || !MonoSingleton<OptionsManager>.Instance.paused)
		{
			if (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame)
			{
				onFire1Pressed.Invoke();
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasCanceledThisFrame)
			{
				onFire1Released.Invoke();
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasPerformedThisFrame)
			{
				onFire2Pressed.Invoke();
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Fire2.WasCanceledThisFrame)
			{
				onFire2Released.Invoke();
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Slide.WasPerformedThisFrame)
			{
				onSlideInputStart.Invoke();
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Slide.WasCanceledThisFrame)
			{
				onSlideInputEnd.Invoke();
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Jump.WasPerformedThisFrame)
			{
				onJumpPressed.Invoke();
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Jump.WasCanceledThisFrame)
			{
				onJumpReleased.Invoke();
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Dodge.WasPerformedThisFrame)
			{
				onDashPressed.Invoke();
			}
			if (MonoSingleton<InputManager>.Instance.InputSource.Dodge.WasCanceledThisFrame)
			{
				onDashReleased.Invoke();
			}
		}
	}
}
