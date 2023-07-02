using UnityEngine;

[DisallowMultipleComponent]
public class FollowBezier : MonoBehaviour
{
	[SerializeField]
	private Transform[] _curvePoints;

	private Vector3[] _points;
	private float _position = 0f;

	void Start()
	{
		_points = new Vector3[_curvePoints.Length];
		for (int i = 0; i < _points.Length; i++)
			_points[i] = new Vector3(
				_curvePoints[i].transform.position.x,
				transform.position.y,
				_curvePoints[i].transform.position.z
			);
	}

	void Update()
	{
		// TODO: Figure out how to make this work
		_position = Mathf.Clamp01(_position + Time.deltaTime * 0.5f);
		transform.position = GetPositionOnCurve(_position);
		Vector3 tangent = GetTangentOnCurve(_position);
		Quaternion rotation = Quaternion.LookRotation(tangent, Vector3.back);
		//Vector3 eulerAngles = transform.eulerAngles;
		//eulerAngles.z = rotation.eulerAngles.y;
		//Debug.Log(rotation.eulerAngles);
		//transform.eulerAngles = eulerAngles;
		transform.rotation = rotation;
		// Reset to go back to the start
		if (_position >= 1f)
			_position = 0f;
	}

	public Vector3 GetPositionOnCurve(float t)
		=> MathUtilities.Bezier3(t, _points);
	public Vector3 GetTangentOnCurve(float t)
	=> MathUtilities.Bezier3Tangent(t, _points);
}
