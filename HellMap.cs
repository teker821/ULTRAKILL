using UnityEngine;
using UnityEngine.UI;

public class HellMap : MonoBehaviour
{
	private Vector3 targetPos;

	private Image targetImage;

	public GameObject arrow;

	public int firstLevelNumber;

	private bool white = true;

	private AudioSource aud;

	private void Start()
	{
		if (MonoSingleton<StatsManager>.Instance.levelNumber < firstLevelNumber)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		Transform child = base.transform.GetChild(MonoSingleton<StatsManager>.Instance.levelNumber - firstLevelNumber);
		targetPos = new Vector3(arrow.transform.localPosition.x, child.transform.localPosition.y - 25f, arrow.transform.localPosition.z);
		arrow.transform.localPosition = targetPos + Vector3.up * 50f;
		targetImage = arrow.GetComponentInChildren<Image>();
		aud = GetComponent<AudioSource>();
		Invoke("FlashImage", 0.075f);
	}

	private void Update()
	{
		arrow.transform.localPosition = Vector3.MoveTowards(arrow.transform.localPosition, targetPos, Time.deltaTime * 4f * Vector3.Distance(arrow.transform.localPosition, targetPos));
	}

	private void FlashImage()
	{
		if (white)
		{
			white = false;
			targetImage.color = new Color(0f, 0f, 0f, 0f);
			if (!base.gameObject.activeSelf)
			{
				return;
			}
			aud.Play();
		}
		else
		{
			white = true;
			targetImage.color = Color.white;
		}
		if (base.gameObject.activeSelf)
		{
			Invoke("FlashImage", 0.075f);
		}
	}
}
