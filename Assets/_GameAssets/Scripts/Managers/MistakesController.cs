using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MyBox;

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
            cross.sprite = whiteCross;
            crossList.Add(cross);
        }
    }

    internal void OnMistake()
    {
        crossList[mistakeIndex].sprite = redCross;
        mistakeIndex++;

        if(mistakeIndex == mistakesCount)
        {
            OnGameOver();
        }
    }

    internal void OnGameOver()
    {
        Debug.Log("<color=red>GameOver</color>");
        //GameManager.Instance.SetInteractable(false);
        Keyboard.Instance.SetInteractable(false);
        UIManager.Instance.SetLosePanel();
    }


    [ButtonMethod]
    internal void OnRevive()
    {
        crossList.ForEach(x => x.sprite = whiteCross);
        mistakeIndex = 0;
        Keyboard.Instance.SetInteractable(true);
    }

    private void OnDisable()
    {
        LevelManager.OnLevelInit -= OnLevelInit;
    }

}//Class