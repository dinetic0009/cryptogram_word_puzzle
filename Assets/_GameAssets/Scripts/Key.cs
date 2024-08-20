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
    [SerializeField] TextMeshProUGUI textComponent;
    [SerializeField] Button btn;
    [SerializeField] Image image;
    [SerializeField] Image hightlightImg;

    [Header("")]
    [SerializeField] Color idleColor;
    [SerializeField] Color highlightColor;
    [SerializeField] Color disableColor;
    [Header("")]
    [SerializeField] Sprite normalSp;
    [SerializeField] Sprite disabledSp;

    public void Init(char _char)
    {
        this.Char = _char;
        image.sprite = normalSp;
        textComponent.text = $"{_char}";
        textComponent.color = idleColor;

        SetListner(() => On_Click());
    }

    public void HightLight()
    {
        textComponent.color = highlightColor;
    }

    public void SetDisabled()
    {
        btn.interactable = false;
        image.sprite = disabledSp;
        textComponent.color = disableColor;
    }

    public void SetEnabled()
    {
        btn.interactable = true;
        image.sprite = normalSp;
        textComponent.color = idleColor;
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
