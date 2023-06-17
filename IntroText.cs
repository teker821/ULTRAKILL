using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IntroText : MonoBehaviour
{
	private Text txt;

	private string fullString;

	private string tempString;

	private StringBuilder sb;

	private bool doneWithDots;

	private bool readyToContinue;

	private bool waitingForInput;

	private AudioSource aud;

	private int dotsAmount = 3;

	private int calibrated;

	public GameObject[] calibrationWindows;

	public GameObject[] activateOnEnd;

	public GameObject[] deactivateOnEnd;

	public GameObject[] activateOnTextTrigger;

	private string colorString;

	private List<int> colorsPositions = new List<int>();

	private List<int> colorsClosePositions = new List<int>();

	private void Start()
	{
		txt = GetComponent<Text>();
		fullString = txt.text;
		aud = GetComponent<AudioSource>();
		StartCoroutine(TextAppear());
	}

	private void Update()
	{
		if (!waitingForInput)
		{
			return;
		}
		if (!Input.GetKeyDown(KeyCode.Y))
		{
			Gamepad current = Gamepad.current;
			if (current == null || !current.buttonSouth.wasPressedThisFrame)
			{
				if (!Input.GetKeyDown(KeyCode.N))
				{
					Gamepad current2 = Gamepad.current;
					if (current2 == null || !current2.buttonEast.wasPressedThisFrame)
					{
						return;
					}
				}
				Over();
				return;
			}
		}
		waitingForInput = false;
	}

	public void DoneWithSetting()
	{
		calibrationWindows[calibrated].SetActive(value: false);
		calibrated++;
		readyToContinue = true;
	}

	private IEnumerator TextAppear()
	{
		int j = fullString.Length;
		for (int i = 0; i < j; i++)
		{
			char c = fullString[i];
			float waitTime = 0.035f;
			bool playSound = true;
			char c2 = c;
			if (c == IntroTextOperators.PauseWithEllipsis)
			{
				sb = new StringBuilder(fullString);
				sb[i] = ' ';
				fullString = sb.ToString();
				txt.text = fullString.Substring(0, i);
				tempString = txt.text;
				doneWithDots = false;
				dotsAmount = 2;
				StartCoroutine(DotsAppear());
				yield return new WaitUntil(() => doneWithDots);
			}
			else if (c == IntroTextOperators.ShortPauseWithEllipsis)
			{
				sb = new StringBuilder(fullString);
				sb[i] = ' ';
				fullString = sb.ToString();
				txt.text = fullString.Substring(0, i);
				tempString = txt.text;
				doneWithDots = false;
				dotsAmount = 1;
				StartCoroutine(DotsAppear());
				yield return new WaitUntil(() => doneWithDots);
			}
			else if (c != IntroTextOperators.Pause)
			{
				if (c2 == ' ')
				{
					waitTime = 0f;
					txt.text = fullString.Substring(0, i);
				}
				else if (c == IntroTextOperators.DrawYes)
				{
					sb = new StringBuilder(fullString);
					if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad)
					{
						if (Gamepad.current.buttonSouth.displayName == "Cross")
						{
							sb[i] = 'X';
						}
						else
						{
							sb[i] = 'A';
						}
					}
					else
					{
						sb[i] = 'Y';
					}
					fullString = sb.ToString();
					txt.text = fullString.Substring(0, i);
				}
				else if (c == IntroTextOperators.DrawNo)
				{
					sb = new StringBuilder(fullString);
					if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad)
					{
						if (Gamepad.current.buttonEast.displayName == "Circle")
						{
							sb[i] = 'O';
						}
						else
						{
							sb[i] = 'B';
						}
					}
					else
					{
						sb[i] = 'N';
					}
					fullString = sb.ToString();
					txt.text = fullString.Substring(0, i);
				}
				else if (c == IntroTextOperators.WaitForYesNo)
				{
					waitTime = 0f;
					sb = new StringBuilder(fullString);
					sb[i] = ' ';
					fullString = sb.ToString();
					txt.text = fullString.Substring(0, i);
					waitingForInput = true;
					yield return new WaitUntil(() => !waitingForInput);
				}
				else if (c == IntroTextOperators.ActivateOnTextTrigger)
				{
					waitTime = 0f;
					sb = new StringBuilder(fullString);
					sb[i] = ' ';
					fullString = sb.ToString();
					txt.text = fullString.Substring(0, i);
					GameObject[] array = activateOnTextTrigger;
					for (int k = 0; k < array.Length; k++)
					{
						array[k].SetActive(value: true);
					}
				}
				else if (c == IntroTextOperators.EndIntro)
				{
					GetComponentInParent<IntroTextController>().introOver = true;
					GameProgressSaver.SetIntro(seen: true);
					waitTime = 0f;
					sb = new StringBuilder(fullString);
					sb[i] = ' ';
					fullString = sb.ToString();
					txt.text = fullString.Substring(0, i);
				}
				else if (c == IntroTextOperators.ShowCalibrationMenu)
				{
					sb = new StringBuilder(fullString);
					sb[i] = ' ';
					fullString = sb.ToString();
					txt.text = fullString.Substring(0, i) + "<color=red>ERROR</color>";
					yield return new WaitForSecondsRealtime(1f);
					calibrationWindows[calibrated].SetActive(value: true);
					readyToContinue = false;
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					yield return new WaitUntil(() => readyToContinue);
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
					tempString = "<color=lime>OK</color>";
					fullString = fullString.Insert(i, tempString);
					i += tempString.Length;
					_ = j + tempString.Length;
					txt.text = fullString.Substring(0, i);
				}
				else if (c == IntroTextOperators.BeginColorRed)
				{
					colorString = "<color=red>";
					PlaceColor(i);
					i += colorString.Length;
					_ = j + colorString.Length;
					txt.text = fullString.Substring(0, i);
				}
				else if (c == IntroTextOperators.BeginColorGreen)
				{
					colorString = "<color=lime>";
					PlaceColor(i);
					i += colorString.Length;
					_ = j + colorString.Length;
					txt.text = fullString.Substring(0, i);
				}
				else if (c == IntroTextOperators.BeginColorGrey)
				{
					colorString = "<color=grey>";
					PlaceColor(i);
					i += colorString.Length;
					_ = j + colorString.Length;
					txt.text = fullString.Substring(0, i);
				}
				else if (c == IntroTextOperators.BeginColorBlue)
				{
					colorString = "<color=#4C99E6>";
					PlaceColor(i);
					i += colorString.Length;
					_ = j + colorString.Length;
					txt.text = fullString.Substring(0, i);
				}
				else if (c == IntroTextOperators.EndColor)
				{
					colorsClosePositions.Add(i);
					sb = new StringBuilder(fullString);
					sb[i] = ' ';
					fullString = sb.ToString();
					string text = "</color>";
					fullString = fullString.Insert(i, text);
					i += text.Length;
					_ = j + text.Length;
					txt.text = fullString.Substring(0, i);
				}
				else
				{
					txt.text = fullString.Substring(0, i);
				}
			}
			else
			{
				sb = new StringBuilder(fullString);
				sb[i] = ' ';
				fullString = sb.ToString();
				playSound = false;
				waitTime = 0.75f;
				txt.text = fullString.Substring(0, i);
			}
			j = fullString.Length;
			if (waitTime != 0f && playSound)
			{
				aud.Play();
			}
			if (colorsPositions.Count > colorsClosePositions.Count)
			{
				txt.text += "</color>";
			}
			yield return new WaitForSecondsRealtime(waitTime);
		}
		Over();
	}

	private void PlaceColor(int i)
	{
		colorsPositions.Add(i);
		sb = new StringBuilder(fullString);
		sb[i] = ' ';
		fullString = sb.ToString();
		fullString = fullString.Insert(i, colorString);
	}

	private void Over()
	{
		GameObject[] array = activateOnEnd;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		array = deactivateOnEnd;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
	}

	private IEnumerator DotsAppear()
	{
		for (int i = 0; i < dotsAmount; i++)
		{
			txt.text = tempString;
			aud.Play();
			yield return new WaitForSecondsRealtime(0.25f);
			txt.text = tempString + ".";
			aud.Play();
			yield return new WaitForSecondsRealtime(0.25f);
			txt.text = tempString + "..";
			aud.Play();
			yield return new WaitForSecondsRealtime(0.25f);
			txt.text = tempString + "...";
			aud.Play();
			yield return new WaitForSecondsRealtime(0.25f);
		}
		doneWithDots = true;
	}
}
