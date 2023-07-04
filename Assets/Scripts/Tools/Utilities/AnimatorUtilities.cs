using UnityEngine;

public static class AnimatorUtilities
{
	public static bool HasParameter(this Animator animator, string name)
	{
		foreach (AnimatorControllerParameter p in animator.parameters)
			if (p.name == name)
				return true;
		return false;
	}

	public static void TrySetFloat(this Animator animator, string name, float value)
	{
		if (HasParameter(animator, name))
			animator.SetFloat(name, value);
	}
	public static void TrySetInt(this Animator animator, string name, int value)
	{
		if (HasParameter(animator, name))
			animator.SetInteger(name, value);
	}
	public static void TrySetBool(this Animator animator, string name, bool value)
	{
		if (HasParameter(animator, name))
			animator.SetBool(name, value);
	}
	public static void TrySetTrigger(this Animator animator, string name)
	{
		if (HasParameter(animator, name))
			animator.SetTrigger(name);
	}
}