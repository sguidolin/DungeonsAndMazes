using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class EventLogger : MonoBehaviour
{
	private TextMeshProUGUI _textComponent;

	[SerializeField, Min(1)]
	private int _queueSize = 5;
	private Queue<string> _events = new Queue<string>();

	void Awake()
	{
		_textComponent = GetComponent<TextMeshProUGUI>();
	}

	public void Log(string text)
	{
		if (_events.Count + 1 > _queueSize)
			_events.Dequeue();
		_events.Enqueue(text);

		StringBuilder eventsTrace = new StringBuilder();
		foreach (string eventTrace in _events)
		{
			eventsTrace.AppendFormat("> {0}", eventTrace);
			eventsTrace.AppendLine();
		}
		_textComponent.text = eventsTrace.ToString();
	}
}
