namespace Assets.Scripts.Interfaces
{
    public interface IEquipment
    {
        string GetPickupPrefabType();

        string ItemName();

        bool CanEquip(IPlayerController player);

        void Equip(IPlayerController player);

        void Unequip(IPlayerController player);
    }
}