using UnityEngine;

namespace ULTRAKILL.Cheats;

public class Flight : ICheat
{
	private bool active;

	private Rigidbody rigidbody;

	private Transform camera;

	public string LongName => "Flight";

	public string Identifier => "ultrakill.flight";

	public string ButtonEnabledOverride { get; }

	public string ButtonDisabledOverride { get; }

	public string Icon => "flight";

	public bool IsActive => active;

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable()
	{
		MonoSingleton<CheatsManager>.Instance.DisableCheat("ultrakill.noclip");
		MonoSingleton<CheatsManager>.Instance.DisableCheat("ultrakill.clash-mode");
		MonoSingleton<NewMovement>.Instance.enabled = false;
		rigidbody = MonoSingleton<NewMovement>.Instance.GetComponent<Rigidbody>();
		camera = MonoSingleton<CameraController>.Instance.transform;
		active = true;
	}

	public void Disable()
	{
		active = false;
		MonoSingleton<NewMovement>.Instance.enabled = true;
		rigidbody.useGravity = true;
	}

	public void Update()
	{
		float num = 1f;
		if (MonoSingleton<InputManager>.Instance.InputSource.Dodge.IsPressed)
		{
			num = 2.5f;
		}
		Vector3 zero = Vector3.zero;
		Vector2 vector = Vector2.ClampMagnitude(MonoSingleton<InputManager>.Instance.InputSource.Move.ReadValue<Vector2>(), 1f);
		zero += camera.right * vector.x;
		zero += camera.forward * vector.y;
		if (MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed)
		{
			zero += Vector3.up;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Slide.IsPressed)
		{
			zero += Vector3.down;
		}
		rigidbody.velocity = zero * 30f * num;
		MonoSingleton<NewMovement>.Instance.enabled = false;
		rigidbody.isKinematic = false;
		rigidbody.useGravity = false;
	}
}
