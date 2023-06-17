using UnityEngine;

namespace ULTRAKILL.Cheats;

public class Noclip : ICheat
{
	private bool active;

	private Rigidbody rigidbody;

	private KeepInBounds kib;

	private Transform transform;

	private Transform camera;

	public string LongName => "Noclip";

	public string Identifier => "ultrakill.noclip";

	public string ButtonEnabledOverride => null;

	public string ButtonDisabledOverride => null;

	public string Icon => "noclip";

	public bool IsActive => active;

	public bool DefaultState { get; }

	public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

	public void Enable()
	{
		MonoSingleton<CheatsManager>.Instance.DisableCheat("ultrakill.flight");
		MonoSingleton<CheatsManager>.Instance.DisableCheat("ultrakill.clash-mode");
		rigidbody = MonoSingleton<NewMovement>.Instance.GetComponent<Rigidbody>();
		kib = MonoSingleton<NewMovement>.Instance.GetComponent<KeepInBounds>();
		transform = MonoSingleton<NewMovement>.Instance.transform;
		camera = MonoSingleton<CameraController>.Instance.transform;
		active = true;
		kib.enabled = false;
	}

	public void Disable()
	{
		active = false;
		MonoSingleton<NewMovement>.Instance.enabled = true;
		kib.enabled = true;
		rigidbody.isKinematic = false;
	}

	public void Update()
	{
		float num = 1f;
		if (MonoSingleton<InputManager>.Instance.InputSource.Dodge.IsPressed)
		{
			num = 2.5f;
		}
		Vector2 vector = Vector2.ClampMagnitude(MonoSingleton<InputManager>.Instance.InputSource.Move.ReadValue<Vector2>(), 1f);
		transform.position += camera.right * vector.x * 40f * Time.deltaTime * num;
		transform.position += camera.forward * vector.y * 40f * Time.deltaTime * num;
		if (MonoSingleton<InputManager>.Instance.InputSource.Jump.IsPressed)
		{
			transform.position += new Vector3(0f, 40f, 0f) * 1f * Time.deltaTime * num;
		}
		if (MonoSingleton<InputManager>.Instance.InputSource.Slide.IsPressed)
		{
			transform.position += new Vector3(0f, -40f, 0f) * 1f * Time.deltaTime * num;
		}
		MonoSingleton<NewMovement>.Instance.enabled = false;
		rigidbody.isKinematic = true;
	}
}
