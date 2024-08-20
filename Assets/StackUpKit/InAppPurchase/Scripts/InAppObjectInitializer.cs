using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InAppObjectInitializer : MonoBehaviour
{
	[SerializeField]
	private GameObject Purchaser_Prefab;
	public static bool isInitialized = false;
	void Start()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			DontDestroyOnLoad(Instantiate(Purchaser_Prefab));
		}
		else
		{
			Debug.LogWarning("Purchase Prefab instance already created");
		}
	}
}
