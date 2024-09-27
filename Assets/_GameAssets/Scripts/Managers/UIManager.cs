using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MyBox;
using TMPro;
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

    bool canPlayAud = false;
    public static GameState gameState;


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
        UIManager.gameState = gameState;

        homePanel.SetActive(gameState is GameState.Home);
        gamePausePanel.SetActive(gameState is GameState.Puase);
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

    /////////////////////////////////////////// Set Panels ////////////////////////////////////////////////////
    [SerializeField] List<TextMeshProUGUI> uiLevelIndex;
    void SetGameplayPanel(LevelSO level)
    {
        uiLevelIndex.ForEach(x => x.text = $"Level {LevelManager.Instance.Level_No_UI}");
        SetPanels(GameState.Gameplay);
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
        var autherName = $"\'{LevelManager.Instance.CurrenLevel.autherName.Trim()}\'";

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