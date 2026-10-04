using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Assets.Tests.CureLabGameControllerFixtures
{
    public class ApplyColourTestFixture
    {
        private List<GameObject> Sockets;

        private GameObject testGameObject;

        private PathogenLibrary pathogenLibrary;

        private GameObject pathogenSynthText;

        private GameObject pathogenWinText;

        private CureLabGameController gameController;

        [SetUp]
        public void SetUp()
        {
            testGameObject = new GameObject();
            pathogenSynthText = new GameObject();
            pathogenSynthText.AddComponent<Text>();

            pathogenWinText = new GameObject();
            pathogenWinText.AddComponent<Text>();

            // Add the game controller under test to the test object.
            gameController = testGameObject.AddComponent<CureLabGameController>();

            // Set the pathogen output texts
            gameController.PathogenSynthesised = pathogenSynthText.GetComponent<Text>();
            gameController.ExecuteGuessErrorText = pathogenWinText.GetComponent<Text>();

            // Setup the availble pathogen.
            gameController.Pathogens = new List<ScriptableObject>();
            gameController.Pathogens.Add((ScriptableObject)Resources.Load("Anthrax"));

            // Setup the sockets.
            gameController.Sockets = new List<GameObject>();
            for (int i = 0; i < 3; i++)
            {
                var socket = new GameObject(string.Format("Socket{0}", i));
                gameController.Sockets.Add(socket);
            }

            // Setup the pathogen element prefab
            gameController.ColourGuessPrefab = (GameObject)GameObject.Instantiate(Resources.Load("ColourPicker"));

            // Setup the guess panel.
            var colourGuessPanel = new GameObject("ColourGuessPanel");
            var colourPickerPanel = new GameObject("ColourPickerPanel");
            colourPickerPanel.transform.SetParent(colourGuessPanel.transform);
            gameController.ColourGuessPanel = colourGuessPanel;
            gameController.ColourGuessItemList = colourPickerPanel;

            // Set the socket values.
            for (int i = 0; i < 3; i++)
            {
                var socketChild = new GameObject(string.Format("Socket{0}Child", i));
                socketChild.AddComponent<PathogenElementDisplay>();
                socketChild.transform.SetParent(gameController.Sockets[i].transform.transform);
            }

            gameController.Sockets[0].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("Doxycycline");
            gameController.Sockets[1].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("Rifampicin");
            gameController.Sockets[2].transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement = (ScriptableObject)Resources.Load("Chloramphenicol");
            gameController.ResetMiniGame();
            gameController.SynthesiseSockets();
        }

        [TearDown]
        public void TearDown()
        {
            // Destroy the player GO.
            GameObject.Destroy(testGameObject);
        }

        [Test]
        public void NoPickerValuesStopsSynthTest()
        {
            gameController.ApplyColourTest();
            Assert.AreEqual("ERROR: ALL ITEMS MUST HAVE A COLOUR", gameController.ExecuteGuessErrorText.text);
        }
    }
}
