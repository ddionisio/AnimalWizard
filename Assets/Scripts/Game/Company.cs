using UnityEngine;
using System.Collections;

public class Company : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(DoIt());
	}

    IEnumerator DoIt() {
        yield return new WaitForSeconds(2.0f);

        Main.instance.sceneManager.LoadScene("start");
    }
}
