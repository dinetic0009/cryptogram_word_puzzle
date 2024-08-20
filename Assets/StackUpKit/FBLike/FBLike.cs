using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBLike : MonoBehaviour
{
    public void FBPageLikeAction()
    {
        StartCoroutine(OpenFacebookPage());
    }
    IEnumerator OpenFacebookPage()
    {
        Application.OpenURL("fb://profile/120393097718076");
        yield return new WaitForSeconds(1);
        if (leftApp)
        {
            leftApp = false;
        }
        else
        {
            Application.OpenURL("https://www.facebook.com/120393097718076");
        }
    }

    bool leftApp = false;

    void OnApplicationPause()
    {
        leftApp = true;
    }
}
