using UnityEngine;
using UnityEngine.UI;

public class PlayerCanvas : MonoBehaviour
{
    public static PlayerCanvas canvas;

    [Header("Component References")]
    [SerializeField]
    Image reticule;
    [SerializeField] UIFader damageImage;
    [SerializeField] Text gameStatusText;
    [SerializeField] Text healthValue;
    [SerializeField] Text killsValue;
    [SerializeField] Text logText;
    [SerializeField] Text clip;
    [SerializeField] Text totalAmmo;
    [SerializeField] Text weaponName;
    [SerializeField] Text shieldValue;
    [SerializeField] AudioSource deathAudio;

    //Ensure there is only one PlayerCanvas
    void Awake()
    {
        if (canvas == null)
            canvas = this;
        else if (canvas != this)
            Destroy(gameObject);
    }

    //Find all of our resources
    void Reset()
    {
        reticule = GameObject.Find("Reticule").GetComponent<Image>();
        damageImage = GameObject.Find("DamagedFlash").GetComponent<UIFader>();
        gameStatusText = GameObject.Find("StatusText").GetComponent<Text>();
        healthValue = GameObject.Find("Health").GetComponent<Text>();
        shieldValue = GameObject.Find("Shield").GetComponent<Text>();
        killsValue = GameObject.Find("Kills").GetComponent<Text>();
        logText = GameObject.Find("Log").GetComponent<Text>();
        clip = GameObject.Find("Clip").GetComponent<Text>();
        totalAmmo = GameObject.Find("TotalAmmo").GetComponent<Text>();
        weaponName = GameObject.Find("WeaponName").GetComponent<Text>();
        deathAudio = GameObject.Find("DeathAudio").GetComponent<AudioSource>();
    }

    public void Initialize()
    {
        reticule.enabled = true;
        gameStatusText.text = "";
    }

    public void HideReticule()
    {
        reticule.enabled = false;
    }

    public void FlashDamageEffect()
    {
        damageImage.Flash();
    }

    public void PlayDeathAudio()
    {
        if (!deathAudio.isPlaying)
            deathAudio.Play();
    }

    public void SetKills(int amount)
    {
        killsValue.text = amount.ToString();
    }

    public void SetHealth(int amount)
    {
        healthValue.text = amount.ToString();
    }

    public void SetClip(float amount)
    {
        clip.text = amount.ToString();
    }

    public void SetTotalAmmo(float amount)
    {
        totalAmmo.text = amount.ToString();
    }

    public void SetWeaponName(string name)
    {
        weaponName.text = name;
    }

    public void SetShield(float amount)
    {
        shieldValue.text = amount.ToString();
    }

    public void WriteGameStatusText(string text)
    {
        gameStatusText.text = text;
    }

    public void WriteLogText(string text, float duration)
    {
        CancelInvoke();
        logText.text = text;
        Invoke("ClearLogText", duration);
    }

    void ClearLogText()
    {
        logText.text = "";
    }
}