using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class CarController : MonoBehaviour
{
    [SerializeField] private float speed = 10f;

    [Header("Tyre Rotation")]
    [SerializeField] private Transform[] tyreTransforms;
    [SerializeField] private float tyreRotationSpeed = 720f;

    [Header("Lane Positions")]
    public float leftXPosition = -5f;
    public float rightXPosition = 5f;

	[SerializeField] private float horizontalStepCount = 5f;
	public float HorizontalStepCount => horizontalStepCount;
	[SerializeField] private float swipeThreshold = 50f;
	[SerializeField] 
	MeshRenderer carMeshRenderer;
	[SerializeField] private int emissionMaterialIndex = 0;
	[SerializeField] private float groundTravelSpeed = 10f;
	[SerializeField, Tooltip("Seconds required to earn one score point.")]
	private float secondsPerPoint = 1f;
	private float scoreTimer = 0f;
	public int Score { get; private set; }
	private MovingBody movingBody;

	int EmissionColor = Shader.PropertyToID("_EmissionColor");
	private bool emissionActive = false;
	private float emissionTimer = 0f;
	[SerializeField] private float emissionDuration = 2f;

	private bool isMoving = false;
	private int currentStep = 0;
	private Vector3 targetPosition;
	private bool isGameOver = false;
	private bool hasCrashed = false;
	public bool IsGameOver => hasCrashed;

	private Vector2 swipeStartPosMouse;
	private bool isSwipingMouse = false;
	private Vector2 swipeStartPosTouch;
	private bool isSwipingTouch = false;

	public enum GameState { Lobby, Playing, Crashing }
	private GameState currentState = GameState.Lobby;
	public GameState CurrentState => currentState;

	[Header("Lobby Settings")]
	[SerializeField] private GameObject lobbyScenePrefab;
	private GameObject activeLobbyInstance;

	[Header("MessageBox Settings")]
	[SerializeField] private GameObject messageBoxPrefab;
	private int passedCarsCount = 0;
	private Transform visualChild;
	private Quaternion initialVisualRotation = Quaternion.identity;

	// stats
	private float distanceTravelled;
	public float DistanceTravelled => distanceTravelled;

	private void Awake()
	{
		targetPosition = transform.position;
		distanceTravelled = 0f;
		scoreTimer = 0f;
		Score = 0;
		movingBody = FindObjectOfType<MovingBody>();
		if (movingBody != null)
		{
			groundTravelSpeed = movingBody.Speed;
		}
		EnsureCollisionComponents();

		// Find visual child for tilting (so we don't rotate the physical BoxCollider)
		for (int i = 0; i < transform.childCount; i++)
		{
			var child = transform.GetChild(i);
			string childName = child.name.ToLower();
			if (childName.Contains("body") || childName.Contains("model") || childName.Contains("car") || childName.Contains("compact"))
			{
				visualChild = child;
				break;
			}
		}
		if (visualChild == null && transform.childCount > 0)
		{
			visualChild = transform.GetChild(0);
		}

		if (visualChild != null)
		{
			initialVisualRotation = visualChild.localRotation;
		}

		currentState = GameState.Lobby;
		if (lobbyScenePrefab == null)
		{
			lobbyScenePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Assets/Prefabs/UI/LobbyScene.prefab");
		}
	}

	private void Update()
	{
		if (isGameOver || currentState == GameState.Lobby)
		{
			return;
		}

		if (carMeshRenderer != null)
		{
			var materials = carMeshRenderer.materials;
			if (emissionMaterialIndex >= 0 && emissionMaterialIndex < materials.Length)
			{
				var mat = materials[emissionMaterialIndex];
				if (emissionActive)
				{
					mat.EnableKeyword("_EMISSION");
				}
				else
				{
					mat.DisableKeyword("_EMISSION");
				}
				materials[emissionMaterialIndex] = mat;
				carMeshRenderer.materials = materials;
			}
		}
		/* if (Keyboard.current != null)
		{
			if (Keyboard.current.leftArrowKey.wasPressedThisFrame) {
				MoveHorizontal(1);
			} else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
			{
				MoveHorizontal(-1);
			}
		} */

		if (!hasCrashed)
		{
			HandleMouseSwipe();
			HandleTouchSwipe();

			// Smooth lane-change tilt/rotation
			float targetYaw = 0f;
			float targetRoll = 0f;

			if (isMoving)
			{
				Vector3 smoothMovePos = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
				transform.position = smoothMovePos;

				float xDiff = targetPosition.x - transform.position.x;
				if (Mathf.Abs(xDiff) > 0.05f)
				{
					targetYaw = Mathf.Clamp(xDiff * 3f, -1f, 1f) * 15f; // Yaw steering (15 degrees max)
					targetRoll = -Mathf.Clamp(xDiff * 3f, -1f, 1f) * 5f; // Roll lean (5 degrees max)
				}
				else
				{
					isMoving = false;
				}
			}
			else
			{
				transform.position = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
			}

			Quaternion targetRot = initialVisualRotation * Quaternion.Euler(0f, targetYaw, targetRoll);
			if (visualChild != null)
			{
				visualChild.localRotation = Quaternion.Slerp(visualChild.localRotation, targetRot, Time.deltaTime * 12f);
			}
			else
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 12f);
			}
		}

		if (emissionActive)
		{
			emissionTimer -= Time.deltaTime;
			if (emissionTimer <= 0f)
			{
				emissionActive = false;
			}
		}

		if (isMoving && tyreTransforms != null && tyreTransforms.Length > 0)
		{
			float wheelSpin = tyreRotationSpeed * Time.deltaTime;
			foreach (Transform tyre in tyreTransforms)
			{
				if (tyre != null)
				{
					tyre.Rotate(0f, 0f, wheelSpin, Space.Self);
				}
			}
		}
		distanceTravelled += groundTravelSpeed * Time.deltaTime;
		if (secondsPerPoint <= 0f)
		{
			secondsPerPoint = 1f;
		}
		scoreTimer += Time.deltaTime;
		while (scoreTimer >= secondsPerPoint)
		{
			Score++;
			scoreTimer -= secondsPerPoint;
		}
	}

	private void HandleMouseSwipe()
	{
		if (Mouse.current == null)
		{
			return;
		}

		if (Mouse.current.leftButton.wasPressedThisFrame)
		{
			isSwipingMouse = true;
			swipeStartPosMouse = Mouse.current.position.ReadValue();
		}
		else if (Mouse.current.leftButton.isPressed && isSwipingMouse)
		{
		}
		else if (Mouse.current.leftButton.wasReleasedThisFrame && isSwipingMouse)
		{
			Vector2 endPos = Mouse.current.position.ReadValue();
			Vector2 delta = endPos - swipeStartPosMouse;
			isSwipingMouse = false;

			if (Mathf.Abs(delta.x) > swipeThreshold && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
			{
				if (delta.x > 0f){
					MoveHorizontal(1);
				}
				else{
					MoveHorizontal(-1);
				}
				emissionActive = true;
				emissionTimer = emissionDuration;
			}
		}
	}

	private void HandleTouchSwipe()
	{
		if (Touchscreen.current == null)
		{
			return;
		}

		var primary = Touchscreen.current.primaryTouch;
		if (!primary.press.isPressed && !isSwipingTouch)
		{
			return;
		}

		if (primary.press.wasPressedThisFrame)
		{
			isSwipingTouch = true;
			swipeStartPosTouch = primary.position.ReadValue();
		}
		else if (primary.press.isPressed && isSwipingTouch)
		{
		}
		else if (primary.press.wasReleasedThisFrame && isSwipingTouch)
		{
			Vector2 endPos = primary.position.ReadValue();
			Vector2 delta = endPos - swipeStartPosTouch;
			isSwipingTouch = false;

			if (Mathf.Abs(delta.x) > swipeThreshold && Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {
				if (delta.x > 0f) {
					MoveHorizontal(1);
				} else {
					MoveHorizontal(-1);
				}
				emissionActive = true;
				emissionTimer = emissionDuration;
			}
		}
	}
	public void HandleCrash()
	{
		if (hasCrashed)
		{
			return;
		}

		hasCrashed = true;
		Debug.Log("HandleCrash called, game over sequence started!");
		ActivateRagdoll();
	}

	private void ActivateRagdoll()
	{
		Debug.Log("ActivateRagdoll started.");
		// Get the root Rigidbody
		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = gameObject.AddComponent<Rigidbody>();
		}
		rb.isKinematic = false;
		rb.useGravity = true;
		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
		rb.mass = 500f; // Give the car a realistic mass
		rb.linearDamping = 0.5f;        // Low drag since we interpolate velocity programmatically
		rb.angularDamping = 0.5f;

		// Get the root collider and make it solid (non-trigger) so it collides with the ground/cars
		Collider rootCollider = GetComponent<Collider>();
		if (rootCollider != null)
		{
			rootCollider.isTrigger = false;
			Debug.Log("Set root collider isTrigger to false.");
		}

		// Apply an initial velocity change forward and slightly sideways to start the slide
		Vector3 slideVelocity = Vector3.forward * 22f + Vector3.right * Random.Range(-8f, 8f);
		rb.linearVelocity = slideVelocity; // Set velocity directly for precise interpolation
		
		// Apply slight yaw torque to spin the car horizontally on the road
		rb.angularVelocity = Vector3.up * Random.Range(-1.5f, 1.5f);
		Debug.Log(string.Format("Started slide with initial velocity: {0}", slideVelocity));

		// Start coroutine to ease out the slide and freeze the scene completely after exactly 3 seconds
		StartCoroutine(SlideAndEaseOut(rb, 3.0f));
		Debug.Log("StartCoroutine SlideAndEaseOut invoked.");
	}

	private System.Collections.IEnumerator SlideAndEaseOut(Rigidbody rb, float duration)
	{
		Debug.Log(string.Format("SlideAndEaseOut coroutine started for duration: {0}", duration));
		
		if (rb != null)
		{
			float elapsed = 0f;
			Vector3 initialVel = rb.linearVelocity;
			Vector3 initialAngVel = rb.angularVelocity;
			
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = Mathf.Clamp01(elapsed / duration);
				
				// Cubic Ease-Out deceleration curve (starts fast, slows down gradually)
				float ease = 1f - Mathf.Pow(1f - t, 3f);
				
				if (rb != null)
				{
					rb.linearVelocity = Vector3.Lerp(initialVel, Vector3.zero, ease);
					rb.angularVelocity = Vector3.Lerp(initialAngVel, Vector3.zero, ease);
				}
				yield return null;
			}
			
			if (rb != null)
			{
				rb.linearVelocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
				rb.isKinematic = true;
				Debug.Log("Car smoothly brought to zero and Rigidbody set to kinematic.");
			}
		}

		// Set game over to freeze score and tyre updates
		isGameOver = true;
		Debug.Log("Set isGameOver to true.");

		// Stop all ground segments
		var movingBodies = FindObjectsByType<MovingBody>();
		foreach (var mb in movingBodies)
		{
			if (mb != null)
			{
				mb.enabled = false;
				Debug.Log(string.Format("Disabled MovingBody on {0}", mb.gameObject.name));
			}
		}

		Debug.Log("Car slide complete. Returning to Lobby...");
		ShowLobby();
	}

	private void EnsureCollisionComponents()
	{
		Collider collider = GetComponent<Collider>();
		if (collider != null)
		{
			collider.isTrigger = true;
		}

		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.useGravity = false;
			rb.isKinematic = true;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		}
	}

	private void MoveHorizontal(int direction)
	{
		currentStep += direction;
		currentStep = Mathf.Clamp(currentStep, 0, (int)horizontalStepCount);

		float stepSize = (rightXPosition - leftXPosition) / horizontalStepCount;
		float newXPos = leftXPosition + currentStep * stepSize;
		targetPosition = new Vector3(newXPos, transform.position.y, transform.position.z);
		if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
		{
			isMoving = true;
		}
		else
		{
			isMoving = false;
		}
	}

	public void OnPassedCar(GameObject trafficCar)
	{
		if (isGameOver) return;

		passedCarsCount++;
		if (passedCarsCount >= 4)
		{
			passedCarsCount = 0;
			SpawnMessageBox();
		}
	}

	private void SpawnMessageBox()
	{
		if (messageBoxPrefab == null)
		{
			messageBoxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Assets/Prefabs/UI/MessageBox.prefab");
		}

		if (messageBoxPrefab == null)
		{
			Debug.LogWarning("CarController: MessageBox prefab not found!");
			return;
		}

		// Find the active Canvas in the scene
		Canvas canvas = FindAnyObjectByType<Canvas>();
		if (canvas == null) return;

		// Instantiate the MessageBox under the Canvas
		Instantiate(messageBoxPrefab, canvas.transform);
	}

	private void Start()
	{
		ShowLobby();
	}

	public void ShowLobby()
	{
		currentState = GameState.Lobby;
		isGameOver = false;
		hasCrashed = false;
		
		// Reset stats
		distanceTravelled = 0f;
		Score = 0;
		scoreTimer = 0f;

		// Clean up existing traffic cars in the scene
		var trafficCars = FindObjectsByType<CarAIMovement>();
		foreach (var car in trafficCars)
		{
			if (car != null)
			{
				Destroy(car.gameObject);
			}
		}

		// Clean up active message boxes
		var bubbles = FindObjectsByType<MessageBoxBubble>();
		foreach (var bubble in bubbles)
		{
			if (bubble != null)
			{
				Destroy(bubble.gameObject);
			}
		}

		// Disable moving bodies
		var movingBodies = FindObjectsByType<MovingBody>();
		foreach (var mb in movingBodies)
		{
			if (mb != null)
			{
				mb.enabled = false;
			}
		}

		// Show Lobby UI
		Canvas canvas = FindAnyObjectByType<Canvas>();
		if (canvas != null && lobbyScenePrefab != null)
		{
			activeLobbyInstance = Instantiate(lobbyScenePrefab, canvas.transform);
			
			// Animate in using DOTween (elastic scale pop-in)
			Transform panelTrans = activeLobbyInstance.transform.Find("Panel");
			if (panelTrans != null)
			{
				panelTrans.localScale = Vector3.zero;
				panelTrans.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBack).SetUpdate(true);
			}

			// Bind Play button onClick
			var playBtnTrans = activeLobbyInstance.transform.Find("Panel/Layout/Button_Play");
			if (playBtnTrans != null)
			{
				var btn = playBtnTrans.GetComponent<UnityEngine.UI.Button>();
				if (btn != null)
				{
					btn.onClick.AddListener(StartGame);
				}
			}
		}
	}

	public void StartGame()
	{
		if (activeLobbyInstance != null)
		{
			Transform panelTrans = activeLobbyInstance.transform.Find("Panel");
			if (panelTrans != null)
			{
				// Animate out using DOTween (shrinking pop-out)
				panelTrans.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
				{
					if (activeLobbyInstance != null)
					{
						Destroy(activeLobbyInstance);
						activeLobbyInstance = null;
					}
					InitializeGameplay();
				});
			}
			else
			{
				Destroy(activeLobbyInstance);
				activeLobbyInstance = null;
				InitializeGameplay();
			}
		}
		else
		{
			InitializeGameplay();
		}
	}

	private void InitializeGameplay()
	{
		currentState = GameState.Playing;
		isGameOver = false;
		hasCrashed = false;
		this.enabled = true; // Ensure script updates are active

		// Reset stats
		distanceTravelled = 0f;
		Score = 0;
		scoreTimer = 0f;
		passedCarsCount = 0;

		// Reset player car position, rotation and Rigidbody
		transform.position = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
		if (visualChild != null)
		{
			visualChild.localRotation = initialVisualRotation;
		}
		else
		{
			transform.rotation = Quaternion.identity;
		}

		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.isKinematic = true;
			rb.linearVelocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		}

		Collider rootCollider = GetComponent<Collider>();
		if (rootCollider != null)
		{
			rootCollider.isTrigger = true;
		}

		// Enable moving bodies
		var movingBodies = FindObjectsByType<MovingBody>();
		foreach (var mb in movingBodies)
		{
			if (mb != null)
			{
				mb.enabled = true;
			}
		}
		
		// Reset spawner spawn timer
		var spawner = FindAnyObjectByType<CarAISpawner>();
		if (spawner != null)
		{
			spawner.enabled = true;
		}

		Debug.Log("Gameplay initialized successfully!");
	}
}
