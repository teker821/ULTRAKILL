using UnityEngine;
using UnityEngine.UI;

public class IconPackSprite : MonoBehaviour
{
	[SerializeField]
	private Sprite[] sprites;

	public void Start()
	{
		int @int = MonoSingleton<PrefsManager>.Instance.GetInt("iconPack");
		GetComponent<Image>().sprite = ((sprites.Length > @int) ? sprites[@int] : sprites[0]);
	}
}
