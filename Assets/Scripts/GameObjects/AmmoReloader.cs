using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public class AmmoReloader : Equipment
    {
        public int AmmoAmount = 0;

        public override string ItemName()
        {
            return this.GetType().Name;
        }

        public override bool CanEquip(IPlayerController player)
        {
            var results = false;
            if (player == null)
            {
                results = base.CanEquip(player);
            }

            var wep = GetWeapon(player);

            // If there is a weapon
            if (wep != null)
            {
                // And it doesn't have a full clip.
                results = wep.GetCurrentAmmo() != wep.GetClipSize();
            }

            return results;
        }

        public override void Equip(IPlayerController player)
        {
            if (player != null)
            {
                // Equip this item instead.
                Debug.Log("Adjusting Player Stats");

                var wep = GetWeapon(player);
                if (wep != null)
                {
                    wep.Reload(AmmoAmount);
                }
            }
        }

        public override void Unequip(IPlayerController player)
        {
        }

        protected override void DropItem(IPlayerController player)
        {
        }

        protected IWeaponary GetWeapon(IPlayerController player)
        {
            Pistol3D wep = null;
            //// var hand = player.GetHandTransform(1);
            Transform hand = null;
            if (hand != null)
            {
                if (hand.childCount > 0)
                {
                    wep = hand.GetChild(0).GetComponent<Pistol3D>();
                }
            }

            return wep;
        }
    }
}