using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameObjects
{
    public class Equipment : NetworkAttachToParent, IEquipment
    {
        public Sprite EquipmentIconUI;

        public Sprite EquipmentIconMap;

        public string PickupPrefabType;

        public string GetPickupPrefabType()
        {
            return PickupPrefabType;
        }

        public virtual string ItemName()
        {
            return this.GetType().Name;
        }

        public virtual bool CanEquip(IPlayerController player)
        {
            return true;
        }

        public virtual void Equip(IPlayerController player)
        {
            //// player.SetEquipment(this);
            if (EquipmentIconUI != null)
            {
                var equipmentUiIcon = player.FindChild("EquipmentIcon").Find("Icon");
                if (equipmentUiIcon != null)
                {
                    equipmentUiIcon.GetComponent<Image>().sprite = EquipmentIconUI;
                    equipmentUiIcon.GetComponent<Image>().enabled = true;
                }
            }
        }

        public virtual void Unequip(IPlayerController player)
        {
            //// player.SetEquipment(null);
            var equipmentUiIcon = player.FindChild("EquipmentIcon").Find("Icon");
            if (equipmentUiIcon != null)
            {
                equipmentUiIcon.GetComponent<Image>().enabled = false;
            }

            DropItem(player);
        }

        protected virtual void DropItem(IPlayerController player)
        {
            if (!string.IsNullOrEmpty(PickupPrefabType))
            {
                Debug.Log(string.Format("Dropping {0}", ItemName()));
                var pickupable = Resources.Load(PickupPrefabType);
                var droppedItem = Instantiate(pickupable,
                    player.GetTransform().position + player.GetTransform().rotation*new Vector3(1, 1, 0),
                    player.GetTransform().rotation);
                ((GameObject) droppedItem).GetComponent<IPickup>().SetEquipment(this);
                ((GameObject) droppedItem).GetComponent<SpriteRenderer>().sprite = EquipmentIconMap;
            }
        }
    }
}
