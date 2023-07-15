using UnityEngine;

public class LookAtConstraint : MonoBehaviour {
	public Transform target; // The target to look at

	void LateUpdate() {
		// Apply LookAt constraint
		transform.LookAt(target);

		// Get the current rotation angles
		Vector3 currentRotation = transform.rotation.eulerAngles;

		// Preserve only the Y rotation
		Vector3 newRotation = new Vector3(0f, currentRotation.y, 0f);

		// Apply the preserved Y rotation
		transform.rotation = Quaternion.Euler(newRotation);
	}
}
