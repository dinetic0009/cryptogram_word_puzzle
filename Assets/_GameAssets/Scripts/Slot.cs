using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Slot : MonoBehaviour
{
    [SerializeField] Image _image, _lock;
    [SerializeField] TextMeshProUGUI textComponent, codeTextComponent;

    [SerializeField] Sprite normalSp, hightlightSp;
    [SerializeField] Sprite singleLockSp, dualLockSp;

    [Header("Text Colors")]
    [SerializeField] Color white;
    [SerializeField] Color red;
    [SerializeField] Color green;
    [SerializeField] Color transparent;

    [SerializeField] Button _button;

    public bool IsInteractable { get => _button.IsInteractable(); set => _button.interactable = value; }
    internal Letter letter;

    static public Slot selectedSlot;

    internal void Init(Letter letter)
    {
        this.letter = letter;
        var isChar = char.IsLetter(letter._char);

        textComponent.text = (letter.IsFilled || letter.IsSpace) ? $"{letter._char}" : "";
        codeTextComponent.text = char.IsLetter(letter._char) ? letter.code.ToString() : "";

        IsInteractable = isChar && !letter.IsFilled;
        _image.color = isChar ? white : transparent;
        textComponent.alignment = isChar ? TextAlignmentOptions.Center : TextAlignmentOptions.Left;

        textComponent.gameObject.SetActive(!letter.IsLock && !letter.IsDualLock);
        codeTextComponent.gameObject.SetActive(!letter.IsLock && !letter.IsDualLock);
        _lock.enabled = (letter.IsLock || letter.IsDualLock);
        _lock.sprite = letter.IsDualLock ? dualLockSp : singleLockSp;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(delegate
        {
            SoundManager.Instance.PlaySfx(SoundType.Select);
            On_Select();
        });
    }

    bool isHighlighted = false;
    internal void SetHighlight(bool hightlight)
    {
        isHighlighted = hightlight;
        _image.color = white;
        _image.sprite = hightlight ? hightlightSp : normalSp;
    }

    void TryMatchLetter(char _char)
    {
        textComponent.DOKill(true);
        textComponent.text = _char.ToString();

        if (char.ToLowerInvariant(letter._char) == char.ToLowerInvariant(_char))
            On_TrueGuess();
        else
            On_FalseGuess();
    }

    internal void On_KeyPress(Key key)
    {
        TryMatchLetter(key.Char);
    }

    internal void On_TrueGuess()
    {
        SoundManager.Instance.PlaySfx(SoundType.Correct);
        letter.state = SlotState.Filled;
        textComponent.color = Color.black;
        IsInteractable = false;
        GameManager.Instance.OnSlotFilled(letter);
        GameManager.Instance.OnUpdate();
    }

    internal void On_FalseGuess()
    {
        SoundManager.Instance.PlaySfx(SoundType.Wrong);
        textComponent.color = red;
        MistakesController.Instance.OnMistake();

        DOTween.Sequence()
            .AppendInterval(.4f)
            .Append(textComponent.DOFade(0, .5f))
            .AppendCallback(() => textComponent.text = "")
            .Append(textComponent.DOFade(1, 0));
    }

    internal void On_Select()
    {
        if (isHighlighted)
        {
            GameManager.Instance.SetHightLightAll = false;
            SetHighlight(false);
            TryMatchLetter(letter._char);
            return;
        }

        if(letter.IsLock || letter.IsDualLock)
        {
            _lock.transform.DOShakeRotation(1, 90, 5, 90, true, ShakeRandomnessMode.Full);
            return;
        }

        if (selectedSlot != null)
            selectedSlot.On_UnSelect();

        _image.color = green;
        selectedSlot = this;
    }

    internal void On_UnSelect()
    {
        _image.color = white;
        selectedSlot = null;
    }


}//Class

