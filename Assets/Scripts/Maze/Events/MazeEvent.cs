using System.Collections;
using UnityEngine;

public abstract class MazeEvent : ScriptableObject
{
	// TODO: Implement generic class for events (portal pit, maybe monster?)
	/*
	 * Monster -> It's a sprite, game over when entering
	 * Portal -> It's a teleport that drops you in another position
	 * Pit -> Sprite? Game Over when entering
	 * 
	 * We handle everything with prefabs anyways
	 */
	public GameObject prefab;

	public abstract IEnumerator OnEventTrigger(ActorController caller);
}
