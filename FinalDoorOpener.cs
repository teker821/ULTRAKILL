using UnityEngine;

public class FinalDoorOpener : MonoBehaviour
{
	public bool startTimer;

	public bool startMusic;

	private bool opened;

	private bool closed;

	private FinalDoor fd;

	private void Awake()
	{
		fd = GetComponentInParent<FinalDoor>();
		if (fd != null)
		{
			fd.Open();
		}
		if (fd != null)
		{
			Invoke("GoTime", 1f);
		}
		else
		{
			GoTime();
		}
	}

	private void OnEnable()
	{
		if (closed)
		{
			if (fd != null)
			{
				fd.Open();
			}
			if (fd != null)
			{
				Invoke("GoTime", 1f);
			}
			else
			{
				GoTime();
			}
		}
	}

	public void GoTime()
	{
		if (!opened)
		{
			opened = true;
			if (startTimer)
			{
				MonoSingleton<StatsManager>.Instance.StartTimer();
			}
			if (startMusic)
			{
				MonoSingleton<MusicManager>.Instance.StartMusic();
			}
			if ((bool)MonoSingleton<OutdoorLightMaster>.Instance)
			{
				MonoSingleton<OutdoorLightMaster>.Instance.FirstDoorOpen();
			}
		}
	}

	public void Close()
	{
		if (opened)
		{
			closed = true;
			opened = false;
			CancelInvoke("GoTime");
			if ((bool)fd)
			{
				fd.Close();
			}
		}
	}
}
