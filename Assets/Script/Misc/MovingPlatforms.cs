using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class MovingPlatforms : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 0.5f;
    private Vector3 target;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        target = pointB.position; //The end point

    }

    private void Update()
    {
        Vector3 pointAOrigin = pointA.position;
        Vector3 pointBOrigin = pointB.position;
        Vector3 direction = (target - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target);

        rb.AddForce(direction * speed, ForceMode.VelocityChange);

        if (distance < 0.1f)
        {
            rb.velocity = Vector3.zero;
            target = (target == pointB.position) ? pointAOrigin : pointBOrigin;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform); // Attach player to platform
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null); // Detach player from platform
        }
    }
}
