using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Assets.Tests.SynthLabGameControllerFixtures
{
    public class SynthesiseSocketsFixture
    {
        private List<GameObject> Sockets;

        private GameObject testGameObject;

        private PathogenLibrary pathogenLibrary;

        private GameObject pathogenSynthText;

        private SynthLabGameController gameController;

        [SetUp]
        public void SetUp()
        {
            testGameObject = new GameObject();
            pathogenSynthText = new GameObject();
            pathogenSynthText.AddComponent<Text>();
            gameController = testGameObject.AddComponent<SynthLabGameController>();
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
        public void NoViablePathogenElementsSame()
        {
            gameController.Pathogens = new List<ScriptableObject>();
            gameController.Pathogens.Add((ScriptableObject)Resources.Load("Anthrax"));

            for (int i = 0; i < 3; i++)
            {
                var socketChild = new GameObject(string.Format("Socket{0}Child", i));
                var pathElemDisplay = socketChild.AddComponent<PathogenElementDisplay>();
                socketChild.transform.SetParent(gameController.Sockets[i].transform.transform);
                pathElemDisplay.PathogenElement = (ScriptableObject)Resources.Load("RnaTriangle");
            }

            gameController.SynthesiseSockets();

            Assert.AreEqual("NO PATHOGEN FOUND", gameController.PathogenSynthesised.text);
        }

        [Test]
        public void NoViablePathogenElementsDifferent()
        {
            gameController.Pathogens = new List<ScriptableObject>();
            gameController.Pathogens.Add((ScriptableObject)Resources.Load("Anthrax"));

            for (int i = 0; i < 3; i++)
            {
                var socketChild = new GameObject(string.Format("Socket{0}Child", i));
                socketChild.AddComponent<PathogenElementDisplay>();
                socketChild.transform.SetParent(gameController.Sockets[i].transform.transform);
            }

            gameController.Sockets[0].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("ProteinTriangle");
            gameController.Sockets[1].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("DnaCircle");
            gameController.Sockets[2].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("RnaTriangle");
            gameController.SynthesiseSockets();

            Assert.AreEqual("NO PATHOGEN FOUND", gameController.PathogenSynthesised.text);
        }

        [Test]
        public void ViablePathogenOrderAgnostic()
        {
            gameController.Pathogens = new List<ScriptableObject>();
            gameController.Pathogens.Add((ScriptableObject)Resources.Load("Anthrax"));

            for (int i = 0; i < 3; i++)
            {
                var socketChild = new GameObject(string.Format("Socket{0}Child", i));
                socketChild.AddComponent<PathogenElementDisplay>();
                socketChild.transform.SetParent(gameController.Sockets[i].transform.transform);
            }

            gameController.Sockets[0].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("ProteinCircle");
            gameController.Sockets[1].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("DnaCircle");
            gameController.Sockets[2].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("RnaTriangle");
            gameController.SynthesiseSockets();

            Assert.AreEqual("VIABLE PATHOGEN FOUND", gameController.PathogenSynthesised.text);
        }

        [Test]
        public void InsufficientElementsSelected()
        {
            gameController.Pathogens = new List<ScriptableObject>();
            gameController.Pathogens.Add((ScriptableObject)Resources.Load("Anthrax"));

            for (int i = 0; i < 2; i++)
            {
                var socketChild = new GameObject(string.Format("Socket{0}Child", i));
                socketChild.AddComponent<PathogenElementDisplay>();
                socketChild.transform.SetParent(gameController.Sockets[i].transform.transform);
            }

            gameController.Sockets[0].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("ProteinCircle");
            gameController.Sockets[1].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("DnaCircle");
            gameController.SynthesiseSockets();

            Assert.AreEqual("All Sockets require elements;", gameController.PathogenSynthesised.text);
        }
    }
}
