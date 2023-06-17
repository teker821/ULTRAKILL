using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IntermissionController : MonoBehaviour
{
	private Text txt;

	private string fullString;

	private string tempString;

	private StringBuilder sb;

	private AudioSource aud;

	private float origPitch;

	private bool waitingForInput;

	private bool skipToInput;

	public UnityEvent onTextEvent;

	public UnityEvent onComplete;

	public string preText;

	private bool restart;

	private void Start()
	{
		txt = GetComponent<Text>();
		fullString = txt.text;
		txt.text = "";
		aud = GetComponent<AudioSource>();
		origPitch = aud.pitch;
		StartCoroutine(TextAppear());
	}

	private void OnDisable()
	{
		restart = true;
	}

	private void OnEnable()
	{
		if (restart)
		{
			restart = false;
			StartCoroutine(TextAppear());
		}
	}

	private void Update()
	{
		if (MonoSingleton<OptionsManager>.Instance.paused)
		{
			return;
		}
		if (waitingForInput)
		{
			if (Input.GetKeyDown(KeyCode.Mouse0) || MonoSingleton<InputManager>.Instance == null || (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame) || Input.GetKey(KeyCode.Space) || MonoSingleton<InputManager>.Instance.InputSource.Dodge.IsPressed || (Gamepad.current != null && Gamepad.current.aButton.wasPressedThisFrame))
			{
				waitingForInput = false;
			}
		}
		else if (Input.GetKeyDown(KeyCode.Mouse0) || MonoSingleton<InputManager>.Instance == null || (!MonoSingleton<InputManager>.Instance.PerformingCheatMenuCombo() && MonoSingleton<InputManager>.Instance.InputSource.Fire1.WasPerformedThisFrame) || Input.GetKey(KeyCode.Space) || MonoSingleton<InputManager>.Instance.InputSource.Dodge.IsPressed || (Gamepad.current != null && Gamepad.current.aButton.wasPressedThisFrame))
		{
			skipToInput = true;
		}
	}

	private IEnumerator TextAppear()
	{
		int j = fullString.Length;
		for (int i = 0; i < j; i++)
		{
			char c = fullString[i];
			float waitTime = 0.05f;
			bool playSound = true;
			if (MonoSingleton<OptionsManager>.Instance.paused)
			{
				yield return new WaitUntil(() => MonoSingleton<OptionsManager>.Instance == null || !MonoSingleton<OptionsManager>.Instance.paused);
			}
			switch (c)
			{
			case '▼':
				sb = new StringBuilder(fullString);
				sb[i] = ' ';
				fullString = sb.ToString();
				txt.text = preText + fullString.Substring(0, i);
				tempString = txt.text;
				skipToInput = false;
				waitingForInput = true;
				StartCoroutine(DotAppear());
				yield return new WaitUntil(() => !waitingForInput || !base.gameObject.scene.isLoaded);
				break;
			case '}':
				sb = new StringBuilder(fullString);
				sb[i] = ' ';
				fullString = sb.ToString();
				playSound = false;
				waitTime = 0f;
				txt.text = preText + fullString.Substring(0, i);
				onTextEvent?.Invoke();
				break;
			case ' ':
				waitTime = 0f;
				txt.text = preText + fullString.Substring(0, i);
				break;
			default:
				txt.text = preText + fullString.Substring(0, i);
				break;
			}
			j = fullString.Length;
			if (waitTime != 0f && playSound)
			{
				aud.pitch = Random.Range(origPitch - 0.05f, origPitch + 0.05f);
				aud.Play();
			}
			if (skipToInput)
			{
				waitTime = 0f;
			}
			yield return new WaitForSecondsRealtime(waitTime);
		}
		onComplete?.Invoke();
	}

	private IEnumerator DotAppear()
	{
		while (waitingForInput)
		{
			if (MonoSingleton<OptionsManager>.Instance.paused)
			{
				yield return new WaitUntil(() => !MonoSingleton<OptionsManager>.Instance.paused || !base.gameObject.scene.isLoaded);
			}
			txt.text = tempString + "<color=black>▼</color>";
			yield return new WaitForSecondsRealtime(0.25f);
			if (waitingForInput)
			{
				txt.text = tempString + "▼";
				yield return new WaitForSecondsRealtime(0.25f);
			}
		}
	}
}
