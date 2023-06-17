using System.Collections.Generic;
using UnityEngine.Events;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class UnlockablesData : MonoSingleton<UnlockablesData>
{
	public UnityAction unlockableFound = delegate
	{
	};

	private bool checkedSave;

	private readonly HashSet<UnlockableType> unlocked = new HashSet<UnlockableType>();

	private void InitDictionary()
	{
		unlocked.Clear();
	}

	private void Start()
	{
		if (!checkedSave)
		{
			CheckSave();
		}
	}

	public bool IsUnlocked(UnlockableType unlockable)
	{
		if (!checkedSave)
		{
			CheckSave();
		}
		return unlocked.Contains(unlockable);
	}

	public void SetUnlocked(UnlockableType unlockable, bool unlocked)
	{
		if (!checkedSave)
		{
			CheckSave();
		}
		if (unlocked && !this.unlocked.Contains(unlockable))
		{
			this.unlocked.Add(unlockable);
			GameProgressSaver.SetUnlockable(unlockable, state: true);
			unlockableFound();
		}
		else if (!unlocked && this.unlocked.Contains(unlockable))
		{
			this.unlocked.Remove(unlockable);
			GameProgressSaver.SetUnlockable(unlockable, state: false);
			unlockableFound();
		}
	}

	public void CheckSave()
	{
		checkedSave = true;
		InitDictionary();
		UnlockableType[] unlockables = GameProgressSaver.GetUnlockables();
		for (int i = 0; i < unlockables.Length; i++)
		{
			unlocked.Add(unlockables[i]);
		}
	}
}
