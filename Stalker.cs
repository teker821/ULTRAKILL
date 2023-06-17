using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.AI;

public class Stalker : MonoBehaviour
{
	private EnemyIdentifier eid;

	private Machine mach;

	private int difficulty = -1;

	private NavMeshAgent nma;

	[HideInInspector]
	public float defaultMovementSpeed;

	private Animator anim;

	private bool inAction;

	private Transform target;

	private float explosionCharge;

	private float countDownAmount;

	private bool exploding;

	private bool exploded;

	public GameObject explosion;

	private float maxHp;

	private Light lit;

	private Color currentColor;

	public Color[] lightColors;

	private bool blinking;

	private float blinkTimer;

	private AudioSource lightAud;

	public AudioClip[] lightSounds;

	public SkinnedMeshRenderer canRenderer;

	public GameObject stepSound;

	public GameObject screamSound;

	private float explodeSpeed = 1f;

	public float prepareTime = 5f;

	public float prepareWarningTime = 3f;

	private void Start()
	{
		mach = GetComponent<Machine>();
		lit = GetComponentInChildren<Light>();
		lightAud = lit.GetComponent<AudioSource>();
		maxHp = mach.health;
		currentColor = lightColors[0];
		lightAud.clip = lightSounds[0];
		lightAud.loop = false;
		lightAud.pitch = 1f;
		lightAud.volume = 0.35f;
		SetSpeed();
		NavigationUpdate();
		SlowUpdate();
	}

	private void UpdateBuff()
	{
		SetSpeed();
	}

	private void SetSpeed()
	{
		if (!anim)
		{
			anim = GetComponent<Animator>();
		}
		if (!eid)
		{
			eid = GetComponent<EnemyIdentifier>();
		}
		if (!nma)
		{
			nma = GetComponent<NavMeshAgent>();
		}
		if (difficulty < 0)
		{
			if (eid.difficultyOverride >= 0)
			{
				difficulty = eid.difficultyOverride;
			}
			else
			{
				difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
			}
		}
		if (defaultMovementSpeed == 0f)
		{
			defaultMovementSpeed = nma.speed;
		}
		nma.speed = defaultMovementSpeed * eid.totalSpeedModifier;
		anim.speed = eid.totalSpeedModifier;
		anim.SetFloat("ExplodeSpeed", explodeSpeed);
	}

	private void OnDisable()
	{
		if (exploding)
		{
			exploding = false;
			explosionCharge = prepareTime;
			inAction = false;
			blinking = false;
		}
	}

	private void NavigationUpdate()
	{
		if ((bool)nma && nma.isOnNavMesh)
		{
			if (BlindEnemies.Blind)
			{
				nma?.SetDestination(base.transform.position);
			}
			else if ((bool)target && !inAction && (bool)mach && mach.grounded)
			{
				nma?.SetDestination(target.position);
			}
			else if ((bool)mach && mach.grounded && inAction)
			{
				nma?.SetDestination(base.transform.position);
			}
		}
		Invoke("NavigationUpdate", 0.1f);
	}

	private void SlowUpdate()
	{
		if (inAction || ((bool)mach && !mach.grounded) || ((bool)nma && !nma.isOnNavMesh) || BlindEnemies.Blind)
		{
			Invoke("SlowUpdate", 0.5f);
			return;
		}
		List<EnemyIdentifier> currentEnemies = MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies();
		if (currentEnemies != null && currentEnemies.Count > 0)
		{
			bool flag = false;
			bool flag2 = false;
			float num = float.PositiveInfinity;
			EnemyIdentifier enemyIdentifier = null;
			for (int num2 = 6; num2 >= 0; num2--)
			{
				for (int i = 0; i < currentEnemies.Count; i++)
				{
					if (currentEnemies[i].flying || currentEnemies[i].sandified || MonoSingleton<EnemyTracker>.Instance.GetEnemyRank(currentEnemies[i]) != num2)
					{
						continue;
					}
					float num3 = Vector3.Distance(base.transform.position, currentEnemies[i].transform.position);
					if (!(num3 < num) || ((currentEnemies[i].enemyType == EnemyType.MaliciousFace || !CheckForPath(currentEnemies[i].transform.position)) && (currentEnemies[i].enemyType != EnemyType.MaliciousFace || !CheckForOffsetPath(currentEnemies[i]))))
					{
						continue;
					}
					if (currentEnemies[i].transform != target)
					{
						if (!MonoSingleton<StalkerController>.Instance.CheckIfTargetTaken(currentEnemies[i].transform))
						{
							if (target != null && MonoSingleton<StalkerController>.Instance.CheckIfTargetTaken(target))
							{
								MonoSingleton<StalkerController>.Instance.targets.Remove(target);
							}
							if (enemyIdentifier != null && MonoSingleton<StalkerController>.Instance.CheckIfTargetTaken(enemyIdentifier.transform))
							{
								MonoSingleton<StalkerController>.Instance.targets.Remove(enemyIdentifier.transform);
							}
							enemyIdentifier = currentEnemies[i];
							MonoSingleton<StalkerController>.Instance.targets.Add(currentEnemies[i].transform);
							if (num3 < 100f)
							{
								flag = true;
							}
							else
							{
								flag2 = true;
							}
							num = Vector3.Distance(base.transform.position, currentEnemies[i].transform.position);
						}
						else if (!flag)
						{
							enemyIdentifier = currentEnemies[i];
							flag2 = true;
							num = Vector3.Distance(base.transform.position, currentEnemies[i].transform.position);
						}
					}
					else
					{
						if (num3 < 100f)
						{
							enemyIdentifier = currentEnemies[i];
							flag = true;
						}
						else
						{
							enemyIdentifier = currentEnemies[i];
							flag2 = true;
						}
						num = Vector3.Distance(base.transform.position, currentEnemies[i].transform.position);
					}
				}
				if (flag)
				{
					target = enemyIdentifier.transform;
					enemyIdentifier.buffTargeter = eid;
					break;
				}
			}
			if (!flag && flag2)
			{
				target = enemyIdentifier.transform;
				enemyIdentifier.buffTargeter = eid;
			}
			else if (!flag && !flag2)
			{
				if (target != null)
				{
					if (MonoSingleton<StalkerController>.Instance.CheckIfTargetTaken(target))
					{
						MonoSingleton<StalkerController>.Instance.targets.Remove(target);
					}
					if (target.TryGetComponent<EnemyIdentifier>(out var component) && component.buffTargeter == eid)
					{
						component.buffTargeter = null;
					}
				}
				target = MonoSingleton<PlayerTracker>.Instance.GetPlayer();
			}
		}
		Invoke("SlowUpdate", 0.5f);
	}

	private void Update()
	{
		if (exploding)
		{
			if (countDownAmount < 2f)
			{
				countDownAmount = Mathf.MoveTowards(countDownAmount, 2f, Time.deltaTime * explodeSpeed * eid.totalSpeedModifier);
			}
			else
			{
				exploding = false;
				SandExplode(0);
			}
		}
		else if (explosionCharge < prepareTime)
		{
			explosionCharge = Mathf.MoveTowards(explosionCharge, prepareTime, Time.deltaTime * eid.totalSpeedModifier);
			if (explosionCharge > prepareWarningTime)
			{
				blinking = true;
			}
		}
		else
		{
			if (lit.color != lightColors[1] * (mach.health / maxHp))
			{
				blinking = false;
				currentColor = lightColors[1];
				lightAud.clip = lightSounds[1];
				lightAud.loop = true;
				lightAud.pitch = 0.5f;
				lightAud.volume = 0.65f;
				lightAud.Play();
			}
			if (explosionCharge < prepareTime + 1f)
			{
				explosionCharge = Mathf.MoveTowards(explosionCharge, prepareTime + 1f, Time.deltaTime * eid.totalSpeedModifier);
			}
			else if ((bool)target && Vector3.Distance(base.transform.position, target.position) < 8f && !exploding && !BlindEnemies.Blind)
			{
				exploding = true;
				Countdown();
			}
		}
		if (!inAction)
		{
			if (nma.velocity.magnitude > 5f)
			{
				anim.SetBool("Running", value: true);
			}
			else
			{
				anim.SetBool("Running", value: false);
			}
			if (nma.velocity.magnitude > 0f)
			{
				anim.SetBool("Walking", value: true);
			}
			else
			{
				anim.SetBool("Walking", value: false);
			}
			if (!mach.grounded)
			{
				anim.SetBool("Falling", value: true);
			}
			else
			{
				anim.SetBool("Falling", value: false);
			}
		}
		if (blinking)
		{
			if (blinkTimer > 0f)
			{
				blinkTimer = Mathf.MoveTowards(blinkTimer, 0f, Time.deltaTime);
			}
			else
			{
				if (lit.color != Color.black)
				{
					lit.color = Color.black;
					lightAud?.Stop();
				}
				else
				{
					lit.color = currentColor * ((mach.health + 0.2f) / (maxHp + 0.2f));
					lightAud?.Play();
				}
				blinkTimer = 0.1f;
			}
		}
		else
		{
			lit.color = currentColor * ((mach.health + 0.2f) / (maxHp + 0.2f));
			blinkTimer = 0f;
		}
		if ((bool)canRenderer)
		{
			canRenderer.material.SetColor("_EmissiveColor", lit.color);
		}
	}

	public void Countdown()
	{
		inAction = true;
		blinking = true;
		currentColor = lightColors[2];
		lightAud.clip = lightSounds[2];
		lightAud.loop = false;
		lightAud.pitch = 1f;
		lightAud.volume = 0.65f;
		explosionCharge = 0f;
		countDownAmount = 0f;
		Object.Instantiate(screamSound, base.transform);
		anim.SetTrigger("Explode");
	}

	public void SandExplode(int onDeath = 1)
	{
		if (exploded)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate(explosion, base.transform.position + Vector3.up * 2.5f, Quaternion.identity);
		if (onDeath != 1)
		{
			gameObject.transform.localScale *= 1.5f;
		}
		if (eid.stuckMagnets.Count > 0)
		{
			float num = 0.75f;
			if (eid.stuckMagnets.Count > 1)
			{
				num -= 0.125f * (float)(eid.stuckMagnets.Count - 1);
			}
			gameObject.transform.localScale *= num;
		}
		if ((difficulty > 3 || eid.blessed || InvincibleEnemies.Enabled) && onDeath != 1)
		{
			exploding = false;
			countDownAmount = 0f;
			explosionCharge = 0f;
			currentColor = lightColors[0];
			lightAud.clip = lightSounds[0];
			blinking = false;
			return;
		}
		exploded = true;
		if (!mach.limp)
		{
			mach.GoLimp();
			eid.Death();
		}
		if (target != null)
		{
			if (MonoSingleton<StalkerController>.Instance.CheckIfTargetTaken(target))
			{
				MonoSingleton<StalkerController>.Instance.targets.Remove(target);
			}
			if (target.TryGetComponent<EnemyIdentifier>(out var component) && component.buffTargeter == eid)
			{
				component.buffTargeter = null;
			}
		}
		if (eid.drillers.Count != 0)
		{
			for (int num2 = eid.drillers.Count - 1; num2 >= 0; num2--)
			{
				Object.Destroy(eid.drillers[num2].gameObject);
			}
		}
		base.gameObject.SetActive(value: false);
		Object.Destroy(base.gameObject);
	}

	public bool CheckForPath(Vector3 pathTarget)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		nma.CalculatePath(pathTarget, navMeshPath);
		if (navMeshPath != null && navMeshPath.status == NavMeshPathStatus.PathComplete)
		{
			return true;
		}
		return false;
	}

	public bool CheckForOffsetPath(EnemyIdentifier ed)
	{
		if (ed.TryGetComponent<NavMeshAgent>(out var component))
		{
			return CheckForPath(ed.transform.position + Vector3.down * component.height * component.baseOffset * ed.transform.localScale.y);
		}
		return false;
	}

	public void StopAction()
	{
		inAction = false;
	}

	public void Step()
	{
		Object.Instantiate(stepSound, base.transform.position, Quaternion.identity);
	}
}
