using UnityEngine;
using NUnit.Framework;
using Assets.Scripts.GameObjects;
using System.Collections.Generic;

public class ColourResultFixture
{
    private GameObject colourResultObject;

    [SetUp]
    public void SetUp()
    {
        var colourResultPrefab = Resources.Load("ColourResult");
        colourResultObject = (GameObject)GameObject.Instantiate(colourResultPrefab);
    }

    [TearDown]
    public void TearDown()
    {
        // Destroy the player GO.
        GameObject.Destroy(colourResultObject);
    }

    [Test]
    public void TwoColourCorrectCorrect()
    {
        List<Color> selectedColours = new List<Color> { Color.green, Color.blue };
        List<Color> hiddenColours = new List<Color> { Color.green, Color.blue };

        var crUnderTest = colourResultObject.GetComponent<ColourResult>();
        crUnderTest.SetResults(selectedColours, hiddenColours);
        var colorResult = crUnderTest.ProcessResult();

        Assert.AreEqual("2", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("0", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void TwoColourMisplacedMisplaced()
    {
        List<Color> selectedColours = new List<Color> { Color.green, Color.blue };
        List<Color> hiddenColours = new List<Color> { Color.blue, Color.green };

        var crUnderTest = colourResultObject.GetComponent<ColourResult>();
        crUnderTest.SetResults(selectedColours, hiddenColours);
        var colorResult = crUnderTest.ProcessResult();

        Assert.AreEqual("0", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("2", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void TwoColourIncorrectIncorrect()
    {
        List<Color> selectedColours = new List<Color> { Color.green, Color.blue };
        List<Color> hiddenColours = new List<Color> { Color.cyan, Color.yellow };

        var crUnderTest = colourResultObject.GetComponent<ColourResult>();
        crUnderTest.SetResults(selectedColours, hiddenColours);
        var colorResult = crUnderTest.ProcessResult();

        Assert.AreEqual("0", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("0", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void ThreeColourIncorrectCorrectCorrect()
    {
        List<Color> selectedColours = new List<Color> { Color.green, Color.green, Color.green };
        List<Color> hiddenColours = new List<Color> { Color.blue, Color.green, Color.green };

        var crUnderTest = colourResultObject.GetComponent<ColourResult>();
        crUnderTest.SetResults(selectedColours, hiddenColours);
        var colorResult = crUnderTest.ProcessResult();

        Assert.AreEqual("2", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("0", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void ThreeColourMisplacedIncorrectIncorrect()
    {
        List<Color> selectedColours = new List<Color> { Color.green, Color.green, Color.yellow };
        List<Color> hiddenColours = new List<Color> { Color.blue, Color.blue, Color.green };

        var crUnderTest = colourResultObject.GetComponent<ColourResult>();
        crUnderTest.SetResults(selectedColours, hiddenColours);
        var colorResult = crUnderTest.ProcessResult();

        Assert.AreEqual("0", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("1", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void ThreeColourMisplacedCorrectMisplaced()
    {
        List<Color> selectedColours = new List<Color> { Color.green, Color.green, Color.blue };
        List<Color> hiddenColours = new List<Color> { Color.blue, Color.green, Color.green };

        var crUnderTest = colourResultObject.GetComponent<ColourResult>();
        crUnderTest.SetResults(selectedColours, hiddenColours);
        var colorResult = crUnderTest.ProcessResult();

        Assert.AreEqual("1", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("2", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void ThreeColourIncorrectCorrectMisplaced()
    {
        List<Color> selectedColours = new List<Color> { Color.green, Color.green, Color.blue };
        List<Color> hiddenColours = new List<Color> { Color.blue, Color.green, Color.yellow };

        var crUnderTest = colourResultObject.GetComponent<ColourResult>();
        crUnderTest.SetResults(selectedColours, hiddenColours);
        var colorResult = crUnderTest.ProcessResult();

        Assert.AreEqual("1", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("1", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void FourColourWithDupesAllMisplaced()
    {
        List<Color> selectedColours = new List<Color> { Color.yellow, Color.blue, Color.blue, Color.green };
        List<Color> hiddenColours = new List<Color> { Color.blue, Color.green, Color.yellow, Color.blue };

        var crUnderTest = colourResultObject.GetComponent<ColourResult>();
        crUnderTest.SetResults(selectedColours, hiddenColours);
        var colorResult = crUnderTest.ProcessResult();

        Assert.AreEqual("0", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("4", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void DoubleMisplacedReportsOnlyOnce()
    {
        List<Color> selectedColours = new List<Color> { Color.yellow, Color.blue, Color.blue };
        List<Color> hiddenColours = new List<Color> { Color.green, Color.yellow, Color.yellow };

        var crUnderTest = colourResultObject.GetComponent<ColourResult>();
        crUnderTest.SetResults(selectedColours, hiddenColours);
        var colorResult = crUnderTest.ProcessResult();

        Assert.AreEqual("0", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("1", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }

    [Test]
    public void MisplacedIsRemovedWhenCorrectPlaceFound()
    {
        List<Color> selectedColours = new List<Color> { Color.yellow, Color.green, Color.blue };
        List<Color> hiddenColours = new List<Color> { Color.blue, Color.green, Color.blue };
        List<string> selectedLetters = new List<string> { "A", "T", "C" };
        List<string> hiddenLetters = new List<string> { "C", "T", "C" };

        var crUnderTest = colourResultObject.GetComponent<ColourResult>();
        crUnderTest.SetResults(selectedColours, hiddenColours);
        var colorResult = crUnderTest.ProcessResult();

        Debug.LogFormat("MisplacedIsRemovedWhenCorrectPlaceFound [{0}:{1}]", string.Concat(selectedLetters), string.Concat(hiddenLetters));
        Debug.LogFormat("Correct: {0}", crUnderTest.Correct.text, "Correct Test Failed");
        Debug.LogFormat("Misplaced: {0}", crUnderTest.Misplaced.text, "Misplaced Test Failed");
        Assert.AreEqual("2", crUnderTest.Correct.text, "Correct Test Failed");
        Assert.AreEqual("0", crUnderTest.Misplaced.text, "Misplaced Test Failed");
    }
}
