using UnityEngine;
using UnityEngine.AI;

public class SplashContinuous : MonoBehaviour
{
	private bool active = true;

	private float cooldown;

	[SerializeField]
	private ParticleSystem particles;

	[SerializeField]
	private GameObject wadingSound;

	[SerializeField]
	private AudioClip[] wadingSounds;

	[SerializeField]
	private float wadingSoundPitch = 0.8f;

	private Vector3 previousPosition;

	[SerializeField]
	private float movingEmissionRate = 20f;

	[SerializeField]
	private float stillEmissionRate = 2f;

	[HideInInspector]
	public NavMeshAgent nma;

	private void FixedUpdate()
	{
		if (!active)
		{
			return;
		}
		ParticleSystem.EmissionModule emission = particles.emission;
		if (((bool)nma && nma.velocity.magnitude > 4f) || Vector3.Distance(base.transform.position, previousPosition) > 0.05f)
		{
			emission.rateOverTime = movingEmissionRate;
			if (cooldown == 0f)
			{
				if (Object.Instantiate(wadingSound, base.transform).TryGetComponent<AudioSource>(out var component))
				{
					component.clip = wadingSounds[Random.Range(0, wadingSounds.Length)];
					component.pitch = Random.Range(wadingSoundPitch - 0.05f, wadingSoundPitch + 0.05f);
					component.Play();
				}
				cooldown = 0.75f;
			}
		}
		else
		{
			emission.rateOverTime = stillEmissionRate;
		}
		cooldown = Mathf.MoveTowards(cooldown, 0f, Time.fixedDeltaTime * (1f + Vector3.Distance(base.transform.position, previousPosition) * 5f));
		previousPosition = base.transform.position;
	}

	public void DestroySoon()
	{
		particles.Stop();
		active = false;
		Invoke("DestroyNow", 2f);
	}

	private void DestroyNow()
	{
		Object.Destroy(base.gameObject);
	}
}
