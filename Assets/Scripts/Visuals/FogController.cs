using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshRenderer))]
public class FogController : MonoBehaviour
{
	[SerializeField, Range(0f, 1f)]
	private float _density = 1f;
	[SerializeField, Min(0.1f)]
	private float _animationDuration = 1f;

	private Material _fog;
	private Color _color;

	public bool IsVisible => _color.a > 0f;

	void Awake()
	{
		MeshRenderer renderer = GetComponent<MeshRenderer>();
		// Ensure that the fog isn't casting shadows
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		// Get the material
		_fog = renderer.material;
	}

	void Start()
	{
		// Reference the color
		_color = _fog.GetColor("_FogColor");
		// Apply the fog density
		_color.a = _density;
		_fog.SetColor("_FogColor", _color);
	}

	IEnumerator Disappear()
	{
		while (IsVisible)
		{
			// Calculate frame variation
			float variation = (_density / _animationDuration) * Time.deltaTime;
			// Apply it clamped to 0..1
			_color.a = Mathf.Clamp01(_color.a - variation);
			_fog.SetColor("_FogColor", _color);
			// Wait for next frame
			yield return new WaitForEndOfFrame();
		}
	}
}
