using System;
using UnityEngine;

namespace NewBlood.IK;

[Serializable]
public sealed class IKChain3D
{
	[SerializeField]
	private Transform m_EffectorTransform;

	[SerializeField]
	private Transform m_TargetTransform;

	[SerializeField]
	private int m_TransformCount;

	[SerializeField]
	private Transform[] m_Transforms;

	[SerializeField]
	private Quaternion[] m_DefaultLocalRotations;

	[SerializeField]
	private Quaternion[] m_StoredLocalRotations;

	private float[] m_Lengths;

	public bool isValid
	{
		get
		{
			if (m_EffectorTransform == null)
			{
				return false;
			}
			if (m_TransformCount == 0)
			{
				return false;
			}
			if (m_Transforms == null || m_Transforms.Length != m_TransformCount)
			{
				return false;
			}
			if (m_DefaultLocalRotations == null || m_DefaultLocalRotations.Length != m_TransformCount)
			{
				return false;
			}
			if (m_StoredLocalRotations == null || m_StoredLocalRotations.Length != m_TransformCount)
			{
				return false;
			}
			if (m_Transforms[0] == null)
			{
				return false;
			}
			if (m_Transforms[m_TransformCount - 1] != m_EffectorTransform)
			{
				return false;
			}
			if (m_TargetTransform != null && IKUtility.IsDescendantOf(m_TargetTransform, m_Transforms[0], m_TransformCount))
			{
				return false;
			}
			return true;
		}
	}

	public int transformCount => m_TransformCount;

	public Transform effector
	{
		get
		{
			return m_EffectorTransform;
		}
		set
		{
			m_EffectorTransform = value;
		}
	}

	public Transform target
	{
		get
		{
			return m_TargetTransform;
		}
		set
		{
			m_TargetTransform = value;
		}
	}

	public Transform[] transforms => m_Transforms;

	public Transform rootTransform
	{
		get
		{
			if (m_TransformCount == 0)
			{
				return null;
			}
			if (m_Transforms == null || m_Transforms.Length != m_TransformCount)
			{
				return null;
			}
			return m_Transforms[0];
		}
	}

	public float[] lengths
	{
		get
		{
			if (isValid)
			{
				PrepareLengths();
				return m_Lengths;
			}
			return null;
		}
	}

	public void Initialize()
	{
		if (!(m_EffectorTransform == null) && m_TransformCount != 0 && IKUtility.AncestorCountAtLeast(m_EffectorTransform, m_TransformCount - 1))
		{
			m_Transforms = new Transform[m_TransformCount];
			m_DefaultLocalRotations = new Quaternion[m_TransformCount];
			m_StoredLocalRotations = new Quaternion[m_TransformCount];
			Transform transform = m_EffectorTransform;
			int num = m_TransformCount - 1;
			while (transform != null && num >= 0)
			{
				m_Transforms[num] = transform;
				m_DefaultLocalRotations[num] = transform.localRotation;
				transform = transform.parent;
				num--;
			}
		}
	}

	public void RestoreDefaultPose(bool targetRotationIsConstrained)
	{
		int num = m_TransformCount;
		if (!targetRotationIsConstrained)
		{
			num--;
		}
		for (int i = 0; i < num; i++)
		{
			m_Transforms[i].localRotation = m_DefaultLocalRotations[i];
		}
	}

	public void StoreLocalRotations()
	{
		for (int i = 0; i < m_Transforms.Length; i++)
		{
			m_StoredLocalRotations[i] = m_Transforms[i].localRotation;
		}
	}

	public void BlendFKToIK(float finalWeight, bool targetRotationIsConstrained)
	{
		int num = m_TransformCount;
		if (!targetRotationIsConstrained)
		{
			num--;
		}
		for (int i = 0; i < num; i++)
		{
			m_Transforms[i].localRotation = Quaternion.Slerp(m_StoredLocalRotations[i], m_Transforms[i].localRotation, finalWeight);
		}
	}

	private void PrepareLengths()
	{
		Transform transform = m_EffectorTransform;
		if (m_Lengths == null || m_Lengths.Length != m_TransformCount - 1)
		{
			m_Lengths = new float[m_TransformCount - 1];
		}
		int num = m_Lengths.Length - 1;
		while (num > 0 && !(transform == null) && !(transform.parent == null))
		{
			m_Lengths[num - 1] = Vector3.Distance(transform.position, transform.parent.position);
			transform = transform.parent;
			num--;
		}
	}
}
