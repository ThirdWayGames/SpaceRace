using UnityEngine;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using System.Linq;
using System.Collections.Generic;

[TestFixture]
public class ForceBarrierManagerFixture
{
    private GameObject _gameManager;

    private List<GameObject> _consoleBarriers;

    private List<ForceBarrierController> _forceControllers;

    [OneTimeSetUp]
    public void SetUpFixture()
    {
        _consoleBarriers = new List<GameObject>();
        _forceControllers = new List<ForceBarrierController>();
    }

    [SetUp]
    public void SetUp()
    {
        // Create 4 force barriers.
        for(int i = 0; i < 4; i++)
        {
            _consoleBarriers.Add(new GameObject(string.Format("ForceBarrier[{0}]", i)));
            _forceControllers.Add(_consoleBarriers[i].AddComponent<ForceBarrierController>());
        }

        // Create a force barrier manager.
        _gameManager = new GameObject();
        _gameManager.SetActive(false);
        var forceBarrierManager = _gameManager.AddComponent<ForceBarrierManagerTest>();
        forceBarrierManager.ConsoleBarriers = _forceControllers;
    }

    [TearDown]
    public void TearDown()
    {
        // Destroy the player GO.
        GameObject.Destroy(_gameManager);
        foreach (var item in _consoleBarriers)
        {
            GameObject.Destroy(item);
        }
    }

    [Test]
    public void UnlocksRandomConsoles()
    {
        /*
         * ACT
         */
        _gameManager.SetActive(true);
        var forceBarrierManager = _gameManager.GetComponent<ForceBarrierManagerTest>();
        var firstActualRandomResults = forceBarrierManager.TestGetRandomLockedConsoles(2);
        var secondActualRandomResults = forceBarrierManager.TestGetRandomLockedConsoles(2);

        var consoleToUnlock = firstActualRandomResults.FirstOrDefault();
        if (consoleToUnlock != null)
        {
            forceBarrierManager.RemoveConsoleBarrier(consoleToUnlock.gameObject.name);
        }

        var thirdActualRandomResults = forceBarrierManager.TestGetRandomLockedConsoles(2);

        /*
         * ASSERT
         */
        Assert.AreEqual(2, firstActualRandomResults.Count);
        Assert.AreEqual(2, secondActualRandomResults.Count);
        Assert.AreEqual(2, thirdActualRandomResults.Count);
        Assert.That(!thirdActualRandomResults.Contains(consoleToUnlock));
    }
}

public class ForceBarrierManagerTest : ForceBarrierManager
{
    public List<ForceBarrierController> TestGetRandomLockedConsoles(int unlockCount = 2)
    {
        return this.GetRandomLockedConsoles(unlockCount);
    }
}
