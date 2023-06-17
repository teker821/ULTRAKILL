using System.Collections.Generic;
using UnityEngine;

public class GabrielVoice : MonoBehaviour
{
	private AudioSource aud;

	public AudioClip[] hurt;

	public AudioClip[] bigHurt;

	public AudioClip phaseChange;

	public string phaseChangeSubtitle;

	public AudioClip[] taunt;

	[SerializeField]
	private string[] taunts;

	public bool secondPhase;

	public AudioClip[] tauntSecondPhase;

	[SerializeField]
	private string[] tauntsSecondPhase;

	private List<int> usedTaunts = new List<int>();

	private int priority;

	private void Start()
	{
		aud = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if ((bool)aud && !aud.isPlaying)
		{
			priority = 0;
		}
	}

	public void Hurt()
	{
		if (priority <= 0)
		{
			if (hurt.Length > 1)
			{
				aud.clip = hurt[Random.Range(0, hurt.Length)];
			}
			else
			{
				aud.clip = hurt[0];
			}
			aud.volume = 0.75f;
			aud.Play();
		}
	}

	public void BigHurt()
	{
		if (priority <= 2)
		{
			priority = 2;
			if (bigHurt.Length > 1)
			{
				aud.clip = bigHurt[Random.Range(0, bigHurt.Length)];
			}
			else
			{
				aud.clip = bigHurt[0];
			}
			aud.volume = 1f;
			aud.Play();
		}
	}

	public void PhaseChange()
	{
		if (priority <= 3)
		{
			priority = 3;
			aud.clip = phaseChange;
			aud.volume = 1f;
			aud.Play();
			MonoSingleton<SubtitleController>.Instance.DisplaySubtitle(phaseChangeSubtitle);
		}
	}

	public void Taunt()
	{
		if (!aud)
		{
			aud = GetComponent<AudioSource>();
		}
		if (secondPhase)
		{
			TauntNow(tauntSecondPhase, tauntsSecondPhase);
		}
		else
		{
			TauntNow(taunt, taunts);
		}
	}

	private void TauntNow(AudioClip[] clips, string[] subs)
	{
		if (priority > 1)
		{
			return;
		}
		priority = 1;
		if (clips.Length > 1)
		{
			int num = Random.Range(0, clips.Length);
			if (usedTaunts.Contains(num))
			{
				for (int i = 0; i < clips.Length; i++)
				{
					if (!usedTaunts.Contains(i))
					{
						num = i;
						break;
					}
				}
			}
			aud.clip = clips[num];
			if (usedTaunts.Count == clips.Length - 1)
			{
				usedTaunts.Clear();
			}
			usedTaunts.Add(num);
			if (subs[num] != "")
			{
				MonoSingleton<SubtitleController>.Instance.DisplaySubtitle(subs[num]);
			}
		}
		else
		{
			aud.clip = clips[0];
		}
		aud.volume = 0.85f;
		aud.Play();
	}
}
