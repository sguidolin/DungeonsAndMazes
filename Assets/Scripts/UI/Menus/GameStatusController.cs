using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public class GameStatusController : MonoBehaviour
{
	public Canvas target;
	public TextMeshProUGUI result;

	void Awake()
	{
		Assert.IsNotNull(target);
		Assert.IsNotNull(result);
	}

	public void ShowEndScreen(bool victory)
	{
		target.gameObject.SetActive(true);

		result.text = victory
			? "Evil Vanquished"
			: "<color=red>Game Over</color>";
	}
}
