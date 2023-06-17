using System.Collections.Generic;
using UnityEngine;

namespace NewBlood.IK;

public sealed class FabrikSolver3D : Solver3D
{
	public const float MinTolerance = 0.001f;

	public const int MinIterations = 1;

	[SerializeField]
	private IKChain3D m_Chain = new IKChain3D();

	[SerializeField]
	[Range(1f, 50f)]
	private int m_Iterations = 10;

	[SerializeField]
	[Range(0.001f, 0.1f)]
	private float m_Tolerance = 0.01f;

	private Vector3[] m_Positions;

	public int iterations
	{
		get
		{
			return m_Iterations;
		}
		set
		{
			m_Iterations = Mathf.Max(value, 1);
		}
	}

	public float tolerance
	{
		get
		{
			return m_Tolerance;
		}
		set
		{
			m_Tolerance = Mathf.Max(value, 0.001f);
		}
	}

	public override IKChain3D GetChain(int index)
	{
		return m_Chain;
	}

	protected override int GetChainCount()
	{
		return 1;
	}

	protected override void DoPrepare()
	{
		base.DoPrepare();
		if (m_Positions == null || m_Positions.Length != m_Chain.transformCount)
		{
			m_Positions = new Vector3[m_Chain.transformCount];
		}
	}

	protected override void DoUpdateIK(List<Vector3> effectorPositions)
	{
		float[] lengths = m_Chain.lengths;
		for (int i = 0; i < m_Positions.Length; i++)
		{
			m_Positions[i] = m_Chain.transforms[i].position;
		}
		Vector3 originPosition = m_Positions[0];
		Vector3 vector = effectorPositions[0];
		float num = Vector3.Magnitude(vector - m_Positions[m_Positions.Length - 1]);
		for (int j = 0; j < iterations; j++)
		{
			if (num <= tolerance)
			{
				break;
			}
			Forward(vector, lengths, m_Positions);
			Backward(originPosition, lengths, m_Positions);
			num = Vector3.Magnitude(vector - m_Positions[m_Positions.Length - 1]);
		}
		for (int k = 0; k < m_Chain.transformCount - 1; k++)
		{
			Vector3 localPosition = m_Chain.transforms[k + 1].localPosition;
			Vector3 toDirection = m_Chain.transforms[k].InverseTransformPoint(m_Positions[k + 1]);
			m_Chain.transforms[k].localRotation *= Quaternion.FromToRotation(localPosition, toDirection);
		}
	}

	private void Forward(Vector3 targetPosition, float[] lengths, Vector3[] positions)
	{
		int num = positions.Length - 1;
		positions[num] = targetPosition;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			Vector3 vector = positions[num2 + 1] - positions[num2];
			float num3 = lengths[num2] / vector.magnitude;
			Vector3 vector2 = (1f - num3) * positions[num2 + 1] + num3 * positions[num2];
			positions[num2] = vector2;
		}
	}

	private void Backward(Vector3 originPosition, float[] lengths, Vector3[] positions)
	{
		positions[0] = originPosition;
		int num = positions.Length - 1;
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = positions[i + 1] - positions[i];
			float num2 = lengths[i] / vector.magnitude;
			Vector3 vector2 = (1f - num2) * positions[i] + num2 * positions[i + 1];
			positions[i + 1] = vector2;
		}
	}
}
