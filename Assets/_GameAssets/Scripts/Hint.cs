using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using MyBox;
using System;


public class Hint : MonoBehaviour
{
    [SerializeField] HintType _type;
    [SerializeField] GameObject counterOb, adOb;
    [SerializeField] TextMeshProUGUI hintCountText;
    [SerializeField] Button btn;

    private int hintCount = 0;
    internal bool onAd = false;

    public int HintCount { get => hintCount; }
    public static Action Hint_ShowHint;

    string PrefebName = "HintCount";

    private void Awake()
    {
        if (_type == HintType.LetterHint)
            PrefebName = "HintCount";
        else
            PrefebName = "WordHintCount";
    }

    private void Start()
    {
        SetVisuals();
    }

    internal void Click_Tutorial()
    {
        btn.onClick?.Invoke();
    }

    internal void GrantHints(int count)
    {
        hintCount += count;
        PlayerPrefs.SetInt(PrefebName, hintCount);
        SetVisuals();
    }

    private void SetVisuals()
    {
        hintCount = PlayerPrefs.GetInt(PrefebName, 2);

        onAd = hintCount <= 0;
        adOb.SetActive(onAd);
        counterOb.SetActive(!onAd);
        hintCountText.text = $"{HintCount}";
    }

    public void On_ConsumeHint()
    {
        if (!onAd)
        {
            hintCount--;
            PlayerPrefs.SetInt(PrefebName, hintCount);
            PerformHint();
        }
        else
        {
#if UNITY_EDITOR
            GrantHints(1);
#else
            if (AdsMediation.instance.IsRewardedVideoReadyForAnyNetwork())
            {
                AdsMediation.rewardCallBack += OnReward_Hint;
                AdsMediation.instance.ShowRewardedVideo();
            }
#endif
        }

    }//

    void OnReward_Hint(bool isCompleted)
    {
        if (isCompleted)
        {
            GrantHints(1);
        }

        AdsMediation.rewardCallBack -= OnReward_Hint;
    }

    internal void GetHintOnAd()
    {
        onAd = true;
        On_ConsumeHint();
    }

    void PerformHint()
    {
        SetVisuals();
        btn.interactable = false;
        if (_type == HintType.WordHint)
            GameManager.Instance.IswordHintActive = true;
        GameManager.Instance.SetHightLightAll(true);
    }

    public void EnableHintBtn()
    {
        btn.interactable = true;
    }

}//Class


public enum HintType
{
    LetterHint,
    WordHint
}

