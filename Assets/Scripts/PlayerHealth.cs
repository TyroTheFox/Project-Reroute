using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] public int maxHealth = 150;
    [SerializeField] public int maxShield = 150;

    [SyncVar(hook = "OnHealthChanged")] int health;
    [SyncVar(hook = "OnShieldChanged")] int shield;

    Player player;

    void Awake()
    {
        player = GetComponent<Player>();
    }

    [ServerCallback]
    void OnEnable()
    {
        health = maxHealth;
        shield = maxShield;
        if (isLocalPlayer)
        {
            Debug.Log("Health: " + health);
            Debug.Log("Shield: " + shield);
        }
    }

    [ServerCallback]
    void Start()
    {
        health = maxHealth;
        shield = maxShield;
        if (isLocalPlayer)
        {
            Debug.Log("Health: " + health);
            Debug.Log("Shield: " + shield);
        }
    }

    [Server]
    public bool TakeDamage(int damage)
    {
        bool died = false;

        if (health <= 0)
            return died;

        if(shield > 0)
        {
            shield -= damage;
            if (isLocalPlayer)
                Debug.Log("Shield: " + shield);
            return false;
        }
        health -= damage;
        if (isLocalPlayer)
            Debug.Log("Health: " + health);
        died = health <= 0;

        RpcTakeDamage(died);

        return died;
    }

    [Server]
    public void SetHealth(int newHealth)
    {
        health = newHealth;
        Debug.Log("Health Value 1: " + newHealth);
        if (isLocalPlayer)
            PlayerCanvas.canvas.SetHealth(health);
    }

    [Server]
    public void SetShield(int newShield)
    {
        shield = newShield;
        if (isLocalPlayer)
            PlayerCanvas.canvas.SetShield(shield);
    }

    [ClientRpc]
    void RpcTakeDamage(bool died)
    {
        if (isLocalPlayer)
            PlayerCanvas.canvas.FlashDamageEffect();

        if (died)
            player.Die();
    }

    void OnHealthChanged(int value)
    {
        health = value;
    }

    void OnShieldChanged(int value)
    {
        shield = value;
    }
}