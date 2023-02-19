using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speedMultiplier;
    public float damage = 1;
    public float rotationSpeed = 0;

    public bool preventDespawn = false;

    private Rigidbody2D rb;
    private float _lifespan = 0;
    private float _lifespanMax = 2;

    // Start is called before the first frame update
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (rotationSpeed != 0)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Manager.deltaTime);
        }

        if (!preventDespawn)
        {
            _lifespan += Manager.deltaTime;
            if (_lifespan >= _lifespanMax)
            {
                Destroy(gameObject);
            }
        }
    }

    public void SetSpeed(Vector2 speed)
    {
        rb.velocity = speedMultiplier * speed;
    }

    public void Impact(bool isEnemy)
    {
        if (!preventDespawn)
        {
            Destroy(gameObject);
        }
    }
}