using UnityEngine;
using UnityEngine.AI;

public class ZombieMelee : MonoBehaviour
{
	public bool harmless;

	public bool damaging;

	public TrailRenderer tr;

	public bool track;

	private AudioSource aud;

	public float coolDown;

	public Zombie zmb;

	private NavMeshAgent nma;

	private Animator anim;

	private EnemyIdentifier eid;

	private bool customStart;

	private bool musicRequested;

	private int difficulty = -1;

	private float defaultCoolDown = 0.5f;

	public GameObject swingSound;

	public LayerMask lmask;

	private Rigidbody rb;

	private SwingCheck2 swingCheck;

	private EnemySimplifier ensim;

	public Material originalMaterial;

	public Material biteMaterial;

	private void Start()
	{
		zmb = GetComponent<Zombie>();
		nma = zmb.nma;
		anim = zmb.anim;
		eid = GetComponent<EnemyIdentifier>();
		if (eid.difficultyOverride >= 0)
		{
			difficulty = eid.difficultyOverride;
		}
		else
		{
			difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
		}
		if (difficulty != 2)
		{
			if (difficulty >= 3)
			{
				defaultCoolDown = 0.25f;
			}
			else if (difficulty == 1)
			{
				defaultCoolDown = 0.75f;
			}
			else if (difficulty == 0)
			{
				defaultCoolDown = 1f;
			}
		}
		if (!musicRequested && !zmb.friendly)
		{
			musicRequested = true;
			zmb.musicRequested = true;
			MusicManager instance = MonoSingleton<MusicManager>.Instance;
			if ((bool)instance)
			{
				instance.PlayBattleMusic();
			}
		}
		ensim = GetComponentInChildren<EnemySimplifier>();
		TrackTick();
	}

	private void Update()
	{
		if (damaging)
		{
			if (rb == null)
			{
				rb = GetComponent<Rigidbody>();
			}
			rb.isKinematic = false;
			rb.velocity = base.transform.forward * 40f * anim.speed;
		}
		if (track && zmb.target != null)
		{
			if (difficulty > 1)
			{
				base.transform.LookAt(new Vector3(zmb.target.position.x, base.transform.position.y, zmb.target.position.z));
			}
			else
			{
				float num = 720f;
				if (difficulty == 0)
				{
					num = 360f;
				}
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(new Vector3(zmb.target.position.x, base.transform.position.y, zmb.target.position.z) - base.transform.position), Time.deltaTime * num * eid.totalSpeedModifier);
			}
		}
		if (coolDown != 0f)
		{
			if (coolDown - Time.deltaTime > 0f)
			{
				coolDown -= Time.deltaTime / 2.5f * eid.totalSpeedModifier;
			}
			else
			{
				coolDown = 0f;
			}
		}
		else if (zmb.target != null && Vector3.Distance(zmb.target.position, base.transform.position) < 3f && zmb.grounded)
		{
			Swing();
		}
	}

	private void OnEnable()
	{
		if (zmb == null)
		{
			zmb = GetComponent<Zombie>();
		}
		if (!musicRequested && !zmb.friendly)
		{
			musicRequested = true;
			zmb.musicRequested = true;
			MusicManager instance = MonoSingleton<MusicManager>.Instance;
			if ((bool)instance)
			{
				instance.PlayBattleMusic();
			}
		}
		CancelAttack();
		if (zmb.grounded && (bool)rb)
		{
			rb.velocity = Vector3.zero;
			rb.isKinematic = true;
		}
	}

	private void OnDisable()
	{
		if (musicRequested && !zmb.friendly && !zmb.limp)
		{
			musicRequested = false;
			zmb.musicRequested = false;
			MusicManager instance = MonoSingleton<MusicManager>.Instance;
			if ((bool)instance)
			{
				instance.PlayCleanMusic();
			}
		}
	}

	private void FixedUpdate()
	{
		if (anim == null)
		{
			anim = zmb.anim;
		}
		if (nma == null)
		{
			nma = zmb.nma;
		}
		if (zmb.grounded && nma != null && nma.enabled && nma.isOnNavMesh && zmb.target != null)
		{
			if (nma.isStopped || nma.velocity == Vector3.zero)
			{
				anim.SetBool("Running", value: false);
			}
			else
			{
				anim.SetBool("Running", value: true);
			}
		}
		else if (nma == null)
		{
			nma = zmb.nma;
		}
	}

	public void Swing()
	{
		if (!harmless)
		{
			if (aud == null)
			{
				aud = GetComponentInChildren<SwingCheck2>().GetComponent<AudioSource>();
			}
			if (nma == null)
			{
				nma = zmb.nma;
			}
			zmb.stopped = true;
			anim.speed = 1f;
			track = true;
			coolDown = defaultCoolDown;
			if (nma.enabled)
			{
				nma.isStopped = true;
			}
			anim.SetTrigger("Swing");
			Object.Instantiate(swingSound, base.transform);
		}
	}

	public void SwingEnd()
	{
		if (nma.isOnNavMesh)
		{
			nma.isStopped = false;
		}
		zmb.stopped = false;
	}

	public void DamageStart()
	{
		if (!harmless)
		{
			damaging = true;
			aud.Play();
			if (tr == null)
			{
				tr = GetComponentInChildren<TrailRenderer>();
			}
			tr.enabled = true;
			if (swingCheck == null)
			{
				swingCheck = GetComponentInChildren<SwingCheck2>();
			}
			swingCheck.damage = 30;
			swingCheck.enemyDamage = 10;
			swingCheck.DamageStart();
			MouthClose();
		}
	}

	public void DamageEnd()
	{
		if (rb == null)
		{
			rb = GetComponent<Rigidbody>();
		}
		damaging = false;
		zmb.attacking = false;
		rb.velocity = Vector3.zero;
		rb.isKinematic = true;
		tr.enabled = false;
		if (swingCheck == null)
		{
			swingCheck = GetComponentInChildren<SwingCheck2>();
		}
		swingCheck.DamageStop();
	}

	public void StopTracking()
	{
		track = false;
		zmb.attacking = true;
	}

	public void CancelAttack()
	{
		damaging = false;
		zmb.attacking = false;
		if (tr == null)
		{
			tr = GetComponentInChildren<TrailRenderer>();
		}
		tr.enabled = false;
		zmb.stopped = false;
		track = false;
		coolDown = defaultCoolDown;
		if (swingCheck == null)
		{
			swingCheck = GetComponentInChildren<SwingCheck2>();
		}
		swingCheck.DamageStop();
	}

	public void TrackTick()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (nma == null)
			{
				nma = zmb.nma;
			}
			if (zmb.grounded && nma != null && nma.enabled && nma.isOnNavMesh && zmb.target != null)
			{
				if (Physics.Raycast(zmb.target.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, float.PositiveInfinity, lmask))
				{
					nma.SetDestination(hitInfo.point);
				}
				else
				{
					nma.SetDestination(zmb.target.position);
				}
			}
		}
		Invoke("TrackTick", 0.1f);
	}

	public void MouthClose()
	{
		if ((bool)ensim)
		{
			ensim.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, biteMaterial);
		}
		CancelInvoke("MouthOpen");
		Invoke("MouthOpen", 0.75f);
	}

	private void MouthOpen()
	{
		if ((bool)ensim)
		{
			ensim.ChangeMaterialNew(EnemySimplifier.MaterialState.normal, originalMaterial);
		}
	}
}
