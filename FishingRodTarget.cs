using UnityEngine;
using UnityEngine.UI;

public class FishingRodTarget : MonoBehaviour
{
	[SerializeField]
	private GameObject goodBadge;

	[SerializeField]
	private GameObject badBadge;

	[SerializeField]
	private Transform canvas;

	public Text waterNameText;

	private void Awake()
	{
		goodBadge.SetActive(value: false);
		badBadge.SetActive(value: false);
	}

	public void SetState(bool isGood, float distance)
	{
		goodBadge.SetActive(isGood);
		badBadge.SetActive(!isGood);
		canvas.localScale = Vector3.one * (0.4f + distance * 0.05f);
	}
}
