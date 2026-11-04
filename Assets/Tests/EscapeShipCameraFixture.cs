using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class EscapeShipCameraFixture
{
    [Test]
    public void OnFootCameraStaysWhenTheMarineHasNone()
    {
        var ship = new GameObject("EscapeShipCameraTest");
        var managerObject = new GameObject("PlayerManagerCameraTest");
        var marine = new GameObject("MarineCameraTest");
        try
        {
            var bootstrap = ship.AddComponent<EscapeShipBootstrap>();
            bootstrap.ConfigureOnFootView();
            var manager = managerObject.AddComponent<PlayerManager3D>();
            manager.LocalPlayerInstance = marine;

            bootstrap.Update();

            var camera = ship.GetComponent<Camera>();
            Assert.IsNotNull(camera);
            Assert.IsTrue(camera.enabled);
            Assert.IsNull(camera.targetTexture);
            Assert.AreEqual(0, camera.targetDisplay);
            Assert.AreEqual("MainCamera", ship.tag);
            Assert.IsNotNull(ship.GetComponent<CameraFollow3D>());
            Assert.IsFalse(EscapeShipBootstrap.ShouldReleaseSceneCamera(marine));
        }
        finally
        {
            Object.DestroyImmediate(marine);
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(ship);
        }
    }

    [Test]
    public void SceneCameraYieldsWhenTheMarineAlreadyRenders()
    {
        var ship = new GameObject("EscapeShipCameraYield");
        var managerObject = new GameObject("PlayerManagerCameraYield");
        var marine = new GameObject("MarineCameraYield");
        try
        {
            var bootstrap = ship.AddComponent<EscapeShipBootstrap>();
            bootstrap.ConfigureOnFootView();
            var marineCamera = marine.AddComponent<Camera>();
            marineCamera.enabled = true;
            var manager = managerObject.AddComponent<PlayerManager3D>();
            manager.LocalPlayerInstance = marine;

            bootstrap.Update();

            Assert.IsTrue(EscapeShipBootstrap.ShouldReleaseSceneCamera(marine));
            Assert.IsFalse(ship.GetComponent<Camera>().enabled);
            Assert.IsTrue(marineCamera.enabled);
        }
        finally
        {
            Object.DestroyImmediate(marine);
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(ship);
        }
    }
}
