using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ThemeManager : MonoBehaviour
{
    public static ThemeManager instance;

    [SerializeField] Button _themebtn;
    [SerializeField] Keyboard _keyboardObject;
    [SerializeField] List<ThemeObjectsData> _themeObjects;
    [SerializeField] List<ThemeColoringObjects> _themeColoroData;


    bool _isLightMode;

    public bool IsLightMode { get => _isLightMode; }

    private void Awake()
    {
        instance = this;
        _isLightMode = true;
    }

    public void OnChangeTheme()
    {
        _isLightMode = !_isLightMode;
        _themebtn.transform.GetChild(0).gameObject.SetActive(_isLightMode);
        _themebtn.transform.GetChild(1).gameObject.SetActive(!_isLightMode);
        ImplimentThemeProperties();
        ImplimentColorsProperties();
        _keyboardObject.ImplimentTheme(_isLightMode);

        ThemePanel[] ActivePanels = FindObjectsOfType<ThemePanel>();
        for (int i = 0; i < ActivePanels.Length; i++)
        {
            ActivePanels[i].ApplyTheme();
        }


        List<Slot> Slots = GameManager.Instance.SlotsObjects;
        for (int i = 0; i < Slots.Count; i++)
        {
            Slots[i].ApplyTheme(_isLightMode);
        }

        List<Key> ArrowKeys = Keyboard.Instance.ArrowKeys;
        for (int i = 0; i < ArrowKeys.Count; i++)
        {
            ArrowKeys[i].ApplyTheme(_isLightMode);
        }
    }

    void ImplimentThemeProperties()
    {
        for (int i = 0; i < _themeObjects.Count; i++)
        {
            for (int j = 0; j < _themeObjects[i].Objects.Count; j++)
            {
                Sprite useSprite = _themeObjects[i].LightModeSprite;
                if (!_isLightMode)
                    useSprite = _themeObjects[i].DarkModeSprite;

                if (_themeObjects[i].Type == ThemeObjectstype.Imagecomponent)
                    _themeObjects[i].Objects[j].GetComponent<Image>().sprite = useSprite;
                else if (_themeObjects[i].Type == ThemeObjectstype.RenderComponent)
                    _themeObjects[i].Objects[j].GetComponent<SpriteRenderer>().sprite = useSprite;
            }
        }
    }

    void ImplimentColorsProperties()
    {
        for (int i = 0; i < _themeColoroData.Count; i++)
        {
            for (int j = 0; j < _themeColoroData[i].Objects.Count; j++)
            {
                Color usecolor = _themeColoroData[i].LightModeColor;
                if (!_isLightMode)
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


[System.Serializable]
public class ThemeObjectsData
{
    public ThemeObjectstype Type;
    public List<Transform> Objects;
    public Sprite LightModeSprite;
    public Sprite DarkModeSprite;
}


[System.Serializable]
public class ThemeColoringObjects
{
    public ThemeObjectstype Type;
    public List<Transform> Objects;
    public Color LightModeColor;
    public Color DarkModeColor;
}

public enum ThemeObjectstype
{
    Imagecomponent,
    RenderComponent,
    TMP_Text
}
