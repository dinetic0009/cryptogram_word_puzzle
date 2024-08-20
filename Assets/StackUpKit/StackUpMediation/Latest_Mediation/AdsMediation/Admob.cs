using System;
using UnityEngine;

[Serializable]
public class Admob
{
    [Header("Google Admob")]
    [Space]

    [Header("Android IDs")]
    public string androidBanner;
    public string androidInterstitial;
    public string androidRewarded;
    public string androidOpenAd;

    [Header("iOS IDs")]
    public string iosBanner;
    public string iosInterstitial;
    public string iosRewarded;
    public string iosOpenAd;

}
