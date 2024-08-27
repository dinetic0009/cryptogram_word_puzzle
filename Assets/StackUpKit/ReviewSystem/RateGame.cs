using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RateGame : MonoBehaviour
{
    public int played_sessions
    {

        set { PlayerPrefs.SetInt("played_sessions", value); }
        get { return PlayerPrefs.GetInt("played_sessions", 1); }
    }
    void Start()
    {
        StartCoroutine(ShowPopUp());
    }

    IEnumerator ShowPopUp()
    {
        yield return new WaitForSeconds(2);
        if (played_sessions % 3 == 0)
        {
#if UNITY_IOS
            UnityEngine.iOS.Device.RequestStoreReview();

#elif UNITY_ANDROID
            //InAppReviewManager.Instance.AskForReview();
#else

#endif
        }
        played_sessions++;
    }
}
