using System;

[Serializable]
public class GameProgressData
{
	public int levelNum;

	public int difficulty;

	public int[] primeLevels;

	public GameProgressData()
	{
		difficulty = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
	}
}
