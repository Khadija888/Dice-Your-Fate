using System.Collections;
using UnityEngine;
using TMPro;

public class TableDiceThrow : MonoBehaviour
{
    [Header("Audio Setup")]
    [SerializeField] private AudioSource audioSource;        // AudioSource component on table or character
    [SerializeField] private AudioClip rattleSound;         // Sound played right before the throw
    [SerializeField] private AudioClip creepyResultSound;   // Creepy sound played when camera zooms in on result

    [Header("Progression Barrier & Prompt")]
    [SerializeField] private Collider blockingBarrier;      // Invisible wall blocking the forward path
    [SerializeField] private GameObject barrierMessageUI;   // UI Text/Panel shown when touching the barrier (e.g., "Roll dice to proceed")

    [Header("UI Prompts & Displays")]
    [SerializeField] private GameObject promptUI;           // "[E] Throw Die" prompt at the table
    [SerializeField] private GameObject resultCanvas;       // UI Canvas/Panel for final result
    [SerializeField] private TMP_Text resultText;           // Text display for die result
    [SerializeField] private string targetNumberToShow = "1"; // Rigged result for this table

    [Header("Die Visuals")]
    [SerializeField] private GameObject handDieObject;      // Dummy die in character's hand
    [SerializeField] private GameObject rollingDiePrefab;   // Physics flying die
    [SerializeField] private GameObject landedDieObject;    // Pre-aligned static landed die

    [Header("Transforms & Arc")]
    [SerializeField] private Transform throwSpawnPoint;     // Hand release position
    [SerializeField] private Transform landingSpot;         // Landing spot on table
    [SerializeField] private float throwDuration = 0.5f;    // Physics flight time

    [Header("Separate Cutscene Camera Setup")]
    [SerializeField] private Camera playerCamera;           // Normal gameplay camera
    [SerializeField] private Camera cutsceneCamera;         // Close-up camera
    [SerializeField] private Transform cutsceneStartPoint;  // Anchor behind player head
    [SerializeField] private Transform cutsceneEndPoint;    // Anchor looking at landed die
    [SerializeField] private float moveInDuration = 0.6f;   // Zoom-in glide time
    [SerializeField] private float cutsceneHoldTime = 2.0f; // Pause duration on result
    [SerializeField] private float moveOutDuration = 0.6f;  // Zoom-out glide time

    [Header("Animation")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string throwAnimatorTrigger = "Throw";
    [SerializeField] private float throwDelay = 0.35f;      // Release frame timing

    private bool isPlayerInZone = false;
    private bool hasThrown = false;

    private void Start()
    {
        if (promptUI != null) promptUI.SetActive(false);
        if (resultCanvas != null) resultCanvas.SetActive(false);
        if (barrierMessageUI != null) barrierMessageUI.SetActive(false);
        if (landedDieObject != null) landedDieObject.SetActive(false);
        if (handDieObject != null) handDieObject.SetActive(true);
        if (cutsceneCamera != null) cutsceneCamera.gameObject.SetActive(false);

        // Ensure barrier is active to block player progress initially
        if (blockingBarrier != null) blockingBarrier.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasThrown)
        {
            isPlayerInZone = true;
            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            if (promptUI != null) promptUI.SetActive(false);
        }
    }

    // --- BARRIER COLLIDER DETECTION ---
    private void OnCollisionEnter(Collision collision)
    {
        // Display warning message when player hits the blocking barrier before throwing
        if (!hasThrown && collision.gameObject.CompareTag("Player"))
        {
            if (barrierMessageUI != null) barrierMessageUI.SetActive(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Hide warning message when player steps away from the barrier
        if (collision.gameObject.CompareTag("Player"))
        {
            if (barrierMessageUI != null) barrierMessageUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerInZone && !hasThrown && Input.GetKeyDown(KeyCode.E))
        {
            if (promptUI != null) promptUI.SetActive(false);
            if (barrierMessageUI != null) barrierMessageUI.SetActive(false);
            StartCoroutine(ExecuteTableThrowRoutine());
        }
    }

    private IEnumerator ExecuteTableThrowRoutine()
    {
        hasThrown = true;

        // Play rattle sound before throw
        if (audioSource != null && rattleSound != null)
        {
            audioSource.PlayOneShot(rattleSound);
        }

        // 1. Trigger animation
        if (characterAnimator != null && !string.IsNullOrEmpty(throwAnimatorTrigger))
        {
            characterAnimator.SetTrigger(throwAnimatorTrigger);
        }

        // 2. Release timing
        yield return new WaitForSeconds(throwDelay);
        if (handDieObject != null) handDieObject.SetActive(false);

        // 3. Spawn physics die arc
        Transform spawnPos = (throwSpawnPoint != null) ? throwSpawnPoint : transform;
        GameObject flyingDie = Instantiate(rollingDiePrefab, spawnPos.position, Random.rotation);

        Rigidbody rb = flyingDie.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;

            Vector3 startPos = spawnPos.position;
            Vector3 endPos = (landingSpot != null) ? landingSpot.position : startPos + (transform.forward * 1.5f);

            float gravity = Physics.gravity.y;
            Vector3 displacement = endPos - startPos;
            Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);

            Vector3 velocityXZ = displacementXZ / throwDuration;
            float velocityY = (displacement.y - (0.5f * gravity * throwDuration * throwDuration)) / throwDuration;

            rb.linearVelocity = velocityXZ + (Vector3.up * velocityY);
            rb.AddTorque(Random.insideUnitSphere * 150f, ForceMode.Impulse);
        }

        // 4. Wait for landing frame
        yield return new WaitForSeconds(throwDuration - 0.05f);

        // 5. Swap physics die with pre-aligned landed die
        if (flyingDie != null) Destroy(flyingDie);
        if (landedDieObject != null) landedDieObject.SetActive(true);

        // 6. Camera cutscene sequence
        if (playerCamera != null && cutsceneCamera != null && cutsceneStartPoint != null && cutsceneEndPoint != null)
        {
            playerCamera.gameObject.SetActive(false);
            cutsceneCamera.gameObject.SetActive(true);

            cutsceneCamera.transform.position = cutsceneStartPoint.position;
            cutsceneCamera.transform.rotation = cutsceneStartPoint.rotation;

            // Zoom in
            float elapsed = 0f;
            while (elapsed < moveInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveInDuration;
                
                cutsceneCamera.transform.position = Vector3.Lerp(cutsceneStartPoint.position, cutsceneEndPoint.position, t);
                cutsceneCamera.transform.rotation = Quaternion.Slerp(cutsceneStartPoint.rotation, cutsceneEndPoint.rotation, t);
                yield return null;
            }

            cutsceneCamera.transform.position = cutsceneEndPoint.position;
            cutsceneCamera.transform.rotation = cutsceneEndPoint.rotation;

            // Play creepy sting sound when camera lands on result
            if (audioSource != null && creepyResultSound != null)
            {
                audioSource.PlayOneShot(creepyResultSound);
            }

            // Display UI Canvas result
            if (resultText != null) resultText.text = targetNumberToShow;
            if (resultCanvas != null) resultCanvas.SetActive(true);

            // Stare at result for 2 seconds
            yield return new WaitForSeconds(cutsceneHoldTime);

            // Hide UI Canvas
            if (resultCanvas != null) resultCanvas.SetActive(false);

            // Zoom out
            elapsed = 0f;
            while (elapsed < moveOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveOutDuration;

                cutsceneCamera.transform.position = Vector3.Lerp(cutsceneEndPoint.position, cutsceneStartPoint.position, t);
                cutsceneCamera.transform.rotation = Quaternion.Slerp(cutsceneEndPoint.rotation, cutsceneStartPoint.rotation, t);
                yield return null;
            }

            // Restore player control
            cutsceneCamera.gameObject.SetActive(false);
            playerCamera.gameObject.SetActive(true);
        }

        // 7. OPEN BARRIER: Disable collider and message so player can move forward
        if (barrierMessageUI != null) barrierMessageUI.SetActive(false);
        if (blockingBarrier != null) blockingBarrier.enabled = false;

        Debug.Log("Dice throw complete. Barrier opened and message cleared.");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
{
    // Check if the player ran into the blocking barrier before throwing
    if (!hasThrown && blockingBarrier != null && hit.collider == blockingBarrier)
    {
        if (barrierMessageUI != null && !barrierMessageUI.activeSelf)
        {
            barrierMessageUI.SetActive(true);
        }
    }
}
}