using UnityEngine;
using UnityEngine.Events;

public class LimboSwitchLock : MonoBehaviour
{
	public SwitchLockType type;

	public MeshRenderer[] locks;

	public MeshRenderer[] primeBossLocks;

	private MaterialPropertyBlock block;

	private float[] lockIntensities;

	private bool[] lockStates;

	private int openedLocks;

	public UnityEvent onAllLocksOpen;

	public int minimumOrderNumber;

	public int primeBossLockNumber = 1;

	private void Start()
	{
		block = new MaterialPropertyBlock();
		lockIntensities = new float[locks.Length + ((primeBossLocks != null && primeBossLocks.Length != 0) ? 1 : 0)];
		lockStates = new bool[locks.Length + ((primeBossLocks != null && primeBossLocks.Length != 0) ? 1 : 0)];
		if (type != 0)
		{
			if (type == SwitchLockType.Limbo)
			{
				for (int i = 0; i < locks.Length; i++)
				{
					if (GameProgressSaver.GetLimboSwitch(i + minimumOrderNumber))
					{
						lockIntensities[i] = 2f;
						lockStates[i] = true;
						locks[i].GetPropertyBlock(block);
						block.SetFloat(UKShaderProperties.EmissiveIntensity, 2f);
						locks[i].SetPropertyBlock(block);
						openedLocks++;
					}
				}
			}
			else if (type == SwitchLockType.PRank)
			{
				int @int = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
				for (int j = 0; j < locks.Length; j++)
				{
					RankData rank = GameProgressSaver.GetRank(j + minimumOrderNumber + 1, returnNull: true);
					if (rank != null && rank.ranks[@int] == 12 && (rank.majorAssists == null || !rank.majorAssists[@int]))
					{
						lockIntensities[j] = 2f;
						lockStates[j] = true;
						locks[j].GetPropertyBlock(block);
						block.SetFloat(UKShaderProperties.EmissiveIntensity, 2f);
						locks[j].SetPropertyBlock(block);
						openedLocks++;
					}
				}
				if (primeBossLocks.Length != 0)
				{
					RankData rank2 = GameProgressSaver.GetRank(primeBossLockNumber + 665, returnNull: true);
					if (rank2 != null && rank2.ranks[@int] >= 0 && (rank2.majorAssists == null || !rank2.majorAssists[@int]))
					{
						lockIntensities[locks.Length] = 2f;
						lockStates[locks.Length] = true;
						MeshRenderer[] array = primeBossLocks;
						foreach (MeshRenderer obj in array)
						{
							obj.GetPropertyBlock(block);
							block.SetFloat(UKShaderProperties.EmissiveIntensity, 2f);
							obj.SetPropertyBlock(block);
						}
						openedLocks++;
					}
				}
			}
		}
		CheckLocks();
	}

	private void Update()
	{
		for (int i = 0; i < locks.Length; i++)
		{
			if (lockStates[i] && lockIntensities[i] != 2f)
			{
				lockIntensities[i] = Mathf.MoveTowards(lockIntensities[i], 2f, Time.deltaTime);
				locks[i].GetPropertyBlock(block);
				block.SetFloat(UKShaderProperties.EmissiveIntensity, lockIntensities[i]);
				locks[i].SetPropertyBlock(block);
				if (lockIntensities[i] == 2f)
				{
					openedLocks++;
					CheckLocks();
				}
			}
		}
	}

	private void CheckLocks()
	{
		if (openedLocks == locks.Length + ((primeBossLocks != null && primeBossLocks.Length != 0) ? 1 : 0))
		{
			onAllLocksOpen?.Invoke();
		}
	}

	public void OpenLock(int num)
	{
		lockStates[num - 1] = true;
		locks[num - 1].GetComponent<AudioSource>()?.Play();
	}
}
