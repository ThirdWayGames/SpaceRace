using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public class Armour : Equipment
    {
        public float RotLerpSpeedAdjustment;
        public float SpeedAdjustment;
        public float StaminaAdjustment;
        public float AmbientViewRadiusAdjustment;
        public float ViewAngleAdjustment;

        public float ArmourValueAdjustment;

        public override string ItemName()
        {
            return this.GetType().Name;
        }

        public override void Equip(IPlayerController player)
        {
            if (player != null)
            {
                // If the player currently has an item equiped.
                //// var equipedItem = player.GetEquipment();
                Equipment equipedItem = null;
                if (equipedItem != null)
                {
                    // Unequip it.
                    equipedItem.Unequip(player);
                }

                base.Equip(player);

                // Equip this item instead.
                var fov = player.GetTransform().GetComponent<FieldOfView3D>();
                if (fov != null)
                {
                    Debug.Log("Adjusting Player Stats (FOV)");

                    // Adjust the ambient view radius
                    fov.AmbientViewRadius += this.AmbientViewRadiusAdjustment;
                    fov.ViewAngle += this.ViewAngleAdjustment;
                }
            }
        }

        public override void Unequip(IPlayerController player)
        {
            if (player != null)
            {
                var fov = player.GetTransform().GetComponent<FieldOfView3D>();
                if (fov != null)
                {
                    Debug.Log("Undoing Player Stat Adjustment (FOV)");
                    fov.AmbientViewRadius -= this.AmbientViewRadiusAdjustment;
                    fov.ViewAngle -= this.ViewAngleAdjustment;
                }

                base.Unequip(player);
            }
        }
    }
}