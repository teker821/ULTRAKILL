using UnityEngine;
using UnityEngine.UI;

public class SpritePoses : MonoBehaviour
{
	public Sprite[] poses;

	private Image img;

	public SpritePoses[] copyChangeTo;

	public void ChangePose(int target)
	{
		if (!img)
		{
			img = GetComponent<Image>();
		}
		if (target < poses.Length)
		{
			img.sprite = poses[target];
		}
		else if (poses.Length != 0)
		{
			img.sprite = poses[0];
		}
		SpritePoses[] array = copyChangeTo;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ChangePose(target);
		}
	}
}
