using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.UI;
using MyBox;
using System;


public class Animations : MonoBehaviour
{
    public AnimationType _Type;

    [SerializeField, ConditionalField(nameof(_Type), false, AnimationType.HomeLogo, order = 1)] Color highlightedColor;

    Vector3 localPosition;
    Vector3 birdStartPosition;
    Vector2 rectPosition;
    RectTransform rectTr;
    Sequence seq;
    float scale, birdEndPos;

    private void Awake()
    {
        localPosition = transform.localPosition;
        if (TryGetComponent(out rectTr))
            rectPosition = rectTr.anchoredPosition;

        scale = transform.localScale.x;
    }

    private void OnEnable()
    {
        seq = DOTween.Sequence();
        switch (_Type)
        {
            case AnimationType.BG:
                var images = transform.GetComponentsInChildren<Image>();
                images.ForEach(x => x.raycastTarget = true);

                seq.Append(transform.DOScale(1, .35f).SetEase(Ease.Linear));
                seq.Join(transform.DOLocalMove(Vector3.zero, .35f).SetEase(Ease.Linear));
                break;

            case AnimationType.BottomPanel:
                rectTr.DOAnchorPos3DY(0, .4f).SetEase(Ease.InSine);
                break;

            case AnimationType.Toast:
                var rect = transform.GetComponent<RectTransform>();
                seq.SetAutoKill();
                seq.Append(rect.DOAnchorPosY(240, 1.6f).SetRelative());
                seq.Join(rect.GetComponent<Image>().DOFade(0, .6f).SetDelay(1f));
                seq.OnComplete(() => Destroy(gameObject));
                break;

            case AnimationType.Spiral:
                transform.DOScale(0, 0f);
                float wait = 0;
                seq.AppendCallback(() => { transform.eulerAngles = Vector3.zero; transform.DOScale(0, 0f); wait = Random.Range(0.4f, 0.9f); });
                seq.AppendInterval(wait);
                seq.Append(transform.DORotate(new(0, 0, 180), .75f)).SetEase(Ease.Linear).SetLoops(2, LoopType.Incremental);
                seq.Join(transform.DOScale(1, .75f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo));
                seq.AppendInterval(1f);
                seq.SetLoops(-1, LoopType.Restart);
                break;

            case AnimationType.CompleteLogo:

                transform.GetChilds(out List<Transform> childs);
                childs.Shuffle();
                childs.ForEach(x => {

                    DOTween.Sequence()
                    .Append(x.DOMoveY(-10f, .7f).SetEase(Ease.Linear).SetDelay(Random.Range(0, 2) == 0 ? .2f : 0).SetRelative().SetLoops(2, LoopType.Yoyo))
                    .AppendInterval(.2f)
                    .SetLoops(-1, LoopType.Restart);
                });
                break;

            case AnimationType.NoAds:

                transform.GetChilds(out List<Transform> _childs);
                _childs.Shuffle();
                _childs.ForEach((x,i) => {

                    DOTween.Sequence()
                    .SetDelay(1f)
                    .Append(x.DOLocalMoveY(-10f, .7f).SetEase(Ease.Linear).SetDelay(i * .2f).SetRelative().SetLoops(2, LoopType.Yoyo))
                    .AppendInterval(.2f)
                    .SetLoops(-1, LoopType.Restart);
                });
                break;

            case AnimationType.HomeLogo:
                StartCoroutine(HomeLogo());
                break;

        }
    }

    //[SerializeField, ConditionalField(nameof(_Type), false ,AnimationType.HomeLogo, order = 1)] float changeWait, nextLetterWait;

    readonly List<char> list = new() { 'c', 'r', 'y', 'p', 't', 'o' };
    readonly List<int> counts = new() { 5, 6, 3, 6, 4, 3};

    IEnumerator HomeLogo()
    {
        LevelManager.Instance.SetLevelRoad();
        yield return new WaitForSeconds(.1f);
        transform.GetChilds(out List<Transform> slots);

        slots.ForEach((x,i) => {

            DOTween.Sequence()
            .OnStart(() => x.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"{GetRandomLetter(list[i])}")
            .Append(x.DOMoveY(-9f, .8f).SetEase(Ease.Linear).SetDelay(i * .2f).SetRelative().SetLoops(2, LoopType.Yoyo))
            .AppendInterval(.2f)
            .SetLoops(-1, LoopType.Restart);
        });


        for(int i = 0; i< slots.Count; i++)
        {
            int _i = i;
            var letter = list[_i];
            var textComponent = slots[_i].GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
            var prevColor = textComponent.color;
            textComponent.color = highlightedColor;

            for (int j = 0; j < counts[_i]; j++)
            {
                var _letter = GetRandomLetter(letter);
                textComponent.text = $"{_letter}";
                yield return new WaitForSeconds(.2f);
            }

            letter = _i == 0 ? char.ToUpper(letter) : char.ToLower(letter);
            slots[_i].GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = letter.ToString();
            yield return new WaitForSeconds(.3f);
            textComponent.color = prevColor;
        }

    }

    private  char lastLetter = '0';
    char GetRandomLetter(char targetLetter)
    {
        char randomLetter = (char)Random.Range('a', 'z' + 1);

        while (true)
        {
            if(randomLetter != lastLetter && randomLetter != targetLetter)
                break;

            randomLetter = (char)Random.Range('a', 'z' + 1);
        }

        lastLetter = randomLetter;
        return randomLetter;
    }


    public void Reset()
    {
        if(_Type is AnimationType.BottomPanel)
        {
            UIManager.Instance.OnClick_Sfx();
            rectTr.DOAnchorPos3DY(-rectTr.rect.height, .2f).SetEase(Ease.OutSine)
                .OnComplete(() => transform.parent.GameObjectSetActive(false));
            return;
        }

        if (_Type is not AnimationType.BG)
            return;

        var images = transform.GetComponentsInChildren<Image>();
        images.ForEach(x => x.raycastTarget = false);

        DOTween.Sequence()
            .OnStart(() => UIManager.Instance.OnClick_Sfx())
            .Append(transform.DOScale(scale, .3f))
            .Join(transform.DOLocalMove(localPosition, .3f))
            .OnComplete(() => transform.parent.GameObjectSetActive(false));
    }

    
}


public enum AnimationType
{
    BG,
    Spiral,
    Toast,
    CompleteLogo,
    HomeLogo,
    NoAds,
    BottomPanel
}