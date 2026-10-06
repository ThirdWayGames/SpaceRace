using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using UnityEngine;

public class ThrowItemAction : Weapon
{
    float chargeSeconds;

    bool charging;

    public override bool ChargesThrow
    {
        get { return true; }
    }

    public override bool IsThrowCharging
    {
        get { return charging; }
    }

    public float ChargedBulletVelocity
    {
        get { return BulletVelocity * FlareThrow.SpeedMultiplier(chargeSeconds); }
    }

    public override void BeginThrowCharge()
    {
        charging = true;
        chargeSeconds = 0f;
    }

    public override void AccumulateThrowCharge(float deltaTime)
    {
        if (!charging || deltaTime <= 0f)
        {
            return;
        }

        chargeSeconds += deltaTime;
    }

    public override void ClearThrowCharge()
    {
        charging = false;
        chargeSeconds = 0f;
    }

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

    protected override ISpawnData GenerateSpawnData(IPlayerController player)
    {
        var data = base.GenerateSpawnData(player) as BulletData;
        if (data != null)
        {
            data.BulletVelocity *= FlareThrow.SpeedMultiplier(chargeSeconds);
        }

        return data;
    }
}
