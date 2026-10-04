using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public class Pistol3D : Weapon
    {
        public override void Equip(IPlayerController player)
        {
            Debug.Log("No special affects.");
        }

        public override void Unequip(IPlayerController player)
        {
            Debug.Log("Pistol Cant be unequiped.");
        }

        public override void Animate(IPlayerController player)
        {
        }
    }
}