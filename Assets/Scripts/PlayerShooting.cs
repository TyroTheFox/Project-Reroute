using UnityEngine;
using UnityEngine.Networking;

public class PlayerShooting : NetworkBehaviour
{
    //[SerializeField] float shotCooldown = .3f;
    [SerializeField] int killsToWin = 5;
    [SerializeField] Transform firePosition;
    [SerializeField] ShotEffectsManager shotEffects;
    [SerializeField] public int damage = 5;
    public bool disableShoot = false;

    Weapon[] playerWeapons = {
        new Weapon("Gun1", 1, 5, 1, 0.5f, 1, 10, 50),
        new Weapon("Gun2", 2, 10, 10, 3, 10, 10, 10),
        new Weapon("Gun3", 1, 15, 15, 0.9f, 5, 10, 10)
    };
    Weapon currentWeapon = new Weapon("Error", 0, 0, 0, 0, 0, 0, 0);
    int weaponPos = 0;

    [SyncVar(hook = "OnScoreChanged")] int score;

    Player player;
    float ellapsedTime;
    bool canShoot;

    void Start()
    {
        player = GetComponent<Player>();
        shotEffects.Initialize();
        currentWeapon = playerWeapons[0];
        Debug.Log("Current Weapon: " + currentWeapon.getName());

        if (isLocalPlayer)
        {
            canShoot = true;
        }
    }

    [ServerCallback]
    void OnEnable()
    {
        score = 0;
    }

    void Update()
    {
        if (!canShoot)
            return;

        ellapsedTime += Time.deltaTime;
        player = GetComponent<Player>();

        if (Input.GetButtonDown("Fire1") && ellapsedTime > currentWeapon.getFireRate() && !disableShoot)
        {
            ellapsedTime = 0f;
            CmdFireShot(firePosition.position, firePosition.forward, currentWeapon.getDamage(),
                currentWeapon.getShots(), currentWeapon.getRange(), currentWeapon.getSpread(), player.Dexterity);
        }

        if (Input.GetButtonDown("ChangeWeapon"))
        {
            weaponPos++;
            if (weaponPos == playerWeapons.Length)
            {
                weaponPos = 0;
            }
            currentWeapon = playerWeapons[weaponPos];
            Debug.Log("Current Weapon: " + currentWeapon.getName());
        }
    }

    [Command]
    void CmdFireShot(Vector3 origin, Vector3 direction, int damage, int noOfShots, float range,
        float spread, float dex)
    {
        RaycastHit hit;

        for (int i = 0; i < noOfShots; i++)
        {
            ////  Generate a random XY point inside a circle:
            //Vector3 newDirection = Random.insideUnitCircle * spread;
            //newDirection.z = dex * range; // circle is at Z units 
            //newDirection = transform.TransformDirection(newDirection.normalized);
            direction = MathCone.RandomInCone(direction, (Mathf.Deg2Rad * (10 - dex)));
            Ray ray = new Ray(origin, direction);
            if (isLocalPlayer)
                Debug.DrawRay(ray.origin, ray.direction * 3f, Color.red, 1f);

            bool result = Physics.Raycast(ray, out hit, range);

            if (result)
            {
                PlayerHealth enemy = hit.transform.GetComponent<PlayerHealth>();

                if (enemy != null)
                {
                    bool wasKillShot = enemy.TakeDamage(damage);
                    Debug.Log("Damage Landed: " + damage);

                    if (wasKillShot && ++score >= killsToWin)
                        player.Won();
                }
            }

            RpcProcessShotEffects(result, hit.point);
        }

    }

    [ClientRpc]
    void RpcProcessShotEffects(bool playImpact, Vector3 point)
    {
        shotEffects.PlayShotEffects();

        if (playImpact)
            shotEffects.PlayImpactEffect(point);
    }

    void OnScoreChanged(int value)
    {
        score = value;
        if (isLocalPlayer)
            PlayerCanvas.canvas.SetKills(value);
    }

    public void FireAsBot()
    {
        CmdFireShot(firePosition.position, firePosition.forward, currentWeapon.getDamage(),
                currentWeapon.getShots(), currentWeapon.getRange(), currentWeapon.getSpread(), player.Dexterity);
    }

}

public static class MathCone
{
    public static Vector3 RandomInCone(this Vector3 dir, float openingAngle)
    {
        Vector3 deviation = Random.insideUnitSphere;
        dir = dir.normalized;
        deviation -= Vector3.Dot(deviation, dir) * dir;
        return (dir * Mathf.Cos(openingAngle) + deviation * Mathf.Sin(openingAngle)).normalized;
    }
}