using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] int killsToWin = 5;
    [SerializeField] Transform firePosition;
    ShotEffectsManager shotEffects;
    [SerializeField] public int damage = 5;
    [SerializeField] GameObject GunCamera;
    [SerializeField] GunPositionSync GPS;
    public bool disableShoot = false;
    [SerializeField] GameObject[] gameWeapons = { };
    [SerializeField] GameObject[] playerWeapons = {};
   /* new Weapon("Revolver", 90, 2, 1, 1, 10, 60, 35, Weapon.ShootMode.NON),
        new Weapon("SMG", 23, 3, 1, 0.2f, 2, 80, 25, Weapon.ShootMode.AUTO),
        new Weapon("Carbine", 50, 2.5f, 1, 0.33f, 5, 45, 10, Weapon.ShootMode.NON),
        new Weapon("Pump-Action Shotgun", 10, 10, 15, 1.5f, 5, 20, 180, Weapon.ShootMode.NON)*/
    public GameObject currentWeapon = null;
    int weaponPos = 0;

    [SyncVar(hook = "OnScoreChanged")] int score;

    Player player;
    float ellapsedTime;
    bool canShoot;

    bool swinging = false;

    void Start()
    {
        player = GetComponent<Player>();
        playerWeapons = gameWeapons;
        currentWeapon = playerWeapons[0];
        LoadCurrentWeapon();

        if (isLocalPlayer)
            canShoot = true;
    }

    public void LoadCurrentWeapon()
    {
        if (GunCamera.GetComponentsInChildren<Transform>().Length > 0)
        {
            var children = new List<GameObject>();
            foreach (Transform child in GunCamera.transform) children.Add(child.gameObject);
            children.ForEach(child => child.GetComponent<Weapon>().setModel(false));
        }
        
        GPS.leftHandHold = currentWeapon.GetComponent<Weapon>().getLeftHandHold();
        GPS.rightHandHold = currentWeapon.GetComponent<Weapon>().getRightHandHold();
        shotEffects = currentWeapon.GetComponent<Weapon>().getShotEffects();
        if (shotEffects != null) shotEffects.Initialize();
        if(currentWeapon.GetComponent<Weapon>().getAttackBox() != null)
        {
            currentWeapon.GetComponent<Weapon>().getAttackBox().holder = gameObject;
        }

        currentWeapon.GetComponent<Weapon>().setModel(true);
        ellapsedTime = 0f;

        if (isLocalPlayer)
            Debug.Log("WEAPON LOADED: " + currentWeapon.name);
        
    }

    [ServerCallback]
    void OnEnable()
    {
        score = 0;
    }

    public int IncreaseScore()
    {
        score++;
        return score;
    }

    public int getKillsToWin()
    {
        return killsToWin;
    }

    void Update()
    {
        if (!canShoot)
            return;
        
        ellapsedTime += Time.deltaTime;
        player = GetComponent<Player>();

        Weapon cw = currentWeapon.GetComponent<Weapon>();
        Debug.Log(cw.getName());
        if (cw.getShootMode() == Weapon.ShootMode.NON)
        {
            if (Input.GetButtonDown("Fire1") && ellapsedTime > cw.getFireRate() && !disableShoot
                && cw.getCurrentCharge() > 0)
            {
                Debug.Log("RECHARGE!");
                ellapsedTime = 0f;
                cw.MakeShot();
                cw.getAnimator().SetTrigger("Fire");
                CmdFireShot(firePosition.position, firePosition.forward, cw.getDamage(),
                    cw.getShots(), cw.getRange(), cw.getSpread(), player.Dexterity);
            }
        }

        if (cw.getShootMode() == Weapon.ShootMode.AUTO)
        {
            if (Input.GetButton("Fire1") && ellapsedTime > cw.getFireRate() && !disableShoot
                && cw.getCurrentCharge() > 0)
            {
                ellapsedTime = 0f;
                cw.MakeShot();
                cw.getAnimator().SetTrigger("Fire");
                CmdFireShot(firePosition.position, firePosition.forward, cw.getDamage(),
                    cw.getShots(), cw.getRange(), cw.getSpread(), player.Dexterity);
            }
        }

        if (cw.getShootMode() == Weapon.ShootMode.MELEE)
        {
            if (Input.GetButton("Fire1") && ellapsedTime > 0.7f && !disableShoot)
            {
                ellapsedTime = 0f;
                cw.getMeleeBox().enabled = true;
                swinging = true;
                cw.getAnimator().SetTrigger("Swing");
            }

            if (ellapsedTime > 2f && swinging)
            {
                ellapsedTime = 0f;
                cw.getMeleeBox().enabled = true;
                swinging = false;
            }
        }

        if (Input.GetButtonDown("ChangeWeapon"))
        {
            weaponPos++;
            if (weaponPos == playerWeapons.Length)
            {
                weaponPos = 0;
            }
            currentWeapon = playerWeapons[weaponPos];
            Debug.Log(playerWeapons[weaponPos]);
            LoadCurrentWeapon();
            Debug.Log("Current Weapon: " + cw.getName());
        }

        if (Input.GetButton("Reload") && ellapsedTime > 1f)
        {
            ellapsedTime = 0f;
            cw.Recharge(10);
            Debug.Log("RECHARGE!");
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
            //direction = MathCone.RandomInCone(direction, (Mathf.Deg2Rad * (10 - dex)), spread, dex * range);
            direction = MathCone.RandomCone(direction, range, spread - dex);
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
        if(shotEffects != null)
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
        Weapon cw = currentWeapon.GetComponent<Weapon>();

        CmdFireShot(firePosition.position, firePosition.forward, cw.getDamage(),
                cw.getShots(), cw.getRange(), cw.getSpread(), player.Dexterity);
    }

}

public static class MathCone
{
    public static Vector3 RandomInCone(this Vector3 dir, float openingAngle, float spread, float range)
    {
        Vector3 deviation = Random.insideUnitCircle * spread;
        deviation.z = range;
        dir = dir.normalized;
        deviation -= Vector3.Dot(deviation, dir) * dir;
        return (dir * Mathf.Cos(openingAngle) + deviation * Mathf.Sin(openingAngle)).normalized;
    }

    public static Vector3 RandomCone(Vector3 aim, float distance, float variance)
    {
        aim.Normalize();
        Vector3 v3;
        do
        {
            v3 = Random.insideUnitSphere;
        } while (v3 == aim || v3 == -aim);
        v3 = Vector3.Cross(aim, v3);
        v3 = v3 * Random.Range(0.0f, variance);
        return aim * distance + v3;
    }
}