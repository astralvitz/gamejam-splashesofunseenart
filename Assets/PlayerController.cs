using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxInk = 100f;          // Maximum amount of ink
    public float minInkPercentage = 0.05f; // Ink percentage at which the player runs out (5%)
    public float inkConsumptionRate = 10f; // Rate at which ink is consumed per unit distance
    public float refillAmount = 50f;       // Amount of ink refilled at refill points

    private float currentInk;            // Current ink level
    private Vector3 originalScale;       // Original scale of the ink drop
    private bool isDragging = false;     // Is the player currently dragging the ink drop
    private Vector3 dragOffset;          // Offset between touch point and ink drop position
    private Camera mainCamera;

    void Start()
    {
        currentInk = maxInk;
        originalScale = transform.localScale;
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        if (isDragging)
        {
            ConsumeInk();
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
                        dragOffset = transform.position - touchPos;
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging && HasInk())
                    {
                        transform.position = touchPos + dragOffset;
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
                dragOffset = transform.position - mousePos;
            }
        }
        if (Input.GetMouseButton(0))
        {
            if (isDragging && HasInk())
            {
                Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f;
                transform.position = mousePos + dragOffset;
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

    void ConsumeInk()
    {
        // Calculate the distance moved since the last frame
        float distanceMoved = (transform.position - (Vector3)dragOffset).magnitude;

        // Consume ink based on distance moved
        float inkUsed = distanceMoved * inkConsumptionRate;
        currentInk -= inkUsed;

        // Clamp current ink to minimum value
        currentInk = Mathf.Max(currentInk, 0f);

        // Update the scale of the ink drop
        UpdateInkDropScale();

        // Check if ink has run out
        if (!HasInk())
        {
            isDragging = false;
            // Optionally, trigger an event or feedback to the player
        }
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Refill"))
        {
            RefillInk();
            // Optionally, provide feedback like a sound or animation
        }
    }
}
