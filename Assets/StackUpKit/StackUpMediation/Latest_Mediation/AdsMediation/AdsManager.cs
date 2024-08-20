using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public GameObject adsPrefab;
    public static bool isInitialised;
    void Start()
    {
        if (!isInitialised)
        {
            isInitialised = true;
            DontDestroyOnLoad(Instantiate(adsPrefab));
        }
    }
}
