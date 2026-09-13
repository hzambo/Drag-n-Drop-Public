using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ButtonFollowVisual : MonoBehaviour
{
    [Header("Button Visual")]
    public Transform visualTarget;

    [Header("Movement")]
    [Tooltip("Direction the button moves when pressed. Use (0, -1, 0) for downward.")]
    public Vector3 localAxis = Vector3.down;

    [Tooltip("Maximum distance the button visual can move.")]
    public float maxPressDistance = 0.02f;

    [Tooltip("How quickly the button returns to its starting position.")]
    public float resetSpeed = 5f;

    private XRBaseInteractable interactable;

    private Transform pokeAttachTransform;
    private Transform buttonSpace;

    private Vector3 initialLocalPos;
    private Vector3 initialPokeLocalPos;

    private bool isFollowing = false;
    private bool freeze = false;

    private void Start()
    {
        // Use the visual's parent as a fixed coordinate space.
        buttonSpace = visualTarget.parent;

        // Remember where the button starts.
        initialLocalPos = visualTarget.localPosition;

        // Get the XR interactable on this object.
        interactable = GetComponent<XRBaseInteractable>();

        // Listen for XR interaction events.
        interactable.hoverEntered.AddListener(Follow);
        interactable.hoverExited.AddListener(Reset);
        interactable.selectEntered.AddListener(Freeze);
    }

    private void OnDestroy()
    {
        if (interactable == null)
            return;

        interactable.hoverEntered.RemoveListener(Follow);
        interactable.hoverExited.RemoveListener(Reset);
        interactable.selectEntered.RemoveListener(Freeze);
    }

    public void Follow(BaseInteractionEventArgs args)
    {
        if (args.interactorObject is XRPokeInteractor pokeInteractor)
        {
            pokeAttachTransform = pokeInteractor.attachTransform;

            // Store the finger position when hover begins.
            initialPokeLocalPos =
                buttonSpace.InverseTransformPoint(
                    pokeAttachTransform.position);

            isFollowing = true;
            freeze = false;
        }
    }

    public void Reset(BaseInteractionEventArgs args)
    {
        if (args.interactorObject is XRPokeInteractor)
        {
            isFollowing = false;
            freeze = false;
            pokeAttachTransform = null;
        }
    }

    public void Freeze(BaseInteractionEventArgs args)
    {
        if (args.interactorObject is XRPokeInteractor)
        {
            // Stop moving once the poke becomes a Select.
            freeze = true;
        }
    }

    private void Update()
    {
        // If nothing is touching the button,
        // smoothly return it to the starting position.
        if (!isFollowing || pokeAttachTransform == null)
        {
            visualTarget.localPosition =
                Vector3.Lerp(
                    visualTarget.localPosition,
                    initialLocalPos,
                    Time.deltaTime * resetSpeed);

            return;
        }

        // If the button has already triggered,
        // leave the visual where it currently is.
        if (freeze)
            return;

        Vector3 axis = localAxis.normalized;

        // Current finger position in the button parent's coordinate space.
        Vector3 currentPokeLocalPos =
            buttonSpace.InverseTransformPoint(
                pokeAttachTransform.position);

        // How far the finger moved since hover started.
        Vector3 fingerMovement =
            currentPokeLocalPos - initialPokeLocalPos;

        // Get only the amount of movement along our allowed axis.
        float pressAmount =
            Vector3.Dot(
                fingerMovement,
                axis);

        // Prevent movement in the opposite direction,
        // and prevent the button from going too far.
        pressAmount =
            Mathf.Clamp(
                pressAmount,
                0f,
                maxPressDistance);

        // Move only along the press axis.
        visualTarget.localPosition =
            initialLocalPos + axis * pressAmount;
    }
}