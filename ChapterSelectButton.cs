using UnityEngine;
using UnityEngine.UI;

public class ChapterSelectButton : MonoBehaviour
{
	public LayerSelect[] layersInChapter;

	public Text rankText;

	private Image buttonBg;

	public int totalScore;

	public bool notComplete;

	public int golds;

	public int allPerfects;

	private void OnEnable()
	{
		CheckScore();
	}

	private void OnDisable()
	{
		totalScore = 0;
		notComplete = false;
		golds = 0;
		allPerfects = 0;
		if (buttonBg == null)
		{
			buttonBg = GetComponent<Image>();
		}
		buttonBg.color = Color.white;
		buttonBg.fillCenter = false;
		rankText.text = "";
		Image component = rankText.transform.parent.GetComponent<Image>();
		component.color = Color.white;
		component.fillCenter = false;
	}

	public void CheckScore()
	{
		totalScore = 0;
		notComplete = false;
		golds = 0;
		allPerfects = 0;
		if (buttonBg == null)
		{
			buttonBg = GetComponent<Image>();
		}
		buttonBg.color = Color.white;
		buttonBg.fillCenter = false;
		LayerSelect[] array = layersInChapter;
		foreach (LayerSelect layerSelect in array)
		{
			layerSelect.CheckScore();
			totalScore += layerSelect.trueScore;
			if (!layerSelect.complete)
			{
				notComplete = true;
			}
			if (layerSelect.allPerfects)
			{
				allPerfects++;
			}
			if (layerSelect.gold)
			{
				golds++;
			}
		}
		if (notComplete)
		{
			return;
		}
		if (allPerfects == layersInChapter.Length)
		{
			rankText.text = "<color=#FFFFFF>P</color>";
			Image component = rankText.transform.parent.GetComponent<Image>();
			component.color = new Color(1f, 0.686f, 0f, 1f);
			component.fillCenter = true;
			if (golds == layersInChapter.Length)
			{
				buttonBg.color = new Color(1f, 0.686f, 0f, 0.75f);
				buttonBg.fillCenter = true;
			}
			return;
		}
		totalScore /= layersInChapter.Length;
		switch (totalScore)
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
		case 7:
		case 8:
		case 9:
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
}
