using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class GalaxyViewLightingFixture
{
    [Test]
    public void HelmAndTurretProxyHasALightOnTheGalaxyLayer()
    {
        var host = new GameObject("GalaxyLightHost");
        GameObject rootObject = null;
        try
        {
            host.AddComponent<GalaxyProxy>().Start();
            rootObject = GameObject.Find("GalaxyProxyRoot");
            Assert.IsNotNull(rootObject);

            var light = rootObject.GetComponentInChildren<Light>();
            Assert.IsNotNull(light);
            Assert.AreEqual(GalaxyProxy.LightName, light.gameObject.name);
            Assert.AreEqual(LightType.Directional, light.type);
            Assert.AreEqual(GalaxyProxy.LightIntensity, light.intensity);
            Assert.AreEqual(LightShadows.None, light.shadows);
            Assert.AreEqual(LightRenderMode.ForcePixel, light.renderMode);

            var galaxyLayer = LayerMask.NameToLayer("GalaxyView");
            Assert.GreaterOrEqual(galaxyLayer, 0);
            Assert.AreEqual(1 << galaxyLayer, light.cullingMask);
            Assert.AreEqual(0, light.cullingMask & 1);
            Assert.AreEqual(GalaxyProxy.ProxyHeight, rootObject.transform.position.y, 0.01f);
        }
        finally
        {
            if (rootObject != null)
            {
                Object.DestroyImmediate(rootObject);
            }

            Object.DestroyImmediate(host);
        }
    }

    [Test]
    public void LightHitsTheRearFaceTheHelmSeesAndTheTopTheScannerSees()
    {
        var ahead = GalaxyProxy.LightRotation(0f) * Vector3.forward;
        Assert.Greater(Vector3.Dot(ahead, Vector3.forward), 0.2f);
        Assert.Less(ahead.y, -0.5f);

        var turned = GalaxyProxy.LightRotation(90f) * Vector3.forward;
        Assert.Greater(Vector3.Dot(turned, Vector3.right), 0.2f);
        Assert.Less(turned.y, -0.5f);
    }
}
