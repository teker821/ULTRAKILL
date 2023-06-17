using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class IntroTextController : MonoBehaviour
{
	public bool firstTime;

	public GameObject page1Screen;

	public GameObject page1SecondTimeScreen;

	public GameObject page2Screen;

	public GameObject[] deactivateOnIntroEnd;

	public Slider soundSlider;

	public Slider musicSlider;

	private AudioMixer[] audmix;

	private Image img;

	private Text page2Text;

	private float fadeValue;

	private bool inMenu;

	public bool introOver;

	private float introOverWait = 1f;

	private Rigidbody rb;

	private void Awake()
	{
		audmix = new AudioMixer[2]
		{
			MonoSingleton<AudioMixerController>.Instance.allSound,
			MonoSingleton<AudioMixerController>.Instance.musicSound
		};
		firstTime = !GameProgressSaver.GetIntro();
		if (firstTime)
		{
			soundSlider.value = 0f;
			musicSlider.value = 0f;
			MonoSingleton<PrefsManager>.Instance.SetFloat("musicVolume", 0f);
			MonoSingleton<PrefsManager>.Instance.SetFloat("allVolume", 0f);
			page1Screen.SetActive(value: true);
		}
		else
		{
			soundSlider.value = MonoSingleton<PrefsManager>.Instance.GetFloat("allVolume") * 100f;
			musicSlider.value = MonoSingleton<PrefsManager>.Instance.GetFloat("musicVolume") * 100f;
			page1SecondTimeScreen.SetActive(value: true);
		}
	}

	public void DoneWithSetting()
	{
		if (page1Screen.activeSelf)
		{
			page1Screen.GetComponent<IntroText>().DoneWithSetting();
		}
		if (page1SecondTimeScreen.activeSelf)
		{
			page1SecondTimeScreen.GetComponent<IntroText>().DoneWithSetting();
		}
	}

	private void Start()
	{
		float value = 0f;
		audmix[0].GetFloat("allVolume", out value);
		Debug.Log("Mixer Volume " + value);
		AudioMixer[] array = audmix;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetFloat("allVolume", -80f);
		}
		audmix[0].GetFloat("allVolume", out value);
		Debug.Log("Mixer Volume " + value);
		Invoke("SlowDown", 0.1f);
		MonoSingleton<OptionsManager>.Instance.inIntro = true;
		rb = MonoSingleton<NewMovement>.Instance.GetComponent<Rigidbody>();
		rb.velocity = Vector3.zero;
		rb.useGravity = false;
	}

	private void SlowDown()
	{
		inMenu = true;
	}

	private void Update()
	{
		if (inMenu)
		{
			rb.velocity = Vector3.zero;
			rb.useGravity = false;
			if (page2Screen.activeSelf)
			{
				MonoSingleton<NewMovement>.Instance.GetComponent<Rigidbody>().useGravity = true;
				inMenu = false;
			}
			if (!firstTime && MonoSingleton<InputManager>.Instance.InputSource.Pause.WasPerformedThisFrame)
			{
				inMenu = false;
				introOver = true;
			}
		}
		else
		{
			if (!introOver)
			{
				return;
			}
			if (!img)
			{
				img = GetComponent<Image>();
				page2Text = page2Screen.GetComponent<Text>();
				fadeValue = 1f;
				MonoSingleton<AudioMixerController>.Instance.forceOff = false;
			}
			if (fadeValue > 0f)
			{
				fadeValue = Mathf.MoveTowards(fadeValue, 0f, Time.deltaTime * 0.375f);
				Color color = img.color;
				color.a = fadeValue;
				img.color = color;
				AudioMixer[] array = audmix;
				foreach (AudioMixer audioMixer in array)
				{
					float value = 0f;
					audioMixer.GetFloat("allVolume", out value);
					if (audioMixer == MonoSingleton<AudioMixerController>.Instance.musicSound && MonoSingleton<AudioMixerController>.Instance.musicVolume > 0f)
					{
						audioMixer.SetFloat("allVolume", Mathf.MoveTowards(value, Mathf.Log10(MonoSingleton<AudioMixerController>.Instance.musicVolume) * 20f, Time.deltaTime * Mathf.Abs(value)));
					}
					else if (audioMixer == MonoSingleton<AudioMixerController>.Instance.allSound)
					{
						audioMixer.SetFloat("allVolume", Mathf.MoveTowards(value, 0f, Time.deltaTime * Mathf.Abs(value)));
					}
				}
				color = page2Text.color;
				color.a = fadeValue;
				page2Text.color = color;
			}
			else if (introOverWait > 0f)
			{
				introOverWait = Mathf.MoveTowards(introOverWait, 0f, Time.deltaTime);
				AudioMixer[] array = audmix;
				foreach (AudioMixer audioMixer2 in array)
				{
					if (audioMixer2 == MonoSingleton<AudioMixerController>.Instance.musicSound && MonoSingleton<AudioMixerController>.Instance.musicVolume > 0f)
					{
						audioMixer2.SetFloat("allVolume", Mathf.Log10(MonoSingleton<AudioMixerController>.Instance.musicVolume) * 20f);
					}
					else if (audioMixer2 == MonoSingleton<AudioMixerController>.Instance.allSound)
					{
						audioMixer2.SetFloat("allVolume", introOverWait * -5.47f);
					}
				}
			}
			else
			{
				MonoSingleton<OptionsManager>.Instance.inIntro = false;
				GameObject[] array2 = deactivateOnIntroEnd;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].SetActive(value: false);
				}
			}
		}
	}
}
