using Sandbox;
using UnityEngine;

public class JumpPad : MonoBehaviour, IAlter, IAlterOptions<float>
{
	public float force;

	private float origPitch;

	private AudioSource aud;

	public AudioClip launchSound;

	public AudioClip lightLaunchSound;

	public bool forceDirection;

	public string alterKey => "jump-pad";

	public string alterCategoryName => "Jump Pad";

	public bool allowOnlyOne => false;

	public AlterOption<float>[] options => new AlterOption<float>[1]
	{
		new AlterOption<float>
		{
			name = "Force",
			key = "force",
			value = force,
			callback = delegate(float value)
			{
				force = value;
			}
		}
	};

	private void Start()
	{
		aud = GetComponent<AudioSource>();
		if ((bool)aud)
		{
			origPitch = aud.pitch;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.isStatic)
		{
			return;
		}
		float num = 1f;
		if (other.gameObject.tag == "Player")
		{
			NewMovement instance = MonoSingleton<NewMovement>.Instance;
			if ((bool)instance && instance.gameObject.activeSelf)
			{
				if (instance.gc.heavyFall)
				{
					num = 1.2f;
				}
				instance.LaunchFromPoint(other.transform.position, 0f);
				if (forceDirection)
				{
					if (instance.sliding)
					{
						instance.StopSlide();
					}
					if (instance.boost)
					{
						instance.boost = false;
					}
				}
			}
			else if ((bool)MonoSingleton<PlatformerMovement>.Instance)
			{
				if (forceDirection)
				{
					MonoSingleton<PlatformerMovement>.Instance.StopSlide();
					MonoSingleton<PlatformerMovement>.Instance.rb.velocity = Vector3.zero;
				}
				MonoSingleton<PlatformerMovement>.Instance.Jump(silent: true, 0f);
			}
		}
		else if (other.gameObject.tag == "Enemy")
		{
			EnemyIdentifier component = other.gameObject.GetComponent<EnemyIdentifier>();
			if (component != null && !component.dead)
			{
				if (component.unbounceable)
				{
					return;
				}
				component.DeliverDamage(other.gameObject, Vector3.up * 10f, other.transform.position, 0f, tryForExplode: false);
			}
		}
		Rigidbody component2 = other.gameObject.GetComponent<Rigidbody>();
		if (!(component2 != null) || component2.isKinematic)
		{
			return;
		}
		Vector3 velocity = component2.velocity;
		if (base.transform.up.x != 0f || forceDirection)
		{
			velocity.x = base.transform.up.x * force * num;
		}
		if (base.transform.up.y != 0f || forceDirection)
		{
			velocity.y = base.transform.up.y * force * num;
		}
		if (base.transform.up.z != 0f || forceDirection)
		{
			velocity.z = base.transform.up.z * force * num;
		}
		component2.velocity = velocity;
		int layer = other.gameObject.layer;
		if (layer == 14)
		{
			other.transform.LookAt(other.transform.position + component2.velocity);
		}
		if ((bool)aud)
		{
			if (layer == 11 || layer == 12 || layer == 2 || layer == 15)
			{
				aud.clip = launchSound;
			}
			else
			{
				aud.clip = lightLaunchSound;
			}
			aud.pitch = origPitch + Random.Range(-0.1f, 0.1f);
			aud.Play();
		}
	}
}
