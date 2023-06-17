using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class ChallengeManager : MonoSingleton<ChallengeManager>
{
	public GameObject challengePanel;

	public FinalRank fr;

	private bool completedCheck;

	private bool startCheckingForChallenge;

	public bool challengeDone;

	public bool challengeFailed;

	protected override void Awake()
	{
		base.Awake();
		if (fr == null)
		{
			fr = GetComponentInParent<FinalRank>();
		}
		SendMessage("OnDisable", base.gameObject);
		base.gameObject.SetActive(value: false);
		startCheckingForChallenge = true;
	}

	private new void OnEnable()
	{
		if (startCheckingForChallenge)
		{
			StatsManager obj = (MonoSingleton<StatsManager>.Instance ? MonoSingleton<StatsManager>.Instance : Object.FindObjectOfType<StatsManager>());
			if (challengeDone && !completedCheck && !challengeFailed)
			{
				ChallengeDone();
			}
			if (obj.challengeComplete && (!challengeDone || challengeFailed))
			{
				challengePanel.GetComponent<Image>().color = new Color(1f, 0.696f, 0f, 0.5f);
				challengePanel.GetComponent<AudioSource>().volume = 0f;
				challengePanel.SetActive(value: true);
			}
		}
	}

	public void ChallengeDone()
	{
		if (!challengeFailed)
		{
			if (fr == null)
			{
				fr = GetComponentInParent<FinalRank>();
			}
			challengeDone = true;
			Debug.Log("! CHALLENGE COMPLETE !");
			challengePanel.GetComponent<Image>().color = new Color(1f, 0.696f, 0f, 1f);
			challengePanel.SetActive(value: true);
			GameProgressSaver.ChallengeComplete();
			completedCheck = true;
			if (fr != null)
			{
				fr.FlashPanel(challengePanel.transform.parent.GetChild(2).gameObject);
			}
		}
	}

	public void ChallengeFailed()
	{
		challengeFailed = true;
	}
}
