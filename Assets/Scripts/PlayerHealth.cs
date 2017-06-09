using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] public int maxHealth = 5;

    [SyncVar(hook = "OnHealthChanged")] int health;

    Player player;


    void Awake()
    {
        player = GetComponent<Player>();
    }

    [ServerCallback]
    void OnEnable()
    {
        health = maxHealth;
        if (isLocalPlayer)
            Debug.Log("Health: " + health);
    }

    [ServerCallback]
    void Start()
    {
        health = maxHealth;
        if (isLocalPlayer)
            Debug.Log("Health: " + health);
    }

    [Server]
    public bool TakeDamage(int damage)
    {
        bool died = false;

        if (health <= 0)
            return died;

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
        if (isLocalPlayer)
            PlayerCanvas.canvas.SetHealth(health);
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
        if (isLocalPlayer)
            Debug.Log("Health: " + health);
        if (isLocalPlayer)
            PlayerCanvas.canvas.SetHealth(value);
    }
}