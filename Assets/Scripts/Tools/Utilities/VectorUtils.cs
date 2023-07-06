using UnityEngine;

public static class VectorUtils
{
	public static Vector3 Uniform3(float v)
		=> new Vector3(v, v, v);

	public static Vector2 ToVector2(this Vector3 v)
		=> new Vector2(v.x, v.z);
}
