using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShipController : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5.0f;
    [SerializeField] Projectile _projectilePrefab;

    Rigidbody2D _rigidbody2D;
    Vector2 _movementInput;

    enum PowerUpState
    {
        Level_0 = 0, // Double Straight Fire (Default, No PowerUps)
        Level_1 = 1, // Double Straight Fire + 1 Backwards Fire
        Level_2 = 2, // Single Straight Fire + 2 Angle Fire + 1 Backwards Fire
        Level_3 = 3, // Single Straight Fire + 2 Angle Fire + 2 Backwards Fire
        Level_4 = 4, // Double Straight Fire + 2 Angle Fire + 2 Backwards Fire
        NumPowerUpStates = 5
    }

    PowerUpState _currentPowerUpState;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _currentPowerUpState = PowerUpState.Level_0;
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
        GameObject.Instantiate<Projectile>(_projectilePrefab, _rigidbody2D.position, Quaternion.identity);
    }

    void OnMove(InputValue inputValue)
    {
        _movementInput = inputValue.Get<Vector2>();
    }

    void OnIncreasePowerUpState()
    {
        Debug.Log("OnIncreasePowerUpState");
        IncrementPowerUpState();
        Debug.Log(" -> _currentPowerUpState: " + _currentPowerUpState);
    }

    void OnDecreasePowerUpState()
    {
        Debug.Log("OnDecreasePowerUpState");
        DecrementPowerUpState();
        Debug.Log(" -> _currentPowerUpState: " + _currentPowerUpState);
    }

    void IncrementPowerUpState()
    {
        int nextPowerUpState = (int)_currentPowerUpState + 1;
        if(nextPowerUpState >= (int)PowerUpState.NumPowerUpStates)
        {
            nextPowerUpState = (int)PowerUpState.Level_4;
        }
        _currentPowerUpState = (PowerUpState)nextPowerUpState;
    }

    void DecrementPowerUpState()
    {
        int nextPowerUpState = (int)_currentPowerUpState - 1;
        if (nextPowerUpState <= (int)PowerUpState.Level_0)
        {
            nextPowerUpState = (int)PowerUpState.Level_0;
        }
        _currentPowerUpState = (PowerUpState)nextPowerUpState;
    }
}
