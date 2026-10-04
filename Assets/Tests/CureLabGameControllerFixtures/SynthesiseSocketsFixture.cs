using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Assets.Tests.CureLabGameControllerFixtures
{
    public class SynthesiseSocketsFixture
    {
        private List<GameObject> Sockets;

        private GameObject testGameObject;

        private PathogenLibrary pathogenLibrary;

        private GameObject pathogenSynthText;

        private CureLabGameController gameController;

        [SetUp]
        public void SetUp()
        {
            testGameObject = new GameObject();
            pathogenSynthText = new GameObject();
            pathogenSynthText.AddComponent<Text>();
            gameController = testGameObject.AddComponent<CureLabGameController>();
            gameController.PathogenSynthesised = pathogenSynthText.GetComponent<Text>();
            gameController.Sockets = new List<GameObject>();
            for (int i = 0; i < 3; i++)
            {
                var socket = new GameObject(string.Format("Socket{0}", i));
                gameController.Sockets.Add(socket);
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Destroy the player GO.
            GameObject.Destroy(testGameObject);
        }

        [Test]
        public void NoViableTreatmentElementsSame()
        {
            gameController.Pathogens = new List<ScriptableObject>();
            gameController.Pathogens.Add((ScriptableObject)Resources.Load("Anthrax"));

            for (int i = 0; i < 3; i++)
            {
                var socketChild = new GameObject(string.Format("Socket{0}Child", i));
                var pathElemDisplay = socketChild.AddComponent<PathogenElementDisplay>();
                socketChild.transform.SetParent(gameController.Sockets[i].transform.transform);
                pathElemDisplay.PathogenElement = (ScriptableObject)Resources.Load("Rifampicin");
            }

            gameController.SynthesiseSockets();

            Assert.AreEqual("NO TREATMENT FOUND", gameController.PathogenSynthesised.text);
        }

        [Test]
        public void NoViableTreatmentElementsDifferent()
        {
            gameController.Pathogens = new List<ScriptableObject>();
            gameController.Pathogens.Add((ScriptableObject)Resources.Load("Anthrax"));

            for (int i = 0; i < 3; i++)
            {
                var socketChild = new GameObject(string.Format("Socket{0}Child", i));
                socketChild.AddComponent<PathogenElementDisplay>();
                socketChild.transform.SetParent(gameController.Sockets[i].transform.transform);
            }

            gameController.Sockets[0].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("Doxycycline");
            gameController.Sockets[1].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("Rifampicin");
            gameController.Sockets[2].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("Corticosteroid");
            gameController.SynthesiseSockets();

            Assert.AreEqual("NO TREATMENT FOUND", gameController.PathogenSynthesised.text);
        }

        [Test]
        public void ViableTreatmentOrderAgnostic()
        {
            gameController.Pathogens = new List<ScriptableObject>();
            gameController.Pathogens.Add((ScriptableObject)Resources.Load("Anthrax"));

            for (int i = 0; i < 3; i++)
            {
                var socketChild = new GameObject(string.Format("Socket{0}Child", i));
                socketChild.AddComponent<PathogenElementDisplay>();
                socketChild.transform.SetParent(gameController.Sockets[i].transform.transform);
            }

            gameController.Sockets[0].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("Rifampicin");
            gameController.Sockets[1].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("Doxycycline");
            gameController.Sockets[2].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("Chloramphenicol");
            gameController.SynthesiseSockets();

            Assert.AreEqual("VIABLE TREATMENT FOUND", gameController.PathogenSynthesised.text);
        }
    }
}
