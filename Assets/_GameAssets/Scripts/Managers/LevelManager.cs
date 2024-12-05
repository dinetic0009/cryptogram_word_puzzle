using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MyBox;
using System;
using Random = UnityEngine.Random;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;


public class LevelManager : Singleton<LevelManager>
{
    [SerializeField] int _maxCount = 10;
    [SerializeField] GameObject leveliconPrefab;
    [SerializeField] RectTransform leveliconParent;

    [SerializeField] Sprite currLevelSp, completedLevelSp, lockedLevelSp;
    [SerializeField] ScrollRect horizontalScroll;

    [Header("")]
    [SerializeField] List<LevelSO> _levels;
    [SerializeField] List<TextAsset> _levelJasons;
    [SerializeField, ReadOnly] private LevelSO _currentLevel;
    [SerializeField, ReadOnly] private TextAsset _currentLevelJason;
    [SerializeField, ReadOnly] private int _currenLevelNo;
    [SerializeField, ReadOnly] private int _levelIndex;
    [SerializeField, ReadOnly] private int _levelNoInProgess;

    public int LEVEL_IN_PROGRESS { get => _levelNoInProgess; }
    public int Level_No_UI { get => _currenLevelNo; }
    public LevelSO CurrenLevel { get => _currentLevel; }

    public static Action<LevelSO> OnLevelInit;
    public static Action ShowTutorial;
    public static bool showCompleteAnimation = false;

    public static Action ClearStacks;

    private void Start()
    {
        _levels = new();
        _levelJasons = new List<TextAsset>();
        _currentLevelJason = null;
        _levels.AddRange(Resources.LoadAll<LevelSO>("_GameResources/Update_1")
                    .OrderBy(e => int.Parse(Regex.Match(e.name, @"-?\d+").Value))
                    .ToList());
        _levelJasons.AddRange(Resources.LoadAll<TextAsset>("LevelJasonFile/Update_1")
                    .OrderBy(e => int.Parse(Regex.Match(e.name, @"-?\d+").Value))
                    .ToList());


        _levels.AddRange(Resources.LoadAll<LevelSO>("_GameResources/Levels")
                    .OrderBy(e => int.Parse(Regex.Match(e.name, @"-?\d+").Value))
                    .ToList());
        _levelJasons.AddRange(Resources.LoadAll<TextAsset>("LevelJasonFile/Levels")
                    .OrderBy(e => int.Parse(Regex.Match(e.name, @"-?\d+").Value))
                    .ToList());


        _levels.AddRange(Resources.LoadAll<LevelSO>("_GameResources/Update_2")
                    .OrderBy(e => int.Parse(Regex.Match(e.name, @"-?\d+").Value))
                    .ToList());
        _levelJasons.AddRange(Resources.LoadAll<TextAsset>("LevelJasonFile/Update_2")
                    .OrderBy(e => int.Parse(Regex.Match(e.name, @"-?\d+").Value))
                    .ToList());

        _currenLevelNo = PlayerPrefs.GetInt("Level", 1);
        _levelIndex = PlayerPrefs.GetInt("LevelIndex", 0);
        _levelNoInProgess = PlayerPrefs.GetInt("LevelInProgress", 0);

        if (_levelNoInProgess == 0)
        {
            _levelNoInProgess = _currenLevelNo;
        }
        else if (_levelIndex < _levels.Count)
        {
            _levelNoInProgess = _levelIndex;
            PlayerPrefs.SetInt("LevelInProgress", _levelIndex);
        }

        ClearStacks += () => DOTween.KillAll(true);
        SetLevelRoad();
    }


    void Init()
    {
        Keyboard.Instance.Init();

        if (_currentLevelJason != null)
        {
            JsonController.Instance.SetPlayerData(_currentLevelJason);
        }

        OnLevelInit?.Invoke(_currentLevel);
        GameAnalyticsSDK.GameAnalytics.NewProgressionEvent(GameAnalyticsSDK.GAProgressionStatus.Start, _levelNoInProgess + "");
    }

    private void Init(int levelIndex)
    {
        Keyboard.Instance.Init();
        OnLevelInit?.Invoke(_levels[levelIndex - 1]);
    }


    public void LoadLevelByIndex(int index)
    {
        Init(Mathf.Clamp(index, 1, _levels.Count));
    }

    public void LoadNextLevel()
    {
        _levelNoInProgess = Mathf.Clamp(_levelNoInProgess, 1, _levels.Count);
        _currentLevel = _levels[_levelNoInProgess - 1];
        if (_levelNoInProgess - 1 <= _levelJasons.Count)
            _currentLevelJason = _levelJasons[_levelNoInProgess - 1];
        Init();
    }


    public void LoadCurrentLevel()
    {
        _levelNoInProgess = Mathf.Clamp(_levelNoInProgess, 1, _levels.Count);
        _currentLevel = _levels[_levelNoInProgess - 1];
        if (_levelNoInProgess - 1 <= _levelJasons.Count)
            _currentLevelJason = _levelJasons[_levelNoInProgess - 1];
        Init();
    }


    public void CompleteLevel()
    {
        GameAnalyticsSDK.GameAnalytics.NewProgressionEvent(GameAnalyticsSDK.GAProgressionStatus.Complete, _levelNoInProgess + "");

        PlayerPrefs.SetInt("Level", ++_currenLevelNo);

        if (_currenLevelNo <= (_levels.Count + 1))
            PlayerPrefs.SetInt("LevelIndex", _currenLevelNo);

        var completedLevelNo = _levelNoInProgess;
        _levelNoInProgess = _currenLevelNo;

        if (_currenLevelNo > _levels.Count)
        {
            var nextLevel = completedLevelNo;
            while (nextLevel == completedLevelNo)
            {
                nextLevel = Random.Range(2, _levels.Count);
            }

            _levelNoInProgess = nextLevel;
        }

        showCompleteAnimation = true;
        PlayerPrefs.SetInt("LevelInProgress", _levelNoInProgess);
        if (UIManager.Instance.GetTimePassedSinceAd() > 30f)
        {
            AdsMediation.instance.ShowInterstial();
        }
        UIManager.Instance.stopAdBreakCoroutine();
    }//CompleteLevel

    #region LevelRoat
    public void SetLevelRoad()
    {
        leveliconParent.ClearChilds();
        var startIndex = !showCompleteAnimation ? Level_No_UI : Level_No_UI - 1;

        for (int i = startIndex; i < Level_No_UI + _maxCount; i++)
        {
            var ob = Instantiate(leveliconPrefab, leveliconParent);

            if (i == Level_No_UI)
                currentLevelOb = ob;

            bool isCureentLevel = i == Level_No_UI && !showCompleteAnimation;

            var textComponent = ob.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            textComponent.text = i.ToString();

            textComponent.gameObject.SetActive(i <= Level_No_UI || isCureentLevel);
            ob.transform.GetChild(1).gameObject.SetActive(i > Level_No_UI && !isCureentLevel);

            Sprite sp;
            if (isCureentLevel)
                sp = currLevelSp;
            else if (i <= Level_No_UI)
                sp = completedLevelSp;
            else
                sp = lockedLevelSp;

            ob.GetComponent<Image>().sprite = sp;
        }

        if (showCompleteAnimation)
            ShowAnimation();

        Debug.Log("progress level is as " + _levelNoInProgess + "   level jason count  " + _levelJasons.Count);
        if (_levelNoInProgess <= _levelJasons.Count - 1)
            _currentLevelJason = _levelJasons[_levelNoInProgess];

        if (_currentLevelJason != null)
        {
            JsonController.Instance.SetPlayerData(_currentLevelJason);
        }
    }

    GameObject currentLevelOb = null;

    [ButtonMethod]
    void ShowAnimation()
    {
        Debug.Log("ShowingVerticleAnimation");
        horizontalScroll.DOHorizontalNormalizedPos(0f, 0f);
        horizontalScroll.DOHorizontalNormalizedPos(.125f, 1f).SetDelay(.5f).OnComplete(() =>
        {
            currentLevelOb.GetComponent<Image>().sprite = currLevelSp;

            currentLevelOb.GetComponentInChildren<TMPro.TextMeshProUGUI>(true).gameObject.SetActive(true);
            currentLevelOb.transform.GetChild(1).gameObject.SetActive(false);

            showCompleteAnimation = false;
        });
    }
    #endregion

    int levelToLoad = 0;
    public void InputLevelIndex(string value)
    {
        if (string.IsNullOrEmpty(value))
            levelToLoad = 1;
        else
            levelToLoad = int.Parse(value);
    }

    public void InputFieldPlay()
    {
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.SetInt("TutorialCompleted_2", 1);

        LoadLevelByIndex(levelToLoad);
    }


}//Class
