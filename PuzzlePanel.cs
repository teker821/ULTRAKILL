using UnityEngine;
using UnityEngine.UI;

public class PuzzlePanel : MonoBehaviour
{
	[HideInInspector]
	public TileType tileType;

	[HideInInspector]
	public TileColor tileColor;

	[HideInInspector]
	public GameObject currentPanel;

	[HideInInspector]
	public GameObject whiteSquare;

	[HideInInspector]
	public GameObject blackSquare;

	[HideInInspector]
	public GameObject fillSquare;

	public GameObject pitSquare;

	private Image img;

	private bool activated;

	private PuzzleController pc;

	[HideInInspector]
	public PuzzleLine pl;

	private ControllerPointer pointer;

	private void Start()
	{
		img = GetComponent<Image>();
		pc = GetComponentInParent<PuzzleController>();
		pl = base.transform.GetChild(0).GetComponent<PuzzleLine>();
		if (pl != null)
		{
			pl.transform.SetParent(base.transform.parent, worldPositionStays: true);
		}
		if (!TryGetComponent<ControllerPointer>(out pointer))
		{
			pointer = base.gameObject.AddComponent<ControllerPointer>();
		}
		pointer.OnEnter.AddListener(delegate
		{
			pc.Hovered(this);
		});
		pointer.OnPressed.AddListener(delegate
		{
			pc.Clicked(this);
		});
		pointer.OnReleased.AddListener(pc.Unclicked);
	}

	public void Activate(TileColor color)
	{
		if (tileType == TileType.WhiteEnd)
		{
			base.transform.GetChild(0).GetComponent<Image>().fillCenter = true;
		}
		activated = true;
		Color color2 = pl.TranslateColor(color);
		img.color = new Color(color2.r, color2.g, color2.b, 0.85f);
	}

	public void DeActivate()
	{
		if (tileType == TileType.WhiteEnd)
		{
			base.transform.GetChild(0).GetComponent<Image>().fillCenter = false;
		}
		activated = false;
		img.color = new Color(1f, 1f, 1f, 0.5f);
	}
}
