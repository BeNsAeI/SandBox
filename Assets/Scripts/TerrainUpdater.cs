using UnityEngine;
using System.Collections;
using RSUnityToolkit;
using System;
using System.Linq; 
using UnityEngine.UI;
using System.Collections.Generic;

using System.Threading;
public class TerrainUpdater : MonoBehaviour {

	public Text LogText;
	public Text Output;
	public int captureRate;
	public UInt16 minimumDistanceMm;
	public UInt16 a, b, c, d,CenterTest;
	public int margin;
//	public Terrain myTerrain;

	private PXCMSenseManager psm;
	private pxcmStatus sts;
	private PXCMImage depthImage;
	private const int CAM_WIDTH = 640;
	private const int CAM_HEIGHT = 480;
	private UInt16[][] plane;
	private int frameCounter;

	private Rigidbody rb;

	void OnDisable()
	{
		if (psm == null) return;
		psm.Dispose();
	}
	void RSSetup()
	{
		//_____________________________REALSENSE_________________________
		/* Initialize a PXCMSenseManager instance */
		psm = PXCMSenseManager.CreateInstance();
		if (psm == null){
			Debug.LogError("SenseManager Initialization Failed");
			return;
		}
		/* Enable the depth stream of size 640x480 and color stream of size 640x480 */
		psm.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_DEPTH, 640, 480);
		/* Initialize the execution pipeline */
		sts = psm.Init();
		if (sts != pxcmStatus.PXCM_STATUS_NO_ERROR){
			Debug.LogError("PXCMSenseManager.Init Failed");
			OnDisable(); // Clean-up
			return;
		}
		//_____________________________END_________________________
	}
	void RSCapture()
	{
		//----------------------RS---------------------
		/* Make sure PXCMSenseManager Instance is Initialized */
		if (psm == null){
			LogText.text = "PXCMSM Failed";
			return;
		}
		/* Wait until any frame data is available true(aligned) false(unaligned) */
		if (psm.AcquireFrame(true) != pxcmStatus.PXCM_STATUS_NO_ERROR)
		{
			LogText.text = "Waiting...";
			return;
		}
		/* Retrieve a sample from the camera */
		PXCMCapture.Sample sample = psm.QuerySample();
		if (sample != null)
		{
			LogText.text = "Capturing...";
		}

		//-----UVMap-----//
		PXCMImage.ImageData imageData = new PXCMImage.ImageData();
		sample.depth.AcquireAccess(PXCMImage.Access.ACCESS_READ_WRITE, PXCMImage.PixelFormat.PIXEL_FORMAT_DEPTH, PXCMImage.Option.OPTION_ANY,out imageData);
		bool found = false;

		unsafe
		{
			UInt16 * ptr = (UInt16*)imageData.planes[0].ToPointer();

			ulong length = (ulong)(sample.depth.info.width * sample.depth.info.height);
			for (ulong i = 0; ((i < length) && !found); i++, ptr++)  
			{  
				found = (*ptr > 0) && (*ptr < minimumDistanceMm);  
			} 
			a = ptr[120 * sample.depth.info.width + 320];
			b = ptr[360 * sample.depth.info.width + 320];
			c = ptr[240 * sample.depth.info.width + 160];
			d = ptr[240 * sample.depth.info.width + 480];
			//	indexer = row*width + column;
		}
		if(found)
			Output.text = "Pass";
		else
			Output.text = "Fail";
		//-----EOUVM-----//

		/* Release the frame to process the next frame */		
		depthImage = sample.depth;
		psm.ReleaseFrame();
		//---------------------EORS--------------------
	}
	void Start ()
	{
		rb = GetComponent<Rigidbody>();
		RSSetup ();
		frameCounter = 0;
	}


	void FixedUpdate ()
	{
		//		Update Terrain here:
		if (frameCounter > captureRate) {
			RSCapture ();
			frameCounter = 0;
		} else {
			LogText.text = "Capturing";
			frameCounter++;
		}
		// template: this.transform.localScale.x.ToString();

	}
}