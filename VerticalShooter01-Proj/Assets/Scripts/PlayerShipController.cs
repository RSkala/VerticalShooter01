using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShipController : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5.0f;

    Rigidbody2D _rigidbody2D;
    Vector2 _movementInput;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //Debug.Log("PlayerShipController.Update");
    }

    void FixedUpdate()
    {
        if(_movementInput != null)
        {
            Vector2 newPosition = _rigidbody2D.position + _movementInput * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody2D.MovePosition(newPosition);
        }
    }

    void OnFire(InputValue inputValue)
    {
        Debug.Log("OnFire");
    }

    void OnMove(InputValue inputValue)
    {
        _movementInput = inputValue.Get<Vector2>();
    }
}
