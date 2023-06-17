using UnityEngine;
using UnityEngine.UI;

public class LayerSelect : MonoBehaviour
{
	public SecretMissionPanel secretMissionPanel;

	public int layerNumber;

	public int levelAmount;

	private float totalScore;

	private float scoresChecked;

	private int perfects;

	public int golds;

	private bool secretMission;

	public Text rankText;

	public bool gold;

	public bool allPerfects;

	public int trueScore;

	public bool complete;

	public bool noSecretMission;

	private void OnDisable()
	{
		totalScore = 0f;
		scoresChecked = 0f;
		perfects = 0;
		golds = 0;
		rankText.text = "";
		Image component = rankText.transform.parent.GetComponent<Image>();
		component.color = Color.white;
		component.fillCenter = false;
		secretMission = false;
		GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
	}

	public void CheckScore()
	{
		totalScore = 0f;
		trueScore = 0;
		scoresChecked = 0f;
		perfects = 0;
		golds = 0;
		complete = false;
		if (rankText == null)
		{
			Debug.LogError("Rank Text is null", base.gameObject);
		}
		rankText.text = "";
		Image component = rankText.transform.parent.GetComponent<Image>();
		component.color = Color.white;
		component.fillCenter = false;
		secretMission = false;
		GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
		LevelSelectPanel[] componentsInChildren = GetComponentsInChildren<LevelSelectPanel>();
		secretMissionPanel.GotEnabled();
		LevelSelectPanel[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CheckScore();
		}
	}

	public void AddScore(int score, bool perfect = false)
	{
		if (rankText == null)
		{
			rankText = base.transform.Find("RankPanel").GetComponentInChildren<Text>();
		}
		if (golds < levelAmount)
		{
			GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
		}
		scoresChecked += 1f;
		totalScore += score;
		if (perfect)
		{
			perfects++;
		}
		if (scoresChecked != (float)levelAmount)
		{
			return;
		}
		complete = true;
		if (perfects == levelAmount)
		{
			rankText.text = "<color=#FFFFFF>P</color>";
			Image component = rankText.transform.parent.GetComponent<Image>();
			component.color = new Color(1f, 0.686f, 0f, 1f);
			component.fillCenter = true;
			allPerfects = true;
			trueScore = Mathf.RoundToInt(totalScore / (float)levelAmount);
			return;
		}
		trueScore = Mathf.RoundToInt(totalScore / (float)levelAmount);
		float num = totalScore / (float)levelAmount;
		Debug.Log("True Score: " + trueScore + ". Real score: " + num);
		switch (trueScore)
		{
		case 1:
			rankText.text = "<color=#4CFF00>C</color>";
			break;
		case 2:
			rankText.text = "<color=#FFD800>B</color>";
			break;
		case 3:
			rankText.text = "<color=#FF6A00>A</color>";
			break;
		case 4:
		case 5:
		case 6:
			rankText.text = "<color=#FF0000>S</color>";
			break;
		default:
			rankText.text = "<color=#0094FF>D</color>";
			break;
		}
		Image component2 = rankText.transform.parent.GetComponent<Image>();
		component2.color = Color.white;
		component2.fillCenter = false;
	}

	public void Gold()
	{
		golds++;
		if (golds == levelAmount && levelAmount != 0 && (noSecretMission || secretMission))
		{
			GetComponent<Image>().color = new Color(1f, 0.686f, 0f, 0.75f);
			gold = true;
		}
	}

	public void SecretMissionDone()
	{
		secretMission = true;
		if (golds == levelAmount && secretMission)
		{
			GetComponent<Image>().color = new Color(1f, 0.686f, 0f, 0.75f);
		}
	}
}
