using UnityEngine;
using UnityEngine.UI;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class UnderwaterController : MonoSingleton<UnderwaterController>
{
	public Image overlay;

	private Color defaultColor;

	private Color offColor;

	private int underWaterCalls;

	private AudioLowPassFilter lowPass;

	public bool inWater;

	private AudioSource aud;

	public AudioClip underWater;

	public AudioClip surfacing;

	private void OnDisable()
	{
		if (underWaterCalls >= 1)
		{
			underWaterCalls = 1;
			OutWater();
		}
	}

	private void Start()
	{
		defaultColor = overlay.color;
		defaultColor.a = 0.3f;
		lowPass = MonoSingleton<CameraController>.Instance.GetComponent<AudioLowPassFilter>();
		aud = overlay.GetComponent<AudioSource>();
	}

	public void InWater(Color clr)
	{
		if (underWaterCalls <= 0)
		{
			underWaterCalls = 0;
			aud.clip = underWater;
			aud.loop = true;
			aud.Play();
		}
		underWaterCalls++;
		if (clr != new Color(0f, 0f, 0f, 0f))
		{
			clr.a = 0.3f;
			overlay.color = clr;
		}
		else
		{
			overlay.color = defaultColor;
		}
		lowPass.enabled = true;
		inWater = true;
	}

	public void OutWater()
	{
		if (underWaterCalls > 0)
		{
			underWaterCalls--;
		}
		if (underWaterCalls <= 0)
		{
			underWaterCalls = 0;
			if (!(overlay == null))
			{
				overlay.color = offColor;
				lowPass.enabled = false;
				aud.clip = surfacing;
				aud.loop = false;
				aud.Play();
				inWater = false;
			}
		}
	}
}
