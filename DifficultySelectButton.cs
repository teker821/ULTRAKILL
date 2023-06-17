using UnityEngine;
using UnityEngine.UI;

public class DifficultySelectButton : MonoBehaviour
{
	public int difficulty;

	private int stage;

	private int level;

	private void Start()
	{
		level = GameProgressSaver.GetProgress(difficulty);
		if (level > 5)
		{
			level -= 5;
			stage = 1;
			while (level > 10)
			{
				level -= 10;
				stage += 3;
			}
			while (level > 4)
			{
				level -= 4;
				stage++;
			}
		}
		base.transform.Find("Progress").GetComponent<Text>().text = stage + "-" + level;
	}

	public void SetDifficulty()
	{
		MonoSingleton<PrefsManager>.Instance.SetInt("difficulty", difficulty);
		Debug.Log("Set Difficulty to: " + difficulty);
	}
}
