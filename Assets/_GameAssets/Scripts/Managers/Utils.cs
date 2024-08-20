using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public static class Utils
{
    public static string CurrencyFormat(float value)
    {
        string str = value.ToString();

        if (value < 1000)
            str = value.ToString("F0");
        else if (value >= 1000 && value < 1000000)
        {
            float _value = value / (float)1000;
            str = $"{_value:F1}K";
        }
        else if (value >= 1000000)
        {
            float _value = value / (float)1000000;
            str = $"{_value:F2}M";
        }

        return str;
    }

    public static string FormatTime(float seconds)
    {
        int hours = Mathf.FloorToInt(seconds / 3600);
        int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60);
        if (hours > 0)
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, remainingSeconds);
        return string.Format("{0:D2}:{1:D2}", minutes, remainingSeconds);
    }

    public static Sequence WaitAndPerform(float wait, System.Action action)
    {
        var seq = DOTween.Sequence();
        seq.AppendInterval(wait);
        seq.AppendCallback(() => action?.Invoke());
        return seq;
    }


}//

public static class Extension
{
    public enum AnchorPreset
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }


    public static void ClearChilds(this Transform parent, bool canDestroy = true)
    {
        List<Transform> childs = new();

        foreach (Transform child in parent)
        {
            if (child.parent == parent)
                childs.Add(child);
        }

        childs.ForEach(x =>
        {
            x.SetParent(null);
            if (canDestroy)
                Object.DestroyImmediate(x.gameObject);
            else
                x.gameObject.SetActive(false);
        });
    }

    public static void SetAnchorPreset(this RectTransform rectTransform, AnchorPreset anchorPreset)
    {
        switch (anchorPreset)
        {
            case AnchorPreset.TopLeft:
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                break;
            case AnchorPreset.TopCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 1);
                rectTransform.anchorMax = new Vector2(0.5f, 1);
                rectTransform.pivot = new Vector2(0.5f, 1);
                break;
            case AnchorPreset.TopRight:
                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(1, 1);
                break;
            case AnchorPreset.MiddleLeft:
                rectTransform.anchorMin = new Vector2(0, 0.5f);
                rectTransform.anchorMax = new Vector2(0, 0.5f);
                rectTransform.pivot = new Vector2(0, 0.5f);
                break;
            case AnchorPreset.MiddleCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                break;
            case AnchorPreset.MiddleRight:
                rectTransform.anchorMin = new Vector2(1, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);
                rectTransform.pivot = new Vector2(1, 0.5f);
                break;
            case AnchorPreset.BottomLeft:
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0, 0);
                break;
            case AnchorPreset.BottomCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 0);
                rectTransform.anchorMax = new Vector2(0.5f, 0);
                rectTransform.pivot = new Vector2(0.5f, 0);
                break;
            case AnchorPreset.BottomRight:
                rectTransform.anchorMin = new Vector2(1, 0);
                rectTransform.anchorMax = new Vector2(1, 0);
                rectTransform.pivot = new Vector2(1, 0);
                break;
        }
    }

    public static void GameObjectSetActive(this Component component, bool value)
    {
        if (component == null)
        {
            //Debug.LogError("Component is null");
            return;
        }

        component.gameObject.SetActive(value);
    }

    public static bool TryGetComponentInChildren<T>(this Transform parent, out T component) where T : Component
    {
        component = null;
        foreach (Transform child in parent)
        {
            if (child.TryGetComponent(out T foundComponent))
            {
                component = foundComponent;
                break;
            }
        }

        return component != null;
    }

    public static void CurrencySetText(this TextMeshProUGUI textMesh, float value)
    {
        if (value < 1000)
            textMesh.text = value.ToString("F0");
        else if (value >= 1000 && value < 1000000)
        {
            float _value = value / 1000f;
            textMesh.text = $"{_value:F1}K";
        }
        else if (value >= 1000000)
        {
            float _value = value / 1000000f;
            textMesh.text = $"{_value:F2}M";
        }
    }
}
