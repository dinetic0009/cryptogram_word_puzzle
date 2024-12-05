using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class ThemePanel : MonoBehaviour
{
    [SerializeField] List<ThemeObjectsData> _themeObjects;
    [SerializeField] List<ThemeColoringObjects> _themeColoroData;
    [SerializeField] UnityEvent handler;


    private void OnEnable()
    {
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        bool islight = ThemeManager.instance.IsLightMode;
        ImplimentThemeProperties(islight);
        ImplimentColorsProperties(islight);
        if (handler != null)
            handler.Invoke();
    }



    void ImplimentThemeProperties(bool islight)
    {
        for (int i = 0; i < _themeObjects.Count; i++)
        {
            for (int j = 0; j < _themeObjects[i].Objects.Count; j++)
            {
                Sprite useSprite = _themeObjects[i].LightModeSprite;
                if (!islight)
                    useSprite = _themeObjects[i].DarkModeSprite;

                if (_themeObjects[i].Type == ThemeObjectstype.Imagecomponent)
                    _themeObjects[i].Objects[j].GetComponent<Image>().sprite = useSprite;
                else if (_themeObjects[i].Type == ThemeObjectstype.RenderComponent)
                    _themeObjects[i].Objects[j].GetComponent<SpriteRenderer>().sprite = useSprite;
            }
        }
    }


    void ImplimentColorsProperties(bool islight)
    {
        for (int i = 0; i < _themeColoroData.Count; i++)
        {
            for (int j = 0; j < _themeColoroData[i].Objects.Count; j++)
            {
                Color usecolor = _themeColoroData[i].LightModeColor;
                if (!islight)
                    usecolor = _themeColoroData[i].DarkModeColor;


                if (_themeColoroData[i].Type == ThemeObjectstype.Imagecomponent)
                    _themeColoroData[i].Objects[j].GetComponent<Image>().color = usecolor;
                else if (_themeColoroData[i].Type == ThemeObjectstype.RenderComponent)
                    _themeColoroData[i].Objects[j].GetComponent<SpriteRenderer>().color = usecolor;
                else if (_themeColoroData[i].Type == ThemeObjectstype.TMP_Text)
                    _themeColoroData[i].Objects[j].GetComponent<TextMeshProUGUI>().color = usecolor;
            }
        }
    }
}

