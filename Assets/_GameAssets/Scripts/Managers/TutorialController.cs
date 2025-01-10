using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using MyBox;
using TMPro;
using System.Linq;
using Coffee.UISoftMask;

public class TutorialController : Singleton<TutorialController>
{
    public Keyboard keyboard;

    public GameObject popup;
    public GameObject TutorialPanel;
    public GameObject completionPanel;

    public RectTransform hintBtn;
    public RectTransform misktakesRect;
    public RectTransform blackBg;
    public RectTransform hand;
    public RectTransform maskPrefab;

    public Button continueBtn;
    public Button _btn;

    public TextMeshProUGUI popupText;
    public Color keyboardGreenClr, yellow, red;
    public Color white, dark;

    public bool CanShowTutorial { get => PlayerPrefs.GetInt("TutorialCompleted", 0) == 0; }
    public bool CanShowHintTutorial { get => PlayerPrefs.GetInt("HintTutorialCompleted", 0) == 0; }
    public bool CanShowMistakesTutorial { get => PlayerPrefs.GetInt("MistakesTutorialCompleted", 0) == 0; }


    private void OnEnable()
    {
        GameManager.TutorialController_ShowTutorial += ShowTutorial;
        MistakesController.TutorialController_ShowMistakesTutorial += ShowMistakesTutorial;
    }


    private void Start()
    {
        SetRectPosition(blackBg, Vector2.zero, null, null);
        keyboard.SetInteractable(false);
    }

    void Set_OnContinue(Action onClick = null)
    {
        continueBtn.gameObject.SetActive(onClick != null);
        TutorialPanel.transform.GetChild(1).GetComponent<RectTransform>().DOSizeDelta(new(200, onClick == null ? 67.5f : 95f), .2f);
        continueBtn.onClick.RemoveAllListeners();
        continueBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.OnClick_Sfx();
            onClick?.Invoke();
        });
    }

    void SetRectPosition(RectTransform mask, Vector2 sizeMultiplier, RectTransform sourceRect = null, Action onClick = null)
    {
        var size = sourceRect == null ? new(10, 10) : sourceRect.sizeDelta;
        size.y *= sizeMultiplier.y;
        size.x *= sizeMultiplier.x;
        mask.DOSizeDelta(size, .4f);

        if (sourceRect == null)
            mask.anchoredPosition = Vector2.zero;
        else
            mask.transform.DOMove(sourceRect.transform.position, .5f);

        _btn.interactable = onClick != null;
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(() => onClick?.Invoke()); ;

    }

    void ShowTutorial()
    {
        if (!CanShowTutorial)
        {
            if (CanShowHintTutorial)
            {
                ShowHintTutorial();
            }
            return;
        }

        TutorialPanel.SetActive(true);
        keyboard.SetInteractable(false);
        GameManager.Instance.SetInteractable(false);

        blackBg.gameObject.SetActive(true);
        keyboard.gameObject.SetActive(false);
        popup.SetActive(true);
        var firstFilledLetter = FirstFilledLetter();

        var sourceRect = firstFilledLetter.slot.GetComponent<RectTransform>();
        SetRectPosition(blackBg, Vector2.zero, new(), null);

        popupText.text = $"A <color={yellow.ToHex()}>digit</color> refers to a letter.\nFor example, <color={yellow.ToHex()}>{firstFilledLetter.code}</color> is <color={yellow.ToHex()}>{char.ToUpper(firstFilledLetter._char)}</color>";
        Set_OnContinue(ChooseCell);
    }


    void ChooseCell()
    {
        SetFirstEmptySpot();
        Set_OnContinue(null);

        hand.gameObject.SetActive(true);
        popupText.text = $"Tap to select a cell.";
        var sourceRect = SelectedEmptySlot.slot.GetComponent<RectTransform>();
        hand.transform.position = sourceRect.transform.position;
        hand.transform.DOScale(-.1f, .8f).SetRelative(true).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo).SetLoops(-1, LoopType.Restart);
        SetRectPosition(blackBg, new Vector2(1.5f, 2f), sourceRect, () => { SelectedEmptySlot.slot.Click(); Utils.WaitAndPerform(.2f, ToKeyboard); });
    }

    Key selectedKey = null;
    void ToKeyboard()
    {
        hand.transform.DOKill();
        keyboard.gameObject.SetActive(true);
        keyboard.GetKey(SelectedEmptySlot._char, out selectedKey);
        hand.transform.DOMove(selectedKey.transform.position, .5f).OnComplete(() =>
        hand.transform.DOScale(-.1f, .8f).SetRelative(true).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo).SetLoops(-1, LoopType.Restart));
        var sourceRect = selectedKey.GetComponent<RectTransform>();
        SetRectPosition(blackBg, Vector2.one, sourceRect, () => { selectedKey.Click(); Utils.WaitAndPerform(.2f, ShowGreenInfo); });

        popupText.text = $"Choose a key.";
        //popup.transform.DOLocalMoveY(100, .5f);
    }

    void ShowGreenInfo()
    {
        Key key = keyboard.Keys.FirstOrDefault(x => x.IsHighlighted() && x != selectedKey);
        SetRectPosition(blackBg, Vector2.one, key.GetComponent<RectTransform>(), null);

        hand.gameObject.SetActive(false);
        popup.transform.DOLocalMoveY(-30, .4f);
        popup.transform.DOScale(1.3f, .1f);
        popupText.text = $"TIP: A <color={keyboardGreenClr.ToHex()}>green</color> letter on the keyboard means there are more instances of this letter in the phrase.";
        Set_OnContinue(LastPopup);
    }

    void LastPopup()
    {

        popup.transform.DOScale(.4f, .5f);
        completionPanel.SetActive(true);
        //popup.transform.DOScale(.9f, .3f);
        //popupText.text = $"Decrypt the whole phrase\nGood luck!";
        //Set_OnContinue(EndTutorial);
    }


    public void EndTutorial()
    {
        TutorialPanel.SetActive(false);
        keyboard.SetInteractable(true);
        completionPanel.SetActive(false);
        GameManager.Instance.SetInteractable(true);
        PlayerPrefs.SetInt("TutorialCompleted", 1);
    }

    Letter FirstFilledLetter()
    {
        var group = GameManager.Instance.sameLetterGroups.FirstOrDefault(x => x.letters.Any(x => x.state is SlotState.Empty));
        return group.letters.FirstOrDefault(x => x.state is SlotState.Filled);
    }

    Letter SelectedEmptySlot = null;
    void SetFirstEmptySpot()
    {
        SelectedEmptySlot = GameManager.Instance.allSlots.FirstOrDefault(x => x.state is SlotState.Empty);
    }


    void ShowHintTutorial()
    {
        TutorialPanel.SetActive(true);
        keyboard.SetInteractable(false);
        popup.SetActive(false);
        popup.transform.DOScale(1, 0f);
        GameManager.Instance.SetInteractable(false);

        blackBg.gameObject.SetActive(true);

        hand.transform.DOKill();
        hand.gameObject.SetActive(true);
        hand.transform.position = hintBtn.transform.position;
        hand.transform.DOScale(-.1f, .8f).SetRelative(true).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo).SetLoops(-1, LoopType.Restart);

        SetRectPosition(blackBg, new Vector2(1.5f, 2f), hintBtn, () => { UIManager.Instance.LetterHintBtn.Click_Tutorial(); ShowHintPopup(); });
    }

    List<GameObject> masks = new();
    void ShowHintPopup()
    {
        hand.GameObjectSetActive(false);
        blackBg.gameObject.SetActive(false);
        popup.SetActive(true);
        popup.transform.DOScale(1, 0f);
        GameManager.Instance.SetInteractable(true);
        popupText.text = "Choose a cell that you want to reveal.";
        Set_OnContinue(null);

        var slots = GameManager.Instance.HighLightedSlots();
        blackBg.gameObject.SetActive(true);
        blackBg.GetComponent<SoftMask>().ignoreSelfGraphic = true;

        masks = new();
        foreach (Slot slot in slots)
        {
            var mask = Instantiate(blackBg.GetChild(0), blackBg);
            mask.GetComponent<Image>().SetImagePixelsPerUnit(10f);
            mask.gameObject.SetActive(true);
            masks.Add(mask.gameObject);

            SetRectPosition(mask.GetComponent<RectTransform>(), Vector2.one, slot.GetComponent<RectTransform>(), delegate { slot.On_Select(); });
        }
    }

    internal void CompleteHintTutorial()
    {
        masks.ForEach(x => x.SetActive(false));
        blackBg.GetComponent<SoftMask>().ignoreSelfGraphic = false;

        PlayerPrefs.SetInt("HintTutorialCompleted", 1);
        TutorialPanel.SetActive(false);
        keyboard.SetInteractable(true);
        hand.DOKill();
    }

    public void ShowMistakesTutorial()
    {
        if (!CanShowMistakesTutorial)
            return;

        TutorialPanel.SetActive(true);
        keyboard.SetInteractable(false);
        GameManager.Instance.SetInteractable(false);

        popup.SetActive(true);
        popupText.text = $"This is a <color={red.ToHex()}>mistake</color> counter\nYou will lose if you make 3\nmistakes using <color={red.ToHex()}>wrong</color> letters";
        popup.transform.DOScale(1.3f, .3f);

        blackBg.gameObject.SetActive(true);
        SetRectPosition(blackBg, new(1, 1.4f), misktakesRect, EndMistakesTutorial);
        Set_OnContinue(EndMistakesTutorial);
    }

    void EndMistakesTutorial()
    {
        PlayerPrefs.SetInt("MistakesTutorialCompleted", 1);
        TutorialPanel.SetActive(false);
        keyboard.SetInteractable(true);
        GameManager.Instance.SetInteractable(true);
    }

    private void OnDisable()
    {
        GameManager.TutorialController_ShowTutorial -= ShowTutorial;
        MistakesController.TutorialController_ShowMistakesTutorial -= ShowMistakesTutorial;
    }

}//Class