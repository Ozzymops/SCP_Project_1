using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    // References
    protected NavMeshAgent navMeshAgent;
    protected CharacterController characterController;
    public GameObject target;

    // Flags

    // Variables
    float velocity;
    float gravity = 0.981f;
    public float speed;

    void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
    }

    void Update() {
        navMeshAgent.speed = speed;
        Move(target);
        
        // Gravity
        if (characterController.isGrounded) {
            velocity = 0;
        }
        else {
            velocity -= gravity * Time.deltaTime;
            characterController.Move(new Vector3(0, velocity, 0));
        }
    }

    protected void Move(GameObject target) {
        navMeshAgent.destination = target.transform.position;
    }
}
