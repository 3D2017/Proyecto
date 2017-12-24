using System;
using UnityEngine;
using ZXing;

public class TestQR : MonoBehaviour {
	public Texture2D marker;
	private WebCamTexture camTexture;
	private Rect screenRect;

	void Start() {
		//screenRect = new Rect(0, 0, Screen.width, Screen.height);
		camTexture = new WebCamTexture();
		//camTexture.requestedHeight = Screen.height;
		//camTexture.requestedWidth = Screen.width;
		if (camTexture != null) {
			camTexture.Play();
		}
	}

	void OnGUI() {
		// drawing the camera on screen
		GUI.DrawTexture(new Rect(0, 0, camTexture.width, camTexture.height), camTexture, ScaleMode.ScaleToFit);
		// do the reading — you might want to attempt to read less often than you draw on the screen for performance sake
		try {
			IBarcodeReader barcodeReader = new BarcodeReader();
			// decode the current frame
			var result = barcodeReader.Decode(camTexture.GetPixels32(),
			  camTexture.width, camTexture.height);
			if (result != null) {
				Debug.Log("DECODED TEXT FROM QR: " + result.Text);
				var points = result.ResultPoints;
				for (int i = 0; i < points.Length; i++) {
					GUI.DrawTexture(new Rect(points[i].X - marker.width / 2f, points[i].Y - marker.height / 2f, marker.width, marker.height), marker);
				}
			}

		}
		catch (Exception ex) { Debug.LogWarning(ex.Message); }
	}
}
