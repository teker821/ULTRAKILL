using ULTRAKILL.Cheats;
using UnityEngine;

public class LeviathanController : MonoBehaviour
{
	[HideInInspector]
	public bool active = true;

	public LeviathanHead head;

	[SerializeField]
	private Transform headWeakPoint;

	public LeviathanTail tail;

	[SerializeField]
	private Transform tailWeakPoint;

	[HideInInspector]
	public EnemyIdentifier eid;

	[HideInInspector]
	public Statue stat;

	public float phaseChangeHealth;

	private bool inSubPhase;

	private bool secondPhase;

	private int currentAttacks;

	private int setDifficulty = -1;

	public UltrakillEvent onEnterSecondPhase;

	[SerializeField]
	private Transform tailPartsParent;

	[SerializeField]
	private Transform headPartsParent;

	private Transform[] tailParts;

	private Transform[] headParts;

	private int currentPart;

	private GoreZone gz;

	private bool shaking;

	private Vector3 defaultPosition;

	public UltrakillEvent onDeathEnd;

	public GameObject bigSplash;

	[HideInInspector]
	public int difficulty
	{
		get
		{
			return GetDifficulty();
		}
		set
		{
			setDifficulty = value;
		}
	}

	private void Awake()
	{
		eid = GetComponent<EnemyIdentifier>();
		stat = GetComponent<Statue>();
		tail.lcon = this;
		head.lcon = this;
	}

	private void UpdateBuff()
	{
		head.SetSpeed();
	}

	private int GetDifficulty()
	{
		if (setDifficulty < 0)
		{
			difficulty = ((eid.difficultyOverride >= 0) ? eid.difficultyOverride : MonoSingleton<PrefsManager>.Instance.GetInt("difficulty", 2));
		}
		return setDifficulty;
	}

	private void OnDestroy()
	{
		if (eid.dead)
		{
			DeathEnd();
		}
	}

	private void Update()
	{
		if (shaking)
		{
			base.transform.localPosition = defaultPosition + Random.onUnitSphere * 0.5f;
		}
		if (active && !secondPhase && stat.health <= phaseChangeHealth)
		{
			secondPhase = true;
			onEnterSecondPhase?.Invoke();
			if (!inSubPhase)
			{
				BeginSubPhase();
			}
			else
			{
				BeginMainPhase();
			}
		}
	}

	private void BeginMainPhase()
	{
		if (active)
		{
			if (!secondPhase)
			{
				inSubPhase = false;
			}
			head.ChangePosition();
			eid.weakPoint = headWeakPoint.gameObject;
		}
	}

	public void MainPhaseOver()
	{
		if (active)
		{
			if (!secondPhase)
			{
				BeginSubPhase();
			}
			else
			{
				BeginMainPhase();
			}
		}
	}

	public void BeginSubPhase()
	{
		if (!inSubPhase && active)
		{
			if (!secondPhase)
			{
				eid.weakPoint = tailWeakPoint.gameObject;
			}
			inSubPhase = true;
			currentAttacks = 2;
			SubAttack();
		}
	}

	private void SubAttack()
	{
		if (active)
		{
			if (!BlindEnemies.Blind || secondPhase)
			{
				tail.ChangePosition();
			}
			else
			{
				BeginMainPhase();
			}
		}
	}

	public void SubAttackOver()
	{
		if (!active)
		{
			return;
		}
		if (BlindEnemies.Blind)
		{
			currentAttacks = 0;
		}
		if (!secondPhase)
		{
			currentAttacks--;
			if (currentAttacks <= 0)
			{
				BeginMainPhase();
			}
			else
			{
				SubAttack();
			}
		}
		else if (difficulty <= 2)
		{
			Invoke("SubAttack", (10f - (float)difficulty * 2.5f) / eid.totalSpeedModifier);
		}
		else
		{
			SubAttack();
		}
	}

	private void SpecialDeath()
	{
		headParts = headPartsParent.GetComponentsInChildren<Transform>();
		tailParts = tailPartsParent.GetComponentsInChildren<Transform>();
		Animator[] componentsInChildren = GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i]);
		}
		tail.SwingEnd();
		head.BiteDamageStop();
		active = false;
		head.active = false;
		defaultPosition = base.transform.localPosition;
		shaking = true;
		currentPart = tailParts.Length - 1;
		if (tail.gameObject.activeSelf)
		{
			ExplodeTail();
			return;
		}
		currentPart = headParts.Length - 1;
		ExplodeHead();
	}

	private void ExplodeTail()
	{
		if (tailParts[currentPart] == null)
		{
			currentPart--;
			ExplodeTail();
			return;
		}
		bool flag = true;
		if (currentPart >= 0)
		{
			flag = tailParts[currentPart].position.y > base.transform.position.y - 5f;
			tailParts[currentPart].localScale = Vector3.zero;
			tailParts[currentPart].localPosition = Vector3.zero;
			if (flag)
			{
				GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, isUnderwater: false, eid.sandified, eid.blessed);
				gore.transform.position = tailParts[currentPart].position;
				gore.transform.localScale *= 2f;
				if (!gz)
				{
					gz = GoreZone.ResolveGoreZone(base.transform);
				}
				gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
				if (gore.TryGetComponent<AudioSource>(out var component))
				{
					component.maxDistance = 500f;
				}
				gore.SetActive(value: true);
			}
		}
		if (currentPart > 0)
		{
			currentPart = Mathf.Max(0, currentPart - 2);
			if (flag)
			{
				Invoke("ExplodeTail", 0.125f / eid.totalSpeedModifier);
			}
			else
			{
				Invoke("ExplodeTail", 0.025f / eid.totalSpeedModifier);
			}
		}
		else
		{
			tail.gameObject.SetActive(value: false);
			currentPart = headParts.Length - 1;
			Invoke("ExplodeHead", 0.125f / eid.totalSpeedModifier);
		}
	}

	private void ExplodeHead()
	{
		if (headParts[currentPart] == null)
		{
			if (currentPart > 0)
			{
				currentPart--;
				ExplodeHead();
			}
			else
			{
				Invoke("FinalExplosion", 0.5f / eid.totalSpeedModifier);
			}
			return;
		}
		bool flag = true;
		if (currentPart >= 0)
		{
			flag = headParts[currentPart].position.y > base.transform.position.y - 5f;
			headParts[currentPart].localScale = Vector3.zero;
			headParts[currentPart].localPosition = Vector3.zero;
			if (flag)
			{
				GameObject gore = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, isUnderwater: false, eid.sandified, eid.blessed);
				gore.transform.position = headParts[currentPart].position;
				gore.transform.localScale *= 2f;
				if (!gz)
				{
					gz = GoreZone.ResolveGoreZone(base.transform);
				}
				gore.transform.SetParent(gz.goreZone, worldPositionStays: true);
				if (gore.TryGetComponent<AudioSource>(out var component))
				{
					component.maxDistance = 500f;
				}
				gore.SetActive(value: true);
			}
		}
		if (currentPart > 0)
		{
			currentPart = Mathf.Max(0, currentPart - 2);
			if (flag)
			{
				Invoke("ExplodeHead", 0.125f / eid.totalSpeedModifier);
			}
			else
			{
				Invoke("ExplodeHead", 0.025f / eid.totalSpeedModifier);
			}
		}
		else
		{
			Invoke("FinalExplosion", 0.5f / eid.totalSpeedModifier);
		}
	}

	private void FinalExplosion()
	{
		Transform[] componentsInChildren = head.tracker.GetComponentsInChildren<Transform>();
		GameObject gameObject = null;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] == null)
			{
				continue;
			}
			for (int j = 0; j < 3; j++)
			{
				switch (j)
				{
				case 0:
					gameObject = MonoSingleton<BloodsplatterManager>.Instance.GetGore(GoreType.Head, isUnderwater: false, eid.sandified, eid.blessed);
					break;
				case 1:
					gameObject = MonoSingleton<BloodsplatterManager>.Instance.GetGib(GibType.Gib);
					break;
				case 2:
					gameObject = MonoSingleton<BloodsplatterManager>.Instance.GetGib((GibType)Random.Range(0, 5));
					break;
				}
				if (!(gameObject == null))
				{
					gameObject.transform.position = componentsInChildren[i].position;
					gameObject.transform.localScale *= (float)((j == 0) ? 3 : 15);
					if (!gz)
					{
						gz = GoreZone.ResolveGoreZone(base.transform);
					}
					gameObject.transform.SetParent(gz.goreZone, worldPositionStays: true);
					gameObject.SetActive(value: true);
				}
			}
		}
		MonoSingleton<TimeController>.Instance.SlowDown(0.0001f);
		DeathEnd();
	}

	public void DeathEnd()
	{
		onDeathEnd?.Invoke();
		base.gameObject.SetActive(value: false);
	}
}
