using UnityEngine;

public class GroundCheck : MonoBehaviour
{
	public bool slopeCheck;

	public bool onGround;

	public bool touchingGround;

	public bool canJump;

	public bool heavyFall;

	public GameObject shockwave;

	public bool enemy;

	public float superJumpChance;

	public float extraJumpChance;

	private NewMovement nmov;

	private PlayerMovementParenting pmov;

	private Collider currentCol;

	private Collider currentEnemyCol;

	public int forcedOff;

	private LayerMask waterMask;

	private void Start()
	{
		nmov = MonoSingleton<NewMovement>.Instance;
		pmov = base.transform.parent.GetComponent<PlayerMovementParenting>();
		if (pmov == null)
		{
			pmov = nmov.GetComponent<PlayerMovementParenting>();
		}
		waterMask = LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies);
		waterMask = (int)waterMask | 4;
	}

	private void OnEnable()
	{
		base.transform.parent.parent = null;
	}

	private void OnDisable()
	{
		if (touchingGround)
		{
			touchingGround = false;
		}
		if ((bool)MonoSingleton<NewMovement>.Instance)
		{
			MonoSingleton<NewMovement>.Instance.groundProperties = null;
		}
		canJump = false;
	}

	private void Update()
	{
		if (forcedOff > 0)
		{
			onGround = false;
		}
		else if (onGround != touchingGround)
		{
			onGround = touchingGround;
		}
		if (superJumpChance > 0f)
		{
			superJumpChance = Mathf.MoveTowards(superJumpChance, 0f, Time.deltaTime);
			if (superJumpChance == 0f)
			{
				if (shockwave != null && nmov.stillHolding)
				{
					if (nmov.boostCharge >= 100f)
					{
						Object.Instantiate(shockwave, base.transform.position, Quaternion.identity).GetComponent<PhysicalShockwave>().force *= nmov.slamForce * 2.25f;
						if (!nmov.asscon.majorEnabled || !nmov.asscon.infiniteStamina)
						{
							nmov.boostCharge -= 100f;
						}
						nmov.cc.CameraShake(0.75f);
					}
					else
					{
						Object.Instantiate(nmov.staminaFailSound);
					}
				}
				extraJumpChance = 0.15f;
				nmov.stillHolding = false;
			}
		}
		if (extraJumpChance > 0f)
		{
			extraJumpChance = Mathf.MoveTowards(extraJumpChance, 0f, Time.deltaTime);
			if (extraJumpChance <= 0f)
			{
				nmov.slamForce = 0f;
			}
		}
		if (touchingGround && (currentCol == null || !currentCol.enabled || !currentCol.gameObject.activeInHierarchy || currentCol.gameObject.layer == 17 || currentCol.gameObject.layer == 10))
		{
			touchingGround = false;
			MonoSingleton<NewMovement>.Instance.groundProperties = null;
		}
		if (canJump && (currentEnemyCol == null || !currentEnemyCol.gameObject.activeInHierarchy || Vector3.Distance(base.transform.position, currentEnemyCol.transform.position) > 40f))
		{
			canJump = false;
		}
	}

	private void FixedUpdate()
	{
		if (!MonoSingleton<UnderwaterController>.Instance.inWater && !slopeCheck && !(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().y >= 0f) && (MonoSingleton<PlayerTracker>.Instance.playerType != 0 || MonoSingleton<NewMovement>.Instance.sliding) && (MonoSingleton<PlayerTracker>.Instance.playerType != PlayerType.Platformer || MonoSingleton<PlatformerMovement>.Instance.sliding) && Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, Mathf.Abs(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().y), waterMask, QueryTriggerInteraction.Collide) && hitInfo.transform.gameObject.layer == 4)
		{
			BounceOnWater(hitInfo.collider);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (!other.gameObject.CompareTag("Slippery") && (other.gameObject.layer == 8 || other.gameObject.layer == 24 || other.gameObject.layer == 11 || other.gameObject.layer == 26) && (!enemy || other.gameObject.tag == "Floor" || other.gameObject.tag == "Wall" || other.gameObject.tag == "GlassFloor"))
		{
			if (currentCol == null)
			{
				currentCol = other;
				if (currentCol.TryGetComponent<CustomGroundProperties>(out var component))
				{
					MonoSingleton<NewMovement>.Instance.groundProperties = component;
				}
				else
				{
					MonoSingleton<NewMovement>.Instance.groundProperties = null;
				}
			}
			touchingGround = true;
			canJump = false;
		}
		else if (!other.gameObject.CompareTag("Slippery") && other.gameObject.layer == 12)
		{
			currentEnemyCol = other;
			canJump = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.gameObject.CompareTag("Slippery") && (other.gameObject.layer == 8 || other.gameObject.layer == 24 || other.gameObject.layer == 11 || other.gameObject.layer == 26) && (!enemy || other.gameObject.tag == "Floor" || other.gameObject.tag == "Wall" || other.gameObject.tag == "GlassFloor"))
		{
			if (touchingGround)
			{
				touchingGround = false;
			}
			if (currentCol == other)
			{
				MonoSingleton<NewMovement>.Instance.groundProperties = null;
			}
			if (!slopeCheck && (other.gameObject.CompareTag("Moving") || other.gameObject.layer == 11 || other.gameObject.layer == 26) && pmov.IsObjectTracked(other.transform))
			{
				pmov.DetachPlayer(other.transform);
			}
		}
		else if (!other.gameObject.CompareTag("Slippery") && other.gameObject.layer == 12)
		{
			canJump = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.CompareTag("Slippery") && (other.gameObject.layer == 8 || other.gameObject.layer == 24 || other.gameObject.layer == 11 || other.gameObject.layer == 26) && (!enemy || other.gameObject.tag == "Floor" || other.gameObject.tag == "Wall" || other.gameObject.tag == "GlassFloor"))
		{
			currentCol = other;
			if (currentCol.TryGetComponent<CustomGroundProperties>(out var component))
			{
				MonoSingleton<NewMovement>.Instance.groundProperties = component;
			}
			else
			{
				MonoSingleton<NewMovement>.Instance.groundProperties = null;
			}
			if (!slopeCheck && (other.gameObject.CompareTag("Moving") || other.gameObject.layer == 11 || other.gameObject.layer == 26) && other.attachedRigidbody != null && !pmov.IsObjectTracked(other.transform))
			{
				pmov.AttachPlayer(other.transform);
			}
		}
		if (heavyFall)
		{
			if (other.gameObject.layer == 10 || other.gameObject.layer == 11)
			{
				EnemyIdentifierIdentifier component2 = other.gameObject.GetComponent<EnemyIdentifierIdentifier>();
				if ((bool)component2 && (bool)component2.eid)
				{
					component2.eid.hitter = "ground slam";
					component2.eid.DeliverDamage(other.gameObject, (base.transform.position - other.transform.position) * 5000f, other.transform.position, 2f, tryForExplode: true);
					if (!component2.eid.exploded)
					{
						heavyFall = false;
					}
				}
			}
			else if (!other.gameObject.CompareTag("Slippery") && (other.gameObject.layer == 8 || other.gameObject.layer == 24))
			{
				Breakable component3 = other.gameObject.GetComponent<Breakable>();
				if (component3 != null && component3.weak)
				{
					component3.Break();
				}
				else
				{
					heavyFall = false;
				}
				if (other.gameObject.TryGetComponent<Bleeder>(out var component4))
				{
					component4.GetHit(other.transform.position, GoreType.Body);
				}
				if (other.transform.TryGetComponent<Idol>(out var component5))
				{
					component5.Death();
				}
				superJumpChance = 0.075f;
			}
		}
		if (!slopeCheck && other.gameObject.layer == 4 && ((MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS && nmov.sliding && nmov.rb.velocity.y < 0f) || (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer && MonoSingleton<PlatformerMovement>.Instance.sliding && MonoSingleton<PlatformerMovement>.Instance.rb.velocity.y < 0f)))
		{
			Vector3 a = other.ClosestPoint(base.transform.position);
			if (!MonoSingleton<UnderwaterController>.Instance.inWater && (Vector3.Distance(a, base.transform.position) < 0.1f || !Physics.Raycast(base.transform.position, Vector3.down, Vector3.Distance(a, base.transform.position), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Collide)))
			{
				BounceOnWater(other);
			}
		}
	}

	private void BounceOnWater(Collider other)
	{
		if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.FPS)
		{
			nmov.rb.velocity = new Vector3(nmov.rb.velocity.x, 0f, nmov.rb.velocity.z);
			nmov.rb.AddForce(Vector3.up * 10f, ForceMode.VelocityChange);
		}
		else
		{
			MonoSingleton<PlatformerMovement>.Instance.rb.velocity = new Vector3(MonoSingleton<PlatformerMovement>.Instance.rb.velocity.x, 0f, MonoSingleton<PlatformerMovement>.Instance.rb.velocity.z);
			MonoSingleton<PlatformerMovement>.Instance.rb.AddForce(Vector3.up * 10f, ForceMode.VelocityChange);
		}
		Water componentInParent = other.GetComponentInParent<Water>();
		if ((bool)componentInParent)
		{
			Object.Instantiate(componentInParent.smallSplash, base.transform.position, Quaternion.LookRotation(Vector3.up)).GetComponent<AudioSource>().volume = 0.65f;
			ChallengeTrigger component = componentInParent.GetComponent<ChallengeTrigger>();
			if ((bool)component)
			{
				component.Entered();
			}
		}
	}

	public void ForceOff()
	{
		onGround = false;
		forcedOff++;
	}

	public void StopForceOff()
	{
		forcedOff--;
		if (forcedOff <= 0)
		{
			onGround = touchingGround;
		}
	}
}
