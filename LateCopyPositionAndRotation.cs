using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(int.MaxValue)]
public sealed class LateCopyPositionAndRotation : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("target")]
	private Transform m_Target;

	[SerializeField]
	[FormerlySerializedAs("copyRotation")]
	private bool m_CopyRotation = true;

	[SerializeField]
	[FormerlySerializedAs("copyPosition")]
	private bool m_CopyPosition = true;

	public Transform target
	{
		get
		{
			return m_Target;
		}
		set
		{
			m_Target = value;
		}
	}

	public bool copyRotation
	{
		get
		{
			return m_CopyRotation;
		}
		set
		{
			m_CopyRotation = value;
		}
	}

	public bool copyPosition
	{
		get
		{
			return m_CopyPosition;
		}
		set
		{
			m_CopyPosition = value;
		}
	}

	private void LateUpdate()
	{
		if (m_CopyRotation)
		{
			base.transform.rotation = m_Target.rotation;
		}
		if (m_CopyPosition)
		{
			base.transform.position = m_Target.position;
		}
	}
}
