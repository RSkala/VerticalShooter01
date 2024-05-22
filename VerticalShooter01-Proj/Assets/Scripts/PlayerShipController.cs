using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShipController : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5.0f;
    [SerializeField] Projectile _projectilePrefab;
    [SerializeField] float _projectileShotsPerSecond = 5.0f;
    [SerializeField] FirePointData[] _firePointDataArray;
    [SerializeField] int _defaultShipSpriteIndex = 0;
    [SerializeField] float _shipTurnDelayTime = 0.25f; // Number of seconds to wait before "turning" the player ship sprite to the side

    Rigidbody2D _rigidbody2D;
    PlayerInput _playerInput;
    SpriteRenderer _spriteRenderer;

    Vector2 _movementInput;

    InputAction _fireInputAction;
    float _fireRate;
    float _timeSinceLastShot;

    // Values for delaying the turning sprite transitions
    float _timeMovingLeft;
    float _timeMovingRight;

    public enum PowerUpState
    {
        Level_0 = 0, // Double Straight Fire (Default, No PowerUps)
        Level_1 = 1, // Double Straight Fire + 1 Straight Backwards Fire
        Level_2 = 2, // Single Straight Fire + 2 Short Angle Fire + 1 Straight Backwards Fire
        Level_3 = 3, // Single Straight Fire + 2 Long Angle Fire + 2 Angle Backwards Fire
        Level_4 = 4, // Double Straight Fire + 2 Long Angle Fire + 2 Angle Backwards Fire
    }

    PowerUpState _currentPowerUpState;
    FirePointData _currentFirePointData;
    int _currentShipSpriteIndex;

    Camera _mainCamera;

    [System.Serializable]
    public class FirePointData
    {
        // Note that default values do not work with serialized lists
        // https://issuetracker.unity3d.com/issues/serializefield-list-objects-are-not-initialized-with-class-slash-struct-default-values-when-adding-objects-in-the-inspector-window
        // https://forum.unity.com/threads/serializable-class-field-initializers-not-working.891484/
        // https://forum.unity.com/threads/default-values-for-serializable-class-not-supported.42499/

        // PowerUpState that this FirePointData is used by the player ship
        public PowerUpState powerUpState;

        // List of transforms used for firing projectiles
        public Transform[] firePointTransforms;

        // Whether the projectile should align/rotate to fire point transform directions when fired
        public bool alignToTransformDirection;
    }

    void ResetTimeSinceLastShot() { _timeSinceLastShot = _fireRate; }
    
    void Awake()
    {
        //Debug.Log("PlayerShipController.Awake");
    }

    void Start()
    {
        // Camera values
        _mainCamera = Camera.main;

        // Set Components
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize the player's ship level to 0
        _currentPowerUpState = PowerUpState.Level_0;

        // Input
        _fireInputAction = _playerInput.actions["Fire"];

        // Check Ship Sprite index
        if(_defaultShipSpriteIndex >= GameManager.Instance.PlayerShipSpritesArray.Length)
        {
            Debug.LogError("_defaultShipSpriteIndex " + _defaultShipSpriteIndex + " is greater than number of valid ship sprites " + GameManager.Instance.PlayerShipSpritesArray.Length);
        }
        _currentShipSpriteIndex = _defaultShipSpriteIndex;

        // Weapons / Projectiles
        _fireRate = 1.0f / _projectileShotsPerSecond;

        // Initialize "time since last shot" to the fire rate, so there is no delay on the very first shot
        ResetTimeSinceLastShot();

        // Ensure FirePointData array is valid on Start
        if (_firePointDataArray.Length == 0)
        {
            Debug.LogError("FirePointDataArray is empty. This must be filled with FirePointData in order to fire projectiles.");
        }

        for(int i = 0; i < _firePointDataArray.Length; ++i)
        {
            if (_firePointDataArray[i].firePointTransforms.Length == 0)
            {
                Debug.LogError("FirePointDataArray at index " + i + " has no fire point transforms. There must be at least 1 per level.");
            }
        }

        // Set the current fire point data (which will be level 0)
        SetCurrentFirePointDataForPowerUpLevel();
    }

    void Reset()
    {
        Debug.Log("PlayerShipController.Reset");
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
                FireProjectiles();
                _timeSinceLastShot = 0.0f;
            }
        }

        // Update the ship sprites
        UpdateShipSprites();
        RestrictShipToScreenBounds();
    }

    void RestrictShipToScreenBounds()
    {
        float shipWidth = _spriteRenderer.bounds.size.x / 2.0f;
        float shipHeight = _spriteRenderer.bounds.size.y / 2.0f;

        Vector3 lowerRightScreenPoint = new Vector3(Screen.width, Screen.height, _mainCamera.transform.position.z);
        Vector2 screenBounds = _mainCamera.ScreenToWorldPoint(lowerRightScreenPoint);

        //Vector2 playerShipPosition = _rigidbody2D.position;
        Vector2 playerShipPosition = transform.position;
        playerShipPosition.x = Mathf.Clamp(playerShipPosition.x, screenBounds.x * -1.0f + shipWidth, screenBounds.x - shipWidth);
        playerShipPosition.y = Mathf.Clamp(playerShipPosition.y, screenBounds.y * -1.0f + shipHeight, screenBounds.y - shipHeight);

        transform.position = playerShipPosition;
        //_rigidbody2D.position = playerShipPosition;
        //_rigidbody2D.MovePosition(playerShipPosition);
    }

    void UpdateShipSprites()
    {
        if (!_movementInput.Equals(Vector2.zero))
        {
            if (_movementInput.x < 0.0f)
            {
                // Player is holding LEFT
                _timeMovingLeft += Time.deltaTime;
                if (_timeMovingLeft >= _shipTurnDelayTime)
                {
                    // Player has held Left long enough to swap to the Left turning sprite
                    _spriteRenderer.sprite = GameManager.Instance.PlayerShipSpritesArray[_currentShipSpriteIndex].ShipSpriteLeft;
                }
                else
                {
                    // Player has NOT held Left long enough to swap sprites, so show the Center (no turn) sprite
                    _spriteRenderer.sprite = GameManager.Instance.PlayerShipSpritesArray[_currentShipSpriteIndex].ShipSpriteCenter;
                }
            }
            else if (_movementInput.x > 0.0f)
            {
                // Player is holding RIGHT
                _timeMovingRight += Time.deltaTime;
                if (_timeMovingRight >= _shipTurnDelayTime)
                {
                    // Player has held Right long enough to swap to the Right turning sprite
                    _spriteRenderer.sprite = GameManager.Instance.PlayerShipSpritesArray[_currentShipSpriteIndex].ShipSpriteRight;
                }
                else
                {
                    // Player has NOT held Right long enough to swap sprites, so show the Center (no turn) sprite
                    _spriteRenderer.sprite = GameManager.Instance.PlayerShipSpritesArray[_currentShipSpriteIndex].ShipSpriteCenter;
                }
            }
            else
            {
                // Player has input but is not pressing Left or Right (which means the ship is likely moving directly up or down)
                _spriteRenderer.sprite = GameManager.Instance.PlayerShipSpritesArray[_currentShipSpriteIndex].ShipSpriteCenter;
            }
        }
        else
        {
            // The player has NO input. Set the Center sprite and clear the input timers
            _spriteRenderer.sprite = GameManager.Instance.PlayerShipSpritesArray[_currentShipSpriteIndex].ShipSpriteCenter;
            _timeMovingLeft = _timeMovingRight = 0.0f;
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

        // Update the input time values
        UpdateInputTimeValues();
    }

    void UpdateInputTimeValues()
    {
        if (_movementInput.x < 0.0f)
        {
            // Player is pressing Left. Clear the "pressing Right" timer.
            _timeMovingRight = 0.0f;
        }

        if (_movementInput.x > 0.0f)
        {
            // Player is pressing Right. Clear the "pressing Left" timer.
            _timeMovingLeft = 0.0f;
        }

        if (Mathf.Approximately(_movementInput.x, 0.0f))
        {
            // Player is NOT pressing Left or Right. Clear the "pressing Left" and "pressing Right" timers.
            _timeMovingLeft = _timeMovingLeft = 0.0f;
        }
    }

    void OnValidate()
    {
        if(_defaultShipSpriteIndex < 0)
        {
            _defaultShipSpriteIndex = 0;
        }

        if(_projectileShotsPerSecond <= 0.0f)
        {
            _projectileShotsPerSecond = 1.0f;
        }
        _fireRate = 1.0f / _projectileShotsPerSecond;
    }

    void OnIncreasePowerUpState()
    {
        IncrementPowerUpState();
    }

    void OnDecreasePowerUpState()
    {
        DecrementPowerUpState();
    }

    // Debug function for changing ship sprite during game
    void OnChangeShipSprite()
    {
        _currentShipSpriteIndex++;
        if(_currentShipSpriteIndex >= GameManager.Instance.PlayerShipSpritesArray.Length)
        {
            _currentShipSpriteIndex = 0;
        }
    }

    void IncrementPowerUpState()
    {
        int nextPowerUpState = (int)_currentPowerUpState + 1;
        if(nextPowerUpState >= (int)PowerUpState.Level_4)
        {
            nextPowerUpState = (int)PowerUpState.Level_4;
        }
        _currentPowerUpState = (PowerUpState)nextPowerUpState;
        SetCurrentFirePointDataForPowerUpLevel();
    }

    void DecrementPowerUpState()
    {
        int nextPowerUpState = (int)_currentPowerUpState - 1;
        if (nextPowerUpState <= (int)PowerUpState.Level_0)
        {
            nextPowerUpState = (int)PowerUpState.Level_0;
        }
        _currentPowerUpState = (PowerUpState)nextPowerUpState;
        SetCurrentFirePointDataForPowerUpLevel();
    }

    void FireProjectiles()
    {
        foreach(Transform firePointTransform in _currentFirePointData.firePointTransforms)
        {
            FireProjectileFromFirePointTransform(firePointTransform);
        }
    }

    void FireProjectileFromFirePointTransform(Transform firePointTransform)
    {
        //GameObject.Instantiate<Projectile>(_projectilePrefab, _rigidbody2D.position, Quaternion.identity);
        GameObject.Instantiate<Projectile>(_projectilePrefab, firePointTransform.position, firePointTransform.rotation);
    }

    //void FireProjectile()
    //{
    //    GameObject.Instantiate<Projectile>(_projectilePrefab, _rigidbody2D.position, Quaternion.identity);
    //}

    void SetCurrentFirePointDataForPowerUpLevel()
    {
        _currentFirePointData = null;
        foreach (FirePointData firePointData in _firePointDataArray)
        {
            if (firePointData.powerUpState == _currentPowerUpState)
            {
                _currentFirePointData = firePointData;
                break;
            }
        }

        if(_currentFirePointData == null)
        {
            Debug.LogError("No FirePointData found for _currentPowerUpState " + _currentPowerUpState);
        }
    }
}
