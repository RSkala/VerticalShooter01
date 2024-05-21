using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field:SerializeField] public PlayerShipSprites[] PlayerShipSpritesArray;

    [System.Serializable]
    public class PlayerShipSprites
    {
        [field:SerializeField] public Sprite ShipSpriteLeft;    // Sprite when player moving left
        [field:SerializeField] public Sprite ShipSpriteCenter;  // Sprite when player not moving
        [field:SerializeField] public Sprite ShipSpriteRight;   // Sprite when player moving right 
    }

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
