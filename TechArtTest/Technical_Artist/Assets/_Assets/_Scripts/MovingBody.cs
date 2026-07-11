using UnityEngine;

public class MovingBody : MonoBehaviour
{
	[SerializeField] private float speed = 10f;
	public float Speed => speed;

	private CarController carController;

	private void Start()
	{
		carController = FindAnyObjectByType<CarController>();
	}
    
    private void Update()
    {
		float currentSpeed = speed;
		if (carController != null)
		{
			if (carController.CurrentState == CarController.GameState.Playing)
			{
				currentSpeed = carController.CurrentGroundSpeed;
			}
			else
			{
				currentSpeed = 0f;
			}
		}
        transform.position -= Vector3.forward * currentSpeed * Time.deltaTime;
    }
}
