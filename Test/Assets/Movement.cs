using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField]float aSpeed = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector2 a = Vector2.zero;
        if (Input.GetKey(KeyCode.D))
            a.x += 1;
        if (Input.GetKey(KeyCode.A))
            a.x -= 1;
        if (Input.GetKey(KeyCode.W))
            a.y += 1;
        if (Input.GetKey(KeyCode.S))
            a.y -= 1;
        a.Normalize();

        rb.velocity = a * aSpeed;
    }
}
