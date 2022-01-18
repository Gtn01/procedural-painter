using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ThreadDispatch : MonoBehaviour
{

	private static readonly Queue<Action> _executionQueue = new Queue<Action>();
	private static ThreadDispatch _instance = null;
	public bool isPersistant;
	
	private void Start()
	{
		if (isPersistant)
		{
			DontDestroyOnLoad(this);
		}
	}
	
	public void Update() {
		lock(_executionQueue) {
			//Debug.Log("dispatcher : [ "+_executionQueue.Count+" ]");
			while (_executionQueue.Count > 0) {
				_executionQueue.Dequeue().Invoke();
			}
		}
	}

	/**
	 ****************************************************************
	 * @brief Ajoute une action dans la file d'attente
	 *****************************************************************/
	public void Enqueue(IEnumerator action) {
		lock (_executionQueue) {
			_executionQueue.Enqueue (() => {
				StartCoroutine (action);
			});
		}
	}


	public void Enqueue(Action action)
	{
		Enqueue(ActionWrapper(action));
	}
	IEnumerator ActionWrapper(Action a)
	{
		a();
		yield return null;
	}




	public static bool Exists() {
		return _instance != null;
	}

	public static ThreadDispatch Instance() {
		if (!Exists ()) {
			throw new Exception ("UnityMainThreadDispatcher could not find the UnityMainThreadDispatcher object. Please ensure you have added the MainThreadExecutor Prefab to your scene.");
		}
		return _instance;
	}


	void Awake() {
		
		if(isPersistant) {
			if(!_instance) {
				_instance = this as ThreadDispatch;
			}
			else {
				DestroyObject(gameObject);
			}
			DontDestroyOnLoad(gameObject);
		}
		else {
			_instance = this as ThreadDispatch;
		}	
	}

	void OnDestroy()
	{
		//_instance = null;			
	}
}
