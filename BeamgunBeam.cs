using UnityEngine;

public class BeamgunBeam : MonoBehaviour
{
	public bool active = true;

	[SerializeField]
	private LineRenderer line;

	[HideInInspector]
	public Vector3 fakeStartPoint;

	[SerializeField]
	private ParticleSystem hitParticle;

	private EnemyIdentifierIdentifier hitTarget;

	private Vector3 hitPosition;

	public bool canHitPlayer;

	private float beamCheckTime;

	public float beamCheckSpeed = 1f;

	private float playerDamageCooldown;

	public float beamWidth = 0.1f;

	private void Start()
	{
		line.widthMultiplier = beamWidth;
		hitParticle.transform.localScale = Vector3.one * beamWidth * 5f;
	}

	private void FixedUpdate()
	{
		if (beamCheckTime > 0f)
		{
			beamCheckTime = Mathf.MoveTowards(beamCheckTime, 0f, Time.deltaTime * beamCheckSpeed * 15f);
		}
		if (beamCheckTime <= 0f)
		{
			beamCheckTime = 1f;
			if ((bool)hitTarget)
			{
				hitTarget.eid.DeliverDamage(hitTarget.transform.gameObject, (hitPosition - base.transform.position).normalized * 10f, hitPosition, 0.15f, tryForExplode: false, 0.5f);
			}
		}
	}

	private void Update()
	{
		hitTarget = null;
		if (playerDamageCooldown > 0f)
		{
			playerDamageCooldown = Mathf.MoveTowards(playerDamageCooldown, 0f, Time.deltaTime);
		}
		if (active)
		{
			line.enabled = true;
			if (line.widthMultiplier != beamWidth)
			{
				line.widthMultiplier = Mathf.MoveTowards(line.widthMultiplier, beamWidth, Time.deltaTime * 25f);
				hitParticle.transform.localScale = Vector3.one * line.widthMultiplier * 5f;
			}
			if (fakeStartPoint != Vector3.zero)
			{
				line.SetPosition(0, fakeStartPoint);
			}
			else
			{
				line.SetPosition(0, line.transform.position);
			}
			hitPosition = base.transform.position + base.transform.forward * 9999f;
			Vector3 forward = base.transform.forward * -1f;
			LayerMask layerMask = LayerMaskDefaults.Get(LMD.EnemiesAndEnvironment);
			if (canHitPlayer && playerDamageCooldown <= 0f)
			{
				layerMask = LayerMaskDefaults.Get(LMD.EnemiesEnvironmentAndPlayer);
			}
			if (Physics.Raycast(base.transform.position, base.transform.forward, out var hitInfo, float.PositiveInfinity, layerMask, QueryTriggerInteraction.Ignore))
			{
				Breakable component2;
				if (hitInfo.transform.gameObject.layer != 8 && hitInfo.transform.gameObject.layer != 24)
				{
					if (hitInfo.collider.gameObject == MonoSingleton<NewMovement>.Instance.gameObject)
					{
						MonoSingleton<NewMovement>.Instance.GetHurt(10, invincible: true);
						playerDamageCooldown = 1f;
					}
					else if (hitInfo.transform.gameObject.layer == 10 || hitInfo.transform.gameObject.layer == 11)
					{
						if (hitInfo.transform.TryGetComponent<EnemyIdentifierIdentifier>(out var component))
						{
							hitTarget = component;
						}
						else
						{
							Grenade componentInParent = hitInfo.transform.GetComponentInParent<Grenade>();
							if ((bool)componentInParent)
							{
								componentInParent.Explode(big: true);
							}
						}
					}
				}
				else if (hitInfo.transform.gameObject.CompareTag("Breakable") && hitInfo.transform.TryGetComponent<Breakable>(out component2) && !component2.precisionOnly)
				{
					component2.Break();
				}
				hitPosition = hitInfo.point;
				forward = hitInfo.normal;
			}
			line.SetPosition(1, hitPosition);
			hitParticle.transform.position = hitPosition;
			hitParticle.transform.forward = forward;
			if (!hitParticle.isPlaying)
			{
				hitParticle.Play();
			}
		}
		else
		{
			if (line.enabled)
			{
				line.enabled = false;
			}
			if (hitParticle.isPlaying)
			{
				hitParticle.Stop();
			}
		}
	}
}
