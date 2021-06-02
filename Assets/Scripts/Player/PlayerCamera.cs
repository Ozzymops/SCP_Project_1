using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    // References
    GameObject playerCamera;

    // Flags
    public bool canLook = true;

    // Static

    // Variable
    public float health;
    public float maxHealth;
    public float mouseSensitivity;
    float fieldOfView;
    float tempFieldOfView;
    public float rotationX = 0.0f;
    public float rotationY = 0.0f;
    public float rotationZ = 0.0f;
    public float offsetX = 0.0f;
    public float offsetY = 0.0f;
    public float offsetZ = 0.0f;

    void Awake() {
        playerCamera = transform.GetChild(0).gameObject;
        tempFieldOfView = playerCamera.GetComponent<Camera>().fieldOfView;
        fieldOfView = playerCamera.GetComponent<Camera>().fieldOfView;
    }

    void Update() {
        if (canLook) {
            // Camera rotation
            playerCamera.GetComponent<Camera>().fieldOfView = fieldOfView;
            tempFieldOfView = fieldOfView;
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            rotationY += mouseX;
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -90, 90);

            playerCamera.transform.eulerAngles = new Vector3(rotationX, rotationY, rotationZ);

            // - Lower and tilt camera based on health
            if (health <= (maxHealth/4)) {
                offsetY = Mathf.Lerp(offsetY, 1.0f * health/(maxHealth/4), 0.05f);
                rotationZ = Mathf.Lerp(rotationZ, 25.0f - (25.0f*health/(maxHealth/4)), 0.05f);
            }
            else {
                offsetY = 1.0f;
                rotationZ = 0.0f;
            }

            playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, offsetY, playerCamera.transform.localPosition.z);
        }
        else {
            playerCamera.GetComponent<Camera>().fieldOfView = tempFieldOfView;
        }
    }

    public void DeathCamera() {
        rotationX = Mathf.Lerp(rotationX, 75.0f, 0.008f);
        rotationZ = Mathf.Lerp(rotationZ, 40.0f, 0.008f);
        tempFieldOfView = Mathf.Lerp(tempFieldOfView, 35, 0.01f);
        playerCamera.transform.eulerAngles = new Vector3(rotationX, rotationY, rotationZ);
    }

    public void ShakeCamera(float intensity) {
        
    }
}
