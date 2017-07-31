using UnityEngine;
using UnityEngine.Networking;
public class Weapon{

    string name = "Gun";
    int damage = 0;
    float spread = 0;
    int shots = 0;
    float fireRate = 0;
    float chargePerShot = 0;
    float currentCharge = 0;
    float maxCharge = 0;
    float range = 0;
    public enum ShootMode {NON, AUTO};
    ShootMode sm = ShootMode.NON;

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

}
