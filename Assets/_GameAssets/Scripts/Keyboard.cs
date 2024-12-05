using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using MyBox;
using TMPro;
using System.Linq;

public class Keyboard : Singleton<Keyboard>
{
    [SerializeField] Key keyPrefab, prevKey, nextKey;
    [SerializeField] List<Transform> parents;
    [SerializeField] GameObject interactabeOb;

    [SerializeField] List<Key> _arrowKeys;
    internal List<int> counter = new() { 10, 9, 7 };
    internal List<char> qwertyKeys = new() { 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x', 'c', 'v', 'b', 'n', 'm' };
    int index = 0;

    internal List<Key> Keys = new();
    internal Key PrevKey, NextKey;

    public List<Key> ArrowKeys { get => _arrowKeys; }

    public void Init()
    {
        if (Keys.IsNullOrEmpty())
            GenerateKeyboeard();

        Keys.ForEach(x => x.SetEnabled());
        SetInteractable(true);
    }

    public void ImplimentTheme(bool islight)
    {
        for (int i = 0; i < Keys.Count; i++)
        {
            Keys[i].ApplyTheme(islight);
        }
    }


    public bool GetKey(char _char, out Key key)
    {
        key = Keys.Find(x => x.Char == _char);
        return key != null;
    }

    public void HightlightKey(char _char)
    {
        if (GetKey(_char, out Key k))
            k.HightLight();
    }

    public void DisableKey(char _char, bool canAnimate)
    {
        if (GetKey(_char, out Key k))
            k.SetDisabled(canAnimate);
    }

    public void SetInteractable(bool isInteractable)
    {
        interactabeOb.SetActive(!isInteractable);
    }

    [ButtonMethod]
    public void GenerateKeyboeard()
    {
        _arrowKeys = new List<Key>();
        bool islighttheme = ThemeManager.instance.IsLightMode;
        Keys = new();
        index = 0;

        for (int i = 0; i < counter.Count; i++)
        {
            parents[i].ClearChilds(true);
            for (int j = 0; j < counter[i]; j++)
            {
                //var key = PrefabUtility.InstantiatePrefab(keyPrefab, parents[i]) as Key;
                var key = Instantiate(keyPrefab, parents[i]);
                key.Init(qwertyKeys[index]);
                Keys.Add(key);
                key.ApplyTheme(islighttheme);
                index++;

            }
        }

        //var leftKey = PrefabUtility.InstantiatePrefab(prevKey, parents[^1]) as Key;
        var leftKey = Instantiate(prevKey, parents[^1]);
        _arrowKeys.Add(leftKey);
        leftKey.SetListner(delegate
        {
            SoundManager.Instance.PlaySfx(SoundType.Keyboard);
            GameManager.Instance.ToPrevSlot();
        });
        PrevKey = leftKey;
        PrevKey.ApplyTheme(islighttheme);

        //var rightKey = PrefabUtility.InstantiatePrefab(nextKey, parents[^1]) as Key;
        var rightKey = Instantiate(nextKey, parents[^1]) as Key;
        _arrowKeys.Add(rightKey);
        rightKey.SetListner(delegate
        {
            SoundManager.Instance.PlaySfx(SoundType.Keyboard);
            GameManager.Instance.ToNextSlot();
        });

        NextKey = rightKey;
        NextKey.ApplyTheme(islighttheme);

    }

}//Class