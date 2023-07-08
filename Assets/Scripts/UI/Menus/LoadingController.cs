using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class LoadingController : MonoBehaviour
{
	IEnumerator Start()
	{
		AsyncOperation load = SceneManager.LoadSceneAsync("DungeonPlay");
		while (!load.isDone) yield return null;
	}
}
