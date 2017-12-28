using System;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using ZXing;

public class TestQR : MonoBehaviour {
	public UserDefinedTargetBuildingBehaviour targetBuilder;
	public float minAreaForUserTargetCreation = .3f;
	public bool debug = false;
	public string hardcodedURL = "";
	public Texture2D debugMarker;

	float currentArea;
	Vector2 camSize;
	ResultPoint[] points = new ResultPoint[0];
	IBarcodeReader barcodeReader = new BarcodeReader();
	Image.PIXEL_FORMAT mPixelFormat;
	Image.PIXEL_FORMAT[] mPixelFormatOpts = new Image.PIXEL_FORMAT[] {
		Image.PIXEL_FORMAT.RGBA8888,
		Image.PIXEL_FORMAT.RGB888
	};
	bool mFormatRegistered = false;

	void Start() {
		VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);  
		VuforiaARController.Instance.RegisterOnPauseCallback(OnPaused);
	}

	void OnVuforiaStarted() {
		CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
		
		foreach (var format in mPixelFormatOpts) {
			if (CameraDevice.Instance.SetFrameFormat(format, true)) {
				Debug.Log("Successfully registered pixel format " + mPixelFormat.ToString());
				mFormatRegistered = true;
				mPixelFormat = format;
				return;
			}
		}

		Debug.LogError(
			"Failed to register pixel format " + mPixelFormat.ToString() +
			"\n the format may be unsupported by your device;" +
			"\n consider using a different pixel format.");
		mFormatRegistered = false;
	}
	private void OnPaused(bool paused) {    
		if (!paused)  {
			// Set again autofocus mode when app is resumed
			CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);    
		}
	}

	public static List<string> loadedQRs = new List<string>();
	string decodedText = "";

	void Update() {
		decodedText = "";
		if (!mFormatRegistered) return;

		if (hardcodedURL != "") {
			targetBuilder.BuildNewTarget("" + loadedQRs.Count, 1);
			loadedQRs.Add(hardcodedURL);
			
			hardcodedURL = "";
			return;
		}
		
		Image image = CameraDevice.Instance.GetCameraImage(mPixelFormat);

		try {
			// decode the current frame
			camSize = new Vector2(image.Width, image.Height);
			var result = barcodeReader.Decode(image.Pixels, image.Width, image.Height, mPixelFormat == Image.PIXEL_FORMAT.RGBA8888 ? RGBLuminanceSource.BitmapFormat.RGBA32 : RGBLuminanceSource.BitmapFormat.RGB24);
			decodedText = result.Text;
			if (result != null) {
				points = result.ResultPoints;
				Rect r = GetPercentageRect(points);
				currentArea = r.width * r.height;
				if (!loadedQRs.Contains(result.Text) && currentArea >= minAreaForUserTargetCreation) {
					targetBuilder.BuildNewTarget("" + loadedQRs.Count, 1);
					loadedQRs.Add(result.Text);
				}
			}
		}
		catch (Exception ex) { Debug.LogWarning(ex.Message); }
	}

	void OnGUI() {
		if (!mFormatRegistered) return;
		if (!debug) return;
		if (decodedText == "") return;

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
		GUI.Box(r, currentArea + " < " + minAreaForUserTargetCreation + "\n\n" + decodedText);
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
