using UnityEngine;
using UnityEngine.Assertions;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
	[Header("Dungeon Generation Settings")]
	[WriteOnlyInEditor] public string seed = "";
	[WriteOnlyInEditor] public int depth = 20;
	[WriteOnlyInEditor] public int width = 20;
	[WriteOnlyInEditor] public float fillRatio = 0.5f;
	[WriteOnlyInEditor] public bool allowOverfilling = true;

	#region Singleton Instance
	private static GameManager _instance = null;

	public static GameManager Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject gm = new GameObject("[Game Manager]");
				//gm.hideFlags = HideFlags.HideAndDontSave;
				gm.hideFlags = HideFlags.DontSave;
				_instance = gm.AddComponent<GameManager>();
			}
			return _instance;
		}
	}

	void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Assert.IsTrue(false, "FATAL ERROR");
			DestroyImmediate(gameObject);
		}
		else
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	void OnDestroy()
	{
		if (Instance == this)
			_instance = null;
	}
	#endregion
}
