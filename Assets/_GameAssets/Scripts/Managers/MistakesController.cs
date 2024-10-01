using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MyBox;
using System;

public class MistakesController : Singleton<MistakesController>
{
    [SerializeField] Transform crossParent;
    [SerializeField] Image crossPrefab;

    [Header("")]
    [SerializeField] Sprite whiteCross;
    [SerializeField] Sprite redCross;

    private List<Image> crossList;
    readonly int mistakesCount = 3;
    private int mistakeIndex = 0;

    public static Action TutorialController_ShowMistakesTutorial;

    private void OnEnable()
    {
        LevelManager.OnLevelInit += OnLevelInit;
    }

    internal void OnLevelInit(LevelSO level = null)
    {
        SetUI();
    }


    void SetUI()
    {
        mistakeIndex = 0;
        crossList = new();
        crossParent.ClearChilds();

        for(int i = 0; i < mistakesCount; i++)
        {
            var cross = Instantiate(crossPrefab, crossParent);
            //cross.sprite = whiteCross;
            crossList.Add(cross);
        }
    }

    internal void OnMistake()
    {
        //crossList[mistakeIndex].sprite = redCross;
        crossList[mistakeIndex].transform.GetChild(0).gameObject.SetActive(true);
        mistakeIndex++;
        TutorialController_ShowMistakesTutorial?.Invoke();

        if(mistakeIndex == mistakesCount)
        {
            OnGameOver();
        }
    }

    internal void OnGameOver()
    {
        Debug.Log("<color=red>GameOver</color>");
        Keyboard.Instance.SetInteractable(false);
        UIManager.Instance.SetLosePanel();
    }


    [ButtonMethod]
    internal void OnRevive()
    {
        SetUI();
        Keyboard.Instance.SetInteractable(true);
    }

    private void OnDisable()
    {
        LevelManager.OnLevelInit -= OnLevelInit;
    }

}//Class