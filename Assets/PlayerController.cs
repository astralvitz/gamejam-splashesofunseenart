using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxInk = 100f;             // Maximum amount of ink
    public float minInkPercentage = 0.05f;  // Ink percentage at which the player runs out (5%)
    public float inkConsumptionRate = 1f;  // Rate at which ink is consumed per unit distance
    public float refillAmount = 50f;        // Amount of ink refilled at refill points

    public float moveSpeed = 5f;            // Speed factor for movement towards the target
    public float steeringSpeed = 5f;        // Speed at which the ink drop turns towards the target
    public float maxVelocity = 10f;         // Maximum velocity of the ink drop

    private float currentInk;               // Current ink level
    private Vector3 originalScale;          // Original scale of the ink drop
    private bool isDragging = false;        // Is the player currently dragging the ink drop
    private Vector2 targetPosition;         // Target position the ink drop moves towards
    private Camera mainCamera;
    private Rigidbody2D rb;
    public PolygonCollider2D boundaryCollider;

    void Start()
    {
        currentInk = maxInk;
        originalScale = transform.localScale;
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        if (HasInk())
        {
            MoveTowardsTarget();
            ConsumeInk();
            ConfineToBoundary();
        }
        else
        {
            rb.velocity = Vector2.zero; // Stop movement when out of ink
        }
    }

    void HandleInput()
    {
        #if UNITY_ANDROID || UNITY_IOS
        // Mobile input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPos = mainCamera.ScreenToWorldPoint(touch.position);
            touchPos.z = 0f;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (IsTouchingPlayer(touchPos))
                    {
                        isDragging = true;
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        targetPosition = touchPos;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }
        #else
        // Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            if (IsTouchingPlayer(mousePos))
            {
                isDragging = true;
            }
        }
        if (Input.GetMouseButton(0))
        {
            if (isDragging)
            {
                Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f;
                targetPosition = mousePos;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        #endif
    }

    bool IsTouchingPlayer(Vector3 position)
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        return collider.OverlapPoint(position);
    }

    void MoveTowardsTarget()
    {
        if (isDragging)
        {
            // Calculate the direction to the target
            Vector2 direction = ((Vector2)targetPosition - rb.position).normalized;

            // Calculate the steering force
            Vector2 desiredVelocity = direction * moveSpeed;
            Vector2 steering = desiredVelocity - rb.velocity;

            // Limit the steering force
            steering = Vector2.ClampMagnitude(steering, steeringSpeed);

            // Apply the steering force
            rb.AddForce(steering, ForceMode2D.Force);

            // Limit the maximum velocity
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);

            // Rotate the ink drop to face the movement direction
            if (rb.velocity.sqrMagnitude > 0.1f)
            {
                float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f;
                rb.rotation = angle;
            }
        }
        else
        {
            // Slow down gradually when not dragging
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.fixedDeltaTime * steeringSpeed);
        }
    }

    void ConsumeInk()
    {
        // Consume ink based on the distance moved since the last frame
        float distanceMoved = rb.velocity.magnitude * Time.fixedDeltaTime;
        float inkUsed = distanceMoved * inkConsumptionRate;
        currentInk -= inkUsed;

        // Clamp current ink to minimum value
        currentInk = Mathf.Max(currentInk, 0f);

        // Update the scale of the ink drop
        UpdateInkDropScale();
    }

    void UpdateInkDropScale()
    {
        // Calculate the scale factor based on current ink percentage
        float inkPercentage = currentInk / maxInk;
        inkPercentage = Mathf.Max(inkPercentage, minInkPercentage);

        // Apply the new scale
        transform.localScale = originalScale * inkPercentage;
    }

    bool HasInk()
    {
        return currentInk > maxInk * minInkPercentage;
    }

    public void RefillInk()
    {
        currentInk += refillAmount;
        currentInk = Mathf.Min(currentInk, maxInk);
        UpdateInkDropScale();
    }

    void ConfineToBoundary()
    {
        if (!IsInsideBoundary(rb.position))
        {
            // The ink drop is outside the boundary; adjust its position
            Vector2 closestPoint = boundaryCollider.ClosestPoint(rb.position);
            rb.position = closestPoint;
            rb.velocity = Vector2.zero; // Stop movement to prevent sliding along the edge
        }
    }

    bool IsInsideBoundary(Vector2 position)
    {
        return boundaryCollider.OverlapPoint(position);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Refill"))
        {
            RefillInk();
            // Optionally, provide feedback like a sound or animation
        }
    }
}
