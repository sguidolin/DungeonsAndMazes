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

	#region Easing Functions
	public static float EaseInCubic(float t)
		=> t * t * t;
	public static float EaseOutCubic(float t)
		=> 1f - Mathf.Pow(1f - t, 3f);

	public static float EaseInQuart(float t)
		=> t * t * t * t;
	public static float EaseOutQuart(float t)
		=> 1f - Mathf.Pow(1f - t, 4f);
	public static float EaseInOutQuart(float t)
		=> t < 0.5f ? 8f * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 4f) / 2f;

	public static float EaseOutBounce(float t)
	{
		// Consts
		const float n1 = 7.5625f;
		const float d1 = 2.75f;

		if (t < 1f / d1)
		{
			return n1 * t * t;
		}
		else if (t < 2f / d1)
		{
			t -= 1.5f / d1;
			return n1 * t * t + 0.75f;
		}
		else if (t < 2.5f / d1)
		{
			t -= 2.25f / d1;
			return n1 * t * t + 0.9375f;
		}
		else
		{
			t -= 2.625f / d1;
			return n1 * t * t + 0.984375f;
		}
	}
	public static float EaseInBounce(float t)
		=> 1f - EaseOutBounce(1f - t);
	#endregion

	public static float Round(float value, int decimals)
	{
		if (decimals < 0) decimals = 0;
		float power = Mathf.Pow(10f, decimals);
		return Mathf.Round(value * power) / power;
	}

	public static int RoundNearest(int value, int multiple)
	{
		int remainder = value % multiple;
		int result = value - remainder;
		if (remainder > 0) // >= (multiple / 2)
			result += multiple;
		return result;
	}

	public static bool RectOverlaps(this Rect a, Rect b)
		=> (a.x < b.x + b.width && a.x + a.width > b.x && a.y < b.y + b.height && a.y + a.height > b.y);
}
