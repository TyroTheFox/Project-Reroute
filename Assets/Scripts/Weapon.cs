using UnityEngine;
using UnityEngine.Networking;
public class Weapon : MonoBehaviour{

    [SerializeField] string name = "Gun";
    [SerializeField] int damage = 0;
    [SerializeField] float spread = 0;
    [SerializeField] int shots = 0;
    [SerializeField] float fireRate = 0;
    [SerializeField] float chargePerShot = 0;
    float currentCharge = 0;
    [SerializeField] float maxCharge = 0;
    [SerializeField] float range = 0;
    public enum ShootMode {NON, AUTO, MELEE};
    [SerializeField] ShootMode sm = ShootMode.NON;
    [SerializeField] BoxCollider meleeBox;
    [SerializeField] AttackBox attackBox;
    [SerializeField] MeshRenderer model;
    [SerializeField] Transform rightHandHold;
    [SerializeField] Transform leftHandHold;
    [SerializeField] ShotEffectsManager shotEffects;
    [SerializeField] Animator animator;

    public void Start()
    {
        currentCharge = this.maxCharge;
    }

    // Use this for initialization
    public Weapon (string name, int damage, float spread, int shots, float fireRate, 
        float chargePerShot, float maxCharge, float range, ShootMode shootMode) {
        this.name = name;
        this.damage = damage;
        this.spread = spread;
        this.shots = shots;
        this.fireRate = fireRate;
        this.chargePerShot = chargePerShot;
        this.maxCharge = maxCharge;
        this.range = range;
        currentCharge = this.maxCharge;
        sm = shootMode;
	}

    public string getName() { return name; }
    public int getDamage() { return damage; }
    public float getSpread() { return spread; }
    public int getShots() { return shots; }
    public float getFireRate() { return fireRate; }
    public float getChargePerShot() { return chargePerShot; }
    public float getMaxCharge() { return maxCharge; }
    public float getRange() { return range; }
    public float getCurrentCharge() { return currentCharge; }
    public void MakeShot() { currentCharge -= chargePerShot; if (currentCharge < 0) { currentCharge = 0; } }
    public void Recharge(float i) { currentCharge += i; if (currentCharge > maxCharge) { currentCharge = maxCharge; } }
    public ShootMode getShootMode() { return sm; }
    public BoxCollider getMeleeBox() { return meleeBox; }
    public AttackBox getAttackBox() { return attackBox; }
    public Transform getLeftHandHold() { return leftHandHold; }
    public Transform getRightHandHold() { return rightHandHold; }
    public ShotEffectsManager getShotEffects() { return shotEffects; }
    public void setModel(bool b) { model.enabled = b; ; }
    public Animator getAnimator() { return animator; }
}
