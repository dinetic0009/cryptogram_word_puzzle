using UnityEngine;
using System;
using UnityEngine.Advertisements;
using GoogleMobileAds.Api;
using GameAnalyticsSDK;
using System.Collections;
using UnityEngine.UI;
using Unity.Collections;
using UnityEngine.Events;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using Gley.MobileAds.Internal;
using Gley.MobileAds;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
public class AdsMediation : MonoBehaviour
{
    public Gley.MobileAds.BannerPosition bannerPosition;
    /// <summary>
    /// Singelton Instance
    /// </summary>
    public static AdsMediation instance;
    /// <summary>
    /// Time Based Ads
    /// </summary>
    [Header("Time Based Ads")]
    public bool isTimeBaseAdAllowed;
    [SerializeField] private int adTimerCount;
    public int timeDelayForAdInSeconds;
    /// <summary>
    /// Rewarded Ad Action
    /// </summary>
    public static Action<bool> rewardCallBack;
    /// <summary>
    /// AdToast UI
    /// </summary>
    [SerializeField] private GameObject ADToast;
#if UNITY_IOS
  private static bool backgroundOnAd = false;
#endif
    private void Awake()
    {
        instance = this;//Singelton Initialization
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;//Listen to application foreground / background events
        MobileAds.SetiOSAppPauseOnBackground(true);
    }
    private void Start()
    {
        MobileAdsManager.Instance.Initialize(GleyInitCompleted);
        if (isTimeBaseAdAllowed)
        {
            adTimerCount = 0;
            StartCoroutine(TimeBaseAutoAd());
        }
        ADToast.SetActive(false);//UI Toast
        GameAnalytics.Initialize();//Analytics Inititalization
    }
    
    public void OnAppStateChanged(AppState state)
    {
        // Display the app open ad when the app is foregrounded.
        UnityEngine.Debug.Log("App State is " + state);

        // OnAppStateChanged is not guaranteed to execute on the Unity UI thread.
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (state == AppState.Foreground)
            {
                if (AdsConfig.Get_RemoveAds_Status() == 0)
                {
                    ShowAppOpenAd();
                }
            }
        });
    }

    public void ShowAppOpenAd()
    {    
        if (MobileAdsManager.Instance.IsAppOpenAvailable())
        {
#if UNITY_IOS
         if (!backgroundOnAd)
         {
           if (PlayerPrefs.GetInt("SessionCount", 0) >= 5)
           {
             MobileAdsManager.Instance.ShowAppOpen(null);
           }
           else
           {
            int adcount = PlayerPrefs.GetInt("SessionCount", 0);
            adcount++;
            PlayerPrefs.SetInt("SessionCount", adcount);
           }
         }
         backgroundOnAd = false;
#elif UNITY_ANDROID
            MobileAdsManager.Instance.ShowAppOpen(null);
#endif
        }
    }

    #region Ads Calling Functions
    public void ShowInterstial()
    {
        if (AdsConfig.Get_RemoveAds_Status() == 0)
        {
            if (IsInterstitialReadyForAnyNetwork())
            {
                MobileAdsManager.Instance.ShowInterstitial(null);
                #if UNITY_IOS
                backgroundOnAd = true;
                #endif
            }
        }
    }

    public void ShowRewardedVideo()
    {
        MobileAdsManager.Instance.ShowRewardedVideo(CompleteMethod);
    }

    public bool IsRewardedVideoReadyForAnyNetwork()
    {
        return MobileAdsManager.Instance.IsRewardedVideoAvailable();
    }

    public bool IsInterstitialReadyForAnyNetwork()
    {
        return MobileAdsManager.Instance.IsInterstitialAvailable();
    }

    public void ShowBanner()
    {
        MobileAdsManager.Instance.ShowBanner(bannerPosition, BannerType.Banner, new Vector2Int(), new Vector2Int());
    }
    public void HideBanner()
    {
        MobileAdsManager.Instance.HideBanner();
    }
    #endregion

    #region Time Calculation For Time Based Ads
    IEnumerator TimeBaseAutoAd()
    {
        if (AdsConfig.Get_RemoveAds_Status() == 0)
        {
            yield return new WaitForSeconds(1);
            adTimerCount++;
                if (IsInterstitialReadyForAnyNetwork())
                {
                    if (timeDelayForAdInSeconds - adTimerCount <= 5)
                    {
                        ADToast.SetActive(true);
                        ADToast.transform.GetChild(0).GetComponent<Image>().fillAmount = ((float)timeDelayForAdInSeconds - (float)adTimerCount) / 5;
                    }
                    if (adTimerCount >= timeDelayForAdInSeconds)
                    {
                        ADToast.SetActive(false);
                        ShowInterstial();
                        adTimerCount = 0;
                    }
                }
            if (adTimerCount >= timeDelayForAdInSeconds + 1)
            {
                adTimerCount = timeDelayForAdInSeconds - 10;
            }
            StartCoroutine(TimeBaseAutoAd());
        }
        else
        {
            ADToast.SetActive(false);
            adTimerCount = 0;
        }
    }

    private void CompleteMethod(bool completed)
    {
        rewardCallBack?.Invoke(completed);
    }

    private void GleyInitCompleted()
    {
        Debug.Log("Gley Init Completed.");
        if (AdsConfig.Get_RemoveAds_Status() == 0)
        {
            ShowBanner();
        }
    }
    #endregion
}