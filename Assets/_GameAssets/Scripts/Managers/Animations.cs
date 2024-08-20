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

        if (_Type == AnimationType.Bird)
        {
            Image image = GetComponent<Image>();
            RectTransform canvasRect = image.canvas.GetComponent<RectTransform>();
            RectTransform imageRect = image.GetComponent<RectTransform>();
            birdStartPosition = GetComponent<RectTransform>().anchoredPosition = new Vector2(-canvasRect.rect.width / 2 - imageRect.rect.width / 2, rectPosition.y + (Random.Range(-3, 1) * imageRect.rect.height / 2));
            birdEndPos = canvasRect.rect.width / 2.5f + imageRect.rect.width;
        }

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

            case AnimationType.Splash:
                float splashTime = Random.Range(2.2f, 3f);
                var slider = GetComponentInChildren<Slider>();
                var text = GetComponentInChildren<TMPro.TextMeshProUGUI>();

                seq.OnStart(() => { slider.value = 0; text.text = "Loading"; });
                seq.Append(slider.DOValue(1, splashTime));
                //seq.OnComplete(() => UIManager.Instance.SetHomePanle());

                seq.InsertCallback((splashTime / 4), () => text.text = "Loading.");
                seq.InsertCallback((splashTime / 4) * 2, () => text.text = "Loading..");
                seq.InsertCallback((splashTime / 4) * 3, () => text.text = "Loading...");

                break;

            case AnimationType.Bird:
                StartCoroutine(AnimateBird());
                break;

            case AnimationType.Eyes:

                seq.Append(transform.DOLocalMoveY(-3f, .2f).SetLoops(2, LoopType.Yoyo).SetRelative(true));
                seq.AppendInterval(Random.Range(2f, 3.5f));
                seq.SetLoops(-1, LoopType.Restart);
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


        }
    }

    IEnumerator AnimateBird()
    {
        while (true)
        {
            if (!transform.parent.gameObject.activeSelf)
                yield break;

            rectTr.anchoredPosition = birdStartPosition;
            yield return new WaitForSeconds(Random.Range(2f, 3.8f));

            rectTr.DOAnchorPosX(birdEndPos, 4f).SetEase(Ease.Linear);
            yield return new WaitForSeconds(4f);
        }
    }

    public void Reset()
    {
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

    private void OnDestroy()
    {
        if (_Type is AnimationType.Bird or AnimationType.Spiral)
        {
            seq.Kill(false);
            DOTween.Kill(transform.gameObject);
        }

    }
}


public enum AnimationType
{
    BG,
    Spiral,
    Toast,
    Bird,
    Splash,
    Eyes
}