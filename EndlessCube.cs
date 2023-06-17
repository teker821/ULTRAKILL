using UnityEngine;

public class EndlessCube : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private MeshFilter meshFilter;

	public Vector2Int positionOnGrid;

	public bool blockedByPrefab;

	private Vector3 targetPos;

	private Transform tf;

	private bool active;

	private float speed;

	private EndlessGrid eg;

	public MeshRenderer MeshRenderer => meshRenderer;

	public MeshFilter MeshFilter => meshFilter;

	private void Awake()
	{
		tf = base.transform;
		eg = GetComponentInParent<EndlessGrid>();
	}

	private void Update()
	{
		if (active)
		{
			tf.position = Vector3.MoveTowards(tf.position, targetPos, (Vector3.Distance(tf.position, targetPos) * 1.75f + speed) * Time.deltaTime);
			if (tf.position == targetPos)
			{
				eg.OneDone();
				active = false;
			}
		}
	}

	public void SetTarget(float target)
	{
		targetPos = new Vector3(tf.position.x, target, tf.position.z);
		speed = Random.Range(9, 11);
		Invoke("StartMoving", Random.Range(0f, 0.5f));
	}

	private void StartMoving()
	{
		active = true;
	}
}
