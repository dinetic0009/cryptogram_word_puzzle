using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsConfig : MonoBehaviour
{

    public static void Set_RemoveAds_Status()
    {
        PlayerPrefs.SetInt("Remove_Ads", 1);
        AdsMediation.instance.HideBanner();
    }

    public static int Get_RemoveAds_Status()
    {
        return PlayerPrefs.GetInt("Remove_Ads", 0);
    }
    public static int rateCounter;
}
