using UnityEngine;
using UnityEngine.UI;

public class EndlessHighScore : MonoBehaviour
{
	private Text text;

	private void OnEnable()
	{
		if (!text)
		{
			text = GetComponent<Text>();
		}
		if ((bool)text)
		{
			int num = (int)Mathf.Floor(GameProgressSaver.GetBestCyber().preciseWavesByDifficulty[MonoSingleton<PrefsManager>.Instance.GetInt("difficulty", 2)]);
			if (num <= 0)
			{
				text.text = "";
			}
			else
			{
				text.text = string.Concat(num);
			}
		}
	}
}
