using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleController : MonoSingleton<SubtitleController>
{
	public class SubtitleData
	{
		public string caption;

		public float time;

		public GameObject origin;
	}

	public bool subtitlesEnabled;

	[SerializeField]
	private Transform container;

	[SerializeField]
	private Subtitle subtitleLine;

	private Subtitle previousSubtitle;

	private List<SubtitleData> delayedSubs = new List<SubtitleData>();

	private void Start()
	{
		subtitlesEnabled = MonoSingleton<PrefsManager>.Instance.GetBool("subtitlesEnabled");
	}

	private void Update()
	{
		if (delayedSubs.Count <= 0)
		{
			return;
		}
		for (int num = delayedSubs.Count - 1; num >= 0; num--)
		{
			if (delayedSubs[num] == null || delayedSubs[num].origin == null || !delayedSubs[num].origin.activeInHierarchy)
			{
				delayedSubs.RemoveAt(num);
			}
			else
			{
				delayedSubs[num].time = Mathf.MoveTowards(delayedSubs[num].time, 0f, Time.deltaTime);
				if (delayedSubs[num].time <= 0f)
				{
					DisplaySubtitle(delayedSubs[num].caption);
					delayedSubs.RemoveAt(num);
				}
			}
		}
	}

	public void NotifyHoldEnd(Subtitle self)
	{
		if (previousSubtitle == self)
		{
			previousSubtitle = null;
		}
	}

	public void DisplaySubtitleTranslated(string translationKey)
	{
		_ = subtitlesEnabled;
	}

	public void DisplaySubtitle(string caption, AudioSource audioSource = null)
	{
		if (subtitlesEnabled)
		{
			Subtitle subtitle = Object.Instantiate(subtitleLine, container, worldPositionStays: true);
			subtitle.GetComponentInChildren<Text>().text = caption;
			if (audioSource != null)
			{
				subtitle.distanceCheckObject = audioSource;
			}
			subtitle.gameObject.SetActive(value: true);
			if (!previousSubtitle)
			{
				subtitle.ContinueChain();
			}
			else
			{
				previousSubtitle.nextInChain = subtitle;
			}
			previousSubtitle = subtitle;
		}
	}

	public void DisplaySubtitle(string caption, float time, GameObject origin)
	{
		SubtitleData subtitleData = new SubtitleData();
		subtitleData.caption = caption;
		subtitleData.time = time;
		subtitleData.origin = origin;
		delayedSubs.Add(subtitleData);
	}

	public void CancelSubtitle(GameObject origin)
	{
		if (delayedSubs.Count <= 0)
		{
			return;
		}
		for (int num = delayedSubs.Count - 1; num >= 0; num--)
		{
			if (delayedSubs[num].origin == origin)
			{
				delayedSubs.RemoveAt(num);
			}
		}
	}
}
