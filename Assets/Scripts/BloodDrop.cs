using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodDrop : MonoBehaviour
{
    public Material[] materialChoice;
    public float lifetime = 30.0f;
    public bool isPool = false;

    void Awake() {
        this.GetComponent<MeshRenderer>().material = materialChoice[Random.Range(0, materialChoice.Length)];
    }

    void Update() {
        lifetime -= 1.0f * Time.deltaTime;

        if (lifetime <= 0) {
            // shrink, then destroy
            float localScaleX = Mathf.Lerp(this.transform.localScale.x, 0, 0.01f);
            float localScaleY = Mathf.Lerp(this.transform.localScale.y, 0, 0.01f);
            this.transform.localScale = new Vector3(localScaleX, localScaleY, this.transform.localScale.z);

            Destroy(this, 5.0f);
        }
        else if (isPool) {
            float poolScaleX = Mathf.Lerp(this.transform.localScale.x, 1.0f, 0.005f);
            float poolScaleY = Mathf.Lerp(this.transform.localScale.y, 1.0f, 0.005f);
            this.transform.localScale = new Vector3(poolScaleX, poolScaleY, this.transform.localScale.z);
        }
    }
}
