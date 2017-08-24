using UnityEngine;
using System.Collections;

public class AttackBox : MonoBehaviour
{

    // Damage dealt on hit
    int damage = 1;
    // Who's holding me?
    [SerializeField] public GameObject holder;
    // Where am I being held / what hand is holding me?
    [SerializeField] Transform handHolding;
 
    // Run on object load
    void Start()
    {
        // Ignore collisions with my holder
        Physics.IgnoreCollision(GetComponent<Collider>(), holder.GetComponent<Collider>());
        // Snap me to the hand that's holding me
        transform.parent = handHolding;
        transform.position = handHolding.position;
        transform.rotation = handHolding.rotation;
    }

// Run whenever something enters my collider
    void OnTriggerEnter(Collider other)
    {
        // Get the Actor component of the thing that I hit
        PlayerHealth enemyHealth = other.GetComponent<PlayerHealth>();
        // If that exists (i.e., I hit an Actor)
        if (enemyHealth)
        {
            PlayerShooting enemyShoot = other.GetComponent<PlayerShooting>();
            bool wasKillShot = enemyHealth.TakeDamage(damage);
            Debug.Log("Damage Landed: " + damage);

            if (wasKillShot && enemyShoot.IncreaseScore() >= enemyShoot.getKillsToWin())
                holder.GetComponent<Player>().Won();
        }
    }

}
