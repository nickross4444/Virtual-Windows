using System.Collections;
using UnityEngine;
using Oculus.Interaction.HandGrab;
using System.Collections.Generic;
using Oculus.Interaction;
using System.Linq;

// Manages the summoning and desummoning of objects in VR using hand gestures
public class InteractionManager : MonoBehaviour
{
    // Add singleton instance
    public static InteractionManager Instance { get; private set; }
    [Header("Managers")]
    [SerializeField] GameObject customCutoutManager;

    [Header("Summonable Objects")]
    [SerializeField] private GameObject[] summonableObjects;  // Objects that can be summoned
    private List<GameObject> activeSummonableObjects = new List<GameObject>();  // Currently available objects
    private Dictionary<GameObject, GameObject> jiggleTargets = new Dictionary<GameObject, GameObject>();  // Tracks jiggle follow targets

    [Header("Configuration")]
    [SerializeField] private float summonAngleThreshold = 30f;  // Palm angle to trigger summoning
    [SerializeField] private float desummonAngleThreshold = 45f;  // Palm angle to trigger desummoning
    [SerializeField] private float summonHeightOffset = 0.1f;  // Vertical offset for summoned objects
    [SerializeField] private float summonAnimationDuration = 0.5f;  // Duration of summon animation
    [SerializeField] private float desummonAnimationDuration = 0.3f;  // Duration of desummon animation
    [SerializeField] private float initialScaleMultiplier = 0.1f;  // Initial scale when objects appear
    [SerializeField] private float summonableObjectSpacing = 0.1f;  // Horizontal spacing between objects


    private HandGrabInteractor[] handGrabInteractors;  // Available hand interactors
    private OVRHand[] OVRHands;
    private Transform playerTransform;  // Reference to player camera
    private HandGrabInteractor summoningHand;  // Currently active summoning hand
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();  // Original object scales
    private Dictionary<GameObject, Coroutine> summonCoroutines = new Dictionary<GameObject, Coroutine>();  // Active animations
    private List<GameObject> selectedObjects = new List<GameObject>();

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Initialize components and store original object scales
    private void Start()
    {
        // Find all hand interactors in the scene
        handGrabInteractors = FindObjectsByType<HandGrabInteractor>(FindObjectsSortMode.None);
        OVRHands = FindObjectsByType<OVRHand>(FindObjectsSortMode.None);
        playerTransform = GameObject.FindGameObjectWithTag("MainCamera")?.transform;

        // Store original scales and setup interaction events for each summonable object
        foreach (GameObject obj in summonableObjects)
        {
            originalScales[obj.transform] = obj.transform.localScale;
            obj.SetActive(false);
            HandGrabInteractable interactable = obj.GetComponentInChildren<HandGrabInteractable>();
            if (interactable != null)
            {
                // Subscribe to state changes to handle when object is grabbed
                interactable.WhenStateChanged += (args) => OnObjectStateChanged(args, obj);
                // could alternatively use: handGrabInteractable.WhenSelectingInteractorAdded.Action +=
            }
            activeSummonableObjects.Add(obj);
        }
    }

    // Check hand gestures each frame for summon/desummon triggers
    private void Update()
    {
        foreach (HandGrabInteractor hand in handGrabInteractors)
        {
            if (hand?.PalmPoint == null) continue;
            // Calculate angle between palm's down direction and world up
            // This determines if palm is facing up (summon) or down (desummon)
            float angleToUp = Vector3.Angle(hand.PalmPoint.rotation * Vector3.down, Vector3.up);
            HandleHandGesture(hand, angleToUp);
        }
    }

    // Process hand gesture angles and trigger appropriate sequences
    private void HandleHandGesture(HandGrabInteractor hand, float angleToUp)
    {
        // Start summoning if palm faces up (small angle) and no active summon
        if (!summoningHand && angleToUp < summonAngleThreshold)
        {
            StartSummonSequence(hand);
            customCutoutManager.SetActive(false);
        }
        // Desummon if palm faces down (large angle) and this hand is currently summoning
        else if (summoningHand == hand && angleToUp > desummonAngleThreshold)
        {
            StartDesummonSequence();
            if(selectedObjects.Count > 0) {
                ActivateCustomCutout(selectedObjects[0]);
            }
        }
    }

    // Handle state changes when objects are grabbed
    private void OnObjectStateChanged(InteractableStateChangeArgs args, GameObject obj)
    {
        // When object is grabbed, remove from active list and restore original scale
        if (args.NewState == InteractableState.Select)
        {
            selectedObjects.Add(obj);
            activeSummonableObjects.Remove(obj);
            if (summonCoroutines.ContainsKey(obj))
            {
                StopCoroutine(summonCoroutines[obj]);
            }
            obj.transform.localScale = originalScales[obj.transform];

            // When grabbed, remove the jiggle target
            if (jiggleTargets.ContainsKey(obj))
            {
                var jiggleFollow = obj.GetComponent<JiggleFollow>();
                if (jiggleFollow != null)
                {
                    jiggleFollow.SetTarget(null);
                }
                Destroy(jiggleTargets[obj]);
                jiggleTargets.Remove(obj);
            }
            // Change to non-trigger collider
            obj.GetComponentInChildren<Collider>().isTrigger = false;

            // Manage custom cutout if not summoning
            if(!summoningHand) {
                ActivateCustomCutout(obj);
            }
        } else  {
            if(selectedObjects.Contains(obj)) {    //deactivate if a the held object is dropped
                customCutoutManager.SetActive(false);
            }
            selectedObjects.Remove(obj);
        }
    }
    void ActivateCustomCutout(GameObject obj) {
        SummonableObject summonableObject = obj.GetComponent<SummonableObject>();
        if(summonableObject)
        {
            customCutoutManager.SetActive(true);
            OVRHand furthestHand = OVRHands.OrderByDescending(d => Vector3.Distance(d.transform.position, obj.transform.position)).FirstOrDefault();
            customCutoutManager.GetComponent<HandPointer>().pointingHand = furthestHand;
            Material stencilMaterial = summonableObject.stencilMaterial;
            HandPinchDetector handPinchDetector = customCutoutManager.GetComponent<HandPinchDetector>();
            handPinchDetector.stencilMaterial = stencilMaterial;
            handPinchDetector.activeEnvironmentType = summonableObject.environmentType;
        }
    }

    // Begin the summoning sequence, positioning objects in a horizontal line
    private void StartSummonSequence(HandGrabInteractor hand)
    {
        summoningHand = hand;
        // Get direction from hand to player (ignoring height)
        Vector3 toPlayer = GetFlatDirectionToPlayer(hand.PalmPoint.position);
        // Create horizontal line perpendicular to player direction
        Vector3 rightVector = Vector3.Cross(Vector3.up, toPlayer).normalized;

        // Center the objects horizontally relative to hand
        float totalWidth = (summonableObjects.Length - 1) * summonableObjectSpacing;
        float startOffset = -totalWidth / 2f;

        for (int i = 0; i < summonableObjects.Length; i++)
        {
            GameObject obj = summonableObjects[i];
            if (!activeSummonableObjects.Contains(obj)) continue;

            obj.SetActive(true);
            Vector3 startPos = hand.PalmPoint.position;
            startPos.y += summonHeightOffset;
            // Start with smaller scale for pop-in effect
            obj.transform.localScale = originalScales[obj.transform] * initialScaleMultiplier;
            obj.transform.position = startPos;
            // Create jiggle target
            GameObject jiggleTarget = new GameObject($"{obj.name}_JiggleTarget");
            jiggleTarget.transform.position = startPos;
            jiggleTargets[obj] = jiggleTarget;

            // Set up jiggle follow
            var jiggleFollow = obj.GetComponent<JiggleFollow>();
            if (jiggleFollow == null)
            {
                jiggleFollow = obj.AddComponent<JiggleFollow>();
            }
            jiggleFollow.SetTarget(jiggleTarget.transform);

            // Position each object along the horizontal line with vertical offset
            float horizontalOffset = startOffset + (i * summonableObjectSpacing);
            Vector3 targetOffset = (Vector3.up * summonHeightOffset) + (rightVector * horizontalOffset);

            summonCoroutines[obj] = StartCoroutine(AnimateSummon(obj.transform, startPos, hand.PalmPoint, targetOffset, toPlayer, jiggleTarget.transform));
        }
    }

    // Begin the desummoning sequence for all active objects
    private void StartDesummonSequence()
    {
        if (!summoningHand) return;

        // Animate each active object back to the palm position
        foreach (GameObject obj in activeSummonableObjects)
        {
            Vector3 toPlayer = GetFlatDirectionToPlayer(summoningHand.PalmPoint.position);
            summonCoroutines[obj] = StartCoroutine(AnimateDesummon(obj.transform, summoningHand.PalmPoint, toPlayer));
        }
        summoningHand = null;
    }

    // Calculate direction from a position to the player, ignoring vertical difference
    private Vector3 GetFlatDirectionToPlayer(Vector3 fromPosition)
    {
        Vector3 direction = playerTransform.position - fromPosition;
        direction.y = 0;  // Ignore height difference
        return direction.normalized;
    }

    // Animate object appearance and maintain position relative to palm
    private IEnumerator AnimateSummon(Transform objTransform, Vector3 startPos, Transform palmPoint, Vector3 targetOffset, Vector3 toPlayer, Transform jiggleTarget)
    {
        // Calculate object's position in the line for consistent spacing
        int objectIndex = System.Array.IndexOf(summonableObjects, objTransform.gameObject);
        float totalWidth = (summonableObjects.Length - 1) * summonableObjectSpacing;
        float startOffset = -totalWidth / 2f;
        float horizontalOffset = startOffset + (objectIndex * summonableObjectSpacing);

        // Initial animation: scale up and move to position
        float elapsed = 0;
        Vector3 originalScale = originalScales[objTransform];
        // Start facing away from player, end facing towards player
        Quaternion startRotation = Quaternion.LookRotation(-toPlayer, Vector3.up);
        Quaternion endRotation = Quaternion.LookRotation(toPlayer, Vector3.up);

        // Animate position, rotation, and scale
        while (elapsed < summonAnimationDuration)
        {
            float t = elapsed / summonAnimationDuration;
            Vector3 currentOffset = Vector3.Lerp(Vector3.zero, targetOffset, t);
            if (jiggleTarget)
            {
                jiggleTarget.position = palmPoint.position + currentOffset;
                jiggleTarget.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            }
            objTransform.localScale = Vector3.Lerp(originalScale * initialScaleMultiplier, originalScale, t);

            elapsed += Time.deltaTime;
            yield return null;
        }
        objTransform.localScale = originalScale;

        // Continuous update: follow hand position while maintaining formation
        while (summoningHand != null)
        {
            if (!activeSummonableObjects.Contains(objTransform.gameObject))
            {
                yield break;
            }
            // Recalculate positions based on current hand position
            Vector3 currentToPlayer = GetFlatDirectionToPlayer(palmPoint.position);
            Vector3 currentRightVector = Vector3.Cross(Vector3.up, currentToPlayer).normalized;

            Vector3 horizontalPosition = currentRightVector * horizontalOffset;
            Vector3 verticalPosition = Vector3.up * summonHeightOffset;

            // Update jiggle target position and rotation to follow hand
            if (jiggleTarget)
            {
                jiggleTarget.position = palmPoint.position + horizontalPosition + verticalPosition;
                jiggleTarget.rotation = Quaternion.LookRotation(currentToPlayer, Vector3.up);
            }

            yield return null;
        }
    }

    // Animate object disappearance
    private IEnumerator AnimateDesummon(Transform objTransform, Transform palmPoint, Vector3 toPlayer)
    {
        float elapsed = 0;
        Vector3 startPos = objTransform.position;
        Vector3 originalScale = originalScales[objTransform];
        Quaternion startRotation = objTransform.rotation;
        // Rotate to face away from player while disappearing
        Quaternion endRotation = Quaternion.LookRotation(-toPlayer, Vector3.up);

        // Get jiggle target if it exists
        GameObject jiggleTarget = null;
        if (jiggleTargets.TryGetValue(objTransform.gameObject, out jiggleTarget))
        {
            JiggleFollow jiggleFollow = objTransform.GetComponent<JiggleFollow>();
            if (jiggleFollow)
            {
                jiggleFollow.SetTarget(null);
            }
        }

        // Animate back to palm position while scaling down
        while (elapsed < desummonAnimationDuration)
        {
            float t = elapsed / desummonAnimationDuration;

            if (jiggleTarget)
            {
                jiggleTarget.transform.position = Vector3.Lerp(startPos, palmPoint.position, t);
                jiggleTarget.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            }
            objTransform.localScale = Vector3.Lerp(originalScale, originalScale * initialScaleMultiplier, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Clean up jiggle target
        if (jiggleTarget)
        {
            Destroy(jiggleTarget);
            jiggleTargets.Remove(objTransform.gameObject);
        }

        objTransform.gameObject.SetActive(false);
    }

    public void PlaceStencil(GameObject obj, EnvironmentSpawner.EnvironmentType environmentType, Material stencilMaterial)
    {
        // TODO: Add a transition here!!
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        if (meshRenderer)
        {
            meshRenderer.material = stencilMaterial;
            obj.layer = LayerMask.NameToLayer("Default");       //allow the stencil to render normally(instead of wall layer passthrough)
        }

        EnvironmentSpawner.Instance.SpawnEnvironment(environmentType, obj.transform);
    }
}
