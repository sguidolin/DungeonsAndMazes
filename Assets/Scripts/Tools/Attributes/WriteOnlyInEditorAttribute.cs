using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class WriteOnlyInEditorAttribute : PropertyAttribute
{

}