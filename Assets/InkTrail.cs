using UnityEngine;

public class InkTrail : MonoBehaviour
{
    public GameObject inkMaskPrefab; // Assign the InkMask prefab
    public float spawnInterval = 0.1f; // Time between mask spawns
    public float decayTime = 10f; // Time to decay the mask
    private float spawnTimer = 0f;
    public float scaleMultiplier = 1.1f; // Factor to make the mask slightly larger than the player (e.g., 1.1 for 10% larger)

    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnInkMask();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnInkMask()
    {
         // Instantiate the ink mask at the player's position
        Vector3 maskPosition = transform.position;
        Quaternion maskRotation = Quaternion.identity;

        GameObject mask = Instantiate(inkMaskPrefab, maskPosition, maskRotation);

        // Adjust the scale based on the player's size and scale multiplier
        float playerScaleX = transform.localScale.x;
        float playerScaleY = transform.localScale.y;

        float maskScaleX = playerScaleX * scaleMultiplier;
        float maskScaleY = playerScaleY * scaleMultiplier;

        mask.transform.localScale = new Vector3(maskScaleX, maskScaleY, 1f);

        // Optional: Parent the mask to keep the hierarchy organized
        mask.transform.parent = null;


        // Destroy the mask after a certain time to prevent buildup
        Destroy(mask, 10f); // Adjust the lifetime as needed
    }
}
