using TMPro;
using UnityEngine;

public class DistanceTravell : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI distanceText;
    private CarController carController;

    void Awake()
    {
        carController = FindObjectOfType<CarController>();
    }

    void Start()
    {
        if (carController == null)
        {
            Debug.LogWarning("DistanceTravell: CarController not found in scene.");
        }
    }

    void Update()
    {
        if (carController != null && distanceText != null)
        {
            distanceText.text = carController.Score.ToString();
        }
    }
}
