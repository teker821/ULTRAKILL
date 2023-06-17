using UnityEngine;

public class ActivateNextWave : MonoBehaviour
{
	public bool lastWave;

	private bool activated;

	public int deadEnemies;

	public int enemyCount;

	private ActivateNextWave[] linkedAnws;

	public GameObject[] nextEnemies;

	private int currentEnemy;

	public Door[] doors;

	private int currentDoor;

	public GameObject[] toActivate;

	private bool objectsActivated;

	public Door doorForward;

	private float slowDown = 1f;

	public bool forEnemies;

	public bool killChallenge;

	public bool noActivationDelay;

	public void CountEnemies()
	{
		enemyCount = base.transform.childCount;
	}

	private void Awake()
	{
		linkedAnws = GetComponents<ActivateNextWave>();
	}

	private void FixedUpdate()
	{
		if (deadEnemies < 0)
		{
			deadEnemies = 0;
		}
		if (activated || deadEnemies < enemyCount)
		{
			return;
		}
		activated = true;
		if (!lastWave)
		{
			if (toActivate.Length != 0)
			{
				GameObject[] array = toActivate;
				foreach (GameObject gameObject in array)
				{
					if (gameObject != null)
					{
						gameObject.SetActive(value: true);
					}
				}
			}
			Invoke("SpawnEnemy", (!noActivationDelay) ? 1 : 0);
		}
		else
		{
			Invoke("EndWaves", (!noActivationDelay) ? 1 : 0);
			if (!forEnemies)
			{
				MonoSingleton<TimeController>.Instance.SlowDown(0.15f);
			}
		}
	}

	private void SpawnEnemy()
	{
		if (nextEnemies.Length != 0)
		{
			if (nextEnemies[currentEnemy] != null)
			{
				nextEnemies[currentEnemy].SetActive(value: true);
			}
			currentEnemy++;
		}
		if (currentEnemy < nextEnemies.Length)
		{
			Invoke("SpawnEnemy", 0.1f);
		}
		else
		{
			Object.Destroy(this);
		}
	}

	private void EndWaves()
	{
		if (toActivate.Length != 0 && !objectsActivated)
		{
			GameObject[] array = toActivate;
			foreach (GameObject gameObject in array)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(value: true);
				}
			}
			objectsActivated = true;
			EndWaves();
		}
		else if (currentDoor < doors.Length)
		{
			doors[currentDoor].Unlock();
			if (doors[currentDoor] == doorForward)
			{
				doors[currentDoor].Open(enemy: false, skull: true);
			}
			currentDoor++;
			Invoke("EndWaves", 0.1f);
		}
		else
		{
			if (!forEnemies)
			{
				MonoSingleton<MusicManager>.Instance.ArenaMusicEnd();
				slowDown = 1f;
			}
			if (killChallenge)
			{
				MonoSingleton<ChallengeManager>.Instance.ChallengeDone();
			}
			Object.Destroy(this);
		}
	}

	public void AddDeadEnemy()
	{
		deadEnemies++;
		if (linkedAnws.Length <= 1)
		{
			return;
		}
		ActivateNextWave[] array = linkedAnws;
		foreach (ActivateNextWave activateNextWave in array)
		{
			if (activateNextWave != this)
			{
				activateNextWave.deadEnemies++;
			}
		}
	}
}
