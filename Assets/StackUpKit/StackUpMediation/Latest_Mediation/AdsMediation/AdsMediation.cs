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
using UnityEngine.Networking;
using System.Linq;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
public class AdsMediation : MonoBehaviour
{
    [SerializeField]
    private string NetworkSelected = "GOOGLE";
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
    public static Action interstitialCallBack;
    /// <summary>
    /// AdToast UI
    /// </summary>
    [SerializeField] private GameObject ADToast;
#if UNITY_IOS
    private static bool backgroundOnAd = false;
#endif

    /// <summary>
    /// Applovin Data
    /// </summary>
    /// 
    private const string MaxSdkKey = "oNZiOxpGQxTFdh1H3t75Qf8AdTmD4vEaDbawBTTsLkewfhH28_PrHhdVJN1IMzlql6CNePcunbJLAQhhcszq7b";

#if UNITY_IOS
    private const string BannerAdUnitId = "e18c28af197263a7";
    private const string InterstitialAdUnitId = "e31718750a06409a";
    private const string RewardedAdUnitId = "85b56ecb4d80ae25";
    private const string RewardedInterstitialAdUnitId = "";
    private const string MRecAdUnitId = "";
#else // UNITY_ANDROID
    private const string BannerAdUnitId = "cb7dc179362544f1";
    private const string InterstitialAdUnitId = "ee64163483c346a4";
    private const string RewardedAdUnitId = "50a47b40ce17e60f";
    private const string RewardedInterstitialAdUnitId = "";
    private const string MRecAdUnitId = "";
#endif


    private int interstitialRetryAttempt;
    private int rewardedRetryAttempt;
    private int rewardedInterstitialRetryAttempt;

    private void Awake()
    {
        instance = this;//Singelton Initialization

    }
    private void Start()
    {
        GameAnalytics.Initialize();//Analytics Inititalization
        if (isTimeBaseAdAllowed)
        {
            adTimerCount = 0;
            StartCoroutine(TimeBaseAutoAd());
        }
        ADToast.SetActive(false);//UI Toast
        StartCoroutine(SetCountry());
    }
    private void InitializeGoogleAds()
    {
        NetworkSelected = "GOOGLE";
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;//Listen to application foreground / background events
        MobileAds.SetiOSAppPauseOnBackground(true);
        MobileAdsManager.Instance.Initialize(GleyInitCompleted);

    }
    private void InitializeApplovinAds()
    {
        NetworkSelected = "APPLOVIN";
        MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
        {
            // AppLovin SDK is initialized, configure and start loading ads.
            Debug.Log("MAX SDK Initialized");
            InitializeInterstitialAds();
            InitializeRewardedAds();
            //InitializeRewardedInterstitialAds();
            //InitializeMRecAds();
        };
        MaxSdk.SetSdkKey(MaxSdkKey);
        MaxSdk.InitializeSdk();
        if (AdsConfig.Get_RemoveAds_Status() == 0)
        {
            InitializeBannerAds();
        }
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
            if (NetworkSelected == "GOOGLE")
            {
                Debug.Log("Show Google Interstitial Ad");

                if (IsInterstitialReadyForAnyNetwork())
                {
                    MobileAdsManager.Instance.ShowInterstitial(InterstitalCompleteMethod);
                }
            }
            else if (NetworkSelected == "APPLOVIN")
            {
                Debug.Log("Show Applovin Interstitial Ad");

                ShowInterstitialApplovin();
            }
#if UNITY_IOS
            backgroundOnAd = true;
#endif

        }
    }

    public void ShowRewardedVideo()
    {
        Debug.Log("Show Rewarded AD");

        if (NetworkSelected == "GOOGLE")
        {
            Debug.Log("Show Google Rewarded Ad");

            MobileAdsManager.Instance.ShowRewardedVideo(CompleteMethod);
        }
        else if (NetworkSelected == "APPLOVIN")
        {
            Debug.Log("Show Applovin Rewarded Ad");
            ShowRewardedAdApplovin();
        }
    }

    public bool IsRewardedVideoReadyForAnyNetwork()
    {
        return MobileAdsManager.Instance.IsRewardedVideoAvailable() || MaxSdk.IsRewardedAdReady(RewardedAdUnitId);
    }

    public bool IsInterstitialReadyForAnyNetwork()
    {
        return MobileAdsManager.Instance.IsInterstitialAvailable();
    }

    public void ShowBanner()
    {
        if (NetworkSelected == "GOOGLE")
        {
            MobileAdsManager.Instance.ShowBanner(bannerPosition, BannerType.Banner, new Vector2Int(), new Vector2Int());
        }
        else if (NetworkSelected == "APPLOVIN")
        {
            MaxSdk.ShowBanner(BannerAdUnitId);
        }
    }
    public void HideBanner()
    {
        if (NetworkSelected == "GOOGLE")
        {
            MobileAdsManager.Instance.HideBanner();
        }
        else if (NetworkSelected == "APPLOVIN")
        {
            MaxSdk.HideBanner(BannerAdUnitId);
        }
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

    private void InterstitalCompleteMethod()
    {
        interstitialCallBack?.Invoke();
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
    #region Applovin Callbacks

    #region Interstitial Ad Methods

    private void InitializeInterstitialAds()
    {
        // Attach callbacks
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialRevenuePaidEvent;

        // Load the first interstitial
        LoadInterstitial();
    }

    void LoadInterstitial()
    {
        Debug.Log("Loading Interstitial");
        MaxSdk.LoadInterstitial(InterstitialAdUnitId);
    }
    public void ShowInterstitialApplovin()
    {
        if (MaxSdk.IsInterstitialReady(InterstitialAdUnitId))
        {
            Debug.Log("Show Interstitial");
            MaxSdk.ShowInterstitial(InterstitialAdUnitId);
        }
        else
        {
            Debug.Log("Interstitial Not Ready");
        }
    }
    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
        Debug.Log("Interstitial loaded");

        // Reset retry attempt
        interstitialRetryAttempt = 0;
    }

    private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
        interstitialRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));
        Debug.Log("Interstitial failed to load with error code: " + errorInfo.Code);

        Invoke("LoadInterstitial", (float)retryDelay);
    }

    private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. We recommend loading the next ad
        Debug.Log("Interstitial failed to display with error code: " + errorInfo.Code);
        LoadInterstitial();
    }

    private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad
        Debug.Log("Interstitial dismissed");
        LoadInterstitial();
        InterstitalCompleteMethod();
    }

    private void OnInterstitialRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad revenue paid. Use this callback to track user revenue.
        Debug.Log("Interstitial revenue paid");

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to

    }
    #endregion
    #region Rewarded Ad Methods

    private void InitializeRewardedAds()
    {
        // Attach callbacks
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;

        // Load the first RewardedAd
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        Debug.Log("Loading Rewarded");
        MaxSdk.LoadRewardedAd(RewardedAdUnitId);
    }

    public void ShowRewardedAdApplovin()
    {
        if (MaxSdk.IsRewardedAdReady(RewardedAdUnitId))
        {
            Debug.Log("Show Rewarded");
            MaxSdk.ShowRewardedAd(RewardedAdUnitId);
        }
        else
        {
            Debug.Log("Rewarded Not Ready");
        }
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
        Debug.Log("Rewarded ad loaded");

        // Reset retry attempt
        rewardedRetryAttempt = 0;
    }

    private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
        rewardedRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, rewardedRetryAttempt));

        Debug.Log("Rewarded ad failed to load with error code: " + errorInfo.Code);

        Invoke("LoadRewardedAd", (float)retryDelay);
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. We recommend loading the next ad
        Debug.Log("Rewarded ad failed to display with error code: " + errorInfo.Code);
        LoadRewardedAd();
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad displayed");
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad clicked");
    }

    private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        Debug.Log("Rewarded ad dismissed");
        CompleteMethod(false);
        LoadRewardedAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad was displayed and user should receive the reward
        Debug.Log("Rewarded ad received reward");
        CompleteMethod(true);
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad revenue paid. Use this callback to track user revenue.
        Debug.Log("Rewarded ad revenue paid");

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to

    }

    #endregion
    #region Rewarded Interstitial Ad Methods

    private void InitializeRewardedInterstitialAds()
    {
        // Attach callbacks
        MaxSdkCallbacks.RewardedInterstitial.OnAdLoadedEvent += OnRewardedInterstitialAdLoadedEvent;
        MaxSdkCallbacks.RewardedInterstitial.OnAdLoadFailedEvent += OnRewardedInterstitialAdFailedEvent;
        MaxSdkCallbacks.RewardedInterstitial.OnAdDisplayFailedEvent += OnRewardedInterstitialAdFailedToDisplayEvent;
        MaxSdkCallbacks.RewardedInterstitial.OnAdDisplayedEvent += OnRewardedInterstitialAdDisplayedEvent;
        MaxSdkCallbacks.RewardedInterstitial.OnAdClickedEvent += OnRewardedInterstitialAdClickedEvent;
        MaxSdkCallbacks.RewardedInterstitial.OnAdHiddenEvent += OnRewardedInterstitialAdDismissedEvent;
        MaxSdkCallbacks.RewardedInterstitial.OnAdReceivedRewardEvent += OnRewardedInterstitialAdReceivedRewardEvent;
        MaxSdkCallbacks.RewardedInterstitial.OnAdRevenuePaidEvent += OnRewardedInterstitialAdRevenuePaidEvent;

        // Load the first RewardedInterstitialAd
        LoadRewardedInterstitialAd();
    }

    private void LoadRewardedInterstitialAd()
    {
        Debug.Log("Loading RewardedInterstitial");
        MaxSdk.LoadRewardedInterstitialAd(RewardedInterstitialAdUnitId);
    }

    private void ShowRewardedInterstitialAd()
    {
        if (MaxSdk.IsRewardedInterstitialAdReady(RewardedInterstitialAdUnitId))
        {
            Debug.Log("Show RewardedInterstitial");
            MaxSdk.ShowRewardedInterstitialAd(RewardedInterstitialAdUnitId);
        }
        else
        {
            Debug.Log("RewardedInterstitial Not Ready");
        }
    }

    private void OnRewardedInterstitialAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded interstitial ad is ready to be shown. MaxSdk.IsRewardedInterstitialAdReady(rewardedInterstitialAdUnitId) will now return 'true'
        Debug.Log("Rewarded interstitial ad loaded");

        // Reset retry attempt
        rewardedInterstitialRetryAttempt = 0;
    }

    private void OnRewardedInterstitialAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
        rewardedInterstitialRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, rewardedInterstitialRetryAttempt));

        Debug.Log("Rewarded interstitial ad failed to load with error code: " + errorInfo.Code);

        Invoke("LoadRewardedInterstitialAd", (float)retryDelay);
    }

    private void OnRewardedInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded interstitial ad failed to display. We recommend loading the next ad
        Debug.Log("Rewarded interstitial ad failed to display with error code: " + errorInfo.Code);
        LoadRewardedInterstitialAd();
    }

    private void OnRewardedInterstitialAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded interstitial ad displayed");
    }

    private void OnRewardedInterstitialAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded interstitial ad clicked");
    }

    private void OnRewardedInterstitialAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded interstitial ad is hidden. Pre-load the next ad
        Debug.Log("Rewarded interstitial ad dismissed");
        LoadRewardedInterstitialAd();

    }

    private void OnRewardedInterstitialAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded interstitial ad was displayed and user should receive the reward
        Debug.Log("Rewarded interstitial ad received reward");
    }

    private void OnRewardedInterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded interstitial ad revenue paid. Use this callback to track user revenue.
        Debug.Log("Rewarded interstitial ad revenue paid");

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to

    }

    #endregion

    #region Banner Ad Methods

    private void InitializeBannerAds()
    {
        // Attach Callbacks
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;

        // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
        // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
        MaxSdk.CreateBanner(BannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

        // Set background or background color for banners to be fully functional.
        MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, Color.black);
        MaxSdk.SetBannerWidth(BannerAdUnitId, 320.0f);
    }

    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Banner ad is ready to be shown.
        // If you have already called MaxSdk.ShowBanner(BannerAdUnitId) it will automatically be shown on the next ad refresh.
        Debug.Log("Banner ad loaded");
        if (AdsConfig.Get_RemoveAds_Status() == 0)
        {
            ShowBanner();
        }
    }

    private void OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Banner ad failed to load. MAX will automatically try loading a new ad internally.
        Debug.Log("Banner ad failed to load with error code: " + errorInfo.Code);
    }

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Banner ad clicked");
    }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Banner ad revenue paid. Use this callback to track user revenue.
        Debug.Log("Banner ad revenue paid");

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to

    }

    #endregion
    public bool IsInterstitialReady()
    {
        return MaxSdk.IsInterstitialReady(InterstitialAdUnitId);
    }
    public bool IsRewardedReady()
    {
        return MaxSdk.IsRewardedAdReady(RewardedAdUnitId);
    }
    #endregion
    public IEnumerator SetCountry()
    {
        string ip = new System.Net.WebClient().DownloadString("https://api.ipify.org");
        string uri = $"https://ipapi.co/{ip}/json/";


        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;
            try
            {
                CountriesAPIData ipApiData = CountriesAPIData.CreateFromJSON(webRequest.downloadHandler.text);
                Debug.LogError(ipApiData.country_name);
                ipApiData.country_name = "Russia";
                if (CountriesAPIData.IsGoogleSupportedCountry(ipApiData.country_name))
                {
                    InitializeGoogleAds();
                    GameAnalytics.NewDesignEvent("Network " + ipApiData.country_name + " GOOGLE");
                }
                else
                {
                    InitializeApplovinAds();
                    GameAnalytics.NewDesignEvent("Network " + ipApiData.country_name + " APPLOVIN");

                }
            }
            catch
            {
                InitializeApplovinAds();
                GameAnalytics.NewDesignEvent("Network " + "UNKNOWN" + " APPLOVIN");

            }

        }
    }
}

[Serializable]
public class CountriesAPIData
{
    public string country_name;

    public static CountriesAPIData CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<CountriesAPIData>(jsonString);
    }
    private static string[] google_supported_countries = { "Anguilla", "Antigua and Barbuda", "Argentina", "Aruba", "Bahamas", "Barbados", "Belize", "Bermuda", "Bolivia", "Brazil", "Canada", "Cayman Islands", "Chile", "Colombia", "Costa Rica", "Dominica", "Dominican Republic", "Ecuador", "El Salvador", "Falkland Islands (Malvinas)", "French Guiana", "Grenada", "Guadeloupe", "Guatemala", "Guyana", "Haiti", "Honduras", "Jamaica", "Martinique", "Mexico", "Montserrat", "Netherlands Antilles", "Nicaragua", "Panama", "Paraguay", "Peru", "Puerto Rico", "Saint Kitts and Nevis", "Saint Lucia", "Saint Vincent and the Grenadines", "St. Pierre and Miquelon", "Suriname", "Trinidad and Tobago", "Turks and Caicos Islands", "United States", "United States Minor Outlying Islands", "Uruguay", "Venezuela", "Virgin Islands (British)", "Virgin Islands (U.S.)", "American Samoa", "Antarctica", "Australia", "Bangladesh", "Bhutan", "Brunei Darussalam", "Cambodia", "Christmas Island", "Cocos (Keeling) Islands", "Cook Islands", "East Timor", "Fiji", "French Polynesia", "French Southern Territories", "Guam", "Heard and McDonald Islands", "Hong Kong", "India", "Indonesia", "Japan", "Kiribati", "Lao People's Democratic Republic", "Macau", "Malaysia", "Maldives", "Marshall Islands", "Micronesia", "Mongolia", "Nauru", "Nepal", "New Caledonia", "New Zealand", "Niue", "Norfolk Island", "Northern Mariana Islands", "Pakistan", "Palau", "Papua New Guinea", "Philippines", "Pitcairn", "Samoa", "Singapore", "Solomon Islands", "South Georgia and The South Sandwich Islands", "South Korea", "Sri Lanka", "St. Helena", "Taiwan", "Thailand", "Tokelau", "Tonga", "Tuvalu", "Vanuatu", "Viet Nam", "Wallis and Futuna Islands", "Albania", "Algeria", "Andorra", "Angola", "Armenia", "Austria", "Azerbaijan", "Bahrain", "Belarus", "Belgium", "Benin", "Bosnia and Herzegovina", "Botswana", "Bouvet Island", "British Indian Ocean Territory", "Bulgaria", "Burkina Faso", "Burundi", "Cameroon", "Cape Verde", "Central African Republic", "Chad", "Comoros", "Congo", "Congo, Democratic Republic", "Cote d'Ivoire", "Croatia", "Cyprus", "Czech Republic", "Denmark", "Djibouti", "Egypt", "Equatorial Guinea", "Eritrea", "Estonia", "Ethiopia", "Faroe Islands", "Finland", "France", "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Gibraltar", "Greece", "Greenland", "Guinea", "Guinea-Bissau", "Hungary", "Iceland", "Iraq", "Ireland", "Israel", "Italy", "Jordan", "Kazakhstan", "Kenya", "Kuwait", "Kyrgyzstan", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Macedonia", "Madagascar", "Malawi", "Mali", "Malta", "Mauritania", "Mauritius", "Mayotte", "Moldova", "Monaco", "Montenegro", "Morocco", "Mozambique", "Namibia", "Netherlands", "Niger", "Nigeria", "Norway", "Oman", "Palestinian Territory", "Poland", "Portugal", "Qatar", "Reunion", "Romania", "Rwanda", "San Marino", "Sao Tome and Principe", "Saudi Arabia", "Senegal", "Serbia", "Seychelles", "Sierra Leone", "Slovakia", "Slovenia", "South Africa", "Spain", "Svalbard and Jan Mayen Islands", "Swaziland", "Sweden", "Switzerland", "Tajikistan", "Tanzania", "Togo", "Tunisia", "Turkey", "Turkmenistan", "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom", "Uzbekistan", "Vatican", "Western Sahara", "Yemen", "Zambia", "Zimbabwe" };
    public static bool IsGoogleSupportedCountry(string country)
    {
        if (google_supported_countries.Any(country.Contains))
        {
            return true;
        }
        return false;
    }
}