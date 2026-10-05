using NUnit.Framework;
using UnityEngine;
using Assert = NUnit.Framework.Assert;

[TestFixture]
public class FlareLightingFixture
{
    [SetUp]
    public void SetUp()
    {
        FlareLightBudget.Reset();
    }

    [TearDown]
    public void TearDown()
    {
        FlareLightBudget.Reset();
    }

    [Test]
    public void LightStaysOnWhileTimeRemainsAboveOffset()
    {
        Assert.IsTrue(FlareLighting.ShouldKeepLight(8f, 1.5f));
        Assert.IsTrue(FlareLighting.ShouldKeepLight(1.51f, 1.5f));
    }

    [Test]
    public void LightTurnsOffOnceRemainingTimeReachesOffset()
    {
        Assert.IsFalse(FlareLighting.ShouldKeepLight(1.5f, 1.5f));
        Assert.IsFalse(FlareLighting.ShouldKeepLight(0f, 1.5f));
        Assert.IsFalse(FlareLighting.ShouldKeepLight(-2f, 1.5f));
    }

    [Test]
    public void LandedFlareRaisesItsLightAboveTheFloor()
    {
        Assert.IsTrue(FlareLighting.IsGroundContact(1f));
        Assert.IsTrue(FlareLighting.IsGroundContact(0.5f));
        Assert.IsFalse(FlareLighting.IsGroundContact(0.2f));

        Assert.AreEqual(0.45f, FlareLighting.LiftAlongNormal(0f, 0.45f), 0.0001f);
        Assert.AreEqual(0.3f, FlareLighting.LiftAlongNormal(0.15f, 0.45f), 0.0001f);
        Assert.AreEqual(0f, FlareLighting.LiftAlongNormal(0.8f, 0.45f), 0.0001f);
    }

    [Test]
    public void OwnerDestroysNetworkedFlareForTheRoom()
    {
        Assert.AreEqual(TimedDestroyAction.NetworkDestroy, DestroyMe.ChooseDestroy(true, true, true, 4));
    }

    [Test]
    public void RemoteClientWaitsForTheOwnerDestroy()
    {
        Assert.AreEqual(TimedDestroyAction.WaitForOwner, DestroyMe.ChooseDestroy(true, true, false, 4));
    }

    [Test]
    public void UninstantiatedViewIsDestroyedLocally()
    {
        Assert.AreEqual(TimedDestroyAction.LocalDestroy, DestroyMe.ChooseDestroy(true, true, true, 0));
        Assert.AreEqual(TimedDestroyAction.LocalDestroy, DestroyMe.ChooseDestroy(true, true, true, -1));
        Assert.AreEqual(TimedDestroyAction.LocalDestroy, DestroyMe.ChooseDestroy(false, true, true, 4));
        Assert.AreEqual(TimedDestroyAction.LocalDestroy, DestroyMe.ChooseDestroy(true, false, true, 4));
    }

    [Test]
    public void OnlyTheNewestFlaresAreSelected()
    {
        var allow = FlareLighting.ChooseActiveLights(new[] { true, true, true, true, true }, FlareLighting.MaxActiveLights);
        Assert.IsFalse(allow[0]);
        Assert.IsTrue(allow[1]);
        Assert.IsTrue(allow[4]);

        var afterNewestExpires = FlareLighting.ChooseActiveLights(new[] { true, true, true, true, false }, FlareLighting.MaxActiveLights);
        Assert.IsTrue(afterNewestExpires[0]);
        Assert.IsFalse(afterNewestExpires[4]);
    }

    [Test]
    public void OnlyTheNewestFlaresKeepALight()
    {
        var created = new GameObject[FlareLighting.MaxActiveLights + 1];
        for (int i = 0; i < created.Length; i++)
        {
            created[i] = CreateFlare(8f);
        }

        Assert.IsFalse(created[0].GetComponent<Light>().enabled);
        for (int i = 1; i < created.Length; i++)
        {
            Assert.IsTrue(created[i].GetComponent<Light>().enabled);
        }

        Object.DestroyImmediate(created[created.Length - 1]);

        Assert.IsTrue(created[0].GetComponent<Light>().enabled);
        for (int i = 0; i < created.Length - 1; i++)
        {
            Object.DestroyImmediate(created[i]);
        }
    }

    [Test]
    public void DisposeCallExtinguishesTheLight()
    {
        var flareObject = CreateFlare(8f);
        var flare = flareObject.GetComponent<DisposeFlare>();
        var light = flareObject.GetComponent<Light>();

        Assert.IsTrue(light.enabled);

        flare.DisposeItem(0f);

        Assert.IsFalse(flare.WantsLight);
        Assert.IsFalse(light.enabled);
        Assert.AreEqual(LightShadows.None, light.shadows);

        Object.DestroyImmediate(flareObject);
    }

    static GameObject CreateFlare(float lifetime)
    {
        var flareObject = new GameObject("flare");
        flareObject.SetActive(false);
        var destroyMe = flareObject.AddComponent<DestroyMe>();
        destroyMe.DestroyTimer = lifetime;
        var light = flareObject.AddComponent<Light>();
        light.shadows = LightShadows.Soft;
        var flare = flareObject.AddComponent<DisposeFlare>();
        flare.Light = light;
        flare.DisposeOffset = 1.5f;
        flareObject.SetActive(true);
        return flareObject;
    }
}
