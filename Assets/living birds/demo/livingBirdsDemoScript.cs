using UnityEngine;
using System.Collections;

public class livingBirdsDemoScript : MonoBehaviour {
	public lb_BirdController birdControl;

	bool cameraDirections = true;
	Ray ray;
	RaycastHit[] hits;

	void Start(){
		birdControl = GameObject.FindFirstObjectByType<lb_BirdController>();
		SpawnSomeBirds();
	}

	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI() {
	
	}

	IEnumerator SpawnSomeBirds(){
		yield return 2;
		birdControl.SendMessage ("SpawnAmount",10);
	}

	
	
}
