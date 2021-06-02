using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    // Assets
    public GameObject bloodDropPrefab;
    public GameObject bloodPoolPrefab;

    // References
    public AudioSource heartbeatAudioSource;
    public AudioSource bleedingAudioSource;
    public AudioSource crippledAudioSource;
    public AudioSource collapseAudioSource;
    public AudioClip[] bleedingAudioClips;
    PlayerManager playerManager;

    // Flags
    public bool dead = false;
    bool deadCheck = false;
    public bool sprinting = false;
    bool canRegenStamina = true;
    public bool exhausted = false;
    public bool bleeding = false;
    public bool crippled = false;
    public bool blinking = false;
    public bool canBlink = true;
    public bool high = false;

    // ShowText checks
    bool textBleedingCheck = false;
    bool textCrippledCheck = false;

    // Static
    public float maxHealth = 100;
    public float maxStamina = 100;
    public float maxBlink = 8;
    float maxSanity = 100;  // for SCP 895

    // Variables
    public float health = 100;
    public float stamina = 100;
    float staminaDrainMultiplier = 2.0f;
    public float blink = 10;
    float blinkHold = 0.5f;
    float blinkDrainMultiplier = 1.0f;
    float bloodTimer;
    float sanity = 100;

    void Awake() {
        playerManager = GetComponent<PlayerManager>();

        heartbeatAudioSource.volume = 0;
    }

    void Update() {
        if (!dead) {
            // Health
            health = Mathf.Clamp(health, 0, maxHealth);
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
            blink = Mathf.Clamp(blink, 0, maxBlink);
            blinkHold = Mathf.Clamp(blinkHold, 0, 0.5f);

            if (health <= maxHealth/2) {
                heartbeatAudioSource.volume = .25f / (health / (maxHealth/2));

                if (!heartbeatAudioSource.isPlaying) {
                    heartbeatAudioSource.Play();  
                }
            }
            else {
                heartbeatAudioSource.Stop();
            }

            if (health <= maxHealth/4) {
                crippled = true;
            }

            if (health <= 0) {
                dead = true;
            }

            // Stamina
            if (sprinting) {
                stamina -= (8.0f * staminaDrainMultiplier) * Time.deltaTime;
            }
            else {
                if (canRegenStamina && stamina < maxStamina) {
                    stamina += 8.0f * Time.deltaTime;
                }
            }

            // Blinking
            if (!blinking) {
                blink -= (1.0f * blinkDrainMultiplier) * Time.deltaTime;

                if (blink <= 0) {
                    blinking = true;
                    blinkHold = 0.5f;
                }
            }
            else {
                if (blinkHold > 0) {
                    blinking = true;
                    blinkHold -= 1.0f * Time.deltaTime;
                }

                if (blinkHold <= 0) {
                    blinking = false;
                    blink = maxBlink;
                }
            }

            // - Manual blinking
            if (Input.GetButton("Blink")) {
                blinkHold = 0.5f;
                blink = 0.0f;
            }

            // Bleeding
            if (bleeding) {
                if (!textBleedingCheck) {
                    textBleedingCheck = true;
                    ShowText("My wounds are dripping blood.");
                }

                health -= 0.5f * Time.deltaTime;

                if (bloodTimer > 0) {
                    bloodTimer -= 1.0f * Time.deltaTime;
                }
                else {
                    bloodTimer = Random.Range(4.0f, 8.0f);
                    bleedingAudioSource.clip = bleedingAudioClips[Random.Range(0, bleedingAudioClips.Length)];
                    bleedingAudioSource.Play();

                    int toPlace = Random.Range(4, 8);
                    
                    for(int placed = 0; placed < toPlace; placed++) {               
                        RaycastHit dropletHit;
                        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out dropletHit)) {
                            GameObject droplet = Instantiate(bloodDropPrefab, new Vector3(dropletHit.point.x + Random.Range(-0.2f, 0.2f), dropletHit.point.y + 0.01f, dropletHit.point.z + Random.Range(-0.2f, 0.2f)), new Quaternion(0, 0, 0, 0));
                            droplet.transform.localEulerAngles = new Vector3(90, Random.Range(0, 360), 0);
                            droplet.transform.localScale = new Vector3(Random.Range(0.1f, 0.4f), 0.2f, Random.Range(0.1f, 0.4f));
                        }
                    }
                }
            }
            else {
                textBleedingCheck = false;
            }

            // Crippling
            if (crippled) {
                if (!textCrippledCheck) {
                    textCrippledCheck = true;
                    ShowText("My legs are crippled.");
                }
            }
            else {
                textCrippledCheck = false;
            }

            // -- TESTING --
            if (Input.GetKeyDown(KeyCode.O)) {
                TakeDamage(10);
            }

            if (Input.GetKeyDown(KeyCode.P)) {
                Heal(10);
            }

            if (Input.GetKeyDown(KeyCode.I)) {
                bleeding = true;
            }
            
            if (Input.GetKeyDown(KeyCode.U)) {
                crippled = true;
            }

            if (Input.GetKeyDown(KeyCode.L)) {
                high = !high;
            }
        }
        else {
            // death stuff
            canBlink = false;
            blink = 0.0f;
            heartbeatAudioSource.Stop();

            if (!deadCheck) {
                if (bleeding) {
                    RaycastHit poolHit;
                    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out poolHit)) {
                        GameObject pool = Instantiate(bloodPoolPrefab, new Vector3(poolHit.point.x, poolHit.point.y + 0.01f, poolHit.point.z), new Quaternion(0, 0, 0, 0));
                        pool.transform.localEulerAngles = new Vector3(90, Random.Range(0, 360), 0);
                        pool.transform.localScale = new Vector3(Mathf.Lerp(0, 5, 0.04f), Mathf.Lerp(0, 5, 0.04f), Mathf.Lerp(0, 5, 0.04f));
                    }
                }

                collapseAudioSource.Play();
                deadCheck = true;
            }
        }
    }

    void ShowText(string text) {
        playerManager.ShowText(text);
    }

    public void TakeDamage(float damage) {
        health -= damage;
    }

    public void Heal(float heal) {
        bleeding = false;
        crippled = false;
        health += heal;
    }
}
