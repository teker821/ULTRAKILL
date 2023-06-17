using UnityEngine;

public class RaceRingTracker : MonoBehaviour
{
	public int raceRingAmount;

	private int currentRaceRings;

	private bool complete;

	private float time;

	private HudMessage hm;

	public UltrakillEvent onVictory;

	public bool infiniteRocketRide;

	private void Start()
	{
		hm = base.gameObject.AddComponent<HudMessage>();
		hm.timed = true;
		hm.message = "RACE START";
		if (infiniteRocketRide)
		{
			MonoSingleton<WeaponCharges>.Instance.infiniteRocketRide = true;
		}
	}

	private void Update()
	{
		if (!complete)
		{
			time += Time.deltaTime;
		}
	}

	public void AddScore()
	{
		currentRaceRings++;
		if (!complete && currentRaceRings == raceRingAmount)
		{
			Victory();
		}
	}

	public void Victory()
	{
		complete = true;
		hm = base.gameObject.AddComponent<HudMessage>();
		hm.timed = true;
		hm.message = "TIME: <color=lime>" + time.ToString("F3") + "</color>";
		if (infiniteRocketRide)
		{
			MonoSingleton<WeaponCharges>.Instance.infiniteRocketRide = false;
		}
		onVictory?.Invoke();
	}
}
