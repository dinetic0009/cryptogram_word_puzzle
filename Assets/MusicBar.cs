using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MyBox;

public class MusicBar : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _barTilte;
    [SerializeField] Button _playBtn;
    [SerializeField] bool isplaying;
    [SerializeField, ReadOnly] AudioClip _clip;

    [SerializeField] Sprite BarLightBg;
    [SerializeField] Sprite BarDarktBg;

    [SerializeField] Color LightTextColor;
    [SerializeField] Color DarkTextColor;


    public void Init(string name, AudioClip clip, bool islight)
    {
        _barTilte.text = name;
        _playBtn.onClick.AddListener(() => OnClickBtn());
        _clip = clip;

        transform.GetComponent<Image>().sprite = BarLightBg;
        _barTilte.color = LightTextColor;

        if (!islight)
        {
            transform.GetComponent<Image>().sprite = BarDarktBg;
            _barTilte.color = DarkTextColor;
        }

    }

    public void OnplayMusic(bool isplay)
    {
        if (isplay)
        {
            _playBtn.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            _playBtn.transform.GetChild(0).gameObject.SetActive(false);
        }
        isplaying = isplay;
    }



    public void OnClickBtn()
    {
        if (!isplaying)
        {
            _playBtn.transform.GetChild(0).gameObject.SetActive(true);
            isplaying = true;
        }
        else
        {
            _playBtn.transform.GetChild(0).gameObject.SetActive(false);
        }

        UIManager.Instance.SetMusicClip(_barTilte.text, _clip, this);
    }

}
