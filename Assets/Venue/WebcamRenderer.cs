using UnityEngine;

public class WebcamRenderer : MonoBehaviour {
	public MeshRenderer objectMeshRenderer; // Reference to the MeshRenderer component of the 3D object
	public int webcamIndex = 0; // Index of the webcam device to use (0 is the default)

	private WebCamTexture webcamTexture;

	void Start() {
		// Get available webcams
		WebCamDevice[] devices = WebCamTexture.devices;

		if (devices.Length > 0) {
			// Create a new instance of WebCamTexture with the chosen webcam
			webcamTexture = new WebCamTexture(devices[webcamIndex].name);
			webcamTexture.Play();

			// Assign the webcam texture to the material of the object's MeshRenderer component
			objectMeshRenderer.material.SetTexture("_MainTex", webcamTexture);
		} else {
			Debug.Log("No webcams found.");
		}
	}

	void Update() {
		// Update the webcam texture each frame
		if (webcamTexture != null && webcamTexture.isPlaying) {
			objectMeshRenderer.material.SetTexture("_MainTex", webcamTexture);
		}
	}
}