using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Linq;
using MyBox;
using DG.Tweening;
using TMPro;


public class Key : MonoBehaviour
{
    internal char Char;
    [SerializeField] KeyBoard_KeyType _type;
    [SerializeField] TextMeshProUGUI textComponent;
    [SerializeField] Button btn;
    [SerializeField] Image image;
    [SerializeField] Image hightlightImg;

    [Header("")]
    [SerializeField] Color idleColor;
    [SerializeField] Color idleDarkColor;
    [SerializeField] Color highlightColor;
    [SerializeField] Color disableColor;

    [Header("--button sprites--")]
    [SerializeField] Sprite normalSp;
    [SerializeField] Sprite disabledSp;
    [SerializeField] Sprite normalDarkSp;
    [SerializeField] Sprite disableDarkSp;

    [SerializeField, ReadOnly] Sprite _activeNormalSp;
    [SerializeField, ReadOnly] Sprite _activeDisableSp;

    public KeyBoard_KeyType Type { get => _type; }

    public void Init(char _char)
    {

        this.Char = _char;
        image.sprite = _activeNormalSp;
        textComponent.text = $"{_char}";
        SetNormalColor();
        SetListner(() => On_Click());
    }

    private void Awake()
    {
        _activeNormalSp = normalSp;
        _activeDisableSp = disabledSp;
    }

    public void HightLight()
    {
        textComponent.color = highlightColor;
    }

    internal bool IsHighlighted()
    {
        return textComponent.color == highlightColor;
    }

    public void SetDisabled(bool canAnimate)
    {
        if (canAnimate)
        {
            textComponent.color = highlightColor;
            textComponent.DOScale(1f, .5f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear).OnComplete(() =>
            {
                DisableKey();
            });
        }
        else
        {
            DisableKey();
        }
    }
    private void DisableKey()
    {
        btn.interactable = false;
        image.sprite = _activeDisableSp;
        textComponent.color = disableColor;
    }

    public void SetEnabled()
    {
        btn.interactable = true;
        image.sprite = _activeNormalSp;
        SetNormalColor();
    }


    void SetNormalColor()
    {
        bool islight = ThemeManager.instance.IsLightMode;

        if (_type == KeyBoard_KeyType.ArrowKey && transform.childCount > 0 && transform.GetChild(0).GetComponent<Image>())
        {
            transform.GetChild(0).GetComponent<Image>();
            transform.GetChild(0).GetComponent<Image>().color = idleColor;
            if (!islight)
                transform.GetChild(0).GetComponent<Image>().color = idleDarkColor;
            return;
        }

        if (textComponent == null)
            return;

        if (textComponent.color == highlightColor || textComponent.color == disableColor)
            return;

        if (islight)
        {
            textComponent.color = idleColor;
            return;
        }

        textComponent.color = idleDarkColor;
    }

    public void ApplyTheme(bool islight)
    {
        if (islight)
        {
            _activeNormalSp = normalSp;
            _activeDisableSp = disabledSp;
        }
        else
        {
            _activeNormalSp = normalDarkSp;
            _activeDisableSp = disableDarkSp;
        }

        if (btn.interactable)
            image.sprite = _activeNormalSp;
        else
            image.sprite = _activeDisableSp;

        SetNormalColor();
    }

    internal void Click()
    {
        btn.onClick?.Invoke();
    }

    void On_Click()
    {
        if (Slot.selectedSlot == null)
            return;

        SoundManager.Instance.PlaySfx(SoundType.Keyboard);

        DOTween.Sequence()
            .Append(hightlightImg.DOFade(1, 0f))
            .AppendInterval(.25f)
            .Append(hightlightImg.DOFade(0, .15f));

        Slot.selectedSlot.On_KeyPress(this);
    }


    internal void SetListner(Action onClickCallback)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClickCallback?.Invoke());
    }
}

public enum KeyBoard_KeyType
{
    CharKey,
    ArrowKey
}
