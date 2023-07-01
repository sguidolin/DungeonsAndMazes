using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(WriteOnlyInEditorAttribute))]
public class WriteOnlyInEditorDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		bool wasGuiEnabled = GUI.enabled;
		GUI.enabled &= !EditorApplication.isPlaying;

		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = wasGuiEnabled;
	}
}
