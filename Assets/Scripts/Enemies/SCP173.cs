using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCP173 : Enemy
{
    // Assets
    public AudioSource movementAudioSource;
    public AudioSource necksnapAudioSource;
    public AudioClip[] necksnapClips;

    // References
    public GameObject[] players;

    // Flags
    public bool canMove = true;
    public bool lineOfSight = false;
    public bool snapped = false;

    void Start() {
        NextTarget();
    }

    void Update() {
        if (players.Length > 0) {
            if (target) {
                target = NextTarget();
                SyncBlink();

                if (target.TryGetComponent(out PlayerStatus player)) {
                    if (!player.dead)  {
                        bool blinking = player.blinking;
                
                        if (blinking) {
                            canMove = true;
                        }
                        else {
                            snapped = false;
                            canMove = false;
                        }
                    }
                }
                else if (target.TryGetComponent(out DummyPlayer dummy)) {
                    if (!dummy.dead) {
                        bool blinking = dummy.blinking;
                
                        if (blinking) {
                            canMove = true;
                        }
                        else {
                            snapped = false;
                            canMove = false;
                        }
                    }
                }

                if (canMove) {
                    this.transform.LookAt(target.transform.position);

                    if (!movementAudioSource.isPlaying) {
                        movementAudioSource.Play();
                    }

                    navMeshAgent.isStopped = false;
                    Move(target);
                }
                else {
                    movementAudioSource.Pause();
                    navMeshAgent.velocity = Vector3.zero;
                    navMeshAgent.isStopped = true;
                }
            }
            else {
                target = NextTarget();
                canMove = false;
            }
        }
        else {
            canMove = false;
        }     
    }

    void OnTriggerStay(Collider collider) {
        if (canMove && collider.gameObject.tag == "Player") {
            if (!snapped) {
                snapped = true;

                if (collider.TryGetComponent(out PlayerManager player)) {
                    if (!player.isDead) {
                        player.NeckSnap();
                        NextTarget();
                    }
                }
                else if (collider.TryGetComponent(out DummyPlayer dummy)) {
                    if (!dummy.dead) {
                        dummy.NeckSnap();
                        NextTarget();
                    }
                }

                necksnapAudioSource.clip = necksnapClips[Random.Range(0, necksnapClips.Length)];
                necksnapAudioSource.Play();
            }          
        }
    }

    GameObject NextTarget() {
        players = GameObject.FindGameObjectsWithTag("Player");

        float bestDistance = -1.0f;
        GameObject bestTarget = null;

        if (players.Length > 0) {
            foreach (GameObject player in players) {
                bool continueScript = false;

                if (player.TryGetComponent(out PlayerStatus playerStatus)) {
                    continueScript = !playerStatus.dead;
                }
                else if (player.TryGetComponent(out DummyPlayer dummyStatus)) {
                    continueScript = !dummyStatus.dead;
                }

                if (continueScript) {
                    float distance = Vector3.Distance(player.transform.position, transform.position);

                    if (bestDistance == -1.0f) {
                        bestDistance = distance;
                        bestTarget = player;
                    }
                    else if (distance < bestDistance) {
                        bestDistance = distance;
                        bestTarget = player;
                    }
                }

            }
        }

        return bestTarget;
    }

    float TargetBlink() {
        if (target != null) {
            if (target.TryGetComponent(out PlayerStatus player)) {
                    return player.blink;
                }
                else if (target.TryGetComponent(out DummyPlayer dummy)) {
                    return dummy.blink;
                }
                else {
                    return 0;
                }
            }
        return 0;
    }

    void SyncBlink() {
        foreach(GameObject player in players) {
            if (player.TryGetComponent(out PlayerManager playerManager)) {
                playerManager.SyncBlink(TargetBlink());
            }
            else if (player.TryGetComponent(out DummyPlayer dummy)) {
                dummy.SyncBlink(TargetBlink());
            }
        }
    }
}