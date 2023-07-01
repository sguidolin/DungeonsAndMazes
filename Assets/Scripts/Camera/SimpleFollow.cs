using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SimpleFollow : MonoBehaviour
{
	public GameObject target;
	public Vector3 offset = Vector3.zero;

	void Start()
	{
		if (target == null)
			enabled = false;
	}

	void LateUpdate() => transform.position = target.transform.position + offset;
}
