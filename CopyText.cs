using UnityEngine;
using UnityEngine.UI;

public class CopyText : MonoBehaviour
{
	private Text txt;

	public Text target;

	private void Start()
	{
		txt = GetComponent<Text>();
	}

	private void Update()
	{
		txt.text = target.text;
	}
}
