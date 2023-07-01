using System.Linq;
using UnityEngine;

public static class MathUtilities
{
	#region Bezier Curve
	private const float SMALL_FLOAT = 0.00001f;

	private static int Factorial(int n)
	{
		if (n < 2) return 1;
		return n * Factorial(n - 1);
	}
	private static int BinomialCoefficient(int n, int k) =>
		Factorial(n) / (Factorial(k) * Factorial(n - k));

	private static float Bezier(float t, params float[] points)
	{
		float bezier = 0f;
		int n = points.Length - 1;

		for (int i = 0; i <= n; i++)
			bezier += BinomialCoefficient(n, i) * points[i] * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i);
		return bezier;
	}

	public static Vector2 Bezier2(float t, params Vector2[] points) =>
		new Vector2(
			Bezier(t, points.Select<Vector2, float>(v => v.x).ToArray()),
			Bezier(t, points.Select<Vector2, float>(v => v.y).ToArray())
		);
	public static Vector2 Bezier2Tangent(float t, params Vector2[] points) =>
		(Bezier2(t + SMALL_FLOAT, points) - Bezier2(t, points)).normalized;

	public static Vector3 Bezier3(float t, params Vector3[] points) =>
		new Vector3(
			Bezier(t, points.Select<Vector3, float>(v => v.x).ToArray()),
			Bezier(t, points.Select<Vector3, float>(v => v.y).ToArray()),
			Bezier(t, points.Select<Vector3, float>(v => v.z).ToArray())
		);
	public static Vector3 Bezier3Tangent(float t, params Vector3[] points) =>
		(Bezier3(t + SMALL_FLOAT, points) - Bezier3(t, points)).normalized;
	#endregion

	public static float Round(float value, int decimals)
	{
		if (decimals < 0) decimals = 0;
		float power = Mathf.Pow(10f, decimals);
		return Mathf.Round(value * power) / power;
	}
}
