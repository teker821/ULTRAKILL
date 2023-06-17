using UnityEngine;

namespace DitzelGames.FastIK;

[DefaultExecutionOrder(int.MaxValue)]
public class FastIKFabric : MonoBehaviour
{
	public int chainLength = 2;

	public Transform target;

	public Transform pole;

	[Header("Solver Parameters")]
	public int iterations = 10;

	public float delta = 0.001f;

	[Range(0f, 1f)]
	public float snapBackStrength = 1f;

	protected float[] bonesLength;

	protected float completeLength;

	protected Transform[] bones;

	protected Vector3[] positions;

	protected Vector3[] startDirectionSucc;

	protected Quaternion[] startRotationBone;

	protected Quaternion startRotationTarget;

	protected Transform root;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		bones = new Transform[chainLength + 1];
		positions = new Vector3[chainLength + 1];
		bonesLength = new float[chainLength];
		startDirectionSucc = new Vector3[chainLength + 1];
		startRotationBone = new Quaternion[chainLength + 1];
		root = base.transform;
		for (int i = 0; i <= chainLength; i++)
		{
			if (root == null)
			{
				throw new UnityException("The chain value is longer than the ancestor chain!");
			}
			root = root.parent;
		}
		if (target == null)
		{
			target = new GameObject(base.gameObject.name + " Target").transform;
			SetPositionRootSpace(target, GetPositionRootSpace(base.transform));
		}
		startRotationTarget = GetRotationRootSpace(target);
		Transform parent = base.transform;
		completeLength = 0f;
		for (int num = bones.Length - 1; num >= 0; num--)
		{
			bones[num] = parent;
			startRotationBone[num] = GetRotationRootSpace(parent);
			if (num == bones.Length - 1)
			{
				startDirectionSucc[num] = GetPositionRootSpace(target) - GetPositionRootSpace(parent);
			}
			else
			{
				startDirectionSucc[num] = GetPositionRootSpace(bones[num + 1]) - GetPositionRootSpace(parent);
				bonesLength[num] = startDirectionSucc[num].magnitude;
				completeLength += bonesLength[num];
			}
			parent = parent.parent;
		}
	}

	private void LateUpdate()
	{
		ResolveIK();
	}

	private void ResolveIK()
	{
		if (target == null)
		{
			return;
		}
		if (bonesLength.Length != chainLength)
		{
			Init();
		}
		for (int i = 0; i < bones.Length; i++)
		{
			positions[i] = GetPositionRootSpace(bones[i]);
		}
		Vector3 positionRootSpace = GetPositionRootSpace(target);
		GetRotationRootSpace(target);
		float num = Vector3.Distance(positionRootSpace, GetPositionRootSpace(bones[0]));
		if ((positionRootSpace - GetPositionRootSpace(bones[0])).sqrMagnitude >= completeLength * completeLength)
		{
			Vector3 normalized = (positionRootSpace - positions[0]).normalized;
			for (int j = 1; j < positions.Length; j++)
			{
				positions[j] = positions[j - 1] + normalized * bonesLength[j - 1];
			}
		}
		else
		{
			for (int k = 0; k < positions.Length - 1; k++)
			{
				positions[k + 1] = Vector3.Lerp(positions[k + 1], positions[k] + startDirectionSucc[k], snapBackStrength);
			}
			for (int l = 0; l < iterations; l++)
			{
				for (int num2 = positions.Length - 1; num2 > 0; num2--)
				{
					if (num2 == positions.Length - 1)
					{
						positions[num2] = positionRootSpace;
					}
					else
					{
						positions[num2] = positions[num2 + 1] + (positions[num2] - positions[num2 + 1]).normalized * (num / (float)positions.Length);
					}
				}
				for (int m = 1; m < positions.Length; m++)
				{
					positions[m] = positions[m - 1] + (positions[m] - positions[m - 1]).normalized * (num / (float)positions.Length);
				}
				if ((positions[positions.Length - 1] - positionRootSpace).sqrMagnitude < delta * delta)
				{
					break;
				}
			}
		}
		if (pole != null)
		{
			Vector3 positionRootSpace2 = GetPositionRootSpace(pole);
			for (int n = 1; n < positions.Length - 1; n++)
			{
				Plane plane = new Plane(positions[n + 1] - positions[n - 1], positions[n - 1]);
				Vector3 vector = plane.ClosestPointOnPlane(positionRootSpace2);
				float angle = Vector3.SignedAngle(plane.ClosestPointOnPlane(positions[n]) - positions[n - 1], vector - positions[n - 1], plane.normal);
				positions[n] = Quaternion.AngleAxis(angle, plane.normal) * (positions[n] - positions[n - 1]) + positions[n - 1];
			}
		}
		for (int num3 = 0; num3 < positions.Length; num3++)
		{
			SetPositionRootSpace(bones[num3], positions[num3]);
		}
	}

	private Vector3 GetPositionRootSpace(Transform current)
	{
		if (root == null)
		{
			return current.position;
		}
		return Quaternion.Inverse(root.rotation) * (current.position - root.position);
	}

	private void SetPositionRootSpace(Transform current, Vector3 position)
	{
		if (root == null)
		{
			current.position = position;
		}
		else
		{
			current.position = root.rotation * position + root.position;
		}
	}

	private Quaternion GetRotationRootSpace(Transform current)
	{
		if (root == null)
		{
			return current.rotation;
		}
		return Quaternion.Inverse(current.rotation) * root.rotation;
	}

	private void SetRotationRootSpace(Transform current, Quaternion rotation)
	{
		if (root == null)
		{
			current.rotation = rotation;
		}
		else
		{
			current.rotation = root.rotation * rotation;
		}
	}

	private void OnDrawGizmos()
	{
	}
}
