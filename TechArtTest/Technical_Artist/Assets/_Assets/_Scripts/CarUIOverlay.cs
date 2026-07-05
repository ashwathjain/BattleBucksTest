using UnityEngine;
using UnityEngine.UI;

public class CarUIOverlay : MonoBehaviour
{
    [Header("Prefab / Canvas")]
    [Tooltip("A UI prefab with a RectTransform. Example: a panel, icon or label.")]
    public RectTransform uiPrefab;

    [Tooltip("The canvas used to display the UI overlay. If left empty, the first active Canvas is used.")]
    public Canvas uiCanvas;

    [Header("Offset Values")]
    [Tooltip("World-space offset from the car position. Use this to move the overlay above the car.")]
    public Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    [Tooltip("Screen-space offset applied after the world position is converted into canvas space.")]
    public Vector2 screenOffset = new Vector2(0f, 50f);

    [Tooltip("When enabled, the UI overlay follows the car every frame.")]
    public bool followEveryFrame = true;

    private RectTransform uiInstance;
    private RectTransform canvasRect;
    private Camera uiCamera;

    private void Start()
    {
        if (uiPrefab == null)
        {
            Debug.LogError("CarUIOverlay: uiPrefab is not assigned.", this);
            enabled = false;
            return;
        }

        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
            if (uiCanvas == null)
            {
                Debug.LogError("CarUIOverlay: No Canvas found in the scene.", this);
                enabled = false;
                return;
            }
        }

        canvasRect = uiCanvas.GetComponent<RectTransform>();
        uiCamera = uiCanvas.renderMode == RenderMode.ScreenSpaceCamera ? uiCanvas.worldCamera : Camera.main;

        uiInstance = Instantiate(uiPrefab, uiCanvas.transform);
        uiInstance.SetAsLastSibling();
        uiInstance.gameObject.SetActive(true);

        UpdateOverlayPosition();
    }

    private void Update()
    {
        if (followEveryFrame)
        {
            UpdateOverlayPosition();
        }
    }

    public void UpdateOverlayPosition()
    {
        if (uiInstance == null || canvasRect == null)
            return;

        Vector3 worldPosition = transform.position + worldOffset;

        if (uiCanvas.renderMode == RenderMode.WorldSpace)
        {
            uiInstance.position = worldPosition;
            uiInstance.localPosition += new Vector3(screenOffset.x, screenOffset.y, 0f);
            return;
        }

        if (uiCamera == null)
        {
            uiCamera = Camera.main;
        }

        Vector3 screenPoint = uiCamera.WorldToScreenPoint(worldPosition);
        bool isVisible = screenPoint.z > 0f;
        uiInstance.gameObject.SetActive(isVisible);

        if (!isVisible)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCamera, out Vector2 localPoint);
        uiInstance.anchoredPosition = localPoint + screenOffset;
    }

    public void SetOffsets(Vector3 newWorldOffset, Vector2 newScreenOffset)
    {
        worldOffset = newWorldOffset;
        screenOffset = newScreenOffset;
        UpdateOverlayPosition();
    }
}
