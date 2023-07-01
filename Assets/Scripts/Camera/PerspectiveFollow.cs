using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class PerspectiveFollow : MonoBehaviour
{
	private Camera _camera;

	public GameObject target;
	public Vector3 offset = Vector3.zero;

	void Awake()
	{
		_camera = GetComponent<Camera>();
		_camera.orthographic = false;
	}

	void Start()
	{
		if (target == null) enabled = false;
	}

	void LateUpdate()
	{
		// Update position to follow the target (with offset)
		transform.position = target.transform.position + offset;
	}
}
