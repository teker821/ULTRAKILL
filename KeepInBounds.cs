using System;
using UnityEngine;

public class KeepInBounds : MonoBehaviour
{
	[Serializable]
	private enum UpdateMode
	{
		None,
		Update,
		FixedUpdate,
		LateUpdate
	}

	[SerializeField]
	private bool useColliderCenter;

	[SerializeField]
	private float maxConsideredDistance;

	[SerializeField]
	private UpdateMode updateMode = UpdateMode.Update;

	private Vector3 previousTracedPosition;

	private Vector3 previousRealPosition;

	private Collider col;

	private Vector3 CurrentPosition
	{
		get
		{
			if (!useColliderCenter)
			{
				return base.transform.position;
			}
			return col.bounds.center;
		}
	}

	private void Awake()
	{
		if (useColliderCenter)
		{
			col = GetComponent<Collider>();
		}
		previousTracedPosition = CurrentPosition;
		previousRealPosition = base.transform.position;
	}

	private void Update()
	{
		if (updateMode == UpdateMode.Update)
		{
			ValidateMove();
		}
	}

	private void FixedUpdate()
	{
		if (updateMode == UpdateMode.FixedUpdate)
		{
			ValidateMove();
		}
	}

	private void LateUpdate()
	{
		if (updateMode == UpdateMode.LateUpdate)
		{
			ValidateMove();
		}
	}

	public void ForceApproveNewPosition()
	{
		previousTracedPosition = CurrentPosition;
		previousRealPosition = base.transform.position;
	}

	public void ValidateMove()
	{
		Vector3 position = base.transform.position;
		if (maxConsideredDistance != 0f && Vector3.Distance(previousTracedPosition, CurrentPosition) > maxConsideredDistance)
		{
			previousTracedPosition = CurrentPosition;
			previousRealPosition = position;
			return;
		}
		if (Physics.Linecast(previousTracedPosition, CurrentPosition, out var _, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
		{
			base.transform.position = previousRealPosition;
		}
		previousTracedPosition = CurrentPosition;
		previousRealPosition = base.transform.position;
	}
}
