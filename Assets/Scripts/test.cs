using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public Transform target;

    // Update is called once per frame
    void Update()
    {
        Vector3 ray = target.position - transform.position;
        Debug.DrawRay(transform.position, ray, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, ray, ray.magnitude, ~(1 << 9 | 1 << 10));
        if (hit.collider)
        {
            Debug.Log(hit.collider.tag + "\n" + hit.collider.name);
        }
    }
}
