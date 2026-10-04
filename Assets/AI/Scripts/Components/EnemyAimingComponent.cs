using Assets.Scripts.Interfaces;
using UnityEngine;
using Weapon = Assets.Scripts.GameObjects.Weapon;

public class EnemyAimingComponent : MonoBehaviour
{

    /// <summary>
    /// The muzzle of the weapon
    /// </summary>
    public GameObject Muzzle;

    /// <summary>
    /// The angle to the enemy
    /// </summary>
    public float AngleToEnemy;

    public float RotationSpeed = 0.3f;

    void Update()
    {
        // Check if muzzle is null
        if (Muzzle == null)
        {
            // Get weapon
            var weapon = GetComponentInChildren<IWeaponary>();
            if (weapon != null)
            {
                // Cast the weapon
                var castWeapon = weapon as Weapon;
                if (castWeapon != null)
                {
                    // Get the game object
                    var weaponGameobject = castWeapon.gameObject;
                    if (weaponGameobject != null)
                    {
                        // Try find the muzzle
                        var muzzle = weaponGameobject.transform.Find("Muzzle");
                        if (muzzle != null)
                        {
                            // Set the muzzle
                            Muzzle = muzzle.gameObject;
                        }
                    }
                }
            }
        }
    }
}
