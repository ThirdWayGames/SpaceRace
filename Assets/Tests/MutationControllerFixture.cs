using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using Assets.Scripts.Components;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.ScriptableObjects;

[TestFixture]
public class MutationControllerFixture {

    private GameObject _player;
    private GameObject _pathogenLibrary;
    private GameObject _gameManager;

    [OneTimeSetUp]
    public void SetUpFixture()
    {
        // Get the player prefab
        var gameManagerPrefab = Resources.Load("GameManager");

        // Instanciate it.
        _gameManager = (GameObject)GameObject.Instantiate(gameManagerPrefab);
        if (_gameManager == null)
        {
            Assert.Fail("No game manager found in scene.");
        }
    }

    [SetUp]
    public void SetUp()
    {
        // Get the player prefab
        var playerPrefab = Resources.Load("BasicCloneBlue");
        var pathogenLibPrefab = Resources.Load("PathogenLibraryManager");

        // Instanciate it.
        _player = (GameObject)GameObject.Instantiate(playerPrefab);
        if (_player == null)
        {
            Assert.Fail("No player found in scene.");
        }

        var mutController = _player.GetComponent<MutationController>();
        if (mutController != null)
        {
            mutController.PersistentPathogen = ScriptableObject.CreateInstance<PathogenLoadout>();
        }

        _pathogenLibrary = (GameObject)GameObject.Instantiate(pathogenLibPrefab);
        if (_pathogenLibrary == null)
        {
            Assert.Fail("No pathogen lib found in scene.");
        }
    }

    [TearDown]
    public void TearDown()
    {
        // Destroy the player GO.
        GameObject.Destroy(_player);
        GameObject.Destroy(_pathogenLibrary);
        _player = null;
        _pathogenLibrary = null;
    }

    [Test]
    public void CanAddPathogen()
    {
        /*
         * ARRANGE
         */
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");
        Assert.AreEqual(0, mutationController.PathogenMutations.Count);
        Assert.That(string.IsNullOrWhiteSpace(((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen));
        Assert.AreEqual(0, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);

        /*
         * ACT
         */
        mutationController.AddPathogen("TestPathogen", 0);

        /*
         * ASSERT
         */
        Assert.AreEqual(1, mutationController.PathogenMutations.Count);
        Assert.That(mutationController.PathogenMutations.Cast<BaseMutation>().FirstOrDefault().IsInitialised);
        Assert.AreEqual("TestPathogen", ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen);
        Assert.AreEqual(5, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);
    }

    [Test]
    public void CanAddPathogenMoreSevere()
    {
        /*
         * ARRANGE
         */
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");
        Assert.AreEqual(0, mutationController.PathogenMutations.Count);
        Assert.That(string.IsNullOrWhiteSpace(((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen));
        Assert.AreEqual(0, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);
        mutationController.AddPathogen("TestPathogen", 0);
        Assert.AreEqual(1, mutationController.PathogenMutations.Count);
        Assert.That(mutationController.PathogenMutations.Cast<BaseMutation>().FirstOrDefault().IsInitialised);
        Assert.AreEqual("TestPathogen", ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen);
        Assert.AreEqual(5, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);

        /*
         * ACT
         */
        mutationController.AddPathogen("TestPathogenMoreSevere", 0);

        /*
         * ASSERT
         */
        Assert.AreEqual(1, mutationController.PathogenMutations.Count);
        Assert.That(mutationController.PathogenMutations.Cast<BaseMutation>().FirstOrDefault().IsInitialised);
        Assert.AreEqual("TestPathogenMoreSevere", ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen);
        Assert.AreEqual(8, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);
    }

    [Test]
    public void CantAddPathogenWithLessEqualSeverity()
    {
        /*
         * ARRANGE
         */
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");
        Assert.AreEqual(0, mutationController.PathogenMutations.Count);
        Assert.That(string.IsNullOrWhiteSpace(((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen));
        Assert.AreEqual(0, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);
        mutationController.AddPathogen("TestPathogen", 0);
        Assert.AreEqual(1, mutationController.PathogenMutations.Count);
        Assert.That(mutationController.PathogenMutations.Cast<BaseMutation>().FirstOrDefault().IsInitialised);
        Assert.AreEqual("TestPathogen", ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen);
        Assert.AreEqual(5, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);

        /*
         * ACT
         */
        mutationController.AddPathogen("TestPathogenLessSevere", 0);

        /*
         * ASSERT
         */
        Assert.AreEqual(1, mutationController.PathogenMutations.Count);
        Assert.That(mutationController.PathogenMutations.Cast<BaseMutation>().FirstOrDefault().IsInitialised);
        Assert.AreEqual("TestPathogen", ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen);
        Assert.AreEqual(5, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);
    }

    [Test]
    public void CanCureKnownPathogen()
    {
        /*
         * ARRANGE
         */
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");
        Assert.AreEqual(0, mutationController.PathogenMutations.Count);
        Assert.That(string.IsNullOrWhiteSpace(((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen));
        Assert.AreEqual(0, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);

        var teamComponent = _player.GetComponent<TeamComponent>();
        Assert.IsNotNull(teamComponent);

        var pathogenLibrary = _pathogenLibrary.GetComponent<PathogenLibrary>();
        pathogenLibrary.AddPathogen(teamComponent.TeamIdentifier, "TestPathogen", true);

        mutationController.AddPathogen("TestPathogen", 0);
        Assert.AreEqual(1, mutationController.PathogenMutations.Count);
        Assert.That(mutationController.PathogenMutations.Cast<BaseMutation>().FirstOrDefault().IsInitialised);
        Assert.AreEqual("TestPathogen", ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen);
        Assert.AreEqual(5, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);

        /*
         * ACT
         */
        mutationController.CurePathogens(teamComponent.TeamIdentifier);

        /*
         * ASSERT
         */
        Assert.AreEqual(0, mutationController.PathogenMutations.Count);
        Assert.That(string.IsNullOrEmpty(((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen));
        Assert.AreEqual(0, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);
    }

    [Test]
    public void CantCureUnknownPathogen()
    {
        /*
         * ARRANGE
         */
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");
        Assert.AreEqual(0, mutationController.PathogenMutations.Count);
        Assert.That(string.IsNullOrWhiteSpace(((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen));
        Assert.AreEqual(0, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);

        var teamComponent = _player.GetComponent<TeamComponent>();
        Assert.IsNotNull(teamComponent);

        var pathogenLibrary = _pathogenLibrary.GetComponent<PathogenLibrary>();
        pathogenLibrary.AddPathogen(teamComponent.TeamIdentifier, "TestPathogenMoreSevere", true);

        mutationController.AddPathogen("TestPathogen", 0);
        Assert.AreEqual(1, mutationController.PathogenMutations.Count);
        Assert.That(mutationController.PathogenMutations.Cast<BaseMutation>().FirstOrDefault().IsInitialised);
        Assert.AreEqual("TestPathogen", ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen);
        Assert.AreEqual(5, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);

        /*
         * ACT
         */
        mutationController.CurePathogens(teamComponent.TeamIdentifier);

        /*
         * ASSERT
         */
        Assert.AreEqual(1, mutationController.PathogenMutations.Count);
        Assert.That(mutationController.PathogenMutations.Cast<BaseMutation>().FirstOrDefault().IsInitialised);
        Assert.AreEqual("TestPathogen", ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogen);
        Assert.AreEqual(5, ((PathogenLoadout)mutationController.PersistentPathogen).CurrentPathogenSeverity);
    }

    [Test]
    public void CanAddMutation()
    {
        /*
         * ARRANGE
         */
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");
        Assert.AreEqual(0, mutationController.Mutations.Count);

        /*
         * ACT
         */
        mutationController.AddMutation("SmallRadSickness", 0);

        /*
         * ASSERT
         */
        Assert.AreEqual(1, mutationController.Mutations.Count);
        Assert.That(mutationController.Mutations.Cast<BaseMutation>().FirstOrDefault().IsInitialised);
    }

    /// <summary>
    /// Tests to make sure that a mutation requires a component and the GO doesn't have that component then it
    /// doesn't add the mutation.
    /// </summary>
    [Test]
    public void CanAddComponentRequiredMutation()
    {
        /*
         * ARRANGE
         */
        // Remove it so that we can't add the mutation that affects it.
        GameObject.DestroyImmediate(_player.GetComponent<PlayerController3D>());
        GameObject.DestroyImmediate(_player.GetComponent<HealthComponent>());
        
        // Get the mutation controller ready to add the mutation.
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");
        Assert.AreEqual(0, mutationController.Mutations.Count);

        /*
         * ACT
         */
        mutationController.AddMutation("SmallRadSickness", 0);

        /*
         * ASSERT
         */
        Assert.AreEqual(0, mutationController.Mutations.Count);
    }

    [Test]
    public void CanGetMutationByPrefab()
    {
        var mutationController = _player.GetComponent<MutationController>();
        var mutationPrefab = Resources.Load("SmallRadSickness") as BaseMutation;

        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");
        mutationController.AddMutation(mutationPrefab.name, 0);
        Assert.AreEqual(1, mutationController.Mutations.Count);

        /*
         * ACT
         */
        var actualMutation = mutationController.GetMutationByPrefab(mutationPrefab);

        /*
         * ASSERT
         */
        Assert.IsNotNull(actualMutation);
        Assert.AreEqual(string.Format("{0}(Clone)", mutationPrefab.name), actualMutation.name);
    }

    [Test]
    public void CantAddMutationsMultipeTimes() {
        /*
         * ARRANGE
         */
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");
        Assert.AreEqual(0, mutationController.Mutations.Count);

        /*
         * ACT
         */
        mutationController.AddMutation("SmallRadSickness", 0);
        mutationController.AddMutation("SmallRadSickness", 0);

        /*
         * ASSERT
         */
        Assert.AreEqual(1, mutationController.Mutations.Count);
    }

    [Test]
    public void CanCureMutation()
    {
        /*
         * ARRANGE
         */
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");

        var mutationToCure = Resources.Load("SmallRadSickness") as BaseMutation;
        mutationController.AddMutation(mutationToCure.name, 0);
        Assert.AreEqual(1, mutationController.Mutations.Count, "There isn't a mutation to cure");

        /*
         * ACT
         */
        mutationController.CureMutation(mutationToCure);

        /*
         * ASSERT
         */
        Assert.AreEqual(0, mutationController.Mutations.Count, "There are too many mutations after cure");
    }

    [Test]
    public void CanCureMutationWithVaccine()
    {
        /*
         * ARRANGE
         */
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");

        var mutationToCure = Resources.Load("VaccinatableMutation") as BaseMutation;
        mutationController.AddMutation(mutationToCure.name, 0);
        Assert.AreEqual(1, mutationController.Mutations.Count, "There isn't a mutation to cure");
        var energyComponent = _player.GetComponent<EnergyComponent>();
        Assert.AreEqual(50, energyComponent.MaxValue);

        /*
         * ACT
         */
        mutationController.CureMutation(mutationToCure);

        /*
         * ASSERT
         */
        Assert.AreEqual(0, mutationController.Mutations.Count, "There are too many mutations after cure");
        Assert.AreEqual(1, mutationController.Immunities.Count(x => x.StartsWith("VaccinatableMutation")), "No immunity found");
        Assert.AreEqual(50.0f, energyComponent.MaxValue);
    }

    [Test]
    public void CantAddVaccinatedMutation()
    {
        /*
         * ARRANGE
         */
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");

        var mutationToCure = Resources.Load("VaccinatableMutation") as BaseMutation;
        mutationController.AddMutation(mutationToCure.name, 0);
        Assert.AreEqual(1, mutationController.Mutations.Count, "There isn't a mutation to cure");
        var energyComponent = _player.GetComponent<EnergyComponent>();
        Assert.AreEqual(50, energyComponent.MaxValue);
        mutationController.CureMutation(mutationToCure);
        Assert.AreEqual(0, mutationController.Mutations.Count, "There are too many mutations after cure");
        Assert.AreEqual(1, mutationController.Immunities.Count(x => x.StartsWith("VaccinatableMutation")), "No immunity found");
        Assert.AreEqual(50.0f, energyComponent.MaxValue);

        /*
         * ACT
         */
        mutationController.AddMutation(mutationToCure.name, 0);

        /*
         * ASSERT
         */
        Assert.AreEqual(0, mutationController.Mutations.Count, "There are too many mutations after cure");
        Assert.AreEqual(1, mutationController.Immunities.Count(x => x.StartsWith("VaccinatableMutation")), "No immunity found");
        Assert.AreEqual(50.0f, energyComponent.MaxValue);
    }

    [Test]
    public void CureNonexistentCausesDebug()
    {
        /*
         * ARRANGE
         */
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");
        var mutationToCure = Resources.Load("VaccinatableMutation") as BaseMutation;

        /*
         * ACT
         */
        mutationController.CureMutation(mutationToCure);

        /*
         * ASSERT
         */
        LogAssert.Expect(LogType.Warning, string.Format("Attempted to cure mutation 'VaccinatableMutation' but was not found in mutation controller for '{0}'", _player.gameObject.name));
    }

    [Test]
    public void MutationCuresAferExecution()
    {
        /*
         * ARRANGE
         */
        var mutationController = _player.GetComponent<MutationController>();
        Assert.IsNotNull(mutationController, "Mutation Controller is required on player prefab");
        var mutationToCure = Resources.Load("HeavyBlasterBulletDmg") as BaseMutation;

        /*
         * ACT
         */
        mutationController.AddMutation(mutationToCure.name, 0);

        /*
         * ASSERT
         */
        var healthComponent = _player.GetComponent<HealthComponent>();
        Assert.AreEqual(0, mutationController.Mutations.Count, "There are too many mutations after cure");
        Assert.AreEqual(34.0f, healthComponent.CurrentValue);
    }
}
