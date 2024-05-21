using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShipController : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5.0f;
    [SerializeField] Projectile _projectilePrefab;
    [SerializeField] float _projectileShotsPerSecond = 5.0f;

    Rigidbody2D _rigidbody2D;
    PlayerInput _playerInput;

    Vector2 _movementInput;

    InputAction _fireInputAction;
    float _fireRate;
    float _timeSinceLastShot;

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

    void ResetTimeSinceLastShot() { _timeSinceLastShot = _fireRate; }

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();
        _currentPowerUpState = PowerUpState.Level_0;

        // Input
        _fireInputAction = _playerInput.actions["Fire"];

        // Weapons / Projectiles
        _fireRate = 1.0f / _projectileShotsPerSecond;

        // Initialize "time since last shot" to the fire rate, so there is no delay on the very first shot
        ResetTimeSinceLastShot();
    }

    void Update()
    {
        // Fire projectiles if Fire button is held down
        if (_fireInputAction.IsPressed())
        {
            _timeSinceLastShot += Time.deltaTime;
            if (_timeSinceLastShot >= _fireRate)
            {
                // Fire projectile/projectiles
                FireProjectile();
                _timeSinceLastShot = 0.0f;
            }
        }
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
        // No need to check if pressed here. We check if the button is held in Update().
    }

    void OnMove(InputValue inputValue)
    {
        _movementInput = inputValue.Get<Vector2>();
    }

    void OnIncreasePowerUpState()
    {
        IncrementPowerUpState();
    }

    void OnDecreasePowerUpState()
    {
        DecrementPowerUpState();
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

    void FireProjectile()
    {
        GameObject.Instantiate<Projectile>(_projectilePrefab, _rigidbody2D.position, Quaternion.identity);
    }
}
