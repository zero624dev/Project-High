using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [SerializeField]
    [InspectorName("Fire Rate (RPM)")]
    public float fireRateRPM;
    [SerializeField]
    private Transform bulletOffset;
    [SerializeField]
    private Transform magazineOffset;
    [SerializeField]
    private Transform catridgeOffset;
    [SerializeField]
    private int maxRounds = 25;
    [SerializeField]
    private float reloadTime = 1f;

    private int currentRounds = 26;

    [Header("Objects")]
    [SerializeField]
    private GameObject catridge;
    [SerializeField]
    private GameObject magazine;
    [SerializeField]
    private GameObject bullet;

    // recoil
    [Header("Recoil")]
    [SerializeField]
    private float _maximumOffsetDistance;
    [SerializeField]
    private float _recoilAcceleration;
    [SerializeField]
    private float _weaponRecoilStartSpeed;

    private bool _recoilInEffect = false;
    private bool _weaponHeadedBackToStartPosition = false;

    private Vector3 _offsetPosition = Vector3.zero;
    private Vector3 _recoilSpeed = Vector3.zero;

    private PlayerControl playerControl;
    private Animator anim;
    [HideInInspector]
    public bool isReloading = false;

    void Start()
    {
        playerControl = transform.parent.parent.GetComponent<PlayerControl>();
        anim = transform.GetComponent<Animator>();
        SetRoundsText();
    }

    public IEnumerator Reload(bool flip)
    {
        if (currentRounds < maxRounds + 1 && playerControl.rounds > 0)
        {
            anim.SetTrigger("Reload");

            SpriteRenderer magRenderer = Instantiate(magazine, magazineOffset.position, magazineOffset.rotation).GetComponent<SpriteRenderer>();
            magRenderer.flipX = flip;
            Destroy(magRenderer.gameObject, 2);
            Physics2D.IgnoreLayerCollision(10, 9);
            Physics2D.IgnoreLayerCollision(10, 10);

            isReloading = true;
            yield return new WaitForSeconds(reloadTime / 4);
            playerControl.roundsText.text = $"|   / {playerControl.rounds}";
            yield return new WaitForSeconds(reloadTime / 4);
            playerControl.roundsText.text = $"||  / {playerControl.rounds}";
            yield return new WaitForSeconds(reloadTime / 4);
            playerControl.roundsText.text = $"||| / {playerControl.rounds}";
            yield return new WaitForSeconds(reloadTime / 4);

            if (currentRounds > 0 && playerControl.rounds + currentRounds > maxRounds + 1)
            {
                playerControl.rounds -= maxRounds - currentRounds + 1;
                currentRounds = maxRounds + 1;
            }
            else if (playerControl.rounds > maxRounds)
            {
                playerControl.rounds -= maxRounds;
                currentRounds = maxRounds;
            }
            else
            {
                currentRounds += playerControl.rounds;
                playerControl.rounds = 0;
            }
            isReloading = false;
            SetRoundsText();
        }
    }

    public void Shoot(bool flip)
    {
        if (currentRounds > 0)
        {
            currentRounds--;
            SetRoundsText();
            AddRecoil(flip);

            GameObject bullet_ = Instantiate(bullet, bulletOffset.position, bulletOffset.rotation);
            bullet_.GetComponent<SpriteRenderer>().flipX = flip;
            bullet_.GetComponent<Rigidbody2D>().AddRelativeForce(bullet.transform.right * (flip ? 100 : -100), ForceMode2D.Impulse);

            GameObject catridge_ = Instantiate(catridge, catridgeOffset.position, catridgeOffset.rotation);
            Rigidbody2D catridgeRigid = catridge_.GetComponent<Rigidbody2D>();
            catridgeRigid.angularVelocity = Random.Range(360, 600);
            catridgeRigid.AddRelativeForce(new Vector2(Random.Range(.05f, .5f), Random.Range(2f, 4f)), ForceMode2D.Impulse);
            catridge_.GetComponent<SpriteRenderer>().flipX = flip;
            Destroy(catridge_, 2);

            Physics2D.IgnoreLayerCollision(10, 9);
            Physics2D.IgnoreLayerCollision(10, 10);
        }
        else
        {
            StartCoroutine(Reload(flip));
        }
    }

    void AddRecoil(bool flip)
    {
        _recoilInEffect = true;
        _weaponHeadedBackToStartPosition = false;
        _recoilSpeed = Vector3.right * _weaponRecoilStartSpeed;
    }

    public void UpdateRecoil()
    {
        if (_recoilInEffect)
        {
            // set up speed and then position variables
            _recoilSpeed += (-_offsetPosition.normalized) * _recoilAcceleration * Time.deltaTime;
            Vector3 newOffsetPosition = _offsetPosition + _recoilSpeed * Time.deltaTime;
            Vector3 newTransformPosition = transform.localPosition - _offsetPosition;

            if (newOffsetPosition.magnitude > _maximumOffsetDistance)
            {
                _recoilSpeed = Vector3.zero;
                _weaponHeadedBackToStartPosition = true;
                newOffsetPosition = _offsetPosition.normalized * _maximumOffsetDistance;
            }
            else if (_weaponHeadedBackToStartPosition && newOffsetPosition.magnitude > _offsetPosition.magnitude)
            {
                // transform.parent.localPosition = originalPosition;
                transform.localPosition -= _offsetPosition;
                _offsetPosition = Vector3.zero;

                // set up our boolean
                _recoilInEffect = false;
                _weaponHeadedBackToStartPosition = false;
                return;
            }

            transform.localPosition = newTransformPosition + newOffsetPosition;
            _offsetPosition = newOffsetPosition;
        }
    }

    void SetRoundsText()
    {
        playerControl.roundsText.text = $" {currentRounds} / {playerControl.rounds}";
    }
}
