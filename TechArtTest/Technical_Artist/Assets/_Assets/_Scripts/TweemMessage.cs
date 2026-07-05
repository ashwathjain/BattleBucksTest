using DG.Tweening;
using UnityEngine;

public class TweemMessage : MonoBehaviour
{
    [Header("Message Prefab")]
    [Tooltip("UI prefab to display. It should contain a RectTransform and can optionally include a CanvasGroup.")]
    public RectTransform messagePrefab;

    [Tooltip("Canvas used to parent the message. If null, the script will try to find the first active Canvas in the scene.")]
    public Canvas parentCanvas;

    [Header("Timing")]
    [Tooltip("How long the message remains visible after the intro animation.")]
    public float visibleDuration = 3f;

    [Tooltip("Duration of the pop-in animation.")]
    public float introDuration = 0.25f;

    [Tooltip("Duration of the pop-out animation.")]
    public float outroDuration = 0.2f;

    [Header("Animation")]
    [Tooltip("End scale when the message is fully visible.")]
    public Vector3 targetScale = Vector3.one;

    [Tooltip("Start scale for the pop-in animation.")]
    public Vector3 startScale = Vector3.zero;

    [Tooltip("Position of the message inside the canvas.")]
    public Vector2 anchoredPosition = Vector2.zero;

    [Tooltip("Ease used for the intro animation.")]
    public Ease introEase = Ease.OutBack;

    [Tooltip("Ease used for the outro animation.")]
    public Ease outroEase = Ease.InBack;

    [Tooltip("If true, the prefab will be destroyed after the outro animation completes.")]
    public bool destroyOnComplete = true;

    private Sequence activeSequence;

    public void ShowMessage()
    {
        if (messagePrefab == null)
        {
            Debug.LogError("TweemMessage: Message prefab is not assigned.", this);
            return;
        }

        if (parentCanvas == null)
        {
            parentCanvas = FindObjectOfType<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("TweemMessage: No Canvas found in scene.", this);
                return;
            }
        }

        RectTransform instance = Instantiate(messagePrefab, parentCanvas.transform);
        instance.anchoredPosition = anchoredPosition;
        instance.localScale = startScale;

        CanvasGroup canvasGroup = instance.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = instance.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        instance.gameObject.SetActive(true);

        activeSequence?.Kill();
        activeSequence = DOTween.Sequence();

        activeSequence.Append(instance.DOScale(targetScale, introDuration).SetEase(introEase));
        activeSequence.Join(canvasGroup.DOFade(1f, introDuration));
        activeSequence.AppendInterval(visibleDuration);
        activeSequence.Append(instance.DOScale(startScale, outroDuration).SetEase(outroEase));
        activeSequence.Join(canvasGroup.DOFade(0f, outroDuration));
        activeSequence.OnComplete(() =>
        {
            if (destroyOnComplete)
            {
                Destroy(instance.gameObject);
            }
            else
            {
                instance.gameObject.SetActive(false);
            }
        });
    }

    public void CloseMessage()
    {
        if (activeSequence != null && activeSequence.IsActive())
        {
            activeSequence.Kill();
            activeSequence = null;
        }
    }
}
