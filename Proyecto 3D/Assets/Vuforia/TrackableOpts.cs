using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class TrackableOpts : MonoBehaviour, ITrackableEventHandler {
	public ImageTargetBehaviour itb;
	public Text text;
	public float wantedScale = 1f;
	public Material baseMaterial;

	Renderer[] objRenderers = new Renderer[0];

	
	void Start() {
		if (TestQR.loadedQRs.Count == 0) {
			// app just started, ignore this start
			return;
		}
		TrackableBehaviour trackableBehaviour = GetComponent<TrackableBehaviour>();

		if (trackableBehaviour) {
			text.text = "Seteando Trackable en Vuforia";
			trackableBehaviour.RegisterTrackableEventHandler(this);
		}

		//ShowObject(false);

		StartCoroutine(loadObj());
	}
	IEnumerator loadObj() {
		using (WWW www = new WWW(TestQR.loadedQRs[int.Parse(itb.TrackableName)])) {
			text.text = "Descargando modelo obj...";
			yield return www;
			text.text = "Extrayendo modelo obj...";
			List<string> lines = new List<string>(www.text.Split('\n'));
			GameObject obj;
			try {
				obj = OBJLoader.LoadOBJ("QR object downloaded", lines, baseMaterial);
				text.text = "";
			} catch (Exception e) {
				text.text = e.ToString();
				yield break;
			}
			objRenderers = obj.GetComponentsInChildren<Renderer>();
			Bounds b = new Bounds();
			bool initiated = false;
			foreach (var r in objRenderers) {
				if (!initiated) {
					b = r.bounds;
					initiated = true;
				} else {
					b.Encapsulate(r.bounds);
				}
			}
			float highestSide = Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
			if (highestSide > 0) {
				Vector3 scale = obj.transform.localScale;
				scale /= highestSide;
				scale *= wantedScale;
				obj.transform.localScale = scale;
			}
			transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			obj.transform.parent = transform;
		}
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

	public void ShowObject(bool tf) {
		foreach (var r in objRenderers) r.enabled = tf;
	}
}
