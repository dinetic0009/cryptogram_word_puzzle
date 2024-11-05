using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using MyBox;
using System;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
//using UnityEditor;
//using UnityEditor.DeviceSimulation;
using System.Reflection;

public class GameManager : Singleton<GameManager>
{
    [SerializeField, ReadOnly, TextArea] string phrase;

    [Header("")]
    [SerializeField] Slot slotPrefab;
    [SerializeField] Keyboard keyboard;
    [SerializeField] GameObject linePrefab;
    [SerializeField] Transform lineParent;
    [SerializeField] GameObject interactableMaskOb;


    [Header("")]
    public List<Letter> allSlots;
    public List<Word> sameLetterGroups = new();
    public List<Word> words = new();


    LevelSO level;
    internal string _phrase;
    readonly int maxLineLength = 15;
    public static Action TutorialController_ShowTutorial;

    //public bool SetHightLightAll { set => allSlots.Where(x => x.IsEmpty).ForEach(x => x.slot.SetHighlight(value)); }

    public void SetHightLightAll(bool highlighted)
    {
        allSlots.Where(x => x.IsEmpty).ForEach(x => x.slot.SetHighlight(highlighted));
    }

    public List<Slot> HighLightedSlots()
    {
        return allSlots.Where(x => x.IsEmpty).Select(y => y.slot).ToList();
    }

    public void SetInteractable(bool isInternactabe)
    {
        interactableMaskOb.SetActive(!isInternactabe);
    }

    private void OnEnable()
    {
        LevelManager.OnLevelInit += SetLevel;
        lineParent.localScale = Vector3.one * GetDeviceScaleMultiplier();
    }



    float GetDeviceScaleMultiplier()
    {
        return 1f;

        //#if UNITY_EDITOR
        //        Debug.Log("Plateform: " + Application.platform);
        //        return 1f;
        //#else
        //        if (Application.platform == RuntimePlatform.IPhonePlayer)
        //        {
        //            // Check if it's an iPad specifically
        //            if (SystemInfo.deviceModel.Contains("iPad"))
        //            {
        //                Debug.Log("Device is an iPad");
        //                return 1.5f;
        //            }
        //            else
        //            {
        //                Debug.Log("Device is an iPhone");
        //                return 1f;
        //            }
        //        }
        //        else
        //        {
        //            Debug.Log("Device is not an iOS device");
        //            return 1f;
        //        }
        //#endif
    }

    public void SetLevel(LevelSO level)
    {
        this.level = level;
        phrase = level.phrase;
        allSlots = new();
        words = new();

        _phrase = phrase;
        _phrase = _phrase.ToLower();

        InitCodes(_phrase);

        var lines = SplitSentenceIntoLines(_phrase);
        Slot.selectedSlot = null;
        lineParent.ClearChilds();


        for (int i = 0; i < lines.Count; i++)
        {
            var _line = Instantiate(linePrefab, lineParent);
            _line.transform.ClearChilds();
            var word = new Word();

            for (int j = 0; j < lines[i].Length; j++)
            {
                var letter = lines[i][j];

                var slot = Instantiate(slotPrefab, _line.transform);
                var code = GetCode(letter);
                var _slot = new Letter(slot, letter, code);

                if (!char.IsLetter(letter))
                {
                    if (!word.letters.IsNullOrEmpty())
                        words.Add(new Word(word.letters.Copy()));

                    word = new();
                    _slot.SetState(SlotState.SymbleOrBlank);
                }
                else
                {
                    word.letters.Add(_slot);
                }

                allSlots.Add(_slot);

            }

            if (!word.letters.IsNullOrEmpty())
                words.Add(new Word(word.letters.Copy()));
        }

        words.Shuffle();
        words = words.OrderByDescending(x => x.letters.Count).ToList();
        allSlots.ForEach((x, i) => x.SetNeighbors(i <= 0 ? null : allSlots[i - 1], (i >= allSlots.Count - 1) ? null : allSlots[i + 1], i));
        InitGroups();

        SetStates();

        allSlots.ForEach((x, i) => x.Init());
        lineParent.GetComponentInParent<ScrollRect>().normalizedPosition = new(0, 1);

        if (!TutorialController.Instance.CanShowTutorial)
            OnUpdate();

        Utils.WaitAndPerform(1.2f, () => TutorialController_ShowTutorial?.Invoke());
    }

    int GetCode(char letter)
    {
        if (!char.IsLetter(letter))
            return -1;

        return codes.Find(x => x._char == letter)._code;
    }

    List<Entry> codes;
    void InitCodes(string phrase)
    {
        if (JsonController.Instance.playerData.hasCodes)
        {
            codes = JsonController.Instance.playerData.Codes;
            return;
        }


        var distinctList = phrase.Where(char.IsLetter).Distinct().ToList();
        codes = new();
        Vector2Int simpleRange = new(1, 20), vowelRange = new(19, 25);

        distinctList.ForEach(letter =>
        {
            int _code;
            var range = letter.IsVowel() ? vowelRange : simpleRange;

            do
            {
                _code = Random.Range(range.x, range.y);
            }
            while (codes.Any(x => x._code == _code));

            codes.Add(new(letter, _code));
        });

        JsonController.Instance.SaveCodes(codes);
    }


    void InitGroups()
    {
        sameLetterGroups = allSlots
           .Where(x => !x.IsSpace)
           .GroupBy(data => data.code)
           .Select(group => group.ToList())
           .OrderByDescending(list => list.Count)
           .Select(list => new Word(list))
           .ToList();
    }


    void SetStates()
    {
        if (JsonController.Instance.playerData.hasStates)
        {
            var states = JsonController.Instance.playerData.States;
            allSlots.ForEach((x, i) =>
            {
                x.SetState((SlotState)states[i]);

                if ((SlotState)states[i] is SlotState.Filled)
                    OnSlotFilled(x);
            });
            return;
        }

        var totalCount = allSlots.Where(x => !x.IsSpace).ToList().Count;
        var preFilledCount = (level.visibilityPercentage * totalCount) / 100;
        //Debug.Log($"{level.visibilityPercentage}% of {totalCount} = {preFilledCount}");

        var _sameLetterGroups = sameLetterGroups.Copy();
        _sameLetterGroups.ForEach(x => x.letters.Shuffle());

        if (level.visibilityPercentage > 50)
            _sameLetterGroups.Reverse();

        List<Letter> selectedLetters = new();

        for (int i = 0; i < _sameLetterGroups.Count; i++)
        {
            var _letters = _sameLetterGroups[i].letters;
            List<Letter> _selectedLetters = _letters.Count > 3 ? _letters.Take((_letters.Count * 65) / 100).ToList() : _letters;
            selectedLetters.AddRangeIfNotNull(_selectedLetters);

            if (selectedLetters.Count >= preFilledCount)
                break;
        }

        var select = Mathf.Clamp(preFilledCount, selectedLetters.Count, preFilledCount);
        selectedLetters.Take(select).ForEach(x =>
        {
            x.SetState(SlotState.Filled);
            OnSlotFilled(x);
        });

        if (LevelManager.Instance.Level_No_UI < 5)
        {
            _sameLetterGroups.ForEach(x =>
            {
                if (!selectedLetters.Contains(x.letters[0]))
                {
                    var letter = x.letters.GetRandom();
                    letter.SetState(SlotState.Filled);
                    OnSlotFilled(letter);
                }
            });

        }

        SetLockedSlots();

    }//

    void SetLockedSlots()
    {
        var allLetters = allSlots.Copy();
        allLetters.Shuffle();

        if (level.singleLockCount > 0)
        {
            GetSingleLockableSlots(allLetters, out List<Letter> singleLockableSlots);
            int singleLockCount = Mathf.Clamp(new List<Letter>().Count, 0, level.singleLockCount);
            new List<Letter>().Take(singleLockCount).ForEach(x => x.SetState(SlotState.DualLocked));
        }

        if (level.dualLockCount > 0)
        {
            GetDualLockableSlots(allLetters, out List<Letter> dualLockableSlots);
            int singleLockCount = Mathf.Clamp(dualLockableSlots.Count, 0, level.dualLockCount);
            dualLockableSlots.Take(singleLockCount).ForEach(x => x.SetState(SlotState.DualLocked));
        }

        JsonController.Instance.SaveStates(allSlots);
    }

    void GetSingleLockableSlots(List<Letter> allLetters, out List<Letter> singleLockable)
    {
        singleLockable = new();

        foreach (Letter x in allLetters)
        {
            if (!x.IsEmpty)
                continue;

            if ((x.prevLetter == null && x.nextLetter.IsEmpty) || (x.nextLetter == null && x.prevLetter.IsEmpty))
            {
                singleLockable.Add(x);
                continue;
            }
            if (x.prevLetter == null || x.nextLetter == null)
                continue;

            if ((x.prevLetter.IsSpace && !x.nextLetter.IsEmpty) || x.nextLetter.IsSpace && !x.prevLetter.IsEmpty)
                continue;

            if (singleLockable.Any(y => y == x || y == x.prevLetter || y == x.nextLetter))
                continue;

            singleLockable.Add(x);
        }
    }

    void GetDualLockableSlots(List<Letter> allLetters, out List<Letter> dualLockable)
    {
        dualLockable = new();

        foreach (Letter x in allLetters)
        {
            if (!x.IsEmpty)
                continue;

            if (x.prevLetter == null || x.nextLetter == null)
                continue;

            if (x.prevLetter.IsSpace || x.nextLetter.IsSpace)
                continue;

            if (!(x.prevLetter.IsEmpty && x.nextLetter.IsEmpty))
                continue;

            if (dualLockable.Any(y => y == x || y == x.prevLetter || y == x.nextLetter))
                continue;

            dualLockable.Add(x);
        }
    }

    internal void OnSlotFilled(Letter letter, bool canAnimate = false)
    {
        var group = sameLetterGroups.Find(y => y.letters[0]._char == letter._char);

        if (group.letters.Any(x => x.IsEmpty || x.IsLock || x.IsDualLock))
        {
            keyboard.HightlightKey(letter._char);
        }
        else
        {
            group.letters.ForEach(x => x.slot.SetTriggerCorrect());
            Utils.WaitAndPerform(1f, () => group.letters.ForEach(x => x.slot.HideCode()));
            keyboard.DisableKey(letter._char, canAnimate);
        }

        CheckToUnlock(letter);
    }

    void CheckToUnlock(Letter letter)
    {
        if (letter.prevLetter != null)
        {
            if (letter.prevLetter.IsLock)
                UnlockSlot(letter.prevLetter);
            if (letter.prevLetter.IsDualLock)
            {
                if (letter.prevLetter.prevLetter.IsFilled || letter.prevLetter.prevLetter.IsSpace)
                    UnlockSlot(letter.prevLetter);
                else
                    letter.prevLetter.SetOneSideUnlock();
            }
        }

        if (letter.nextLetter != null)
        {
            if (letter.nextLetter.IsLock)
                UnlockSlot(letter.nextLetter);
            if (letter.nextLetter.IsDualLock)
            {
                if (letter.nextLetter.nextLetter.IsFilled)
                    UnlockSlot(letter.prevLetter);
                else
                    letter.nextLetter.SetOneSideUnlock();
            }
        }

    }//

    void UnlockSlot(Letter letter)
    {
        letter.SetState(SlotState.Empty);
        letter.Init();
    }


    internal void OnUpdate()
    {
        var nextSlot = NextSlot();

        if (nextSlot == null)
        {
            Debug.Log("<color=green>LevelCompleted!</color>");
            Utils.WaitAndPerform(1f, () => UIManager.Instance.SetWinPanel());
            return;
        }

        nextSlot.On_Select();
    }

    Slot NextSlot()
    {
        if (Slot.selectedSlot != null)
        {
            var next = allSlots.Skip(allSlots.FindIndex(x => x.slot == Slot.selectedSlot) + 1)?.FirstOrDefault(s => s.slot.letter.IsEmpty);
            if (next != null)
                return next.slot;
        }

        return allSlots.FirstOrDefault(x => x.slot.letter.IsEmpty)?.slot;
    }

    Slot PreviousSlot()
    {
        if (Slot.selectedSlot != null)
        {
            var prev = allSlots.Take(allSlots.FindIndex(x => x.slot == Slot.selectedSlot))?.LastOrDefault(s => s.slot.letter.IsEmpty);
            if (prev != null)
                return prev.slot;
        }

        return allSlots.LastOrDefault(s => s.slot.letter.IsEmpty)?.slot;
    }

    public void ToNextSlot()
    {
        var nextSlot = NextSlot();
        if (nextSlot != null)
            nextSlot.On_Select();
    }

    public void ToPrevSlot()
    {
        var prevSlot = PreviousSlot();
        if (prevSlot != null)
            prevSlot.On_Select();
    }

    List<string> SplitSentenceIntoLines(string sentence)
    {
        string[] words = sentence.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        List<string> lines = new();
        string currentLine = string.Empty;

        foreach (string word in words)
        {
            if (currentLine.Length + word.Length + (currentLine.Length > 0 ? 1 : 0) > maxLineLength)
            {
                currentLine += ' ';
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                if (currentLine.Length > 0)
                    currentLine += " ";

                currentLine += word;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        return lines;

    }//Split

    private void OnDisable()
    {
        LevelManager.OnLevelInit -= SetLevel;
    }

}//Class

[Serializable]
public class Word
{
    public List<Letter> letters = new();

    public Word(List<Letter> letters = null)
    {
        this.letters = letters.IsNullOrEmpty() ? new() : letters;
    }
}

[Serializable]
public class Letter
{
    [ReadOnly] public string name = "";
    public int index = 0;
    public char _char;
    public Slot slot;
    public int code;
    public SlotState state = SlotState.Empty;

    public Letter prevLetter = null, nextLetter = null;

    public bool IsLock { get => state is SlotState.Locked; }
    public bool IsEmpty { get => state is SlotState.Empty; }
    public bool IsFilled { get => state is SlotState.Filled; }
    public bool IsDualLock { get => state is SlotState.DualLocked; }
    public bool IsSpace { get => state is SlotState.SymbleOrBlank; }

    public Letter(Slot slot, char _char, int code)
    {
        name = $"{_char}";
        prevLetter = null;
        nextLetter = null;

        this._char = _char;
        this.slot = slot;
        this.code = code;
    }

    public void SetNeighbors(Letter prevLetter = null, Letter nextLetter = null, int index = -1)
    {
        this.prevLetter ??= prevLetter;
        this.nextLetter ??= nextLetter;
        this.index = index;
    }

    public void Init()
    {
        slot.Init(this);
    }


    public void SetOneSideUnlock()
    {
        SetState(SlotState.Locked);
        slot.Init(this);
    }

    public void SetState(SlotState state)
    {
        this.state = state;
    }
}

public static class MyExtensions
{
    public static bool IsVowel(this char _char)
    {
        return new List<char>() { 'a', 'e', 'i', 'o', 'u' }.Any(x => x == char.ToLowerInvariant(_char));
    }

    public static T GetRandomExcept<T>(this IList<T> collection, List<T> Except)
    {
        if (collection.Count <= Except.Count)
            return collection[Random.Range(0, collection.Count)];

        T randomItem;
        do
        {
            randomItem = collection[Random.Range(0, collection.Count)];
        } while (Except.Any(x => Equals(x, randomItem)));

        return randomItem;
    }

    public static void AddRangeIfNotNull<T>(this IList<T> list, List<T> listToAdd)
    {
        if (listToAdd.IsNullOrEmpty())
            return;

        listToAdd.ForEach(x => list.Add(x));
    }

    public static List<T> Copy<T>(this List<T> list)
    {
        var newList = new List<T>();
        newList.AddRange(list);
        return newList;
    }

}//


public enum SlotState
{
    Filled,
    Empty,
    SymbleOrBlank,
    Locked,
    DualLocked,
}