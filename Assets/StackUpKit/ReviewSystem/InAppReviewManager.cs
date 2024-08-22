using System.Collections;
using System.Collections.Generic;
using Google.Play.Review;
using UnityEngine;

public class InAppReviewManager : MonoBehaviour
{

    public static InAppReviewManager Instance { get; set; }

    public int totalRequestCap = 3;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(this.gameObject);
        }

    }

    public void AskForReview()
    {
        int askedCount = PlayerPrefs.GetInt("IN_APP_REVIEW_ASKED", 0);
        if (askedCount < totalRequestCap)
        {
            StartCoroutine(AskForReviewCoroutine());
            PlayerPrefs.SetInt("IN_APP_REVIEW_ASKED", askedCount + 1);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.Log("Requests count exceeded...");
        }
    }

    private IEnumerator AskForReviewCoroutine()
    {
        ReviewManager _reviewManager;

        _reviewManager = new ReviewManager();
        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            yield break;
        }
        PlayReviewInfo _playReviewInfo = requestFlowOperation.GetResult();
        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;
        _playReviewInfo = null; // Reset the object
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            yield break;
        }
    }
}