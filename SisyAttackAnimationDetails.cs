using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ULTRAKILL/Attack Animation Details")]
public class SisyAttackAnimationDetails : ScriptableObject
{
	[Header("Boulder")]
	public float minBoulderSpeed = 0.01f;

	public float boulderDistanceDivide = 100f;

	public float maxBoulderSpeed = 1E+10f;

	[FormerlySerializedAs("durationMulti")]
	public float finalDurationMulti = 1f;

	[Header("Anim")]
	public float speedDistanceMulti = 1f;

	[FormerlySerializedAs("minSpeedCap")]
	public float minAnimSpeedCap = 0.1f;

	[FormerlySerializedAs("maxSpeedCap")]
	public float maxAnimSpeedCap = 1f;
}
