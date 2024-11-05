using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MyBox;
using TMPro;
using System;
using DG.Tweening;
using MoreMountains.NiceVibrations;

public class UIManager : Singleton<UIManager>
{
    [Header("Panels")]
    [SerializeField] GameObject homePanel;
    [SerializeField] GameObject gameplayPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject gamePausePanel;
    [SerializeField] GameObject losePanel;
    [SerializeField] GameObject winPanel;

    [Header("Settings")]
    [SerializeField] Toggle soundToggle;
    [SerializeField] Toggle vibrationToggle;
    [SerializeField] Sprite toggleOnSp, toggleOffSp;


    [Header("TMPrp")]
    [SerializeField] TextMeshProUGUI winPanel_FinalQuote;
    [SerializeField] TextMeshProUGUI winPanel_FinalQuoteAuther;
    [SerializeField] List<TextMeshProUGUI> uiLevelIndex;

    [Header("Gameplay Colors")]
    [SerializeField] Image gameplayHeaderImage;
    [SerializeField] List<Color> gameplayColors;

    bool canPlayAud = false;
    public static GameState gameState;

    /* Adbreak Popup*/
    Coroutine AdBreakCoroutine = null;
    public int DelayForAdBreak;
    [SerializeField] private int timePassedSinceAd = 0;
    public AdBreakPopup adBreakPopup;

    private void OnEnable()
    {
        LevelManager.OnLevelInit += SetGameplayPanel;
    }

    private void Start()
    {
        int bol = PlayerPrefs.GetInt("UiPrfsSet", 0);
        if (bol == 0)
        {
            PlayerPrefs.SetInt("Sound", 1);
            PlayerPrefs.SetInt("Vibration", 1);
            PlayerPrefs.SetInt("UiPrfsSet", 1);
        }

        Init();
    }

    private void Init()
    {
        canPlayAud = false;

        On_SoundToggle(PlayerPrefs.GetInt("Sound") == 1);
        On_VibrationToggle(PlayerPrefs.GetInt("Vibration") == 1);
        SetPanels(GameState.Home);

        canPlayAud = true;
    }

    internal void SetPanels(GameState gameState)
    {
        var lastState = UIManager.gameState;
        UIManager.gameState = gameState;

        if(gameState is GameState.Gameplay && lastState is not GameState.Lose)
        {
            gameplayHeaderImage.color = gameplayColors.GetRandom();
        }

        gamePausePanel.SetActive(gameState is GameState.Puase);
        homePanel.SetActive(gameState is GameState.Home);
        losePanel.SetActive(gameState is GameState.Lose);
        winPanel.SetActive(gameState is GameState.Win);
    }

    public void OnClick_Sfx()
    {
        if (canPlayAud)
            SoundManager.Instance.PlaySfx(SoundType.Click);
    }

    /////////////////////////////////////////// OnClick Listners ////////////////////////////////////////////////////

    public void On_Play()
    {
        OnClick_Sfx();
        LevelManager.Instance.LoadCurrentLevel();
    }

    public void On_RestartLevel()
    {
        stopAdBreakCoroutine();
        OnClick_Sfx();
        LevelManager.Instance.LoadCurrentLevel();
    }

    public void On_Revive()
    {
        OnClick_Sfx();

#if UNITY_EDITOR
        GrantRevive();
#else
        if (AdsMediation.instance.IsRewardedVideoReadyForAnyNetwork())
        {
            AdsMediation.rewardCallBack += OnReward_GrantRevive;
            AdsMediation.instance.ShowRewardedVideo();
        }
#endif
    }

    void OnReward_GrantRevive(bool isCompleted)
    {
        if (isCompleted)
        {
            GrantRevive();
        }

        AdsMediation.rewardCallBack -= OnReward_GrantRevive;
    }

    void GrantRevive()
    {
        MistakesController.Instance.OnRevive();
        SetPanels(GameState.Gameplay);
    }

    public void On_NextLevel()
    {
        OnClick_Sfx();
        LevelManager.Instance.LoadNextLevel();
    }

    public void On_Home(Animations animation)
    {
        stopAdBreakCoroutine();
        OnClick_Sfx();
        animation.Reset();
        //gamePausePanel.SetActive(false);
        SetPanels(GameState.Home);
    }

    public void On_PauseSettings(Animations animation)
    {
        OnClick_Sfx();
        //gamePausePanel.SetActive(false);
        animation.Reset();
        settingsPanel.SetActive(true);
    }

    public void On_Hint()
    {
        OnClick_Sfx();
        Hint.Instance.On_ConsumeHint();
    }

    public void OnBuy_Hints20()
    {
        OnClick_Sfx();

#if UNITY_EDITOR
        Hint.Instance.GrantHints(Purchaser.instance.Consumable_Products.Find(x => x.ProductID == "hints_20").RewardAmount);
#else 
        Purchaser.instance.consumableAction += OnPurchase_Hints;
        Purchaser.instance.BuyConsumableProduct("hints_20");
#endif

    }

    public void OnBuy_Hints50()
    {
        OnClick_Sfx();

#if UNITY_EDITOR
        Hint.Instance.GrantHints(Purchaser.instance.Consumable_Products.Find(x => x.ProductID == "hints_50").RewardAmount);
#else
        Purchaser.instance.consumableAction += OnPurchase_Hints;
        Purchaser.instance.BuyConsumableProduct("hints_50");
#endif
    }


    void OnPurchase_Hints(Consumable product, bool isPurchased)
    {
        if (isPurchased)
        {
            Hint.Instance.GrantHints(product.RewardAmount);
        }

        Purchaser.instance.consumableAction -= OnPurchase_Hints;
    }

    public void OnAd_GetHint()
    {
        OnClick_Sfx();
        Hint.Instance.GetHintOnAd();
    }

    /////////////////////////////////////////// ------------ Set Panels -------------- ////////////////////////////////////////////////////

    void SetGameplayPanel(LevelSO level)
    {
        uiLevelIndex.ForEach(x => x.text = $"Level {LevelManager.Instance.Level_No_UI}");
        SetPanels(GameState.Gameplay);
        setupAdBreakCoroutine();
    }

    public void setupAdBreakCoroutine()
    {
        timePassedSinceAd = 0;
        AdBreakCoroutine = null;
        AdBreakCoroutine = StartCoroutine(CheckAndShowAd());
    }

    IEnumerator CheckAndShowAd()
    {
        while (timePassedSinceAd != DelayForAdBreak)
        {
            yield return new WaitForSeconds(1f);
            timePassedSinceAd++;
        }

        if (AdsMediation.instance.IsInterstitialReadyForAnyNetwork())
        {
            adBreakPopup.OnShowing();
        }
        else
        {
            setupAdBreakCoroutine();
        }
    }

    public int GetTimePassedSinceAd()
    {
        return timePassedSinceAd;
    }

    public void stopAdBreakCoroutine()
    {
        if (AdBreakCoroutine != null)
        {
            StopCoroutine(AdBreakCoroutine);
        }
    }

    public void SetLosePanel()
    {
        //SoundManager.Instance.PlaySfx(SoundType.Lose);
        SetPanels(GameState.Lose);
        GameAnalyticsSDK.GameAnalytics.NewProgressionEvent(GameAnalyticsSDK.GAProgressionStatus.Fail, LevelManager.Instance.LEVEL_IN_PROGRESS + "");

    }

    public void SetWinPanel()
    {
        SoundManager.Instance.PlaySfx(SoundType.Win);
        StartCoroutine(DisplayWords());

        JsonController.Instance.ResetData();
        LevelManager.Instance.CompleteLevel();
        SetPanels(GameState.Win);
    }

    IEnumerator DisplayWords()
    {
        winPanel_FinalQuote.text = "";
        winPanel_FinalQuoteAuther.text = "";


        yield return new WaitForSeconds(.5f);

        var quote = $"\"{LevelManager.Instance.CurrenLevel.phrase.Trim()}\"";
        var autherName = $"-{LevelManager.Instance.CurrenLevel.autherName.Trim()}";

        //string[] words = quote.Split(' ');
        float delay = .05f;

        for (int i = 0; i < quote.Length; i++)
        {
            winPanel_FinalQuote.text += quote[i];//words[i] + " ";
            yield return new WaitForSeconds(delay);
        }

        //words = autherName.Split(' ');

        for (int i = 0; i < autherName.Length; i++)
        {
            winPanel_FinalQuoteAuther.text += autherName[i]; //words[i] + " ";
            yield return new WaitForSeconds(delay);
        }

    }

    /////////////////////////////////////////// Setting Panel  ////////////////////////////////////////////////////

    public void On_SoundToggle(bool isOn)
    {
        int value = isOn ? 1 : 0;
        PlayerPrefs.SetInt("Sound", value);
        SetToggleVisuals(soundToggle, isOn);
        SoundManager.Instance.SetSfxVolume(!isOn);
        OnClick_Sfx();
    }


    public void On_VibrationToggle(bool isOn)
    {
        OnClick_Sfx();
        PlayerPrefs.SetInt("Vibration", isOn ? 1 : 0);
        SetToggleVisuals(vibrationToggle, isOn);
    }

    public void Vibrate(HapticTypes hapticTypes = HapticTypes.MediumImpact)
    {
        if (PlayerPrefs.GetInt("Vibration") == 1)
            MMVibrationManager.Haptic(hapticTypes);
    }

    void SetToggleVisuals(Toggle toggle, bool isOn)
    {
        toggle.SetIsOnWithoutNotify(isOn);
        toggle.GetComponent<Image>().sprite = isOn ? toggleOnSp : toggleOffSp;
        toggle.transform.GetChild(0).GetComponent<RectTransform>().DOAnchorPos3DX((isOn ? 7.5f : -7.5f), .3f).SetEase(Ease.Linear);
    }


    public void On_RemoveAds()
    {
        OnClick_Sfx();
        Purchaser.instance.NonConsumableAction += OnPurchase_Ads;
        Purchaser.instance.BuyNonConsumableProduct("RemoveAds");

    }

    void OnPurchase_Ads(NonConsumable product, bool isPurchased)
    {
        if (isPurchased)
        {
            AdsConfig.Set_RemoveAds_Status();
        }

        Purchaser.instance.NonConsumableAction -= OnPurchase_Ads;
    }

    public void On_RestorePurchases()
    {
        Purchaser.instance.RestorePurchases();
        OnClick_Sfx();
    }

    private void OnDisable()
    {
        LevelManager.OnLevelInit -= SetGameplayPanel;
    }

}//Class

public enum GameState
{
    Splash,
    Home,
    Gameplay,
    Lose,
    Win,
    Puase
}