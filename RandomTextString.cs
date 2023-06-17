using UnityEngine;
using UnityEngine.UI;

public class RandomTextString : MonoBehaviour
{
	[SerializeField]
	private string[] strings;

	private void Start()
	{
		Text component = GetComponent<Text>();
		if ((bool)component)
		{
			component.text = strings[Random.Range(0, strings.Length)];
		}
	}
}
