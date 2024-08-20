using System;
using Unity.Collections;
using UnityEngine;
[Serializable]
public class UnityAds
{
    [Header("Android IDs")]
    public string androidGameId;
    [Header("iOS IDs")]
    public string iOSGameId;


    [ReadOnlyAttribute]
    [SerializeField]
    private string interstitial_android = "Interstitial_Android";

    [ReadOnlyAttribute]
    [SerializeField]
    private string rewardedVideo_android = "Rewarded_Android";

    [ReadOnlyAttribute]
    [SerializeField]
    private string interstitial_iOS = "Interstitial_iOS";

    [ReadOnlyAttribute]
    [SerializeField]
    private string rewardedVideoiOS = "Rewarded_iOS";




    public string get_interstitial()
    {
#if UNITY_ANDROID
        return interstitial_android;
#elif UNITY_IOS
        return interstitial_iOS;
#endif

    }
    public string get_rewarded()
    {
#if UNITY_ANDROID
        return rewardedVideo_android;
#elif UNITY_IOS
        return rewardedVideoiOS;

#endif

    }
    public string get_game_id()
    {
#if UNITY_ANDROID
        return androidGameId;
#elif UNITY_IOS
        return iOSGameId;

#endif

    }
}
