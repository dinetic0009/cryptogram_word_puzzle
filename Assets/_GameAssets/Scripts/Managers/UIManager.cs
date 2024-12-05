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
    [SerializeField] Toggle musicToggle;
    [SerializeField] Toggle vibrationToggle;
    [SerializeField] Sprite toggleOnSp, toggleOffSp;
    [SerializeField] Sprite toggleDarkOnSp, toggleDarkOffSp;

    [Header("TMPrp")]
    [SerializeField] TextMeshProUGUI winPanel_FinalQuote;
    [SerializeField] TextMeshProUGUI winPanel_FinalQuoteAuther;
    [SerializeField] List<TextMeshProUGUI> uiLevelIndex;

    [Space(5)]
    [Header("Gameplay Music Object")]
    [SerializeField] GameObject _musicObjectPanel;
    [SerializeField] List<GameMusicData> _gameMusicClips;
    [SerializeField] Transform _musicObjectsParent;
    [SerializeField] Transform _musicBtnObject;
    [SerializeField] MusicBar _musicObjectPrefeb;
    [SerializeField] Image _musictitleicon;
    [SerializeField] TextMeshProUGUI _musicPaneltitle;
    [SerializeField] AudioSource _musicSrc;
    [SerializeField] GameMusicType _defaultMusic;

    private string PlayingMusicName;
    private MusicBar _currentMusicBar;

    [Space(5)]
    [Header("hint buttons")]
    [SerializeField] Hint _letterHintBtn;
    [SerializeField] Hint _WordHintBtn;


    [Space(5)]
    [Header("Gameplay Colors")]
    [SerializeField] Image gameplayHeaderImage;
    [SerializeField] List<Color> gameplayColors;

    bool canPlayAud = false;
    bool canplaymusic = false;
    public static GameState gameState;

    /* Adbreak Popup*/
    Coroutine AdBreakCoroutine = null;
    public int DelayForAdBreak;
    [SerializeField] private int timePassedSinceAd = 0;
    public AdBreakPopup adBreakPopup;

    GameMusicData currentClipData;

    public Hint LetterHintBtn { get => _letterHintBtn; }
    public Hint WordHintBtn { get => _WordHintBtn; }

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
            PlayerPrefs.SetInt("Music", 1);
            PlayerPrefs.SetInt("UiPrfsSet", 1);
        }

        Init();
    }

    private void Init()
    {
        canPlayAud = false;
        canplaymusic = false;
        On_SoundToggle(PlayerPrefs.GetInt("Sound") == 1);
        On_MusicToggle(PlayerPrefs.GetInt("Music") == 1);
        On_VibrationToggle(PlayerPrefs.GetInt("Vibration") == 1);
        SetPanels(GameState.Home);
        Invoke("PlayDefaultMusic", 0.5f);
    }

    internal void SetPanels(GameState gameState)
    {
        var lastState = UIManager.gameState;
        UIManager.gameState = gameState;

        //if(gameState is GameState.Gameplay && lastState is not GameState.Lose)
        //{
        //    gameplayHeaderImage.color = gameplayColors.GetRandom();
        //}

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

    public void PlayDefaultMusic()
    {
        if (!canplaymusic)
            return;

        if (PlayerPrefs.HasKey("selectedtheme") && PlayerPrefs.HasKey("selectedclip"))
        {
            int themeindex = PlayerPrefs.GetInt("selectedtheme");
            GameMusicData data = _gameMusicClips[themeindex];
            string clipname = PlayerPrefs.GetString("selectedclip");

            for (int i = 0; i < data.ClipData.Count; i++)
            {
                if (clipname == data.ClipData[i].name)
                {
                    SelectMusicType(data);
                    SetMusicClip(data.ClipData[i].name, data.ClipData[i], null);
                    break;
                }
            }

            return;
        }

        for (int i = 0; i < _gameMusicClips.Count; i++)
        {

            if (_gameMusicClips[i].Type == _defaultMusic && _gameMusicClips[i].ClipData.Count > 0)
            {
                SelectMusicType(_gameMusicClips[i]);
                SetMusicClip(_gameMusicClips[i].ClipData[0].name, _gameMusicClips[i].ClipData[0], null);
                PlayerPrefs.SetInt("selectedtheme", i);
                break;
            }
            else if (_gameMusicClips[i].Type == _defaultMusic)
                break;
        }
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

    public void On_Revive(Animations _animation)
    {
        OnClick_Sfx();

#if UNITY_EDITOR
        _animation.Reset();
        GrantRevive();
#else
        if (AdsMediation.instance.IsRewardedVideoReadyForAnyNetwork())
        {
            _animation.Reset();
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



    #region  ---- Letter Hint Functions -----

    public void On_Hint()
    {
        OnClick_Sfx();
        _letterHintBtn.On_ConsumeHint();
    }

    public void OnBuy_Hints20()
    {
        OnClick_Sfx();

#if UNITY_EDITOR
        _letterHintBtn.GrantHints(Purchaser.instance.Consumable_Products.Find(x => x.ProductID == "hints_20").RewardAmount);
#else 
        Purchaser.instance.consumableAction += OnPurchase_Hints;
        Purchaser.instance.BuyConsumableProduct("hints_20");
#endif

    }

    public void OnBuy_Hints50()
    {
        OnClick_Sfx();

#if UNITY_EDITOR
        _letterHintBtn.GrantHints(Purchaser.instance.Consumable_Products.Find(x => x.ProductID == "hints_50").RewardAmount);
#else
        Purchaser.instance.consumableAction += OnPurchase_Hints;
        Purchaser.instance.BuyConsumableProduct("hints_50");
#endif
    }


    void OnPurchase_Hints(Consumable product, bool isPurchased)
    {
        if (isPurchased)
        {
            _letterHintBtn.GrantHints(product.RewardAmount);
        }

        Purchaser.instance.consumableAction -= OnPurchase_Hints;
    }

    public void OnAd_GetHint()
    {
        OnClick_Sfx();
        _letterHintBtn.GetHintOnAd();
    }

    #endregion



    #region  ---------  word hint Functions -------

    public void On_WordHint()
    {
        OnClick_Sfx();
        _WordHintBtn.On_ConsumeHint();
    }

    public void OnBuy_WordHints20()
    {
        OnClick_Sfx();

#if UNITY_EDITOR
        _WordHintBtn.GrantHints(Purchaser.instance.Consumable_Products.Find(x => x.ProductID == "Wordhints_20").RewardAmount);
#else 
        Purchaser.instance.consumableAction += OnPurchase_WordHints;
        Purchaser.instance.BuyConsumableProduct("Wordhints_20");
#endif

    }

    public void OnBuy_WordHints50()
    {
        OnClick_Sfx();

#if UNITY_EDITOR
        _WordHintBtn.GrantHints(Purchaser.instance.Consumable_Products.Find(x => x.ProductID == "Wordhints_50").RewardAmount);
#else
        Purchaser.instance.consumableAction += OnPurchase_WordHints;
        Purchaser.instance.BuyConsumableProduct("Wordhints_50");
#endif
    }


    void OnPurchase_WordHints(Consumable product, bool isPurchased)
    {
        if (isPurchased)
        {
            _WordHintBtn.GrantHints(product.RewardAmount);
        }

        Purchaser.instance.consumableAction -= OnPurchase_WordHints;
    }

    public void OnAd_GetWordHint()
    {
        OnClick_Sfx();
        _WordHintBtn.GetHintOnAd();
    }


    #endregion
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
        canPlayAud = isOn;
        PlayerPrefs.SetInt("Sound", value);
        SetToggleVisuals(soundToggle, isOn, SettingOptionType.sound);
        SoundManager.Instance.SetSfxVolume(!isOn);
        OnClick_Sfx();
    }

    public void On_VibrationToggle(bool isOn)
    {
        OnClick_Sfx();
        PlayerPrefs.SetInt("Vibration", isOn ? 1 : 0);
        SetToggleVisuals(vibrationToggle, isOn, SettingOptionType.viberation);
    }

    public void OnclickMusicToggle()
    {
        On_MusicToggle(musicToggle.isOn);
    }

    void On_MusicToggle(bool isOn)
    {
        int value = isOn ? 1 : 0;
        canplaymusic = isOn;
        OnClick_Sfx();
        PlayerPrefs.SetInt("Music", value);

        if (isOn && _musicSrc.clip != null)
            _musicSrc.Play();
        else if (!isOn)
            _musicSrc.Stop();

        SetToggleVisuals(musicToggle, isOn, SettingOptionType.music);
    }


    public void Vibrate(HapticTypes hapticTypes = HapticTypes.MediumImpact)
    {
        if (PlayerPrefs.GetInt("Vibration") == 1)
            MMVibrationManager.Haptic(hapticTypes);
    }

    public void OpenMusicClipPanel(int index)
    {
        bool islight = ThemeManager.instance.IsLightMode;
        if (_gameMusicClips.Count <= index)
            return;

        for (int i = _musicObjectsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(_musicObjectsParent.GetChild(i).gameObject);
        }
        SelectMusicType(_gameMusicClips[index]);
        _musicPaneltitle.text = currentClipData.Type.ToString();
        _musictitleicon.sprite = currentClipData.Icon;
        for (int i = 0; i < currentClipData.ClipData.Count; i++)
        {
            MusicBar bar = Instantiate(_musicObjectPrefeb, _musicObjectsParent, false);
            bar.Init(currentClipData.ClipData[i].name, currentClipData.ClipData[i], islight);

            if (PlayingMusicName == currentClipData.ClipData[i].name)
                bar.OnplayMusic(true);
        }
        _musicObjectPanel.SetActive(true);
        PlayerPrefs.SetInt("selectedtheme", index);
    }

    void SelectMusicType(GameMusicData data)
    {
        currentClipData = data;
        _musicBtnObject.GetChild(0).GetComponent<Image>().sprite = currentClipData.Icon;
    }


    public void SetMusicClip(string clipname, AudioClip clip, MusicBar bar)
    {

        if (clipname != PlayingMusicName)
        {
            _musicSrc.Stop();
            _musicSrc.clip = clip;

            if (canplaymusic)
            {
                _musicSrc.Play();
                PlayerPrefs.SetString("selectedclip", clipname);
            }
            PlayingMusicName = clipname;
            if (_currentMusicBar != null)
                _currentMusicBar.OnplayMusic(false);

            _currentMusicBar = bar;

            if (currentClipData != null)
                _musicBtnObject.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            _musicSrc.Stop();
            PlayingMusicName = "";
            _musicBtnObject.GetChild(0).gameObject.SetActive(false);
        }
    }


    public void ResetSettingToogles()
    {
        SetToggleVisuals(soundToggle, soundToggle.isOn, SettingOptionType.sound);
        SetToggleVisuals(vibrationToggle, vibrationToggle.isOn, SettingOptionType.viberation);
    }

    public void ResetMusicToogles()
    {
        SetToggleVisuals(musicToggle, musicToggle.isOn, SettingOptionType.music);
    }

    void SetToggleVisuals(Toggle toggle, bool isOn, SettingOptionType _type)
    {
        toggle.SetIsOnWithoutNotify(isOn);
        float onPosValue = 7.5f;
        float offPosValue = -7.5f;

        if (_type == SettingOptionType.music)
        {
            onPosValue = 12.5f;
            offPosValue = -12.5f;
        }

        if (ThemeManager.instance.IsLightMode)
            toggle.GetComponent<Image>().sprite = isOn ? toggleOnSp : toggleOffSp;
        else
            toggle.GetComponent<Image>().sprite = isOn ? toggleDarkOnSp : toggleDarkOffSp;


        toggle.transform.GetChild(0).GetComponent<RectTransform>().DOAnchorPos3DX((isOn ? onPosValue : offPosValue), .3f).SetEase(Ease.Linear);
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

public enum SettingOptionType
{
    sound,
    music,
    viberation
}


public enum GameMusicType
{
    Happy,
    Night,
    Piano,
    Metallic,
    Relaxing
}

[System.Serializable]
public class GameMusicData
{
    public GameMusicType Type;
    public Sprite Icon;
    public List<AudioClip> ClipData;
}

//[System.Serializable]
//public class MusicClipData
//{
//    public string MusicName;
//    public AudioClip MusicClip;
//}

