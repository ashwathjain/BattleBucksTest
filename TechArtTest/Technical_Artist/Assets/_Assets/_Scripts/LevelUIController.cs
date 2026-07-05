using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUIController : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Distance required to complete one level.")]
    [SerializeField] private float distancePerLevel = 300f;

    [Header("UI References")]
    [Tooltip("The slider component used as a progress bar. If not set, it will look on this GameObject.")]
    [SerializeField] private Slider progressBar;
    [Tooltip("The text component displaying the current level. If not set, will search children.")]
    [SerializeField] private TextMeshProUGUI textStartLevel;
    [Tooltip("The text component displaying the next level. If not set, will search children.")]
    [SerializeField] private TextMeshProUGUI textEndLevel;

    private CarController carController;
    private int currentLevel = 1;

    private void Awake()
    {
        // Find components on this GameObject if not set
        if (progressBar == null)
        {
            progressBar = GetComponent<Slider>();
        }
        if (textStartLevel == null)
        {
            var tslGO = transform.Find("TextStartLevel");
            if (tslGO != null) textStartLevel = tslGO.GetComponent<TextMeshProUGUI>();
        }
        if (textEndLevel == null)
        {
            var telGO = transform.Find("TextEndLevel");
            if (telGO != null) textEndLevel = telGO.GetComponent<TextMeshProUGUI>();
        }

        // Find the player's car controller in the scene
        carController = FindAnyObjectByType<CarController>();
    }

    private void Start()
    {
        UpdateLevelUI();
    }

    private void Update()
    {
        if (carController == null)
        {
            carController = FindAnyObjectByType<CarController>();
            if (carController == null) return;
        }

        // Don't update if not playing
        if (carController.CurrentState != CarController.GameState.Playing)
        {
            return;
        }

        float distance = carController.DistanceTravelled;
        float progressInCurrentLevel = (distance % distancePerLevel) / distancePerLevel;
        int calculatedLevel = Mathf.FloorToInt(distance / distancePerLevel) + 1;

        if (calculatedLevel != currentLevel)
        {
            currentLevel = calculatedLevel;
            UpdateLevelUI();
        }

        if (progressBar != null)
        {
            progressBar.value = progressInCurrentLevel;
        }
    }

    private void UpdateLevelUI()
    {
        if (textStartLevel != null)
        {
            textStartLevel.text = currentLevel.ToString();
        }
        if (textEndLevel != null)
        {
            textEndLevel.text = (currentLevel + 1).ToString();
        }
    }
}
