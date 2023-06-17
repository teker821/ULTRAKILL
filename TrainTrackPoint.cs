using UnityEngine;

public class TrainTrackPoint : MonoBehaviour
{
	public enum TurningMethod
	{
		None,
		CurveToNext,
		TurnInstantly,
		SmoothStopAndTurn
	}

	public enum StoppingMethod
	{
		StopInstantly,
		StopSlowly
	}

	public bool isAllowed = true;

	public TurningMethod turn = TurningMethod.TurnInstantly;

	public StoppingMethod ifLast;

	public TrainTrackPoint next;

	public TrainTrackPoint previous;

	public void SetAllowed(bool state)
	{
		isAllowed = state;
	}
}
