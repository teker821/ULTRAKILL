using UnityEngine;

public class Crossfade : MonoBehaviour
{
	public AudioSource from;

	public AudioSource to;

	public float time;

	private float fadeAmount;

	[HideInInspector]
	public float fromMaxVolume;

	[HideInInspector]
	public float toOriginalVolume;

	[HideInInspector]
	public float toMaxVolume;

	[HideInInspector]
	public float toMinVolume;

	[HideInInspector]
	public bool inProgress;

	public bool match;

	public bool dontActivateOnStart;

	public bool oneTime;

	private bool activated;

	private void Awake()
	{
		if ((bool)to)
		{
			toOriginalVolume = to.volume;
		}
	}

	private void Start()
	{
		if (!dontActivateOnStart && !inProgress)
		{
			StartFade();
		}
	}

	private void OnEnable()
	{
		if (!dontActivateOnStart && !inProgress)
		{
			StartFade();
		}
	}

	private void Update()
	{
		if (inProgress)
		{
			fadeAmount = Mathf.MoveTowards(fadeAmount, 1f, Time.deltaTime / time);
			if ((bool)from)
			{
				from.volume = Mathf.Lerp(fromMaxVolume, 0f, fadeAmount);
			}
			if ((bool)to)
			{
				to.volume = Mathf.Lerp(toMinVolume, toMaxVolume, fadeAmount);
			}
			if (fadeAmount == 1f)
			{
				StopFade();
			}
		}
	}

	public void StartFade()
	{
		if (!activated)
		{
			activated = true;
		}
		else if (oneTime)
		{
			return;
		}
		if ((bool)from)
		{
			if (MonoSingleton<CrossfadeTracker>.Instance.actives.Count > 0)
			{
				for (int i = 0; i < MonoSingleton<CrossfadeTracker>.Instance.actives.Count; i++)
				{
					if (MonoSingleton<CrossfadeTracker>.Instance.actives[i].from == from || MonoSingleton<CrossfadeTracker>.Instance.actives[i].to == from)
					{
						MonoSingleton<CrossfadeTracker>.Instance.actives[i].StopFade();
					}
				}
			}
			fromMaxVolume = from.volume;
		}
		if ((bool)to)
		{
			if (MonoSingleton<CrossfadeTracker>.Instance.actives.Count > 0)
			{
				bool flag = false;
				for (int j = 0; j < MonoSingleton<CrossfadeTracker>.Instance.actives.Count; j++)
				{
					if (MonoSingleton<CrossfadeTracker>.Instance.actives[j].from == to || MonoSingleton<CrossfadeTracker>.Instance.actives[j].to == to)
					{
						flag = true;
					}
					if (flag)
					{
						MonoSingleton<CrossfadeTracker>.Instance.actives[j].StopFade();
						toMinVolume = to.volume;
					}
				}
				if (!flag)
				{
					to.volume = 0f;
				}
			}
			else
			{
				to.volume = 0f;
			}
			toMaxVolume = toOriginalVolume;
			if (!to.isPlaying)
			{
				to.Play();
			}
			if (match)
			{
				to.time = from.time;
			}
		}
		MonoSingleton<CrossfadeTracker>.Instance.actives.Add(this);
		fadeAmount = 0f;
		inProgress = true;
	}

	public void StopFade()
	{
		if (inProgress)
		{
			inProgress = false;
			if (MonoSingleton<CrossfadeTracker>.Instance.actives.Contains(this))
			{
				MonoSingleton<CrossfadeTracker>.Instance.actives.Remove(this);
			}
		}
	}
}
