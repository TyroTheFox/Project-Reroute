using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

[System.Serializable]
public class ToggleEvent : UnityEvent<bool> { }

public class Player : NetworkBehaviour
{
    [SyncVar(hook = "OnNameChanged")] public string playerName;
    [SyncVar(hook = "OnColorChanged")] public Color playerColor;

    [SyncVar(hook = "OnStrengthChanged")] public float Strength = 5;
    [SyncVar(hook = "OnAgilityChanged")] public float Agility = 5;
    [SyncVar(hook = "OnConstitutionChanged")] public float Constitution = 5;
    [SyncVar(hook = "OnDexterityChanged")] public float Dexterity = 5;
    [SyncVar(hook = "OnOverloadChanged")] public float Overload = 5;

    [SerializeField] ToggleEvent onToggleShared;
    [SerializeField] ToggleEvent onToggleLocal;
    [SerializeField] ToggleEvent onToggleRemote;
    [SerializeField] float respawnTime = 5f;
    [SerializeField] UIUpdater[] upgradeOptions;
    [SerializeField] PointPool pp;

    static List<Player> players = new List<Player>();

    GameObject mainCamera;
    NetworkAnimator anim;
    public PlayerCanvas pCanvas;

    private bool showUMState = false;
    private bool showUMPrevState = false;

    void Start()
    {
        anim = GetComponent<NetworkAnimator>();
        mainCamera = Camera.main.gameObject;
        GetComponent<FirstPersonController>().UpdateRunSpeed(Agility);
        //GetComponent<PlayerHealth>().maxHealth = (int)Constitution;
        GetComponent<PlayerShooting>().damage = (int)Dexterity;
        GameObject cw = GetComponent<PlayerShooting>().currentWeapon;
        if (cw != null)
        {
            PlayerCanvas.canvas.SetClip(cw.GetComponent<Weapon>().getCurrentCharge());
            PlayerCanvas.canvas.SetTotalAmmo(cw.GetComponent<Weapon>().getMaxCharge());
            PlayerCanvas.canvas.SetWeaponName(cw.GetComponent<Weapon>().getName());
        }
        foreach (UIUpdater option in upgradeOptions)
        {
            if (option.stat == UIUpdater.Stat.Strength)
            {
                option.UpdateStatValue(Strength);
            }
            if (option.stat == UIUpdater.Stat.Agility)
            {
                option.UpdateStatValue(Agility);
            }
            if (option.stat == UIUpdater.Stat.Constitution)
            {
                option.UpdateStatValue(Constitution);
            }
            if (option.stat == UIUpdater.Stat.Dexterity)
            {
                option.UpdateStatValue(Dexterity);
            }
            if (option.stat == UIUpdater.Stat.Overload)
            {
                option.UpdateStatValue(Overload);
            }
        }
        EnablePlayer();
    }

    [ServerCallback]
    void OnEnable()
    {
        if (!players.Contains(this))
            players.Add(this);
    }

    [ServerCallback]
    void OnDisable()
    {
        if (players.Contains(this))
            players.Remove(this);
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        anim.animator.SetFloat("Speed", Input.GetAxis("Vertical"));
        anim.animator.SetFloat("Strafe", Input.GetAxis("Horizontal"));

        showUMPrevState = showUMState;
        showUMState = Input.GetKey(KeyCode.E);

        GameObject cw = GetComponent<PlayerShooting>().currentWeapon;
        if (cw != null)
        {
            PlayerCanvas.canvas.SetClip(cw.GetComponent<Weapon>().getCurrentCharge());
            PlayerCanvas.canvas.SetTotalAmmo(cw.GetComponent<Weapon>().getMaxCharge());
            PlayerCanvas.canvas.SetWeaponName(cw.GetComponent<Weapon>().getName());
        }

        if (!showUMPrevState && showUMState)
        {
            GetComponent<FirstPersonController>().m_MouseLook.SetCursorLock(!GetComponent<FirstPersonController>().m_MouseLook.lockCursor);
            GameObject c = GameObject.FindGameObjectWithTag("HUD");
            c.GetComponent<Canvas>().enabled = !c.GetComponent<Canvas>().enabled;
            c = GameObject.FindGameObjectWithTag("Upgrade Screen");
            c.GetComponent<Canvas>().enabled = !c.GetComponent<Canvas>().enabled;
            GetComponent<PlayerShooting>().disableShoot = !GetComponent<PlayerShooting>().disableShoot;
            //Debug.Log("Lock " + GetComponent<FirstPersonController>().m_MouseLook.lockCursor);
           // Debug.Log("E");
        }
    }

    void DisablePlayer()
    {
        if (isLocalPlayer)
        {
            PlayerCanvas.canvas.HideReticule();
            mainCamera.SetActive(true);
        }

        onToggleShared.Invoke(false);

        if (isLocalPlayer)
            onToggleLocal.Invoke(false);
        else
            onToggleRemote.Invoke(false);
    }

    void EnablePlayer()
    {
        if (isLocalPlayer)
        {
            PlayerCanvas.canvas.Initialize();
            mainCamera.SetActive(false);
        }

        onToggleShared.Invoke(true);

        if (isLocalPlayer)
            onToggleLocal.Invoke(true);
        else
            onToggleRemote.Invoke(true);
    }

    public void Die()
    {
        if (isLocalPlayer || playerControllerId == -1)
            anim.SetTrigger("Died");

        if (isLocalPlayer)
        {
            PlayerCanvas.canvas.WriteGameStatusText("You Died!");
            PlayerCanvas.canvas.PlayDeathAudio();
        }

        DisablePlayer();

        Invoke("Respawn", respawnTime);
    }

    void Respawn()
    {
        if (isLocalPlayer || playerControllerId == -1)
            anim.SetTrigger("Restart");

        if (isLocalPlayer)
        {
            Transform spawn = NetworkManager.singleton.GetStartPosition();
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
        }

        EnablePlayer();
    }

    void OnNameChanged(string value)
    {
        playerName = value;
        gameObject.name = playerName;
        GetComponentInChildren<Text>(true).text = playerName;
    }

    void OnColorChanged(Color value)
    {
        playerColor = value;
        GetComponentInChildren<RendererToggler>().ChangeColor(playerColor);
    }

    void OnStrengthChanged(float s)
    {
        Strength = s;
    }

    void OnAgilityChanged(float a)
    {
        Agility = a;
    }

    void OnConstitutionChanged(float c)
    {
        Constitution = c;
    }

    void OnDexterityChanged(float d)
    {
        Dexterity = d;
    }

    void OnOverloadChanged(float o)
    {
        Overload = o;
    }

    public void SetStrength(float s)
    {
        if (isLocalPlayer)
        {
            Strength = s;
            pp.SetTotal((int)s);
        }
    }

    public void IncreaseStrength(float s)
    {
        if (isLocalPlayer && s <= pp.currentTotal && pp.currentTotal > 0)
        {
            Strength += s;
            pp.DecreaseTotal((int)s);
            Debug.Log("Strength +" + s);
            foreach (UIUpdater option in upgradeOptions)
            {
                if (option.stat == UIUpdater.Stat.Strength)
                {
                    option.UpdateStatValue(Strength);
                }
            }
        }
    }

    public void DecreaseStrength(float s)
    {
        if (isLocalPlayer && Strength > 0)
        {
            Strength -= s;
            pp.IncreaseTotal((int)s);
            Debug.Log("Strength -" + s);
            foreach (UIUpdater option in upgradeOptions)
            {
                if (option.stat == UIUpdater.Stat.Strength)
                {
                    option.UpdateStatValue(Strength);
                }
            }
        }
    }

    public void SetAgility(float a)
    {
        if (isLocalPlayer)
        {
            Agility = a;
            GetComponent<FirstPersonController>().UpdateRunSpeed(Agility);
            pp.SetTotal((int)a);
        }
    }

    public void IncreaseAgility(float a)
    {
        if (isLocalPlayer && a <= pp.currentTotal && pp.currentTotal > 0)
        {
            Agility += a;
            pp.DecreaseTotal((int)a);
            Debug.Log("Agility +" + a);
            GetComponent<FirstPersonController>().UpdateRunSpeed(Agility);
            foreach (UIUpdater option in upgradeOptions)
            {
                if (option.stat == UIUpdater.Stat.Agility)
                {
                    option.UpdateStatValue(Agility);
                }
            }
        }
    }

    public void DecreaseAgility(float a)
    {
        if (isLocalPlayer && Agility > 0)
        {
            Agility -= a;
            pp.IncreaseTotal((int)a);
            Debug.Log("Agility -" + a);
            GetComponent<FirstPersonController>().UpdateRunSpeed(Agility);
            foreach (UIUpdater option in upgradeOptions)
            {
                if (option.stat == UIUpdater.Stat.Agility)
                {
                    option.UpdateStatValue(Agility);
                }
            }
        }
    }

    public void SetConstitution(float c)
    {
        if (isLocalPlayer)
        {
            Constitution = c;
            pp.SetTotal((int)c);
            Debug.Log("Constitution " + c);
            GetComponent<PlayerHealth>().maxHealth = (int)Constitution * 25;
            GetComponent<PlayerHealth>().SetHealth((int)Constitution * 25);
            foreach (UIUpdater option in upgradeOptions)
            {
                if (option.stat == UIUpdater.Stat.Constitution)
                {
                    option.UpdateStatValue(Constitution);
                }
            }
        }
    }

    public void IncreaseConstitution(float c)
    {
        if (isLocalPlayer && c <= pp.currentTotal && pp.currentTotal > 0)
        {
            Constitution += c;
            pp.DecreaseTotal((int)c);
            Debug.Log("Constitution +" + c);
            GetComponent<PlayerHealth>().maxHealth = (int)Constitution * 25;
            GetComponent<PlayerHealth>().SetHealth((int)Constitution * 25);
            foreach (UIUpdater option in upgradeOptions)
            {
                if (option.stat == UIUpdater.Stat.Constitution)
                {
                    option.UpdateStatValue(Constitution);
                }
            }
        }
    }

    public void DecreaseConstitution(float c)
    {
        if (isLocalPlayer && Constitution > 0)
        {
            Constitution -= c;
            pp.IncreaseTotal((int)c);
            Debug.Log("Constitution -" + c);
            GetComponent<PlayerHealth>().maxHealth = (int)Constitution * 25;
            GetComponent<PlayerHealth>().SetHealth((int)Constitution * 25);
            foreach (UIUpdater option in upgradeOptions)
            {
                if (option.stat == UIUpdater.Stat.Constitution)
                {
                    option.UpdateStatValue(Constitution);
                }
            }
        }
    }

    public void SetDexterity(float d)
    {
        if (isLocalPlayer)
        {
            Dexterity = d;
            pp.SetTotal((int)d);
            Debug.Log("Dexterity " + d);
            GetComponent<PlayerShooting>().damage = (int)Dexterity;
            foreach (UIUpdater option in upgradeOptions)
            {
                if (option.stat == UIUpdater.Stat.Dexterity)
                {
                    option.UpdateStatValue(Dexterity);
                }
            }
        }
    }

    public void IncreaseDexterity(float d)
    {
        if (isLocalPlayer && d <= pp.currentTotal && pp.currentTotal > 0)
        {
            Dexterity += d;
            pp.DecreaseTotal((int)d);
            Debug.Log("Dexterity +" + d);
            GetComponent<PlayerShooting>().damage = (int)Dexterity;
            foreach (UIUpdater option in upgradeOptions)
            {
                if (option.stat == UIUpdater.Stat.Dexterity)
                {
                    option.UpdateStatValue(Dexterity);
                }
            }
        }
    }

    public void DecreaseDexterity(float d)
    {
        if (isLocalPlayer && Dexterity > 0)
        {
            Dexterity -= d;
            pp.IncreaseTotal((int)d);
            Debug.Log("Dexterity -" + d);
            GetComponent<PlayerShooting>().damage = (int)Dexterity;
            foreach (UIUpdater option in upgradeOptions)
            {
                if (option.stat == UIUpdater.Stat.Dexterity)
                {
                    option.UpdateStatValue(Dexterity);
                }
            }
        }
    }

    public void SetOverload(float o)
    {
        if (isLocalPlayer)
        {
            Overload = o;
            pp.SetTotal((int)o);
        }
    }

    public void IncreaseOverload(float o)
    {
        if (isLocalPlayer && o <= pp.currentTotal && pp.currentTotal > 0)
        {
            Overload += o;
            pp.DecreaseTotal((int)o);
            Debug.Log("Overload +" + o);
            foreach (UIUpdater option in upgradeOptions)
            {
                if (option.stat == UIUpdater.Stat.Overload)
                {
                    option.UpdateStatValue(Overload);
                }
            }
        }
    }

    public void DecreaseOverload(float o)
    {
        if (isLocalPlayer && Overload > 0)
        {
            Overload -= o;
            pp.IncreaseTotal((int)o);
            Debug.Log("Overload -" + o);
            foreach (UIUpdater option in upgradeOptions)
            {
                if (option.stat == UIUpdater.Stat.Overload)
                {
                    option.UpdateStatValue(Overload);
                }
            }
        }
    }

    [Server]
    public void Won()
    {
        for (int i = 0; i < players.Count; i++)
            players[i].RpcGameOver(netId, name);

        Invoke("BackToLobby", 5f);
    }

    [ClientRpc]
    void RpcGameOver(NetworkInstanceId networkID, string name)
    {
        DisablePlayer();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (isLocalPlayer)
        {
            if (netId == networkID)
                PlayerCanvas.canvas.WriteGameStatusText("You Won!");
            else
                PlayerCanvas.canvas.WriteGameStatusText("Game Over!\n" + name + " Won");
        }
    }

    void BackToLobby()
    {
        FindObjectOfType<NetworkLobbyManager>().SendReturnToLobby();
    }
}