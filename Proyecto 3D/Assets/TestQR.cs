using System;
using UnityEngine;
using Vuforia;
using ZXing;

public class TestQR : MonoBehaviour {
	public UserDefinedTargetBuildingBehaviour targetBuilder;
	public float minAreaForUserTargetCreation = .3f;
	public bool debug = false;
	public Texture2D debugMarker;

	float currentArea;
	Vector2 camSize;
	ResultPoint[] points = new ResultPoint[0];
	IBarcodeReader barcodeReader = new BarcodeReader();
	Image.PIXEL_FORMAT mPixelFormat = Image.PIXEL_FORMAT.RGBA8888;
	bool mFormatRegistered = false;

	void Start() {
		VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
	}

	void OnVuforiaStarted() {
		if (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true)) {
			Debug.Log("Successfully registered pixel format " + mPixelFormat.ToString());
			mFormatRegistered = true;
		} else {
			Debug.LogError(
				"Failed to register pixel format " + mPixelFormat.ToString() +
				"\n the format may be unsupported by your device;" +
				"\n consider using a different pixel format.");
			mFormatRegistered = false;
		}
	}

	void Update() {
		if (!mFormatRegistered) return;
		
		Image image = CameraDevice.Instance.GetCameraImage(mPixelFormat);

		try {
			// decode the current frame
			camSize = new Vector2(image.Width, image.Height);
			var result = barcodeReader.Decode(image.Pixels, image.Width, image.Height, RGBLuminanceSource.BitmapFormat.RGBA32);
			if (result != null) {
				Debug.Log("DECODED TEXT FROM QR: " + result.Text);
				points = result.ResultPoints;
				Rect r = GetPercentageRect(points);
				currentArea = r.width * r.height;
				if (currentArea >= minAreaForUserTargetCreation) {
					targetBuilder.BuildNewTarget(result.Text, 1);

					// download model

					enabled = false;
				}
			}
		}
		catch (Exception ex) { Debug.LogWarning(ex.Message); }
	}

	void OnGUI() {
		if (!mFormatRegistered) return;
		if (!debug) return;

		for (int i = 0; i < points.Length; i++) {
			GUI.DrawTexture(new Rect(
				points[i].X / camSize.x * Screen.width - debugMarker.width / 2f,
				points[i].Y / camSize.y * Screen.height - debugMarker.height / 2f, 
				debugMarker.width, debugMarker.height), debugMarker);
		}

		Rect r = GetPercentageRect(points);
		r.x *= Screen.width;
		r.y *= Screen.height;
		r.width *= Screen.width;
		r.height *= Screen.height;
		GUI.Box(r, currentArea + " < " + minAreaForUserTargetCreation);
	}

	public Rect GetPercentageRect(ResultPoint[] points) {
		Vector4 minmax = new Vector4(9999999, 9999999, 0, 0);
		for (int i = 0; i < points.Length; i++) {
			minmax.x = Math.Min(minmax.x, points[i].X);
			minmax.y = Math.Min(minmax.y, points[i].Y);
			minmax.z = Math.Max(minmax.z, points[i].X);
			minmax.w = Math.Max(minmax.w, points[i].Y);
		}

		minmax.x /= camSize.x;
		minmax.y /= camSize.y;
		minmax.z /= camSize.x;
		minmax.w /= camSize.y;

		return new Rect(minmax.x, minmax.y, minmax.z - minmax.x, minmax.w - minmax.y);
	}
}
