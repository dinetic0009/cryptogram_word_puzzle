using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShareGame : MonoBehaviour
{
    public void ShareGameAction(Texture2D icon)
    {
        string url = "";
        string filePath = Path.Combine(Application.temporaryCachePath, "shared img.png");
        File.WriteAllBytes(filePath, icon.EncodeToPNG());
#if UNITY_IOS
        url = "https://apps.apple.com/us/app/goods-sort-sorting-games/id1598897051";
#elif UNITY_ANDROID
        //url = "amzn://apps/android?p=" + Application.identifier;
        //url = "https://play.google.com/store/apps/details?id=" + Application.identifier;
#endif
        new NativeShare().AddFile(filePath)
            .SetSubject("Chill Out with Jelly Boba - DIY Bubble Tea: Perfect for Casual Gamers :video_game::leaves:").SetText("Hey! :wave: I've discovered an amazing game that I think you'll love. It's called Jelly Boba - DIY Bubble Tea, and it's seriously addictive! Do install and play it. Whether you're a seasoned gamer or just looking for some casual fun, this game has something for everyone.\n").SetUrl(url)
            .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
            .Share();
        GameAnalyticsSDK.GameAnalytics.NewDesignEvent("Share Game");
    }
}
