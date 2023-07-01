using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Slider))]
public class SliderValueUpdate : MonoBehaviour
{
	private Slider _slider;
	[SerializeField]
	private TextMeshProUGUI _text;

	public string format = "{0:N2}";

	void Awake()
	{
		_slider = GetComponent<Slider>();
		if (_text == null) enabled = false;
	}

	void Start() => ToText();

	public void ToText()
	{
		_slider.value = MathUtilities.Round(_slider.value, 2);
		if (_text != null) _text.text = string.Format(format, _slider.value);
	}
}
