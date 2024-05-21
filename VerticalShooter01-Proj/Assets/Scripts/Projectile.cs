using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 20.0f;
    [SerializeField] float _lifetimeSeconds = 10.0f;

    Rigidbody2D _rigidbody2D;
    Vector2 _movementDirection;

    float _timeAlive;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        // By default, move directly forward (2D up) direction
        // Use the "Up" vector as that is actually the forward vector in Unity 2D (Note: "forward" refers to the Z direction, i.e. in the camera facing direction)
        Vector2 movementDirection = _rigidbody2D.transform.up;
        Vector2 newPos = _rigidbody2D.position + movementDirection * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody2D.MovePosition(newPos);

        // Deactivate owning GameObject if time alive has exceeded the lifetime
        _timeAlive += Time.fixedDeltaTime;
        if (_timeAlive >= _lifetimeSeconds)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Projectile.OnCollisionEnter2D - " + gameObject.name + ", collision: " + collision.gameObject.name);
    }
}
