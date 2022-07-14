using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 _startPosition;
    public float damage = 1;

    private Boss boss;

    void Start()
    {
        _startPosition = transform.position;
        boss = GameObject.FindGameObjectWithTag("Boss Head").GetComponent<Boss>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 ray = transform.position - _startPosition;
        Debug.DrawRay(_startPosition, ray, Color.red, 1);
        RaycastHit2D hit = Physics2D.Raycast(_startPosition, ray.normalized, ray.magnitude, ~(1 << 9 | 1 << 10));
        if (hit.collider)
        {
            Destroy(gameObject);
            if (hit.collider.tag == "Boss Head")
            {
                boss.TakeDamage(damage, "Head");
            }
            else if (hit.collider.tag == "Boss Left Arm")
            {
                boss.TakeDamage(damage, "LeftArm");
            }
            else if (hit.collider.tag == "Boss Right Arm")
            {
                boss.TakeDamage(damage, "RightArm");
            }
        }
        _startPosition = transform.position;
    }
}
