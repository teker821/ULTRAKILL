using ULTRAKILL.Cheats;
using UnityEngine;

public class LeviathanTail : MonoBehaviour
{
	private SwingCheck2[] tails;

	public Vector3[] spawnPositions;

	private int previousSpawnPosition;

	private Animator anim;

	[HideInInspector]
	public LeviathanController lcon;

	[SerializeField]
	private AudioSource swingSound;

	[SerializeField]
	private AudioSource[] spawnAuds;

	[SerializeField]
	private AudioClip swingHighSound;

	[SerializeField]
	private AudioClip swingLowSound;

	private bool blinded;

	private void Awake()
	{
		tails = GetComponentsInChildren<SwingCheck2>();
		anim = GetComponent<Animator>();
		EnemyIdentifier component = (lcon ? lcon.eid : (component = lcon.GetComponent<EnemyIdentifier>()));
		SwingCheck2[] array = tails;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].eid = component;
		}
	}

	private void Update()
	{
		if (blinded && !BlindEnemies.Blind)
		{
			blinded = false;
			anim.speed = GetAnimSpeed() * lcon.eid.totalSpeedModifier;
			AudioSource[] array = spawnAuds;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
		}
	}

	public void PlayerBeenHit()
	{
		SwingCheck2[] array = tails;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DamageStop();
		}
	}

	private void SwingStart()
	{
		SwingCheck2[] array = tails;
		foreach (SwingCheck2 obj in array)
		{
			obj.DamageStart();
			obj.col.isTrigger = true;
		}
		Object.Instantiate(swingSound, base.transform.position, Quaternion.identity).pitch = 0.5f;
	}

	public void SwingEnd()
	{
		if (tails != null && tails.Length != 0)
		{
			SwingCheck2[] array = tails;
			foreach (SwingCheck2 obj in array)
			{
				obj.DamageStop();
				obj.col.isTrigger = false;
			}
		}
	}

	private void ActionOver()
	{
		base.gameObject.SetActive(value: false);
		lcon.SubAttackOver();
	}

	public void ChangePosition()
	{
		int num = Random.Range(0, spawnPositions.Length);
		if (spawnPositions.Length > 1 && num == previousSpawnPosition)
		{
			num++;
		}
		if (num >= spawnPositions.Length)
		{
			num = 0;
		}
		if ((bool)lcon.head && lcon.head.gameObject.activeInHierarchy && Vector3.Distance(spawnPositions[num], new Vector3(lcon.head.transform.localPosition.x, spawnPositions[num].y, lcon.head.transform.localPosition.z)) < 10f)
		{
			num++;
		}
		if (num >= spawnPositions.Length)
		{
			num = 0;
		}
		base.transform.localPosition = spawnPositions[num];
		previousSpawnPosition = num;
		bool flag = Random.Range(0f, 1f) > 0.5f;
		base.transform.localPosition += (flag ? (Vector3.up * -30.5f) : (Vector3.up * -4.5f));
		base.transform.localScale = new Vector3((!flag) ? 1 : (-1), 1f, 1f);
		if (lcon.difficulty <= 2)
		{
			spawnAuds[0].clip = (flag ? swingLowSound : swingHighSound);
		}
		base.transform.rotation = Quaternion.LookRotation(base.transform.position - new Vector3(base.transform.parent.position.x, base.transform.position.y, base.transform.parent.position.z));
		base.gameObject.SetActive(value: true);
		anim.Rebind();
		anim.Update(0f);
		if (BlindEnemies.Blind)
		{
			blinded = true;
			anim.speed = 0f;
			return;
		}
		anim.speed = GetAnimSpeed() * lcon.eid.totalSpeedModifier;
		AudioSource[] array = spawnAuds;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	private void BigSplash()
	{
		Object.Instantiate(lcon.bigSplash, new Vector3(base.transform.position.x, base.transform.position.y, base.transform.position.z), Quaternion.LookRotation(Vector3.up));
	}

	private float GetAnimSpeed()
	{
		if (lcon.difficulty >= 3)
		{
			return 1f;
		}
		return lcon.difficulty switch
		{
			2 => 0.85f, 
			1 => 0.65f, 
			0 => 0.45f, 
			_ => 1f, 
		};
	}
}
