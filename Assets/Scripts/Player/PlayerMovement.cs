using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Assets
    public AudioSource footstepAudioSource;
    
    [System.Serializable]
    public class FootstepClips {
        public AudioClip[] footstepTileClips;
        public AudioClip[] footstepConcreteClips;
        // etc.
    }

    public FootstepClips footstepClips;

    // References
    PlayerManager playerManager;
    public CharacterController characterController;
    public AudioSource exhaustedAudioSource;

    // Flags
    bool isMoving = false;
    public bool canMove = true;
    bool canSprint = true;
    public bool sprinting = false;
    public bool exhausted = false;
    public bool crippled = false;
    bool sneaking = false;

    // Static
    float movementSpeed = 3.0f;
    float footstepInterval = 0.66f;
    float exhaustedTimer = 8.0f;
    
    // Variables
    float velocity;
    float gravity = 0.981f;
    public float stamina;
    float currentMovementSpeed = 3.0f;
    float movementSpeedMultiplier = 1.0f;
    float sprintMultiplier = 2.0f;
    float currentFootstepInterval = 0.66f;
    float footstepTimer = 0.0f;

    void Awake() {
        characterController = GetComponent<CharacterController>();
        exhaustedAudioSource.volume = 0;
    }

    void Update() {
        if (canMove) {
            // Regular movement
            float horizontal = Input.GetAxis("Horizontal") * currentMovementSpeed;
            float vertical = Input.GetAxis("Vertical") * currentMovementSpeed;
        
            characterController.Move((characterController.transform.right * horizontal + characterController.transform.forward * vertical) * Time.deltaTime);

            if (horizontal != 0 || vertical != 0) {
                isMoving = true;
            }
            else {
                isMoving = false;
            }

            // Sprinting
            if (Input.GetButton("Sprint") && canSprint && isMoving && !exhausted) {
                sprinting = true;
            }
            else {
                sprinting = false;
            }

            if (sprinting) {
                currentMovementSpeed = movementSpeed * sprintMultiplier;
            }
            else {
                currentMovementSpeed = movementSpeed * movementSpeedMultiplier;
            }
            
            if (stamina <= 0) {
                exhausted = true;
                exhaustedTimer = 8.0f;
                canSprint = false;
            }
            else {
                canSprint = true;
            }

            if (exhausted) {
                exhaustedTimer -= 1.0f * Time.deltaTime;

                if (!exhaustedAudioSource.isPlaying) {
                    exhaustedAudioSource.Play();
                }

                exhaustedAudioSource.volume = .8f * (exhaustedTimer / 8.0f);

                if (exhaustedTimer <= 0) {
                    if (exhaustedAudioSource.isPlaying) {
                        exhaustedAudioSource.Stop();
                    }
                    
                    exhausted = false;
                    exhaustedTimer = 0;
                }
            }

            // Crippled
            if (crippled) {
                canSprint = false;
                movementSpeedMultiplier = 0.5f;
            }
            else {
                canSprint = true;
                movementSpeedMultiplier = 1.0f;
            }

            // Footsteps
            if (isMoving) {
                footstepTimer -= 1.0f * Time.deltaTime;

                if (footstepTimer <= 0) {
                    if (sprinting) {
                        footstepTimer = footstepInterval / sprintMultiplier;
                    }
                    else {
                        footstepTimer = footstepInterval / movementSpeedMultiplier;
                    }

                    footstepAudioSource.clip = footstepClips.footstepTileClips[Random.Range(0, footstepClips.footstepTileClips.Length)];
                    footstepAudioSource.Play();
                }
            }

            // Gravity
            if (characterController.isGrounded) {
                velocity = 0;
            }
            else {
                velocity -= gravity * Time.deltaTime;
                characterController.Move(new Vector3(0, velocity, 0));
            }
        }
    }

    public void DisableSprint() {
        sprinting = false;
    }
}
