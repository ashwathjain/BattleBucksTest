using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LobbyUIController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The main Panel containing the buttons and background.")]
    [SerializeField] private RectTransform panel;
    [Tooltip("The Play button in the layout.")]
    [SerializeField] private Button playButton;

    [Header("DOTween Animation Settings")]
    [Tooltip("Duration of the pop-in scale animation.")]
    [SerializeField] private float popInDuration = 0.45f;
    [Tooltip("Ease curve for the pop-in animation (e.g. OutBack for elastic bounce).")]
    [SerializeField] private Ease popInEase = Ease.OutBack;
    
    [Tooltip("Duration of the pop-out scale animation.")]
    [SerializeField] private float popOutDuration = 0.35f;
    [Tooltip("Ease curve for the pop-out animation (e.g. InBack for shrinking).")]
    [SerializeField] private Ease popOutEase = Ease.InBack;

    private System.Action onPlayPressed;

    private void Awake()
    {
        // Auto-find components if not assigned in Inspector
        if (panel == null)
        {
            var pTrans = transform.Find("Panel");
            if (pTrans != null) panel = pTrans as RectTransform;
        }
        if (playButton == null)
        {
            var playBtnTrans = transform.Find("Panel/Layout/Button_Play");
            if (playBtnTrans != null) playButton = playBtnTrans.GetComponent<Button>();
        }
    }

    public void Initialize(System.Action playCallback)
    {
        onPlayPressed = playCallback;

        // Bind Play button onClick
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayClicked);
        }

        // Animate in using DOTween (elastic scale pop-in)
        if (panel != null)
        {
            panel.localScale = Vector3.zero;
            panel.DOScale(Vector3.one, popInDuration).SetEase(popInEase).SetUpdate(true);
        }
    }

    private void OnPlayClicked()
    {
        if (panel != null)
        {
            // Disable button interaction to prevent double-clicks
            if (playButton != null) playButton.interactable = false;

            // Animate out using DOTween (shrinking pop-out)
            panel.DOScale(Vector3.zero, popOutDuration).SetEase(popOutEase).SetUpdate(true).OnComplete(() =>
            {
                onPlayPressed?.Invoke();
                Destroy(gameObject);
            });
        }
        else
        {
            onPlayPressed?.Invoke();
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Kill active tweens when destroyed to prevent warnings/leaks
        if (panel != null)
        {
            panel.DOKill();
        }
    }
}
