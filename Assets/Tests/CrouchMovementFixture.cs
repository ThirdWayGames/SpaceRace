using Assets.Scripts.Components;
using Assets.Scripts.Systems;
using NUnit.Framework;
using UnityEngine;
using Assert = NUnit.Framework.Assert;

[TestFixture]
public class CrouchMovementFixture
{
    [Test]
    public void CrouchSpeedIsSlowerThanWalking()
    {
        var clone = new GameObject("crouch-speed");
        clone.SetActive(false);
        var movement = clone.AddComponent<MovementComponent>();
        movement.CurrentValue = 6f;
        movement.CrouchSpeedMultiplier = 0.5f;
        movement.IsDucking = true;

        Assert.AreEqual(3f, movement.CrouchSpeed);

        movement.IsDucking = false;
        Assert.AreEqual(6f, movement.CurrentValue);
        Object.DestroyImmediate(clone);
    }

    [Test]
    public void MovingWhileCrouchedKeepsTheDuckPose()
    {
        Assert.AreEqual(3, AnimatorUpdateSystem.ResolveState(true, true, false));
        Assert.AreEqual(3, AnimatorUpdateSystem.ResolveState(true, true, true));
        Assert.AreEqual(3, AnimatorUpdateSystem.ResolveState(true, false, false));
    }

    [Test]
    public void CrouchInputStillRequestsTranslation()
    {
        Assert.IsTrue(MovementSystem.ShouldTranslate(1f, 0f));
        Assert.IsTrue(MovementSystem.ShouldTranslate(0f, -1f));
        Assert.IsFalse(MovementSystem.ShouldTranslate(0f, 0f));
    }

    [Test]
    public void StandingStillUsesIdleWalkAndRun()
    {
        Assert.AreEqual(0, AnimatorUpdateSystem.ResolveState(false, false, false));
        Assert.AreEqual(1, AnimatorUpdateSystem.ResolveState(false, true, false));
        Assert.AreEqual(2, AnimatorUpdateSystem.ResolveState(false, true, true));
    }
}
