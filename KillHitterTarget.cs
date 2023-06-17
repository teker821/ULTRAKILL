using UnityEngine;

public class KillHitterTarget : MonoBehaviour
{
	public string[] acceptedHitters;

	private KillHitterCache khc;

	public int id;

	private EnemyIdentifier eid;

	private bool done;

	private void Start()
	{
		eid = GetComponent<EnemyIdentifier>();
	}

	private void Update()
	{
		if (done || !eid.dead)
		{
			return;
		}
		done = true;
		string[] array = acceptedHitters;
		foreach (string text in array)
		{
			if (eid.hitter == text)
			{
				if (khc == null)
				{
					khc = MonoSingleton<KillHitterCache>.Instance;
				}
				khc.OneDone(id);
				GoreZone componentInParent = GetComponentInParent<GoreZone>();
				if ((bool)componentInParent)
				{
					componentInParent.AddKillHitterTarget(id);
				}
				break;
			}
		}
		Object.Destroy(this);
	}
}
