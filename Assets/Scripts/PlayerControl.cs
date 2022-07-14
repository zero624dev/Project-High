using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField]
    private float speed = 10.0f;
    [SerializeField]
    private float jumpForce = 10.0f;
    [SerializeField]
    private float camSpeed = 10.0f;

    [Header("Objects")]
    [SerializeField]
    public GameObject arm;

    [Header("UI")]
    [SerializeField]
    public Text roundsText;
    [SerializeField]
    GameObject deathPannel;

    [Header("Other")]
    public int rounds = 300;
    public int health = 1;

    Rigidbody2D rb;
    BoxCollider2D boxCol;
    Animator anim;

    Gun currentGun;
    Vector2 mousePos;
    bool isGrounded;
    float dirX;

    float fireTime;

    [HideInInspector]
    public bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCol = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();

        currentGun = arm.transform.GetChild(0).gameObject.GetComponent<Gun>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (isDead)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
            return;
        }
        isGrounded = (bool)Physics2D.Raycast(transform.position, Vector2.down, boxCol.size.y / 2 + 0.1f, 9);
        Movement();
        Anime();
        LookAtCursor();
        UseWeapon();
        TryReload();
        currentGun.UpdateRecoil();
    }

    void Movement()
    {
        anim.SetBool("isGrounded", isGrounded);
        if (isGrounded)
        {
            dirX = Input.GetAxis("Horizontal");
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(new Vector2(dirX, 1) * jumpForce, ForceMode2D.Impulse);
                anim.SetTrigger("Jump");
            }
            rb.velocity = new Vector2(dirX * speed, rb.velocity.y);
        }

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(
            transform.position.x, Camera.main.transform.position.y,
            Camera.main.transform.position.z), camSpeed * Time.deltaTime);
    }

    void Anime()
    {
        if (isGrounded && rb.velocity.x != 0)
        {
            anim.SetBool("isWalking", transform.localScale.x * dirX > 0);
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
    }

    void LookAtCursor()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float armDegree = Mathf.Atan2(mousePos.y - currentGun.transform.position.y, mousePos.x - currentGun.transform.position.x) * Mathf.Rad2Deg * transform.localScale.x;

        // flip player
        if (mousePos.x > arm.transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            arm.transform.localEulerAngles = new Vector3(0, 0, Mathf.Clamp(armDegree, -50, 16));
            if (armDegree < -50)
            {
                currentGun.transform.localEulerAngles = new Vector3(0, 0, armDegree + 50);
            }
            else
            {
                currentGun.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
        }
        else if (mousePos.x < arm.transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
            if (armDegree < 0) armDegree += 180;
            else armDegree -= 180;
            arm.transform.localEulerAngles = new Vector3(0, 0, Mathf.Clamp(armDegree, -50, 16));
            if (armDegree < -50)
            {
                currentGun.transform.localEulerAngles = new Vector3(0, 0, armDegree + 50);
            }
            else
            {
                currentGun.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
        }
        else
        {
            arm.transform.localEulerAngles = new Vector3(0, 0, -50);
            currentGun.transform.localEulerAngles = new Vector3(0, 0, -40);
        }
    }

    void UseWeapon()
    {
        if (Input.GetMouseButton(0) && Time.time > fireTime + (60 / currentGun.fireRateRPM) && !currentGun.isReloading)
        {
            currentGun.Shoot(mousePos.x > arm.transform.position.x);
            fireTime = Time.time;
        }
    }

    void TryReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !currentGun.isReloading)
        {
            StartCoroutine(currentGun.Reload(mousePos.x > arm.transform.position.x));
        }
    }

    public void TakeDamage()
    {
        if (--health <= 0)
        {
            isDead = true;
            anim.SetBool("isDead", true);
            deathPannel.SetActive(true);
        }
    }
}
