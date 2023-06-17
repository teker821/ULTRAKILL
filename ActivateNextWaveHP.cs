using UnityEngine;

public class ActivateNextWaveHP : MonoBehaviour
{
	public bool lastWave;

	private bool activated;

	public EnemyIdentifier target;

	public float health;

	public GameObject[] nextEnemies;

	private int currentEnemy;

	public Door[] doors;

	private int currentDoor;

	public GameObject[] toActivate;

	private bool objectsActivated;

	public Door doorForward;

	private float slowDown = 1f;

	public bool forEnemies;

	private void Update()
	{
		if (activated || ((bool)target && (!target.gameObject.activeInHierarchy || !(target.health <= health))))
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
			Invoke("SpawnEnemy", 1f);
		}
		else
		{
			Invoke("EndWaves", 1f);
			if (!forEnemies)
			{
				MonoSingleton<TimeController>.Instance.SlowDown(0.15f);
			}
		}
	}

	private void SpawnEnemy()
	{
		if (nextEnemies.Length != 0 && nextEnemies.Length > currentEnemy)
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
				MonoSingleton<AudioMixerController>.Instance.allSound.SetFloat("allPitch", slowDown);
				MonoSingleton<AudioMixerController>.Instance.doorSound.SetFloat("allPitch", slowDown);
			}
			Object.Destroy(this);
		}
	}
}
