using UnityEngine;
using Vuforia;

public class TrackableOpts : MonoBehaviour, ITrackableEventHandler {
	void Start() {
		TrackableBehaviour trackableBehaviour = GetComponent<TrackableBehaviour>();

		if (trackableBehaviour) {
			trackableBehaviour.RegisterTrackableEventHandler(this);
		}

		ShowObject(false);
	}

	/// <summary>
	/// Implementation of the ITrackableEventHandler function called when the
	/// tracking state changes.
	/// </summary>
	public void OnTrackableStateChanged(
									TrackableBehaviour.Status previousStatus,
									TrackableBehaviour.Status newStatus) {
		if (newStatus == TrackableBehaviour.Status.DETECTED ||
			newStatus == TrackableBehaviour.Status.TRACKED ||
			newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED) {

			ShowObject(true);
		} else {
			ShowObject(false);
		}
	}

	public Renderer renderer;

	public void ShowObject(bool tf) {
		renderer.enabled = tf;
	}
}
