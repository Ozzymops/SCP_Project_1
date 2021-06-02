using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class Player : MonoBehaviour
{
    [System.Serializable]
    public class ReferenceClass {
        public GameObject playerCamera;
        public CharacterController playerCharCon;
        public PostProcessVolume postProcessingVolume;
        public AudioReverbFilter deadFilter;
        public AudioSource footstepSource;
        public AudioSource damageSource;
        public AudioSource heartbeatSource;
        public AudioSource breatheSource;
        public AudioSource collapseSource;
        public AudioSource bloodSource;
        public AudioClip[] footstepClips;
        // todo - different groups for different footstep sounds

        public AudioClip[] damageClips;
        public AudioClip[] bloodClips;
        public GameObject bloodDropPrefab;
        public GameObject bloodPoolPrefab;
    }

    [System.Serializable]
    public class UIClass {
        public Text statusText;
        public Text staminaText;
        public Text healthText;
        public Text blinkText;
        public Image staminaBar;
        public Image healthBar;
        public Image blinkBar;
        public Image blinkOverlay;
    }

    // Settings
    public float mouseSensitivity;
    public float fieldOfView;
    float tempFieldOfView;

    // Flags
    bool canMove = true;
    bool canLook = true;
    bool canFootstep = true;
    bool allowStaminaRegen = true;
    bool blinking = false;
    bool bleeding = false;
    bool crippled = false;
    bool breathe = false;
    bool dead = false;

    // Camera
    float rotationX = 0;
    float rotationY = 0;
    float rotationZ = 0;
    float offsetX = 0;
    float offsetY = 0;
    float offsetZ = 0;

    // Post Processing

    // Movement
    float currentSpeed = 2.0f;
    float movementSpeed = 2.0f;
    float movementSpeedMultiplier = 1.0f;
    float sprintMultiplier = 2.0f;
    float gravity = .098f;
    float velocity = 0.0f;
    float stepTimer = 0.0f;
    float currentInterval = 0.8f;
    float stepInterval = 0.8f;

    // Player status
    float health = 100;
    float maxHealth = 100;
    float stamina = 100;
    float maxStamina = 100;
    float staminaDrain = 10;
    float blink = 10;
    float blinkHold = 1.0f;
    float blinkMultiplier = 1.0f;
    float bloodTimer = 0.0f;
    
    // Status text
    float statusTextTimer = 0.0f;
    bool statusBleedCheck = false;
    bool statusHealthLowCheck = false;
    bool statusHealthDyingCheck = false;

    // Specifics
    int sanity = 1000;  // for SCP-895 
    float colorAlpha = 0.0f;    // for death eye closing
    
    // Class setup
    public ReferenceClass referenceClass;
    public UIClass uiClass;

    void Awake() {
        if (referenceClass.postProcessingVolume.profile.TryGetSettings<Vignette>(out var vignette)) {
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.0f;
        }

        referenceClass.heartbeatSource.volume = 0;
        referenceClass.heartbeatSource.Play();

        referenceClass.breatheSource.volume = 0;
        referenceClass.breatheSource.Play();

        // Debug
        ShowText("Welcome!", 5);
    }

    void Update() {
        // Camera 
        if (canLook) {
            referenceClass.playerCamera.GetComponent<Camera>().fieldOfView = fieldOfView;
            tempFieldOfView = fieldOfView;
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            rotationY += mouseX;
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -90, 90);

            referenceClass.playerCamera.transform.eulerAngles = new Vector3(rotationX, rotationY, rotationZ);
            referenceClass.playerCharCon.transform.localEulerAngles = new Vector3(0, rotationY, 0);
        }
        else {
            referenceClass.playerCamera.GetComponent<Camera>().fieldOfView = tempFieldOfView;
        }

        // Movement
        if (canMove) {
            float horizontal = Input.GetAxis("Horizontal") * currentSpeed;
            float vertical = Input.GetAxis("Vertical") * currentSpeed;
        
            referenceClass.playerCharCon.Move((referenceClass.playerCharCon.transform.right * horizontal + referenceClass.playerCharCon.transform.forward * vertical) * Time.deltaTime);

            // - Footsteps
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) {
                if (canFootstep) {
                    if (stepTimer <= 0) {
                        referenceClass.footstepSource.clip = referenceClass.footstepClips[Random.Range(0, referenceClass.footstepClips.Length)];
                        referenceClass.footstepSource.Play();

                        stepTimer = currentInterval;
                    }
                }

                stepTimer -= 1.0f * Time.deltaTime;
            }

            // - Sprinting
            if (Input.GetButton("Sprint") && stamina > 0 && !crippled) {
                currentSpeed = movementSpeed * sprintMultiplier;

                if (horizontal != 0 || vertical != 0) {
                    allowStaminaRegen = false;
                    currentInterval = stepInterval / sprintMultiplier;
                    stamina -= staminaDrain * Time.deltaTime;
                }
                else {
                    allowStaminaRegen = true;
                }
                        
            }
            else {
                allowStaminaRegen = true;
                currentInterval = stepInterval;
                currentSpeed = movementSpeed * movementSpeedMultiplier;
            }
        }

        // - Stamina regen
        if (allowStaminaRegen && stamina < maxStamina) {
            stamina += staminaDrain/2 * Time.deltaTime;
        }

        if (stamina >= maxStamina) {
            stamina = maxStamina;
        }

        if (stamina <= 33) {
            referenceClass.breatheSource.volume = (1 - stamina/(maxStamina/3));
        }
        else {
            referenceClass.breatheSource.volume = 0;
        }

        // Health
        if (health <= 0) {
            if (!dead) {
                ShowText("You collapse and slowly drift away into nothingness...", 5);
                referenceClass.collapseSource.Play();

                RaycastHit poolHit;
                if (Physics.Raycast(referenceClass.playerCharCon.gameObject.transform.position, referenceClass.playerCharCon.gameObject.transform.TransformDirection(Vector3.down), out poolHit)) {
                    GameObject pool = Instantiate(referenceClass.bloodPoolPrefab, new Vector3(poolHit.point.x, poolHit.point.y + 0.01f, poolHit.point.z), new Quaternion(0, 0, 0, 0));
                    pool.transform.localEulerAngles = new Vector3(90, Random.Range(0, 360), 0);
                    pool.transform.localScale = new Vector3(Mathf.Lerp(0, 5, 0.04f), Mathf.Lerp(0, 5, 0.04f), Mathf.Lerp(0, 5, 0.04f));
                }
            }

            health = 0;
            stamina = 0;
            dead = true;
            bleeding = false;
            crippled = false;
            blinking = false;
            blink = 0;
            canMove = false;
            canLook = false;

            colorAlpha += 0.33f * Time.deltaTime;
            uiClass.blinkOverlay.color = new Color(0, 0, 0, colorAlpha);

            referenceClass.deadFilter.enabled = true;
            referenceClass.heartbeatSource.volume = 0;
            referenceClass.breatheSource.volume = 0;

            rotationX = Mathf.Lerp(rotationX, 75, 0.01f);
            rotationZ = Mathf.Lerp(rotationZ, -20, 0.01f);
            tempFieldOfView = Mathf.Lerp(tempFieldOfView, 40, 0.01f);
            referenceClass.playerCamera.transform.eulerAngles = new Vector3(rotationX, rotationY, rotationZ);
        }

        if (health >= maxHealth) {
            health = maxHealth;
        }
        
        if (health <= maxHealth/2 && !dead) {
            if (!statusHealthLowCheck) {
                statusHealthLowCheck = true;
                ShowText("You feel weak and are in pain.", 5);
            }
            referenceClass.heartbeatSource.volume = (1.0f - health/(maxHealth/2));
        }
        else {
            statusHealthLowCheck = false;
            referenceClass.heartbeatSource.volume = 0;
        }

        if (health <= maxHealth/4) {
            if (!statusHealthDyingCheck) {
                statusHealthDyingCheck = true;
                ShowText("You feel faint and are in agony.", 5);
            }
            crippled = true;
            // begin tilting/lowering camera
            offsetY = Mathf.Lerp(offsetY, -0.2f + health/(maxHealth/4), 0.05f) ;
            referenceClass.playerCamera.transform.localPosition = new Vector3(referenceClass.playerCamera.transform.localPosition.x, offsetY, referenceClass.playerCamera.transform.localPosition.z);
        }
        else {
            statusHealthDyingCheck = false;
            referenceClass.playerCamera.transform.localPosition = new Vector3(referenceClass.playerCamera.transform.localPosition.x, 0.8f, referenceClass.playerCamera.transform.localPosition.z);
        }

        if (bleeding) {
            if (!statusBleedCheck) {
                statusBleedCheck = true;
                ShowText("You're losing blood, fast.", 5);
            }
            
            health -= 0.5f * Time.deltaTime;
            
            if (bloodTimer <= 0) {
                bloodTimer = Random.Range(4, 8);
                referenceClass.bloodSource.clip = referenceClass.bloodClips[Random.Range(0, referenceClass.bloodClips.Length)];
                referenceClass.bloodSource.Play();

                int toPlace = Random.Range(4, 8);
                
                for(int placed = 0; placed < toPlace; placed++) {               
                    RaycastHit dropletHit;
                    if (Physics.Raycast(referenceClass.playerCharCon.gameObject.transform.position, referenceClass.playerCharCon.gameObject.transform.TransformDirection(Vector3.down), out dropletHit)) {
                        GameObject droplet = Instantiate(referenceClass.bloodDropPrefab, new Vector3(dropletHit.point.x + Random.Range(-0.2f, 0.2f), dropletHit.point.y + 0.01f, dropletHit.point.z + Random.Range(-0.2f, 0.2f)), new Quaternion(0, 0, 0, 0));
                        droplet.transform.localEulerAngles = new Vector3(90, Random.Range(0, 360), 0);
                        droplet.transform.localScale = new Vector3(Random.Range(0.1f, 0.4f), 0.2f, Random.Range(0.1f, 0.4f));
                    }
                }
            }

            bloodTimer -= 1.0f * Time.deltaTime;
        } 
        else {
            statusBleedCheck = false;
            bloodTimer = 0;
        }

        if (crippled) {
            movementSpeedMultiplier = 0.5f;
        }
        else {
            movementSpeedMultiplier = 1.0f;
        }

        // Blinking
        if (!dead) {
            if (!blinking) {
                blink -= (1.0f * blinkMultiplier) * Time.deltaTime;

                if (blink <= 0) {
                    blinking = true;
                    blink = 0.0f;
                    blinkHold = 0.5f;
                    CloseEyes();
                }
            }
            else {
                if (blinkHold > 0) {
                    blinking = true;
                    blinkHold -= 1.0f * Time.deltaTime;
                }

                if (blinkHold <= 0) {
                    blinking = false;
                    blink = 10.0f;
                    blinkHold = 0;
                    OpenEyes();
                }
            }
        }

        // - Manual blink
        if (Input.GetKeyDown(KeyCode.Q)) {
            blink = 0;
        }

        // Gravity
        if (referenceClass.playerCharCon.isGrounded) {
            velocity = 0;
        }
        else {
            velocity -= gravity * Time.deltaTime;
            referenceClass.playerCharCon.Move(new Vector3(0, velocity, 0));
        }

        // UI
        if (statusTextTimer <= 0) {
            statusTextTimer = 0;
            uiClass.statusText.enabled = false;
        }
        else {
            statusTextTimer -= 1.0f * Time.deltaTime;
        }
        
        uiClass.staminaText.text = stamina.ToString("N0");
        uiClass.healthText.text = health.ToString("N0");
        uiClass.blinkText.text = blink.ToString("N1");
        uiClass.staminaBar.fillAmount = stamina/maxStamina;
        uiClass.healthBar.fillAmount = health/maxHealth;
        uiClass.blinkBar.fillAmount = blink/10.0f;

        // Post Processing
        if (referenceClass.postProcessingVolume.profile.TryGetSettings<Vignette>(out var vignette)) {
            vignette.intensity.value = (0.6f - health/maxHealth);
        }

        // Debug input
        if (Input.GetKeyDown(KeyCode.O)) {
            TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            Heal(30);
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            bleeding = true;
        }
    }

    public void TakeDamage(float damage) {
        health -= damage;
        referenceClass.damageSource.clip = referenceClass.damageClips[Random.Range(0, referenceClass.damageClips.Length)];
        referenceClass.damageSource.Play();
    }

    public void Heal(float heal) {
        if (heal > 50) {
            ShowText("You apply some bandages and stop the bleeding. You also take a painkiller, and feel a lot better.", 5);
        }
        else {
            ShowText("You apply some bandages and stop the bleeding.", 5);
        }

        crippled = false;
        bleeding = false;
        health += heal;
    }

    public void ShowText(string text, float time) {
        uiClass.statusText.enabled = true;
        statusTextTimer = time;
        uiClass.statusText.text = text.ToString();
    }

    public void CloseEyes() {
        uiClass.blinkOverlay.color = new Color(0, 0, 0, 1);
    }

    public void OpenEyes() {
        uiClass.blinkOverlay.color = new Color(0, 0, 0, 0);
    }
}
