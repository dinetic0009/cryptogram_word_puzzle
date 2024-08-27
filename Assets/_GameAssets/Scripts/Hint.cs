using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using MyBox;
using System;


public class Hint : Singleton<Hint>
{
    [SerializeField] GameObject counterOb, adOb;
    [SerializeField] TextMeshProUGUI hintCountText;
    [SerializeField] Button btn;

    private int hintCount = 0;
    private bool onAd = false;

    public int HintCount { get => hintCount; }
    public static Action Hint_ShowHint;


    private void Start()
    {
        SetVisuals();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(delegate { On_ConsumeHint(onAd); });
    }

    internal void GrantHints(int count)
    {
        hintCount += count;
        PlayerPrefs.SetInt("HintCount", hintCount);
        SetVisuals();
    }

    private void SetVisuals()
    {
        hintCount = PlayerPrefs.GetInt("HintCount", 2);

        onAd = hintCount <= 0;
        adOb.SetActive(onAd);
        counterOb.SetActive(!onAd);
        hintCountText.text = $"{HintCount}";
    }

    public void On_ConsumeHint(bool showAd)
    {
        UIManager.Instance.OnClick_Sfx();

        if (!showAd)
        {
            hintCount--;
            PlayerPrefs.SetInt("HintCount", hintCount);
            GrantHint();
        }
        else
        {
#if UNITY_EDITOR
            GrantHint();
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
        if(isCompleted)
        {
            GrantHint();
        }

        AdsMediation.rewardCallBack -= OnReward_Hint;
    }

    void GrantHint()
    {
        SetVisuals();
        btn.interactable = false;
        Hint_ShowHint?.Invoke();
    }


    public void EnableHintBtn()
    {
        btn.interactable = true;
    }

}//Class