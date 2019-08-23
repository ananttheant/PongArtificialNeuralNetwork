using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBall : MonoBehaviour
{
    Vector3 ballStartPos;

    Rigidbody2D rb;

    float force = 400;

    public AudioSource blip;
    public AudioSource blop;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();

        ballStartPos = transform.position;
        ResetBall();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "backwall")
        {
            blop.Play();
        }
        else
        {
            blip.Play();
        }
    }

    public void ResetBall()
    {
        transform.position = ballStartPos;
        rb.velocity = Vector3.zero;

        Vector3 dir = new Vector3(Random.Range(100,300), Random.Range(-100,100), 0).normalized;
        rb.AddForce(dir * force);
    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            ResetBall();
        }
    }

}
