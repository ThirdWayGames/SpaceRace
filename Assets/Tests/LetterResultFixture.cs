using UnityEngine;
using NUnit.Framework;
using Assets.Scripts.GameObjects;
using System.Collections.Generic;

public class LetterResultFixture
{
    private GameObject letterResultObject;

    [SetUp]
    public void SetUp()
    {
        var letterResultPrefab = Resources.Load("LetterResult");
        letterResultObject = (GameObject)GameObject.Instantiate(letterResultPrefab);
    }

    [TearDown]
    public void TearDown()
    {
        // Destroy the player GO.
        GameObject.Destroy(letterResultObject);
    }

    [Test]
    public void TwoLettersCorrectCorrect()
    {
        List<string> selectedLetters = new List<string> { "G", "B" };
        List<string> hiddenLetters = new List<string> { "G", "B" };

        var crUnderTest = letterResultObject.GetComponent<LetterResult>();
        crUnderTest.SetResults(selectedLetters, hiddenLetters);
        var colorResult = crUnderTest.ProcessResult();

        Debug.LogFormat("[{0}:{1}]", string.Concat(selectedLetters), string.Concat(hiddenLetters));
        Assert.AreEqual("2", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("0", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void TwoLettersMisplacedMisplaced()
    {
        List<string> selectedLetters = new List<string> { "G", "B" };
        List<string> hiddenLetters = new List<string> { "B", "G" };

        var crUnderTest = letterResultObject.GetComponent<LetterResult>();
        crUnderTest.SetResults(selectedLetters, hiddenLetters);
        var colorResult = crUnderTest.ProcessResult();

        Debug.LogFormat("[{0}:{1}]", string.Concat(selectedLetters), string.Concat(hiddenLetters));
        Assert.AreEqual("0", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("2", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void TwoLettersIncorrectIncorrect()
    {
        List<string> selectedLetters = new List<string> { "G", "B" };
        List<string> hiddenLetters = new List<string> { "C", "Y" };

        var crUnderTest = letterResultObject.GetComponent<LetterResult>();
        crUnderTest.SetResults(selectedLetters, hiddenLetters);
        var colorResult = crUnderTest.ProcessResult();

        Debug.LogFormat("[{0}:{1}]", string.Concat(selectedLetters), string.Concat(hiddenLetters));
        Assert.AreEqual("0", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("0", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void ThreeLettersIncorrectCorrectCorrect()
    {
        List<string> selectedLetters = new List<string> { "G", "G", "G" };
        List<string> hiddenLetters = new List<string> { "B", "G", "G" };

        var crUnderTest = letterResultObject.GetComponent<LetterResult>();
        crUnderTest.SetResults(selectedLetters, hiddenLetters);
        var colorResult = crUnderTest.ProcessResult();

        Debug.LogFormat("[{0}:{1}]", string.Concat(selectedLetters), string.Concat(hiddenLetters));
        Assert.AreEqual("2", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("0", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void ThreeLettersMisplacedIncorrectIncorrect()
    {
        List<string> selectedLetters = new List<string> { "G", "G", "Y" };
        List<string> hiddenLetters = new List<string> { "B", "B", "G" };

        var crUnderTest = letterResultObject.GetComponent<LetterResult>();
        crUnderTest.SetResults(selectedLetters, hiddenLetters);
        var colorResult = crUnderTest.ProcessResult();

        Debug.LogFormat("[{0}:{1}]", string.Concat(selectedLetters), string.Concat(hiddenLetters));
        Assert.AreEqual("0", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("1", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void ThreeLettersMisplacedCorrectMisplaced()
    {
        List<string> selectedLetters = new List<string> { "G", "G", "B" };
        List<string> hiddenLetters = new List<string> { "B", "G", "G" };

        var crUnderTest = letterResultObject.GetComponent<LetterResult>();
        crUnderTest.SetResults(selectedLetters, hiddenLetters);
        var colorResult = crUnderTest.ProcessResult();

        Debug.LogFormat("[{0}:{1}]", string.Concat(selectedLetters), string.Concat(hiddenLetters));
        Assert.AreEqual("1", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("2", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void ThreeLettersIncorrectCorrectMisplaced()
    {
        List<string> selectedLetters = new List<string> { "G", "G", "B" };
        List<string> hiddenLetters = new List<string> { "B", "G", "Y" };

        var crUnderTest = letterResultObject.GetComponent<LetterResult>();
        crUnderTest.SetResults(selectedLetters, hiddenLetters);
        var colorResult = crUnderTest.ProcessResult();

        Debug.LogFormat("[{0}:{1}]", string.Concat(selectedLetters), string.Concat(hiddenLetters));
        Assert.AreEqual("1", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("1", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void FourLettersWithDupesAllMisplaced()
    {
        List<string> selectedLetters = new List<string> { "T", "U", "U", "C" };
        List<string> hiddenLetters = new List<string> { "U", "C", "T", "U" };

        var crUnderTest = letterResultObject.GetComponent<LetterResult>();
        crUnderTest.SetResults(selectedLetters, hiddenLetters);
        var colorResult = crUnderTest.ProcessResult();

        Debug.LogFormat("[{0}:{1}]", string.Concat(selectedLetters), string.Concat(hiddenLetters));
        Assert.AreEqual("0", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("4", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void DoubleMisplacedReportsOnlyOnce()
    {
        List<string> selectedLetters = new List<string> { "U", "C", "C" };
        List<string> hiddenLetters = new List<string> { "A", "U", "U" };

        var crUnderTest = letterResultObject.GetComponent<LetterResult>();
        crUnderTest.SetResults(selectedLetters, hiddenLetters);
        var colorResult = crUnderTest.ProcessResult();

        Debug.LogFormat("DoubleMisplacedReportsOnlyOnce [{0}:{1}]", string.Concat(selectedLetters), string.Concat(hiddenLetters));
        Debug.LogFormat("Correct: {0}", crUnderTest.Correct.text, "Correct Test Failed");
        Debug.LogFormat("Misplaced: {0}", crUnderTest.Misplaced.text, "Misplaced Test Failed");
        Assert.AreEqual("0", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("1", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void MisplacedIsRemovedWhenCorrectPlaceFound()
    {
        List<string> selectedLetters = new List<string> { "A", "T", "C", "G", "U" };
        List<string> hiddenLetters = new List<string> { "C", "T", "C", "G", "U" };

        var crUnderTest = letterResultObject.GetComponent<LetterResult>();
        crUnderTest.SetResults(selectedLetters, hiddenLetters);
        var colorResult = crUnderTest.ProcessResult();

        Debug.LogFormat("MisplacedIsRemovedWhenCorrectPlaceFound [{0}:{1}]", string.Concat(selectedLetters), string.Concat(hiddenLetters));
        Debug.LogFormat("Correct: {0}", crUnderTest.Correct.text, "Correct Test Failed");
        Debug.LogFormat("Misplaced: {0}", crUnderTest.Misplaced.text, "Misplaced Test Failed");
        Assert.AreEqual("4", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("0", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }
}
