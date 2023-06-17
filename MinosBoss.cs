using System.Collections.Generic;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.Events;

public class MinosBoss : MonoBehaviour
{
	private Animator anim;

	private EnemyIdentifier eid;

	private Statue stat;

	public Transform head;

	private bool inAction;

	private bool inPhaseChange;

	private float cooldown = 3f;

	public int phase = 1;

	public Transform rightArm;

	public Transform rightHand;

	private Transform[] rightHandBones;

	private SwingCheck2[] scRight;

	private bool attackingRight;

	public Transform leftArm;

	public Transform leftHand;

	private Transform[] leftHandBones;

	private SwingCheck2[] scLeft;

	private bool attackingLeft;

	public GameObject windupSound;

	public GameObject bigHurtSound;

	public GameObject punchExplosion;

	public bool onRight;

	public bool onMiddle;

	public bool onLeft;

	private float blackHoleCooldown = 10f;

	public GameObject blackHole;

	private BlackHoleProjectile currentBlackHole;

	public Transform blackHoleSpawnPos;

	private float lowMiddleChance = 0.5f;

	public GameObject[] eyes;

	public Material eyeless;

	public Parasite[] parasites;

	private float originalHealth;

	public UnityEvent onDeathImpact;

	public UnityEvent onDeathOver;

	private bool dead;

	private int difficulty = -1;

	private bool beenParried;

	public bool parryChallenge;

	private void Start()
	{
		stat = GetComponent<Statue>();
		originalHealth = stat.health;
		scRight = rightArm.GetComponentsInChildren<SwingCheck2>();
		rightHandBones = rightArm.GetComponentsInChildren<Transform>();
		scLeft = leftArm.GetComponentsInChildren<SwingCheck2>();
		leftHandBones = leftArm.GetComponentsInChildren<Transform>();
		SetSpeed();
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
		if (difficulty == 1)
		{
			anim.speed = 0.85f;
		}
		else if (difficulty == 0)
		{
			anim.speed = 0.65f;
		}
		else
		{
			anim.speed = 1f;
		}
		anim.speed *= eid.totalSpeedModifier;
		Parasite[] array = parasites;
		foreach (Parasite obj in array)
		{
			obj.speedMultiplier = eid.totalSpeedModifier;
			obj.damageMultiplier = eid.totalDamageModifier;
		}
	}

	private void Update()
	{
		if (dead && !anim.GetCurrentAnimatorStateInfo(0).IsName("Death"))
		{
			anim.Play("Death");
		}
		if (currentBlackHole == null && blackHoleCooldown > 0f && (phase < 2 || difficulty > 2))
		{
			blackHoleCooldown = Mathf.MoveTowards(blackHoleCooldown, 0f, Time.deltaTime * eid.totalSpeedModifier);
		}
		if ((bool)stat && stat.health < originalHealth / 2f && phase < 2 && !anim.IsInTransition(0))
		{
			inPhaseChange = true;
			PhaseChange(2);
		}
		if (BlindEnemies.Blind || inAction || inPhaseChange)
		{
			return;
		}
		if (currentBlackHole == null && blackHoleCooldown == 0f && difficulty >= 2 && (phase < 2 || difficulty > 2))
		{
			BlackHole();
		}
		else if (cooldown > 0f)
		{
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime * anim.speed);
		}
		else
		{
			if (anim.IsInTransition(0))
			{
				return;
			}
			if (phase == 1)
			{
				cooldown = 2f;
			}
			else if (phase == 2)
			{
				cooldown = 3f;
			}
			else
			{
				cooldown = 0f;
			}
			if (onRight)
			{
				if (onMiddle && Random.Range(0f, 1f) > 0.5f)
				{
					SlamMiddle();
				}
				else
				{
					SlamRight();
				}
			}
			else if (onLeft)
			{
				if (onMiddle && Random.Range(0f, 1f) > 0.5f)
				{
					SlamMiddle();
				}
				else
				{
					SlamLeft();
				}
			}
			else
			{
				SlamMiddle();
			}
		}
	}

	private void SlamRight()
	{
		inAction = true;
		anim.SetTrigger("SlamRight");
		Object.Instantiate(windupSound, head);
		attackingRight = true;
	}

	private void SlamLeft()
	{
		inAction = true;
		anim.SetTrigger("SlamLeft");
		Object.Instantiate(windupSound, head);
		attackingLeft = true;
	}

	private void SlamMiddle()
	{
		inAction = true;
		Object.Instantiate(windupSound, head);
		if (Random.Range(0f, 1f) > lowMiddleChance)
		{
			if (lowMiddleChance < 0.5f)
			{
				lowMiddleChance = 0.5f;
			}
			lowMiddleChance += 0.25f;
			anim.SetTrigger("SlamMiddle");
			attackingLeft = true;
		}
		else
		{
			if (lowMiddleChance > 0.5f)
			{
				lowMiddleChance = 0.5f;
			}
			lowMiddleChance -= 0.25f;
			anim.SetTrigger("SlamMiddleLow");
			attackingRight = true;
		}
	}

	public void SwingStart()
	{
		if (attackingRight)
		{
			SwingCheck2[] array = scRight;
			foreach (SwingCheck2 obj in array)
			{
				obj.damage = 45;
				obj.DamageStart();
			}
			stat.partiallyParryable = true;
			Transform[] array2 = rightHandBones;
			foreach (Transform item in array2)
			{
				stat.parryables.Add(item);
			}
		}
		if (attackingLeft)
		{
			SwingCheck2[] array = scLeft;
			foreach (SwingCheck2 obj2 in array)
			{
				obj2.damage = 45;
				obj2.DamageStart();
			}
			stat.partiallyParryable = true;
			Transform[] array2 = leftHandBones;
			foreach (Transform item2 in array2)
			{
				stat.parryables.Add(item2);
			}
		}
	}

	public void SpecialDeath()
	{
		inAction = true;
		dead = true;
		anim.Play("Death");
		if (currentBlackHole != null)
		{
			currentBlackHole.Explode();
		}
		Parasite[] array = parasites;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		Object.Instantiate(bigHurtSound, head).GetComponent<AudioSource>().pitch = 0.75f;
		MonoSingleton<CameraController>.Instance.CameraShake(0.5f);
	}

	public void Impact()
	{
		Object.Instantiate(punchExplosion, head);
		MonoSingleton<CameraController>.Instance.CameraShake(3f);
		onDeathImpact?.Invoke();
	}

	public void DeathOver()
	{
		onDeathOver?.Invoke();
	}

	public void SwingEnd()
	{
		MonoSingleton<CameraController>.Instance.CameraShake(2f);
		List<Transform> list = new List<Transform>();
		if (attackingRight)
		{
			SwingCheck2[] array = scRight;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStop();
			}
			list.Add(rightHand.transform);
		}
		if (attackingLeft)
		{
			SwingCheck2[] array = scLeft;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStop();
			}
			list.Add(leftHand.transform);
		}
		stat.partiallyParryable = false;
		stat.parryables.Clear();
		foreach (Transform item in list)
		{
			if (Physics.Raycast(item.position, item.up, out var hitInfo, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
			{
				Object.Instantiate(punchExplosion, hitInfo.point, Quaternion.identity);
			}
			else
			{
				Object.Instantiate(punchExplosion, item.position, Quaternion.identity);
			}
		}
		list.Clear();
	}

	private void BlackHole()
	{
		blackHoleCooldown = 5f;
		inAction = true;
		anim.SetTrigger("SpawnBlackHole");
	}

	public void SpawnBlackHole()
	{
		if (!inPhaseChange)
		{
			GameObject gameObject = Object.Instantiate(blackHole, blackHoleSpawnPos);
			currentBlackHole = gameObject.GetComponent<BlackHoleProjectile>();
			currentBlackHole.FadeIn();
			currentBlackHole.speed *= eid.totalSpeedModifier;
		}
	}

	public void LaunchBlackHole()
	{
		currentBlackHole.Activate();
	}

	public void GotParried()
	{
		if (dead)
		{
			return;
		}
		Object.Instantiate(bigHurtSound, head);
		if (!beenParried)
		{
			beenParried = true;
			if (parryChallenge)
			{
				MonoSingleton<ChallengeManager>.Instance.ChallengeDone();
			}
		}
		MonoSingleton<StyleHUD>.Instance.AddPoints(500, "ultrakill.downtosize", null, eid);
		if (attackingRight)
		{
			SwingCheck2[] array = scRight;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStop();
			}
			anim.SetTrigger("ParryRight");
			eid.hitter = "";
			Transform[] array2 = rightHandBones;
			foreach (Transform transform in array2)
			{
				stat.GetHurt(transform.gameObject, Vector3.zero, 35 / rightHandBones.Length, 0f, transform.position);
				transform.gameObject.layer = 10;
			}
		}
		if (attackingLeft)
		{
			SwingCheck2[] array = scLeft;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStop();
			}
			anim.SetTrigger("ParryLeft");
			eid.hitter = "";
			Transform[] array2 = leftHandBones;
			foreach (Transform transform2 in array2)
			{
				stat.GetHurt(transform2.gameObject, Vector3.zero, 35 / leftHandBones.Length, 0f, transform2.position);
				transform2.gameObject.layer = 10;
			}
		}
		stat.partiallyParryable = false;
		stat.parryables.Clear();
		eid.hitter = "";
	}

	public void ResetColliders()
	{
		if (attackingRight)
		{
			Transform[] array = rightHandBones;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.layer = 11;
			}
		}
		if (attackingLeft)
		{
			Transform[] array = leftHandBones;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.layer = 11;
			}
		}
	}

	public void StopAction()
	{
		attackingLeft = false;
		attackingRight = false;
		inAction = false;
	}

	public void PlayerInZone(int zone)
	{
		switch (zone)
		{
		case 0:
			onRight = true;
			break;
		case 1:
			onMiddle = true;
			break;
		case 2:
			onLeft = true;
			break;
		}
	}

	public void PlayerExitZone(int zone)
	{
		switch (zone)
		{
		case 0:
			onRight = false;
			break;
		case 1:
			onMiddle = false;
			break;
		case 2:
			onLeft = false;
			break;
		}
	}

	private void PhaseChange(int targetPhase)
	{
		phase = targetPhase;
		inAction = true;
		cooldown = 4f;
		Object.Instantiate(bigHurtSound, head);
		if (phase == 2)
		{
			anim.SetTrigger("PhaseParasite");
			if (attackingRight)
			{
				SwingCheck2[] array = scRight;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].DamageStop();
				}
			}
			if (attackingLeft)
			{
				SwingCheck2[] array = scLeft;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].DamageStop();
				}
			}
			stat.partiallyParryable = false;
			stat.parryables.Clear();
			if ((bool)currentBlackHole && (difficulty <= 2 || currentBlackHole.fadingIn))
			{
				currentBlackHole.Explode();
			}
		}
		else
		{
			inPhaseChange = false;
		}
	}

	public void ShutEye(int eye)
	{
		eyes[eye].SetActive(value: false);
		if (eye == 1)
		{
			SkinnedMeshRenderer componentInChildren = GetComponentInChildren<SkinnedMeshRenderer>();
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			componentInChildren.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetTexture("_MainTex", eyeless.GetTexture("_MainTex"));
			componentInChildren.SetPropertyBlock(materialPropertyBlock, 0);
			eid.UpdateBuffs(visualsOnly: true);
		}
	}

	public void SpawnParasites()
	{
		MonoSingleton<CameraController>.Instance.CameraShake(2f);
		GoreZone componentInParent = GetComponentInParent<GoreZone>();
		GameObject[] array = eyes;
		foreach (GameObject gameObject in array)
		{
			for (int j = 0; j < 3; j++)
			{
				GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, eid.underwater, eid.sandified, eid.blessed);
				if ((bool)gore)
				{
					gore.transform.position = gameObject.transform.position;
					gore.transform.localScale = gore.transform.localScale * 3f;
					if ((bool)componentInParent)
					{
						gore.transform.SetParent(componentInParent.goreZone, worldPositionStays: true);
					}
					else
					{
						gore.transform.parent = null;
					}
				}
			}
		}
		Parasite[] array2 = parasites;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].gameObject.SetActive(value: true);
		}
		inPhaseChange = false;
		eid.weakPoint = parasites[0].GetComponentInChildren<EnemyIdentifierIdentifier>().gameObject;
	}

	public void PlayerBeenHit()
	{
		if (attackingRight)
		{
			SwingCheck2[] array = scRight;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStop();
			}
			Transform[] array2 = rightHandBones;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].gameObject.layer = 10;
			}
		}
		if (attackingLeft)
		{
			SwingCheck2[] array = scLeft;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DamageStop();
			}
			Transform[] array2 = leftHandBones;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].gameObject.layer = 10;
			}
		}
	}
}
