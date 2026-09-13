using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class XRJumpProvider : LocomotionProvider
{
    [Header("Input")]
    [SerializeField]
    private InputActionReference jumpAction;

    [Header("Jump")]
    [SerializeField]
    private float jumpHeight = 1.25f;

    [SerializeField]
    private float gravity = -9.81f;

    [Header("Grounding")]
    [SerializeField]
    private float groundedVelocity = -2f;

    [SerializeField]
    private CharacterController cc;

    private float verticalVelocity;

    // IMPORTANT:
    // We store our own grounded state instead of constantly
    // reading CharacterController.isGrounded.
    private bool isGrounded;

    private bool jumpPressed;
    private bool isJumping;


    private void Start()
    {
        // This is safe for our initial state.
        isGrounded = cc.isGrounded;
    }


    private void OnEnable()
    {
        if (jumpAction != null)
            jumpAction.action.performed += OnJumpPerformed;
    }


    private void OnDisable()
    {
        if (jumpAction != null)
            jumpAction.action.performed -= OnJumpPerformed;
    }


    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }


    private void Update()
    {
        // ---------------------------------------------
        // JUMP
        // ---------------------------------------------

        if (jumpPressed && isGrounded && !isJumping)
        {
            if (TryStartLocomotionImmediately())
            {
                // v = sqrt(2gh)
                verticalVelocity =
                    Mathf.Sqrt(jumpHeight * -2f * gravity);

                isJumping = true;
                isGrounded = false;
            }
        }

        jumpPressed = false;


        // ---------------------------------------------
        // GRAVITY
        // ---------------------------------------------

        if (isGrounded && verticalVelocity < 0f)
        {
            // Small downward force keeps the Character Controller
            // in contact with slopes/floors.
            verticalVelocity = groundedVelocity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }


        // ---------------------------------------------
        // VERTICAL MOVEMENT
        // ---------------------------------------------

        Vector3 verticalMovement =
            Vector3.up * verticalVelocity * Time.deltaTime;

        // VERY IMPORTANT:
        // Use the result of THIS Move instead of cc.isGrounded.
        CollisionFlags collisionFlags =
            cc.Move(verticalMovement);


        // ---------------------------------------------
        // GROUND CHECK
        // ---------------------------------------------

        bool hitGround =
            (collisionFlags & CollisionFlags.Below) != 0;

        bool hitCeiling =
            (collisionFlags & CollisionFlags.Above) != 0;


        // Hit ceiling while jumping.
        if (hitCeiling && verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }


        // Landed.
        if (hitGround && verticalVelocity <= 0f)
        {
            isGrounded = true;
            verticalVelocity = groundedVelocity;

            if (isJumping)
            {
                isJumping = false;
                TryEndLocomotion();
            }
        }
        else
        {
            isGrounded = false;
        }
    }
}