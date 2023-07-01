using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class IsometricFollow : MonoBehaviour
{
	private Camera _camera;

	public GameObject target;
	[Range(1f, 5f)] public float distance = 1f;
	public Vector3 offset = Vector3.zero;

	void Awake()
	{
		_camera = GetComponent<Camera>();
		_camera.orthographic = true;
	}

	void Start()
	{
		if (target == null) enabled = false;
	}

	void LateUpdate()
	{
		// Update the camera distance by the otrographic size
		_camera.orthographicSize = distance;
		// Update position to follow the target (with offset)
		transform.position = target.transform.position + offset;
	}
}
