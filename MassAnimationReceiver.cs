using UnityEngine;
using UnityEngine.Events;

public class MassAnimationReceiver : MonoBehaviour
{
	public GameObject groundBreakEffect;

	public GameObject smallGroundBreakEffect;

	public bool fakeMass;

	public bool otherBossIntro;

	public GameObject realMass;

	private Mass mass;

	public GameObject footstep;

	private Animator anim;

	private StatueIntroChecker sic;

	private bool turnTowards;

	private Transform player;

	private int breaks;

	public bool skipEntirelyOnReplay;

	public UnityEvent animationEvent1;

	public UnityEvent onSkip;

	public void Start()
	{
		anim = GetComponent<Animator>();
		if (fakeMass)
		{
			sic = MonoSingleton<StatueIntroChecker>.Instance;
			if (!otherBossIntro)
			{
				anim.speed = 0f;
			}
		}
		else
		{
			mass = GetComponentInParent<Mass>();
		}
		if ((bool)sic && sic.beenSeen && skipEntirelyOnReplay)
		{
			onSkip?.Invoke();
			SpawnMass();
		}
	}

	private void Update()
	{
		if (turnTowards)
		{
			Quaternion b = Quaternion.LookRotation(new Vector3(player.position.x, base.transform.position.y, player.position.z) - base.transform.position, Vector3.up);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * (Quaternion.Angle(base.transform.rotation, b) / 2f + 1f));
		}
	}

	public void GroundBreak()
	{
		Object.Instantiate(groundBreakEffect, base.transform.position, Quaternion.identity);
		breaks++;
		if (breaks == 3)
		{
			player = MonoSingleton<CameraController>.Instance.transform;
			turnTowards = true;
		}
	}

	public void SmallGroundBreak()
	{
		Object.Instantiate(smallGroundBreakEffect, base.transform.position, Quaternion.identity);
	}

	public void SpawnMass()
	{
		if ((bool)sic && !sic.beenSeen)
		{
			sic.beenSeen = true;
		}
		realMass.SetActive(value: true);
		base.gameObject.SetActive(value: false);
	}

	public void Footstep()
	{
		if (anim.GetLayerWeight(1) > 0.5f)
		{
			Object.Instantiate(footstep, base.transform.position, Quaternion.identity);
		}
	}

	public void SkipOnReplay()
	{
		if ((bool)sic && sic.beenSeen)
		{
			SpawnMass();
		}
	}

	public void AnimationEvent(int i)
	{
		if (i == 1)
		{
			animationEvent1?.Invoke();
		}
	}

	public void ShootSpear()
	{
		mass.ShootSpear();
	}

	public void StopAction()
	{
		mass.StopAction();
	}

	public void ShootHomingR()
	{
		mass.ShootHoming(0);
	}

	public void ShootHomingL()
	{
		mass.ShootHoming(1);
	}

	public void ShootExplosiveR()
	{
		mass.ShootExplosive(0);
	}

	public void ShootExplosiveL()
	{
		mass.ShootExplosive(1);
	}

	public void Slam()
	{
		mass.SlamImpact();
	}

	public void SwingStart()
	{
		mass.SwingStart();
	}

	public void SwingEnd()
	{
		mass.SwingEnd();
	}

	public void CrazyReady()
	{
		mass.CrazyReady();
	}

	public void Enrage()
	{
		mass.Enrage();
	}
}
