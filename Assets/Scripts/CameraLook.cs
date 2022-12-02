using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Camera Look")]
public class CameraLook : MonoBehaviour
{

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 };
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 150F;
	public float sensitivityY = 150F;
	 
	public float minimumX = -360F;
	public float maximumX = 360F;
	 
	public float minimumY = -60F;
	public float maximumY = 60F;
	public Vector3 refVelocity = Vector3.one;
    public float smoothTime = 0.0f;
    public float maxSmoothSpeed = 5.0f;
    public Transform fixTransform;
    public SoundManager soundManager;
    public float soundTriggerLimit;

    private bool playingFace = false;

	float positionX = 0F;
	float positionY = 0F;
	
	private Vector3 pos;
	Quaternion originalRotation;
    private FaceCaptureSDK faceCaptureSDK;

    void LateUpdate()
    {
        if (playingFace)
        {
            var dx = faceCaptureSDK.GetMockedInputAxisX();
            var dy = faceCaptureSDK.GetMockedInputAxisY();
            positionX += dx * sensitivityX;
            positionY += dy * sensitivityY;

            if ((Mathf.Abs(dx) > soundTriggerLimit) ||
                (Mathf.Abs(dy) > soundTriggerLimit))
            {
                soundManager.StartAudio();
            }

            if ((Mathf.Abs(dx) < soundTriggerLimit) ||
                (Mathf.Abs(dy) < soundTriggerLimit))
			{
				soundManager.stopAudio();
			}

			positionX = Mathf.Clamp(positionX, minimumX, maximumX);
			positionY = Mathf.Clamp(positionY, minimumY, maximumY);
		

			Vector3 toPos = fixTransform.position + new Vector3(positionX, positionY, 0.0f);
			Vector3 curPos = Vector3.SmoothDamp(transform.position, toPos, ref refVelocity, smoothTime, maxSmoothSpeed);
			transform.position = curPos;
		}
	}
	 
	void Start()
	{
		pos = transform.position;
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
		originalRotation = transform.localRotation;
        faceCaptureSDK = FindObjectOfType<FaceCaptureSDK>();
	}
	 
	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}

	public void enableCameraLook(Transform t)
	{	
		playingFace = true;
		fixTransform = t;
		positionX = 0;
		positionY = 0;
	}
	
	public void disableCameraLook()
	{	
		playingFace = false;
		soundManager.stopAudio();
	}
}