using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Slot : MonoBehaviour
{
    [SerializeField] Animator _animator;
    public Image _image, _lock;
    [SerializeField] TextMeshProUGUI textComponent, codeTextComponent;

    [SerializeField] Sprite normalSp, _normalDarkSp, hightlightSp;
    [SerializeField] Sprite singleLockSp, dualLockSp;

    [Header("Text Colors")]
    [SerializeField] Color _normalColor;
    [SerializeField] Color _darkModeColor;

    [SerializeField] Color white;
    [SerializeField] Color red;
    [SerializeField] Color green;
    [SerializeField] Color transparent;

    private Sprite _activeNormalSp;

    public Button _button;

    public bool IsInteractable { get => _button.IsInteractable(); set => _button.interactable = value; }
    internal Letter letter;

    readonly string CORRECT_KEY = "Correct";
    readonly string WRONG_KEY = "Wrong";

    static public Slot selectedSlot;

    TextAlignmentOptions GetAlignmentOptionForSymble(char _char)
    {
        return _char switch
        {
            '-' => TextAlignmentOptions.Center,
            _ => TextAlignmentOptions.Left
        };
    }

    private void Awake()
    {

    }

    internal void Init(Letter letter)
    {
        this.letter = letter;
        var isChar = char.IsLetter(letter._char);

        textComponent.text = (letter.IsFilled || letter.IsSpace) ? $"{letter._char}" : "";
        codeTextComponent.text = char.IsLetter(letter._char) ? letter.code.ToString() : "";

        IsInteractable = isChar && !letter.IsFilled;
        _image.color = isChar ? white : transparent;

        textComponent.alignment = isChar ? TextAlignmentOptions.Center : GetAlignmentOptionForSymble(letter._char);

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

        ApplyTheme(ThemeManager.instance.IsLightMode);
    }

    internal void Click()
    {
        _button.onClick?.Invoke();
    }


    public void ApplyTheme(bool isLight)
    {
        Sprite usesprite = normalSp;
        Color useColor = _darkModeColor;

        if (isLight)
        {
            _activeNormalSp = normalSp;
            useColor = _normalColor;
        }
        else
        {
            _activeNormalSp = _normalDarkSp;
            useColor = _darkModeColor;
        }

        textComponent.color = useColor;
        codeTextComponent.color = useColor;
        SetHighlight(isHighlighted);


        if (!char.IsLetter(letter._char))
        {
            Color clr = Color.white;
            clr.a = 0;
            _image.color = clr;
        }

    }

    bool isHighlighted = false;
    internal void SetHighlight(bool hightlight)
    {
        isHighlighted = hightlight;
        _image.color = white;
        _image.sprite = hightlight ? hightlightSp : _activeNormalSp;
    }

    void TryMatchLetter(char _char)
    {
        seq.Kill(true);
        textComponent.text = _char.ToString();

        if (char.ToLowerInvariant(letter._char) == char.ToLowerInvariant(_char))
            On_TrueGuess();
        else
            On_FalseGuess();
    }

    public void SetTriggerCorrect()
    {
        _animator.SetTrigger(CORRECT_KEY);
    }

    internal void On_KeyPress(Key key)
    {
        TryMatchLetter(key.Char);
    }

    internal void On_TrueGuess()
    {
        SetTriggerCorrect();
        SoundManager.Instance.PlaySfx(SoundType.Correct);
        letter.state = SlotState.Filled;
        ApplyTheme(ThemeManager.instance.IsLightMode);
        //textComponent.color = Color.black;
        IsInteractable = false;
        GameManager.Instance.OnSlotFilled(letter, true);
        GameManager.Instance.OnUpdate();
    }

    Sequence seq;
    internal void On_FalseGuess()
    {
        SoundManager.Instance.PlaySfx(SoundType.Wrong);
        textComponent.color = red;
        MistakesController.Instance.OnMistake();

        seq = DOTween.Sequence()
            .AppendInterval(.6f)
            .Append(textComponent.DOFade(0, .5f))
            .AppendCallback(() => textComponent.text = "")
            .Append(textComponent.DOFade(1, 0));
    }


    public void AutoSelect()
    {
        SetHighlight(false);
        TryMatchLetter(letter._char);
        UIManager.Instance.WordHintBtn.EnableHintBtn();
    }

    internal void On_Select()
    {
        if (isHighlighted)
        {
            if (GameManager.Instance.IswordHintActive)
            {
                GameManager.Instance.CheckAndFillWord(this);
                return;
            }

            GameManager.Instance.SetHightLightAll(false);
            SetHighlight(false);
            UIManager.Instance.LetterHintBtn.EnableHintBtn();
            TryMatchLetter(letter._char);
            TutorialController.Instance.CompleteHintTutorial();
            return;
        }

        if (letter.IsLock || letter.IsDualLock)
        {
            _lock.transform.DOShakeRotation(1, new Vector3(0, 0, 90), 5, 90, true, ShakeRandomnessMode.Full);
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

    internal void HideCode()
    {
        codeTextComponent.gameObject.SetActive(false);
    }


}//Class

