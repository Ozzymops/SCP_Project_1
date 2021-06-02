using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPlayer : MonoBehaviour
{
    // Assets
    public AudioSource collapseAudioSource;
    public GameObject bloodPoolPrefab;

    // Variables
    public float health = 100;
    public float blink = 5.0f;
    float blinkHold = 0.0f;
    public bool blinking = false;
    public bool dead = false;
    bool deadCheck = false;

    // Static
    public float movementSpeed;
    public Vector3 movementDirection;

    void Update() {
        if (!dead) {

            GetComponent<CharacterController>().Move(movementDirection * movementSpeed * Time.deltaTime);

            if (health <= 0) {
                dead = true;
            }

            if (!blinking) {
                blink -= 1.0f * Time.deltaTime;

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
                    blink = 5.0f;
                }
            }
        }
        else {
            blink = 0;
            health = 0;
            transform.localEulerAngles = new Vector3(Mathf.Lerp(transform.localEulerAngles.x, 90, 0.04f), transform.localEulerAngles.y, transform.localEulerAngles.z);
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, 0.33f, 0.04f), transform.localPosition.z);
            
            if (!deadCheck) {
                deadCheck = true;

                GetComponent<Collider>().enabled = false;
                GetComponent<CharacterController>().enabled = false;

                collapseAudioSource.Play();

                RaycastHit poolHit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out poolHit)) {
                    GameObject pool = Instantiate(bloodPoolPrefab, new Vector3(poolHit.point.x, poolHit.point.y + 0.01f, poolHit.point.z), new Quaternion(0, 0, 0, 0));
                    pool.transform.localEulerAngles = new Vector3(90, Random.Range(0, 360), 0);
                    pool.transform.localScale = new Vector3(Mathf.Lerp(0, 5, 0.04f), Mathf.Lerp(0, 5, 0.04f), Mathf.Lerp(0, 5, 0.04f));
                }
            }
        }
    }

    public void NeckSnap() {
        health = 0;
    }

    public void SyncBlink(float blink) {
        this.blink = blink;
    }
}
