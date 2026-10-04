using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class ConsoleCommandField : MonoBehaviour
{
    public void ExecuteCommand(string value)
    {
        var methodParts = value.Split(' ');
        var methodInfo = typeof(GameManager3D).GetMethod(methodParts[0]);

        if (methodInfo != null)
        {
            if (methodParts.Length > 1)
            {
                methodInfo.Invoke(GameManager3D.instance, new object[] { methodParts[1] });
            }
            else
            {
                methodInfo.Invoke(GameManager3D.instance, null);
            }

            this.transform.Find("Text").GetComponent<Text>().text = string.Empty;
        }
    }
}
