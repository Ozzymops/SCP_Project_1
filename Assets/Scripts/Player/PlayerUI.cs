using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    // Classes
    public class StatusTextEntry {
        public string text {get;set;}
        public float timer {get;set;}
        public bool active {get;set;}

        public StatusTextEntry(string text, float timer) {
            this.text = text;
            this.timer = timer;
            active = false;
        }
    }

    // References

    // Assets
    public Image blinkOverlay;
    public Image ExhaustedSymbol;
    public Image BleedingSymbol;
    public Image CrippledSymbol;
    public Image healthBar;  
    public Image staminaBar;
    public Image blinkBar;
    public Text healthText;
    public Text staminaText;
    public Text blinkText;
    public Text statusText;

    // Flags
    public bool exhausted = false;
    public bool bleeding = false;
    public bool crippled = false;
    public bool dead = false;

    // Static

    // Variables
    public float health;
    public float maxHealth;
    public float stamina;
    public float maxStamina;
    public float blink;
    public float maxBlink;

    // Status text
    public List<StatusTextEntry> statusTextList;
    public string statusTextString = "";
    public float statusTextTimer;

    void Awake() {
        statusTextList = new List<StatusTextEntry>();
        statusText.enabled = false;
    }

    void Update() {
        // Set bars
        healthBar.fillAmount = (health/maxHealth);
        staminaBar.fillAmount = (stamina/maxStamina);
        blinkBar.fillAmount = (blink/maxBlink);

        healthText.text = health.ToString("N0");
        staminaText.text = stamina.ToString("N0");
        blinkText.text = blink.ToString("N1");

        // Status text
        if (statusTextList.Count == 0 || dead) {
            statusText.enabled = false;
        }
        else {
            foreach (StatusTextEntry entry in statusTextList) {
                if (entry.timer > 0) {
                    if (!entry.active) {
                        entry.active = true;
                        statusTextString += entry.text + "\n";
                    }
                    
                    entry.timer -= 1.0f * Time.deltaTime;
                }
                else {
                    foreach (StatusTextEntry anotherEntry in statusTextList) {
                        anotherEntry.active = false;
                    }

                    statusTextString = "";
                    statusTextList.Remove(entry);
                }
            }

            statusText.enabled = true;
            statusText.text = statusTextString;
        }

        // Show symbols
        if (exhausted) {
            ExhaustedSymbol.enabled = true;
        }
        else {
            ExhaustedSymbol.enabled = false;
        }

        if (bleeding) {
            BleedingSymbol.enabled = true;
        }
        else {
            BleedingSymbol.enabled = false;
        }

        if (crippled) {
            CrippledSymbol.enabled = true;
        }
        else {
            CrippledSymbol.enabled = false;
        }
    }

    public void CloseEyes(float time) {
        blinkOverlay.color = new Color(0, 0, 0, Mathf.Lerp(blinkOverlay.color.a, 1, time));
    }
    
    public void OpenEyes(float time) {
        blinkOverlay.color = new Color(0, 0, 0, Mathf.Lerp(blinkOverlay.color.a, 0, time));
    }

    public void ShowText(string text) {
        statusTextList.Add(new StatusTextEntry(text, 5.0f));
    }
}
