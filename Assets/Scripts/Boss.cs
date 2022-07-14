using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;

public class Boss : MonoBehaviour
{
    [SerializeField]
    float health = 100;
    // float headHealth = 100.0f;
    // [SerializeField]
    // float leftArmHealth = 100.0f;
    // [SerializeField]
    // float rightArmHealth = 100.0f;
    [SerializeField]
    float laserDistance = 100.0f;
    [SerializeField]
    float actionRate = 1.0f;

    [Header("GameObjects")]
    [SerializeField]
    GameObject eye;
    [SerializeField]
    GameObject iris;
    [SerializeField]
    GameObject leftArmWarning;
    [SerializeField]
    GameObject rightArmWarning;
    [SerializeField]
    GameObject leftHandTarget;
    [SerializeField]
    GameObject rightHandTarget;

    [Header("UI")]
    [SerializeField]
    Slider healthBar;

    [SerializeField]
    PlayerControl player;

    LineRenderer lineRenderer;
    SpriteRenderer irisSpriteRenderer;
    Animator anim;

    Volume volume;


    bool isFiringLaser = false;
    int leftHandPhase = 0;
    int rightHandPhase = 0;
    Vector3 seeLine;
    float actionTime;

    bool isDead = false;
    float maxHealth;

    void Start()
    {
        maxHealth = health;
        volume = Camera.main.GetComponent<Volume>();
        irisSpriteRenderer = iris.GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (player.isDead || isDead)
        {
            return;
        }
        LookAtPlayer();
        RandomAction();
        FireLaser();
        HandAttack();
    }

    public void TakeDamage(float damage, string hitPart)
    {
        // if (hitPart == "Head")
        // {
        //     headHealth -= damage;
        //     if (headHealth <= 0)
        //     {
        //         headHealth = 0;
        //     }
        // }
        // else if (hitPart == "LeftArm")
        // {
        //     leftArmHealth -= damage;
        //     if (leftArmHealth <= 0)
        //     {
        //         leftArmHealth = 0;
        //     }
        // }
        // else if (hitPart == "RightArm")
        // {
        //     rightArmHealth -= damage;
        //     if (rightArmHealth <= 0)
        //     {
        //         rightArmHealth = 0;
        //     }
        // }
        // healthBar.value = (headHealth + leftArmHealth + rightArmHealth) / 300.0f;
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            isDead = true;
            anim.SetBool("isDead", true);
            lineRenderer.enabled = false;
        }
        healthBar.value = health / maxHealth;
    }

    void LookAtPlayer()
    {
        if (!isFiringLaser)
        {
            seeLine = player.transform.position - eye.transform.position;
            iris.transform.localPosition = Vector3.Lerp(iris.transform.localPosition, seeLine.normalized * Mathf.Clamp(seeLine.magnitude - 4.5f, 0, .9f), Time.deltaTime * 10.0f);
        }
    }

    void RandomAction()
    {
        if (Time.time > actionTime + actionRate)
        {
            actionTime = Time.time;
            int action = UnityEngine.Random.Range(0, 3);
            if (Mathf.Abs(player.transform.position.x) > 10)
                action = 0;

            if (action == 0 && !isFiringLaser)
            {
                isFiringLaser = true;
            }
            else if (action == 1 && leftHandPhase == 0)
            {
                leftHandPhase = 1;
            }
            else if (action == 2 && rightHandPhase == 0)
            {
                rightHandPhase = 1;
            }
        }
    }

    void FireLaser()
    {
        if (isFiringLaser)
        {
            float x = Mathf.Lerp(irisSpriteRenderer.color.g, 0, 3f * Time.deltaTime);
            irisSpriteRenderer.color = new Color(1, x, x);
            if (irisSpriteRenderer.color.g < 0.05f)
            {
                if (!lineRenderer.enabled)
                {
                    volume.enabled = true;
                    lineRenderer.enabled = true;
                    RaycastHit2D hit = Physics2D.Raycast(iris.transform.position, seeLine, laserDistance, ~(1 << 8 | 1 << 9));
                    lineRenderer.SetPosition(0, new Vector3(iris.transform.position.x, iris.transform.position.y, -1));
                    if (hit.collider)
                    {
                        lineRenderer.SetPosition(1, hit.point);
                    }
                    else
                    {
                        lineRenderer.SetPosition(1, seeLine.normalized * laserDistance);
                    }
                    StartCoroutine(WaitForSeconds(1.0f, () => { isFiringLaser = false; }));
                }

                RaycastHit2D hit_ = Physics2D.Raycast(iris.transform.position, seeLine, laserDistance, ~(1 << 8));
                if (hit_.collider.tag == "Player")
                {
                    player.TakeDamage();
                }
            }
        }
        else
        {
            volume.enabled = false;
            lineRenderer.enabled = false;
            irisSpriteRenderer.color = new Color(1, 1, 1);
        }
        anim.SetBool("isFiringLaser", isFiringLaser);
    }

    void HandAttack()
    {
        if (leftHandPhase == 1)
        {
            if (!leftArmWarning.activeSelf)
            {
                leftArmWarning.SetActive(true);
                leftArmWarning.transform.position = new Vector3(player.transform.position.x, 0, 0);
            }
            leftHandTarget.transform.position = Vector3.Lerp(leftHandTarget.transform.position,
             new Vector3(leftArmWarning.transform.position.x, 15, 0), Time.deltaTime * 20.0f);
            StartCoroutine(WaitForSeconds(1.0f, () =>
            {
                leftHandPhase = 2;
                leftArmWarning.SetActive(false);
            }));
        }
        else if (leftHandPhase == 2)
        {
            leftHandTarget.transform.position = Vector3.Lerp(leftHandTarget.transform.position,
            new Vector3(leftHandTarget.transform.position.x, -3, 0), Time.deltaTime * 35.0f);
            if (leftHandTarget.transform.position.y < -2.5f)
            {
                leftHandPhase = 0;
            }
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(leftHandTarget.transform.position, Vector2.down, 0.1f, ~(1 << 8));
                if (hit && hit.collider.tag == "Player")
                {
                    player.TakeDamage();
                }
            }
        }
        else
        {
            leftHandTarget.transform.position = Vector3.Lerp(leftHandTarget.transform.position,
            new Vector3(-7, 0, 0), Time.deltaTime * 15.0f);
        }

        if (rightHandPhase == 1)
        {
            if (!rightArmWarning.activeSelf)
            {
                rightArmWarning.SetActive(true);
                rightArmWarning.transform.position = new Vector3(player.transform.position.x, 0, 0);
            }
            rightHandTarget.transform.position = Vector3.Lerp(rightHandTarget.transform.position,
             new Vector3(rightArmWarning.transform.position.x, 15, 0), Time.deltaTime * 20.0f);
            StartCoroutine(WaitForSeconds(1.0f, () =>
            {
                rightHandPhase = 2;
                rightArmWarning.SetActive(false);
            }));
        }
        else if (rightHandPhase == 2)
        {
            rightHandTarget.transform.position = Vector3.Lerp(rightHandTarget.transform.position,
            new Vector3(rightHandTarget.transform.position.x, -3, 0), Time.deltaTime * 35.0f);
            if (rightHandTarget.transform.position.y < -2.5f)
            {
                rightHandPhase = 0;
            }
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(rightHandTarget.transform.position, Vector2.down, 0.1f, ~(1 << 8));
                if (hit && hit.collider.tag == "Player")
                {
                    player.TakeDamage();
                }
            }
        }
        else
        {
            rightHandTarget.transform.position = Vector3.Lerp(rightHandTarget.transform.position,
            new Vector3(7, 0, 0), Time.deltaTime * 15.0f);
        }
    }

    IEnumerator WaitForSeconds(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback.Invoke();
    }
}
