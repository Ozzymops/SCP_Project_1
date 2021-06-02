using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerManager : MonoBehaviour
{
    // Assets
    public AudioSource musicAudioSource;
    public AudioSource horrorAudioSource;
    public AudioSource snapAudioSource;
    public AudioClip[] horrorClips;

    // References
    PlayerMovement playerMovement;
    public PlayerCamera playerCamera;
    PlayerStatus playerStatus;
    PlayerUI playerUI;
    public PostProcessVolume playerPostProcessing;

    // Flags
    public bool isDead = false;
    bool canHorror = true;

    // Shared variables
    public float health;
    public float maxHealth;
    public float stamina;
    public float maxStamina;
    public float blink;
    public float maxBlink;

    // Specifics
    float distance173;

    void Awake() {
        playerMovement = GetComponent<PlayerMovement>();
        playerCamera = GetComponent<PlayerCamera>();
        playerStatus = GetComponent<PlayerStatus>();
        playerUI = GetComponent<PlayerUI>();

        if (playerPostProcessing.profile.TryGetSettings<Vignette>(out var vignette)) {
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.0f;
        }

        if (playerPostProcessing.profile.TryGetSettings<ColorGrading>(out var colorGrading)) {
            colorGrading.hueShift.overrideState = true;
            colorGrading.saturation.overrideState = true;
            colorGrading.hueShift.value = 0.0f;
            colorGrading.saturation.value = 0.0f;
        }

        if (playerPostProcessing.profile.TryGetSettings<DepthOfField>(out var dof)) {
            dof.focusDistance.overrideState = true;
        }

        if (playerPostProcessing.profile.TryGetSettings<LensDistortion>(out var lensDistortion)) {
            lensDistortion.intensity.overrideState = true;
        }
    }

    void Update() {
        // Get shared variables
        health = playerStatus.health;
        maxHealth = playerStatus.maxHealth;
        stamina = playerStatus.stamina;
        maxStamina = playerStatus.maxStamina;
        blink = playerStatus.blink;
        maxBlink = playerStatus.maxBlink;

        // Set shared variables
        playerMovement.stamina = stamina;
        playerMovement.crippled = playerStatus.crippled;
        playerMovement.canMove = !playerStatus.dead;
        playerCamera.canLook = !playerStatus.dead;
        playerCamera.health = playerStatus.health;
        playerCamera.maxHealth = playerStatus.maxHealth;
        playerUI.health = health;
        playerUI.maxHealth = maxHealth;
        playerUI.stamina = stamina;
        playerUI.maxStamina = maxStamina;
        playerUI.blink = blink;
        playerUI.maxBlink = maxBlink;
        playerUI.exhausted = playerMovement.exhausted;
        playerUI.bleeding = playerStatus.bleeding;
        playerUI.crippled = playerStatus.crippled;
        playerUI.dead = playerStatus.dead;

        // Set player rotation (Camera -> Movement)
        this.transform.localEulerAngles = new Vector3(0, playerCamera.rotationY, 0);

        // Sync sprinting status (Movement -> Status)
        playerStatus.sprinting = playerMovement.sprinting;

        // Blinking
        distance173 = Vector3.Distance(GameObject.FindGameObjectWithTag("173").transform.position, transform.position);

        if (playerStatus.canBlink) {
            if (playerStatus.blinking) {
                playerUI.CloseEyes(0.4f);

                canHorror = true;
            }
            else {
                playerUI.OpenEyes(0.4f);

                if (canHorror && distance173 <= 8) {
                    canHorror = false;

                    horrorAudioSource.clip = horrorClips[Random.Range(0, horrorClips.Length)];
                    horrorAudioSource.Play();
                }
            }
        }

        // Post processing
        if (playerPostProcessing.profile.TryGetSettings<Vignette>(out var vignette)) {
            vignette.intensity.value = (0.5f - (health/maxHealth));
        }

        if (playerPostProcessing.profile.TryGetSettings<ColorGrading>(out var colorGrading)) {
            colorGrading.saturation.value = (-80 * (1.0f - health/maxHealth));

            if (playerStatus.high) {
                if (colorGrading.hueShift.value >= 180) {
                    colorGrading.hueShift.value = -180;
                }
                
                colorGrading.hueShift.value += 10.0f * Time.deltaTime;
            }
            else {
                colorGrading.hueShift.value = 0;
            }
        }

        // 173
        if (distance173 <= 8) {
            if (playerPostProcessing.profile.TryGetSettings<DepthOfField>(out var dof)) {
                dof.active = true;
                dof.focusDistance.value = new FloatParameter { value = distance173 };
                dof.aperture.value = new FloatParameter { value = distance173 };
            }
     
            if (playerPostProcessing.profile.TryGetSettings<LensDistortion>(out var lensDistortion)) {
                lensDistortion.intensity.value = new FloatParameter { value = Mathf.Lerp(lensDistortion.intensity, (-50.0f * (1.0f - (distance173/8))), 0.05f) };
            }
        }
        else {
            if (playerPostProcessing.profile.TryGetSettings<DepthOfField>(out var dof)) {
                dof.active = false;
            }
            
            if (playerPostProcessing.profile.TryGetSettings<LensDistortion>(out var lensDistortion)) {
                lensDistortion.intensity.value = new FloatParameter { value = Mathf.Lerp(lensDistortion.intensity, 0.0f, 0.01f) };
            }
        }

        // Death effects
        if (!playerStatus.canBlink && playerStatus.dead) {
            playerCamera.DeathCamera();
            playerUI.CloseEyes(0.005f);
        }
    }

    public void ShowText(string text) {
        playerUI.ShowText(text);
    }

    public void NeckSnap() {
        playerUI.OpenEyes(1f);
        playerStatus.health = 0;
    }

    public void SyncBlink(float blink) {
        playerStatus.blink = blink;
    }
}
