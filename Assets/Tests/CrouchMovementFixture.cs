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
    public void CrouchingFirstCanStartInEveryDirection()
    {
        // The axis is empty because Ctrl swallowed the key. The physical key fills it in.
        var forward = MovementKeyState.ApplyKeys(0f, false, true);
        var back = MovementKeyState.ApplyKeys(0f, true, false);
        var left = MovementKeyState.ApplyKeys(0f, true, false);
        var right = MovementKeyState.ApplyKeys(0f, false, true);

        Assert.AreEqual(1f, forward);
        Assert.AreEqual(-1f, back);
        Assert.AreEqual(-1f, left);
        Assert.AreEqual(1f, right);
        Assert.IsTrue(MovementSystem.ShouldTranslate(forward, 0f));
        Assert.IsTrue(MovementSystem.ShouldTranslate(back, 0f));
        Assert.IsTrue(MovementSystem.ShouldTranslate(0f, left));
        Assert.IsTrue(MovementSystem.ShouldTranslate(0f, right));
    }

    [Test]
    public void CrouchingMidMoveCanChangeDirection()
    {
        // Already strafing right, then A is pressed while crouched.
        Assert.AreEqual(-1f, MovementKeyState.ApplyKeys(1f, true, false));
        // Already moving forward, and the key is still held.
        Assert.AreEqual(1f, MovementKeyState.ApplyKeys(1f, false, true));
        // Gamepad with no keyboard key stays on the stick.
        Assert.AreEqual(0.5f, MovementKeyState.ApplyKeys(0.5f, false, false));
        Assert.AreEqual(-0.25f, MovementKeyState.ApplyKeys(-0.25f, true, true));
    }

    [Test]
    public void StandingStillUsesIdleWalkAndRun()
    {
        Assert.AreEqual(0, AnimatorUpdateSystem.ResolveState(false, false, false));
        Assert.AreEqual(1, AnimatorUpdateSystem.ResolveState(false, true, false));
        Assert.AreEqual(2, AnimatorUpdateSystem.ResolveState(false, true, true));
    }
}
