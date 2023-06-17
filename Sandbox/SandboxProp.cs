using UnityEngine;

namespace Sandbox;

public class SandboxProp : SandboxSpawnableInstance
{
	[SerializeField]
	private PhysicsSounds.PhysMaterial physicsMaterial;

	[SerializeField]
	private bool enableImpactDamage;

	public bool forceFullWorldPreview;

	private TimeSince timeSinceLastImpact;

	private void Start()
	{
		timeSinceLastImpact = 0f;
	}

	private void OnCollisionEnter(Collision other)
	{
		if (!rigidbody.isKinematic && !(other.impulse.magnitude < 3f) && !((float)timeSinceLastImpact < 0.1f))
		{
			timeSinceLastImpact = 0f;
			MonoSingleton<PhysicsSounds>.Instance.ImpactAt(other.GetContact(0).point, other.impulse.magnitude, physicsMaterial);
		}
	}

	public SavedProp SaveProp()
	{
		SavedGeneric saveObject;
		SavedGeneric result = (saveObject = new SavedProp());
		BaseSave(ref saveObject);
		return (SavedProp)result;
	}

	private void OnCollisionStay(Collision other)
	{
		OnCollisionEnter(other);
	}

	public override void Pause(bool freeze = true)
	{
		base.Pause(freeze);
		if (TryGetComponent<Rigidbody>(out var component))
		{
			component.isKinematic = true;
			component.velocity = Vector3.zero;
		}
		collider.gameObject.isStatic = true;
	}

	public override void Resume()
	{
		base.Resume();
		if (TryGetComponent<Rigidbody>(out var component))
		{
			component.isKinematic = false;
		}
		collider.gameObject.isStatic = false;
	}
}
