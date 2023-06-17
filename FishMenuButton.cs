using UnityEngine;
using UnityEngine.UI;

public class FishMenuButton : MonoBehaviour
{
	[SerializeField]
	private Image fishIcon;

	public void Populate(FishObject fish, bool locked)
	{
		fishIcon.sprite = (locked ? fish.blockedIcon : fish.icon);
		fishIcon.color = (locked ? Color.black : Color.white);
	}
}
