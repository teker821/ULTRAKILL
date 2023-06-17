using UnityEngine;
using UnityEngine.UI;

public class LevelSelectPanel : MonoBehaviour
{
	public float collapsedHeight = 260f;

	public float expandedHeight = 400f;

	public GameObject leaderboardPanel;

	private RectTransform rectTransform;

	public int levelNumber;

	public int levelNumberInLayer;

	private Text panelTitle;

	private bool allSecrets;

	public Sprite lockedSprite;

	public Sprite unlockedSprite;

	private Sprite origSprite;

	public Image[] secretIcons;

	public Image challengeIcon;

	private int tempInt;

	private string origName;

	private LayerSelect ls;

	private GameObject challengeChecker;

	private bool beenChecked;

	public bool forceOff;

	private void OnEnable()
	{
		if (!beenChecked)
		{
			CheckScore();
		}
	}

	private void OnDisable()
	{
		beenChecked = false;
	}

	public void CheckScore()
	{
		rectTransform = GetComponent<RectTransform>();
		beenChecked = true;
		if (ls == null)
		{
			ls = base.transform.parent.GetComponent<LayerSelect>();
		}
		if (ls == null && base.transform.parent.parent != null)
		{
			ls = base.transform.parent.parent.GetComponent<LayerSelect>();
		}
		if (origSprite == null)
		{
			origSprite = base.transform.Find("Image").GetComponent<Image>().sprite;
		}
		if (origName == null)
		{
			origName = base.transform.Find("Name").GetComponent<Text>().text;
		}
		if (levelNumber == 666)
		{
			tempInt = GameProgressSaver.GetPrime(MonoSingleton<PrefsManager>.Instance.GetInt("difficulty"), levelNumberInLayer);
		}
		else
		{
			tempInt = GameProgressSaver.GetProgress(MonoSingleton<PrefsManager>.Instance.GetInt("difficulty"));
		}
		int num = levelNumber;
		if (levelNumber == 666)
		{
			num += levelNumberInLayer - 1;
		}
		if ((levelNumber == 666 && tempInt == 0) || (levelNumber != 666 && tempInt < levelNumber) || forceOff)
		{
			string text = ls.layerNumber.ToString();
			if (ls.layerNumber == 666)
			{
				text = "P";
			}
			base.transform.Find("Name").GetComponent<Text>().text = text + "-" + levelNumberInLayer + ": ???";
			base.transform.Find("Image").GetComponent<Image>().sprite = lockedSprite;
			GetComponent<Button>().enabled = false;
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, collapsedHeight);
			leaderboardPanel.SetActive(value: false);
		}
		else
		{
			bool flag = false;
			if (tempInt == levelNumber || (levelNumber == 666 && tempInt == 1))
			{
				flag = false;
				base.transform.Find("Image").GetComponent<Image>().sprite = unlockedSprite;
			}
			else
			{
				flag = true;
				base.transform.Find("Image").GetComponent<Image>().sprite = origSprite;
			}
			base.transform.Find("Name").GetComponent<Text>().text = GetMissionName.GetMission(num);
			GetComponent<Button>().enabled = true;
			if (challengeIcon != null)
			{
				if (challengeChecker == null)
				{
					challengeChecker = challengeIcon.transform.Find("EventTrigger").gameObject;
				}
				if (tempInt > levelNumber)
				{
					challengeChecker.SetActive(value: true);
				}
			}
			if (MonoSingleton<PrefsManager>.Instance.GetBool("levelLeaderboards") && flag)
			{
				rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, expandedHeight);
				leaderboardPanel.SetActive(value: true);
			}
			else
			{
				rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, collapsedHeight);
				leaderboardPanel.SetActive(value: false);
			}
		}
		RankData rank = GameProgressSaver.GetRank(num);
		if (rank != null)
		{
			int @int = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
			if (rank.levelNumber == levelNumber || (levelNumber == 666 && rank.levelNumber == levelNumber + levelNumberInLayer - 1))
			{
				Text componentInChildren = base.transform.Find("Stats").Find("Rank").GetComponentInChildren<Text>();
				if (rank.ranks[@int] == 12 && (rank.majorAssists == null || !rank.majorAssists[@int]))
				{
					componentInChildren.text = "<color=#FFFFFF>P</color>";
					Image component = componentInChildren.transform.parent.GetComponent<Image>();
					component.color = new Color(1f, 0.686f, 0f, 1f);
					component.fillCenter = true;
					ls.AddScore(4, perfect: true);
				}
				else if (rank.majorAssists != null && rank.majorAssists[@int])
				{
					if (rank.ranks[@int] < 0)
					{
						componentInChildren.text = "";
					}
					else
					{
						switch (rank.ranks[@int])
						{
						case 1:
							componentInChildren.text = "C";
							ls.AddScore(1);
							break;
						case 2:
							componentInChildren.text = "B";
							ls.AddScore(2);
							break;
						case 3:
							componentInChildren.text = "A";
							ls.AddScore(3);
							break;
						case 4:
						case 5:
						case 6:
							ls.AddScore(4);
							componentInChildren.text = "S";
							break;
						default:
							ls.AddScore(0);
							componentInChildren.text = "D";
							break;
						}
						Image component2 = componentInChildren.transform.parent.GetComponent<Image>();
						component2.color = new Color(0.3f, 0.6f, 0.9f, 1f);
						component2.fillCenter = true;
					}
				}
				else if (rank.ranks[@int] < 0)
				{
					componentInChildren.text = "";
					Image component3 = componentInChildren.transform.parent.GetComponent<Image>();
					component3.color = Color.white;
					component3.fillCenter = false;
				}
				else
				{
					switch (rank.ranks[@int])
					{
					case 1:
						componentInChildren.text = "<color=#4CFF00>C</color>";
						ls.AddScore(1);
						break;
					case 2:
						componentInChildren.text = "<color=#FFD800>B</color>";
						ls.AddScore(2);
						break;
					case 3:
						componentInChildren.text = "<color=#FF6A00>A</color>";
						ls.AddScore(3);
						break;
					case 4:
					case 5:
					case 6:
						ls.AddScore(4);
						componentInChildren.text = "<color=#FF0000>S</color>";
						break;
					default:
						ls.AddScore(0);
						componentInChildren.text = "<color=#0094FF>D</color>";
						break;
					}
					Image component4 = componentInChildren.transform.parent.GetComponent<Image>();
					component4.color = Color.white;
					component4.fillCenter = false;
				}
				if (rank.secretsAmount > 0)
				{
					allSecrets = true;
					for (int i = 0; i < 5; i++)
					{
						if (i < rank.secretsAmount && rank.secretsFound[i])
						{
							secretIcons[i].fillCenter = true;
						}
						else if (i < rank.secretsAmount)
						{
							allSecrets = false;
							secretIcons[i].fillCenter = false;
						}
						else if (i >= rank.secretsAmount)
						{
							secretIcons[i].enabled = false;
						}
					}
				}
				else
				{
					Image[] array = secretIcons;
					for (int j = 0; j < array.Length; j++)
					{
						array[j].enabled = false;
					}
				}
				if ((bool)challengeIcon)
				{
					if (rank.challenge)
					{
						challengeIcon.fillCenter = true;
						Text componentInChildren2 = challengeIcon.GetComponentInChildren<Text>();
						componentInChildren2.text = "C O M P L E T E";
						if (rank.ranks[@int] == 12 && (allSecrets || rank.secretsAmount == 0))
						{
							componentInChildren2.color = new Color(0.6f, 0.4f, 0f, 1f);
						}
						else
						{
							componentInChildren2.color = Color.black;
						}
					}
					else
					{
						challengeIcon.fillCenter = false;
						Text componentInChildren3 = challengeIcon.GetComponentInChildren<Text>();
						componentInChildren3.text = "C H A L L E N G E";
						componentInChildren3.color = Color.white;
					}
				}
			}
			else
			{
				Debug.Log("Error in finding " + levelNumber + " Data");
				Image component5 = base.transform.Find("Stats").Find("Rank").GetComponent<Image>();
				component5.color = Color.white;
				component5.fillCenter = false;
				component5.GetComponentInChildren<Text>().text = "";
				allSecrets = false;
				Image[] array = secretIcons;
				foreach (Image obj in array)
				{
					obj.enabled = true;
					obj.fillCenter = false;
				}
			}
			if ((rank.challenge || !challengeIcon) && rank.ranks[@int] == 12 && (allSecrets || rank.secretsAmount == 0))
			{
				ls.Gold();
				GetComponent<Image>().color = new Color(1f, 0.686f, 0f, 0.75f);
			}
			else
			{
				GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
			}
		}
		else
		{
			Debug.Log("Didn't Find Level " + levelNumber + " Data");
			Image component6 = base.transform.Find("Stats").Find("Rank").GetComponent<Image>();
			component6.color = Color.white;
			component6.fillCenter = false;
			component6.GetComponentInChildren<Text>().text = "";
			allSecrets = false;
			Image[] array = secretIcons;
			foreach (Image obj2 in array)
			{
				obj2.enabled = true;
				obj2.fillCenter = false;
			}
		}
	}
}
