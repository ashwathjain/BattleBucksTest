using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MessageBoxBubble : MonoBehaviour
{
    [Header("Target & Offset")]
    [Tooltip("The offset from the target car position in world space.")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.8f, 0f);

    [Header("DOTween Animation Settings")]
    [Tooltip("Duration of the pop-in scale animation.")]
    [SerializeField] private float popInDuration = 0.35f;
    [Tooltip("Ease curve for the pop-in animation (e.g. OutBack for bounce).")]
    [SerializeField] private Ease popInEase = Ease.OutBack;
    
    [Tooltip("How long the bubble stays visible between animations.")]
    [SerializeField] private float lifetime = 1.2f;
    
    [Tooltip("Duration of the pop-out scale animation.")]
    [SerializeField] private float popOutDuration = 0.25f;
    [Tooltip("Ease curve for the pop-out animation (e.g. InBack for shrinking).")]
    [SerializeField] private Ease popOutEase = Ease.InBack;

    [Header("Image Components")]
    [Tooltip("Assign the 3 Image components here that will display the random sprites.")]
    [SerializeField] private Image emoteImage1;
    [SerializeField] private Image emoteImage2;
    [SerializeField] private Image emoteImage3;

    [Header("Emote Sprites")]
    [Tooltip("Assign your list of textures/sprites here. The bubble will randomly choose from this list for each of the assigned image components.")]
    [SerializeField] private Sprite[] emoteSprites;

    private Transform targetCar;
    private Canvas canvas;
    private Camera mainCamera;
    private RectTransform rectTransform;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();

        // Find the player's car automatically if target not set
        if (targetCar == null)
        {
            var playerCar = FindAnyObjectByType<CarController>();
            if (playerCar != null)
            {
                targetCar = playerCar.transform;
            }
        }

        // Setup random sprites on the assigned images
        AssignRandomSprite(emoteImage1);
        AssignRandomSprite(emoteImage2);
        AssignRandomSprite(emoteImage3);

        // Initialize scale to zero for pop-in animation
        transform.localScale = Vector3.zero;

        // Create DOTween animation sequence (Ease-In -> Delay -> Ease-Out -> Destroy)
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector3.one, popInDuration).SetEase(popInEase));
        seq.AppendInterval(lifetime);
        seq.Append(transform.DOScale(Vector3.zero, popOutDuration).SetEase(popOutEase));
        seq.OnComplete(() => Destroy(gameObject));
    }

    private void AssignRandomSprite(Image img)
    {
        if (img != null)
        {
            if (emoteSprites != null && emoteSprites.Length > 0)
            {
                img.sprite = emoteSprites[Random.Range(0, emoteSprites.Length)];
                img.color = Color.white; // Make visible
            }
            else
            {
                // Hide image component if no sprites are assigned in inspector
                img.color = new Color(1f, 1f, 1f, 0f);
            }
        }
    }

    private void Update()
    {
        if (targetCar == null || canvas == null || mainCamera == null)
        {
            // Stop any active tweens on this object to prevent DOTween leaks
            transform.DOKill();
            Destroy(gameObject);
            return;
        }

        // Convert target world position to screen space
        Vector3 worldPosition = targetCar.position + worldOffset;
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPosition);

        if (screenPoint.z > 0f)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out Vector2 localPoint
            );
            rectTransform.anchoredPosition = localPoint;
        }
        else
        {
            rectTransform.anchoredPosition = new Vector2(-9999f, -9999f);
        }
    }

    private void OnDestroy()
    {
        // Kill active tweens when destroyed to prevent errors/leaks
        transform.DOKill();
    }
}
