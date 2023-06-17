using System;
using System.Collections.Generic;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
public class BestiaryData : MonoSingleton<BestiaryData>
{
	private bool checkedSave;

	private Dictionary<EnemyType, int> foundEnemies = new Dictionary<EnemyType, int>();

	private void InitDictionary()
	{
		foundEnemies.Clear();
		foreach (object value in Enum.GetValues(typeof(EnemyType)))
		{
			foundEnemies.Add((EnemyType)value, 0);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.AddComponent<UnlockablesData>();
	}

	private void Start()
	{
		if (!checkedSave)
		{
			CheckSave();
		}
	}

	public int GetEnemy(EnemyType enemy)
	{
		if (!checkedSave)
		{
			CheckSave();
		}
		return foundEnemies[enemy];
	}

	public void SetEnemy(EnemyType enemy, int newState = 2)
	{
		if (!checkedSave)
		{
			CheckSave();
		}
		if (foundEnemies[enemy] >= newState)
		{
			return;
		}
		foundEnemies[enemy] = newState;
		GameProgressSaver.SetBestiary(enemy, newState);
		foreach (EnemyInfoPage instance in ListComponent<EnemyInfoPage>.InstanceList)
		{
			instance.UpdateInfo();
			instance.DisplayInfo();
		}
	}

	public void CheckSave()
	{
		checkedSave = true;
		InitDictionary();
		int[] bestiary = GameProgressSaver.GetBestiary();
		for (int i = 0; i < bestiary.Length; i++)
		{
			int value = bestiary[i];
			foundEnemies[(EnemyType)i] = value;
		}
	}
}
