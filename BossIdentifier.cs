using UnityEngine;

public class BossIdentifier : MonoBehaviour
{
	private EnemyIdentifier eid;

	private void Awake()
	{
		CheckDifficultyOverride();
	}

	private void OnEnable()
	{
		CheckDifficultyOverride();
	}

	public void CheckDifficultyOverride()
	{
		if (!eid && !TryGetComponent<EnemyIdentifier>(out eid))
		{
			Object.Destroy(this);
		}
		if (MonoSingleton<AssistController>.Instance.majorEnabled)
		{
			eid.difficultyOverride = MonoSingleton<AssistController>.Instance.difficultyOverride;
		}
		else
		{
			eid.difficultyOverride = -1;
		}
	}
}
