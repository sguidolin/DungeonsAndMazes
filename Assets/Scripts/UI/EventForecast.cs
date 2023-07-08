using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public class EventForecast : MonoBehaviour
{
	[SerializeField]
	private EventForecastPanel[] _panels;

	[SerializeField, WriteOnlyInEditor]
	private Vector2 _leftBounds = new Vector2(-0.75f, 1.25f);
	[SerializeField, WriteOnlyInEditor]
	private Vector2 _rightBounds = new Vector2(0.75f, 0.75f);

	void Awake()
	{
		Assert.IsNotNull(_panels, "Event Panels not set!");
	}

	public void SetEventForecast()
		=> SetEventForecast(new string[] { });
	public void SetEventForecast(IEnumerable<string> events)
	{
		// Reset all panels
		foreach (EventForecastPanel panel in _panels)
			panel.SetTrigger("None");

		// Get the currently active events
		// But only take up to the panels we have
		string[] activeEvents = events
			.Take<string>(_panels.Length)
			.Where<string>(@event => @event != "None")
			.ToArray<string>();
		if (activeEvents.Length > 0)
		{
			// Get the number of panels
			int activePanels = activeEvents.Length;
			float offset = 1f / (_panels.Length + 1);
			float set = offset * 2f;
			// Calculate the initial t value
			float t = 0f - (offset * (activePanels - 1));
			for (int index = 0; index < activePanels; index++)
			{
				// Set the event panel position
				_panels[index].transform.localPosition = Vector2.Lerp(_leftBounds, _rightBounds, t + set);
				_panels[index].SetTrigger(activeEvents[index]);
				// Update the t value
				t += set;
			}
		}
	}
}
