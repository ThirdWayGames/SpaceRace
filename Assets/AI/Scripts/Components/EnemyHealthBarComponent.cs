using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarComponent : MonoBehaviour {

    public Transform ui;
    public Image healthSlider;
    public GameObject uiPrefab;

    public Transform target;

    public float visiableTime = 5f;
    public float lastMadeVisibleTime;

    public Transform cam;

    // Use this for initialization
    void Start () {
	    cam = Camera.main.transform;
	    foreach (var canvas in FindObjectsOfType<Canvas>())
	    {
	        if (canvas.renderMode == RenderMode.WorldSpace)
	        {
	            ui = Instantiate(uiPrefab, canvas.transform).transform;
	            healthSlider = ui.GetChild(0).GetComponent<Image>();
	            ui.gameObject.SetActive(false);
	            break;
	        }
	    }
    }
	
}
