using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatforms : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    private Vector3 pointAPos;
    private Vector3 pointBPos;
    public float speed = 2f;
    private Vector3 target;


    private void Start()
    {
        pointAPos = pointA.position;
        pointBPos = pointB.position;
        target = pointBPos;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            if (target == pointBPos)
            {
                target = pointAPos;
            }
            else
            {
                target = pointBPos;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}