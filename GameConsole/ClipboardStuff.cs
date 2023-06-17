using System.Linq;
using UnityEngine;

namespace GameConsole;

public class ClipboardStuff : MonoBehaviour
{
	public void TogglePopup()
	{
		base.gameObject.SetActive(!base.gameObject.activeSelf);
	}

	public void CopyToClipboard()
	{
		GUIUtility.systemCopyBuffer = string.Join("\n", MonoSingleton<Console>.Instance.logs.Select((CapturedLog l) => $"[{l.time:HH:mm:ss.f}] [{l.type}] {l.message}\n{l.stackTrace}"));
	}

	public void OpenLogFile()
	{
		string text = Application.persistentDataPath + "/Player.log";
		Application.OpenURL("file://" + text);
	}
}
