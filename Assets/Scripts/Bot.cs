using UnityEngine;
using UnityEngine.Networking;

public class Bot : NetworkBehaviour
{
    public bool botCanShoot = true;

    [SerializeField] float shotCooldown = 1f;

    PlayerShooting playerShooting;
    NetworkAnimator anim;
    float ellapsedTime = 0f;


    void Awake()
    {
        playerShooting = GetComponent<PlayerShooting>();
        anim = GetComponent<NetworkAnimator>();

        GetComponent<Player>().playerName = "Bot";
    }

    [ServerCallback]
    void Update()
    {
        anim.animator.SetFloat("Speed", 0f);
        anim.animator.SetFloat("Strafe", 0f);

        if (Input.GetKey(KeyCode.Keypad8))
            anim.animator.SetFloat("Speed", 1f);

        if (Input.GetKey(KeyCode.Keypad2))
            anim.animator.SetFloat("Speed", -1f);

        if (Input.GetKey(KeyCode.Keypad4))
            anim.animator.SetFloat("Strafe", -1f);

        if (Input.GetKey(KeyCode.Keypad6))
            anim.animator.SetFloat("Strafe", 1f);

        if (Input.GetKeyDown(KeyCode.Keypad7))
            anim.SetTrigger("Died");

        if (Input.GetKeyDown(KeyCode.Keypad9))
            anim.SetTrigger("Restart");


        BotAutoFire();
    }

    [Server]
    void BotAutoFire()
    {
        ellapsedTime += Time.deltaTime;

        if (ellapsedTime < shotCooldown)
            return;

        ellapsedTime = 0f;
        if (playerShooting.enabled)
            playerShooting.FireAsBot();
    }
}