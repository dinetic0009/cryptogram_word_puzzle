using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AdBreakPopup : MonoBehaviour
{
    public GameObject preAdsBreak;
    public GameObject postAdsBreak;
    public GameObject panel;
    public Text countDownText;
    int countDown = 4;
    public int hintsToReward;
    int counterToShowPurchasePopup = 0;

    public void OnShowing()
    {
        countDown = 4;
        countDownText.text = countDown.ToString();
        panel.SetActive(true);
        preAdsBreak.SetActive(true);
        postAdsBreak.SetActive(false);
        StartCoroutine(startCountDown());
    }

    IEnumerator startCountDown()
    {
        while (countDown != 1)
        {
            yield return new WaitForSeconds(1f);
            countDown--;
            countDownText.text = countDown.ToString();
        }
        yield return new WaitForSeconds(0.5f);
        AdsMediation.interstitialCallBack += OnInterstitialComplete;
        AdsMediation.instance.ShowInterstial();
    }

    void OnInterstitialComplete()
    {
        preAdsBreak.SetActive(false);
        postAdsBreak.SetActive(true);
        AdsMediation.interstitialCallBack -= OnInterstitialComplete;
        counterToShowPurchasePopup++;
    }

    public void OnDoneClick()
    {
        UIManager.Instance.LetterHintBtn.GrantHints(1);
        UIManager.Instance.WordHintBtn.GrantHints(1);
        UIManager.Instance.setupAdBreakCoroutine();
        panel.SetActive(false);
        if (counterToShowPurchasePopup % 3 == 0)
        {
            counterToShowPurchasePopup = 0;
            //if (AdsConfig.Get_RemoveAds_Status() == 0)
            //{
            //    PopupManager.Instance.Show("NoAds");
            //}

        }
    }
}

