using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.GameObjects;
using UnityEngine;

public static class HandIcons
{
    static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    public static Sprite ForEquipment(Equipment equipment)
    {
        if (equipment == null)
        {
            return null;
        }

        if (equipment.EquipmentIconUI != null)
        {
            return equipment.EquipmentIconUI;
        }

        if (equipment.EquipmentIconMap != null)
        {
            return equipment.EquipmentIconMap;
        }

        var key = CatalogKey(equipment.gameObject.name);
        var sprite = Load(key);
        if (sprite != null)
        {
            return sprite;
        }

        var weapon = equipment as Weapon;
        if (weapon != null)
        {
            return Load(key + ModeSuffix(weapon.CurrentFireMode));
        }

        return null;
    }

    public static string CatalogKey(string objectName)
    {
        var name = objectName ?? string.Empty;
        var clone = name.IndexOf("(Clone)");
        if (clone >= 0)
        {
            name = name.Substring(0, clone);
        }

        name = name.Trim();
        if (name.IndexOf("Pathogen") >= 0)
        {
            return "PathogenBlaster";
        }

        if (name.IndexOf("Heal") >= 0 || name.IndexOf("Medi") >= 0)
        {
            return "MediRay";
        }

        if (name.IndexOf("Heavy") >= 0 && name.IndexOf("Blaster") >= 0)
        {
            return "HeavyBlaster";
        }

        if (name.IndexOf("Light") >= 0 && name.IndexOf("Blaster") >= 0)
        {
            return "LightBlaster";
        }

        if (name.IndexOf("Energy") >= 0 && name.IndexOf("Blaster") >= 0)
        {
            return "EnergyRecharger";
        }

        if (name.IndexOf("Stun") >= 0)
        {
            return "Lightning";
        }

        if (name.IndexOf("Shield") >= 0)
        {
            return "EnergyShield";
        }

        if (name.IndexOf("Engineer") >= 0)
        {
            return "EngineeringTool";
        }

        if (name.IndexOf("Flare") >= 0 || name.IndexOf("Throwable") >= 0)
        {
            return "Flare";
        }

        if (name.IndexOf("Sniper") >= 0)
        {
            return "sniper-rifle";
        }

        return name;
    }

    static string ModeSuffix(FireMode mode)
    {
        switch (mode)
        {
            case FireMode.Cloud:
                return "CloudMode";
            case FireMode.Beam:
                return "BeamMode";
            default:
                return "BulletMode";
        }
    }

    static Sprite Load(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        Sprite sprite;
        if (Cache.TryGetValue(name, out sprite))
        {
            return sprite;
        }

        sprite = Resources.Load<Sprite>("HandIcons/" + name);
        Cache[name] = sprite;
        return sprite;
    }
}
