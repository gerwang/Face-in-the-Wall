using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppableRig : MonoBehaviour
{

    private const int blendShapeOffset = 12;
    public int blendShapeCount = 50;
    private const float scale = 1e5f;
    public Transform launchCenter;

    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private bool[] meshDeads;
    public int liveCount;

    public float explosionForce = 200.0f;
    public float explisionRadius = 1.0f;
    public float upwardsModifier = 3.0f;
    public int stayFrameCnt = 3;

    public int[][] eventIndices = new int[][]{
        new int[]{33, 34 ,36, 31, 32},
        new int[]{19,29, 30},
        new int[]{9, 10, 11, 15, 16,21},
        new int[]{0, 1 ,2},
        new int[]{27, 28 },
        new int[]{6, 7 ,8, 17, 18, 20 },
        new int[]{22, 23, 24, 25, 26, 35},
        new int[]{12, 13, 14 },
        new int[]{3, 4, 5},
    };

    private int[] eventCounter = new int[9];
    private bool isRecording = false;
    private float recordStartTime, recordEndTime;
    private List<FaceMovementEvent> recordedEvents = new List<FaceMovementEvent>();
    private bool isRewinding = false;
    private float rewindStartTime, rewindEndTime;
    private float[] latestValues;

    public bool droppable = true;

    [HideInInspector]
    public bool rewindDone = false;

    public void StartRecord()
    {
        if (!isRecording)
        {
            if (isRewinding)
            {
                StopRewind();
            }
            recordedEvents.Clear();
            isRecording = true;
            recordStartTime = Time.time;
        }
    }

    public void StopRecord()
    {
        if (isRecording)
        {
            isRecording = false;
            recordEndTime = Time.time;
        }
    }

    public void StartRewind(float targetLength)
    {
        if (!isRewinding)
        {
            if (isRecording)
            {
                StopRecord();
            }
            isRewinding = true;
            rewindDone = false;
            rewindStartTime = Time.time;
            rewindEndTime = rewindStartTime + targetLength;
        }
    }

    private void StopRewind() // I'm unstoppable
    {
        if (isRewinding)
        {
            isRewinding = false;
            rewindDone = true;
        }
    }

    public void notifyNewMovement(FaceMovementEvent e)
    {
        if (isRecording)
        {
            recordedEvents.Add(e);
        }
    }


    public void TriggerEvent(int ev, bool happened)
    {
        if (eventCounter[ev] == 0)
        {
            if (happened)
            {
                eventCounter[ev]++;
            }
            else
            {
                eventCounter[ev]--;
            }
        }
        else if (eventCounter[ev] < 0)
        {
            if (happened)
            {
                eventCounter[ev] = 1;
            }
            else
            {
                eventCounter[ev]--;
            }
        }
        else // if(eventCounter[ev]>0)
        {
            if (happened)
            {
                eventCounter[ev]++;
                if (eventCounter[ev] == stayFrameCnt)
                {
                    List<int> alivedIndices = new List<int>();
                    foreach (var x in eventIndices[ev])
                    {
                        if (!meshDeads[x])
                        {
                            alivedIndices.Add(x);
                        }
                    }
                    if (alivedIndices.Count > 0)
                    {
                        var selected = alivedIndices[Random.Range(0, alivedIndices.Count - 1)];
                        Drop(selected);
                    }
                }
            }
            else
            {
                eventCounter[ev] = -1;
            }
        }
    }

    void Awake()
    {
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        meshDeads = new bool[skinnedMeshRenderers.Length];
        blendShapeCount = skinnedMeshRenderers[0].sharedMesh.blendShapeCount;
        liveCount = skinnedMeshRenderers.Length;
        /*
        foreach (var x in GetComponentsInChildren<BoxCollider>())
        {
            x.isTrigger = true;
        }
        */
    }

    public void Drop(int index)
    {
        if (!droppable)
        {
            return;
        }
        if (!meshDeads[index])
        {
            meshDeads[index] = true;
            var rigidbody = skinnedMeshRenderers[index].gameObject.GetComponent<Rigidbody>();
            var collider = skinnedMeshRenderers[index].gameObject.GetComponent<BoxCollider>();
            collider.isTrigger = false;
            rigidbody.isKinematic = false;
            rigidbody.AddExplosionForce(explosionForce, launchCenter.position, explisionRadius, upwardsModifier);
            liveCount--;
            if (isRecording)
            {
                recordedEvents.Add(new FaceMovementEvent(null, index, true, Time.time));
                var timeBody = skinnedMeshRenderers[index].gameObject.GetComponent<FallTimeBody>();
                timeBody.StartRecord();
            }
        }
    }

    public void DropAll() 
    {
        for (int i = 0; i < meshDeads.Length; i++) 
        { 
                Drop(i);
        }
    }

    public void Restore(int index)
    {
        if (meshDeads[index])
        {
            meshDeads[index] = false;
            var rigidbody = skinnedMeshRenderers[index].gameObject.GetComponent<Rigidbody>();
            var collider = skinnedMeshRenderers[index].gameObject.GetComponent<BoxCollider>();
            collider.isTrigger = true;
            rigidbody.isKinematic = true;
            rigidbody.transform.localRotation = new Quaternion();
            rigidbody.transform.localPosition = new Vector3();
            rigidbody.transform.localScale = Vector3.one;
            liveCount++;
            if (latestValues != null)
            {
                SetPartBlendshapeWeight(index, latestValues);
            }
        }
    }

    private void SetPartBlendshapeWeight(int partIndex, float[] values)
    {
        for (var i = 0; i < blendShapeCount; i++)
        {
            var weight = 100f * values[blendShapeOffset + i];
            if (i < 40)
            {
                weight /= scale;
            }
            skinnedMeshRenderers[partIndex].SetBlendShapeWeight(i, weight);
        }
    }

    public void RestoreAll()
    {
        for (var i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            Restore(i);
        }
    }

    public void SetBlendshapeWeights(float[] weights)
    {
        for (var j = 0; j < skinnedMeshRenderers.Length; j++)
        {
            if (!meshDeads[j])
            {
                for (var i = 0; i < blendShapeCount; i++)
                {
                    skinnedMeshRenderers[j].SetBlendShapeWeight(i, weights[i]);
                }
            }
        }
    }

    public void SetBlendShapeWeight(int index, float weight)
    {
        for (var j = 0; j < skinnedMeshRenderers.Length; j++)
        {
            if (!meshDeads[j])
            {
                skinnedMeshRenderers[j].SetBlendShapeWeight(index, weight);
            }
        }
    }

    // private float totalTime = 0.0f;
    // private bool firstDrop = true, firstRestore = true;

    // Start is called before the first frame update
    // float startTime;
    void Start()
    {
        // FindObjectOfType<FaceCaptureSDK>().registerListener(this); // fixme
        // startTime = Time.time;
        // StartRecord(); // fixme
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRewind();
        // if (Time.time > startTime + 30f)
        // {
        //     StartRewind(5f);
        // }
    }

    void UpdateRewind()
    {
        if (isRewinding)
        {
            if (recordedEvents.Count == 0)
            {
                StopRewind();
            }
            else
            {
                float rewindProcess = (Time.time - rewindStartTime) / (rewindEndTime - rewindStartTime);
                FaceMovementEvent lastEvent = null;
                while (recordedEvents.Count > 0)
                {
                    var tmpEvent = recordedEvents[recordedEvents.Count - 1];
                    float eventProcess = (recordEndTime - tmpEvent.time) / (recordEndTime - recordStartTime);
                    if (eventProcess > rewindProcess)
                    {
                        break;
                    }
                    if (tmpEvent.isDrop)
                    {
                        var index = tmpEvent.dropIndex;
                        var timeBody = skinnedMeshRenderers[index].gameObject.GetComponent<FallTimeBody>();
                        timeBody.StartRewindScaled((rewindEndTime - rewindStartTime) / (recordEndTime - recordStartTime));
                    }
                    else
                    {
                        lastEvent = tmpEvent;
                    }
                    recordedEvents.RemoveAt(recordedEvents.Count - 1);
                }
                if (lastEvent != null)
                {
                    SetNewInput(lastEvent.values);
                }
            }
        }
        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            var timeBody = skinnedMeshRenderers[i].GetComponent<FallTimeBody>();
            if (meshDeads[i] && timeBody.rewindDone)
            {
                timeBody.rewindDone = false;
                Restore(i);
            }
        }
    }


    private float convertRange(float value)
    {
        if (value > 180)
        {
            value -= 360;
        }
        return value;
    }

    public void NotifyNewInput(float[] values)
    {
        if (isRewinding)
        {
            Debug.Log("Warning: Don't set value during rewinding!");
        }
        else
        {
            SetNewInput(values);
        }
    }

    private void SetNewInput(float[] values)
    {
        var mat = new Matrix4x4();
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                mat[i, j] = values[i * 4 + j];
            }
        }
        SetTransformFromMatrix(transform, ref mat);
        for (var i = 0; i < blendShapeCount; i++)
        {
            var weight = 100f * values[blendShapeOffset + i];
            if (i < 40)
            {
                weight /= scale;
            }
            SetBlendShapeWeight(i, weight);
        }
        if (isRecording)
        {
            recordedEvents.Add(new FaceMovementEvent(values, -1, false, Time.time));
        }
        if (!isRewinding)
        {
            TriggerEvent(0, values[blendShapeOffset + 40] < -5);
            // Debug.Log("mouth open");
            // 33 34 36 31 32 
            TriggerEvent(1, values[blendShapeOffset + 41] > 1);
            // Debug.Log("chin raise");
            // 19 29 30
            TriggerEvent(2, values[blendShapeOffset + 46] < -0.5);
            // Debug.Log("brow raise");
            // 9 10 11 15 16 21 
            TriggerEvent(3, convertRange(transform.localEulerAngles.x) > 45.0f);
            // Debug.Log("Look down");
            // 0 1 2
            TriggerEvent(4, convertRange(transform.localEulerAngles.x) < -45.0f);
            // Debug.Log("Look up");
            // 27 28 
            TriggerEvent(5, convertRange(transform.localEulerAngles.y) > 45.0f);
            // Debug.Log("Turn right");
            // 6 7 8 17 18 20 
            TriggerEvent(6, convertRange(transform.localEulerAngles.y) < -45.0f);
            // Debug.Log("Turn left");
            // 22 23 24 25 26 35
            TriggerEvent(7, convertRange(transform.localEulerAngles.z) > 30);
            // Debug.Log("Left tilt");
            // 12 13 14 
            TriggerEvent(8, convertRange(transform.localEulerAngles.z) < -30);
            // Debug.Log("right tilt");
            // 3 4 5
        }

        latestValues = values;
    }

    
    private void OnDestroy()
    {
        StopSynchronizing();
    }

    public void StartSynchronizing()
    {
        FindObjectOfType<FaceCaptureSDK>().registerListener(this);
    }

    public void StopSynchronizing()
    {
        FindObjectOfType<FaceCaptureSDK>().unregisterListener(this);
    }


    /// <summary>
    /// Set transform component from TRS matrix.
    /// </summary>
    /// <param name="transform">Transform component.</param>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    public void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
    {
        var rigLocalRotation = ExtractRotationFromMatrix(ref matrix);
        // transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
        // transform.localScale = ExtractScaleFromMatrix(ref matrix);
        float x, y;
        // if (constrainXY)
        // {
        x = transform.localPosition.x;
        y = transform.localPosition.y;
        // }
        // else
        // {
        //     var localPosition = ExtractTranslationFromMatrix(ref matrix);
        //     x = (localPosition.x / cameraWidth - 0.5f) * radiusX;
        //     y = (localPosition.y / cameraHeight - 0.5f) * radiusY;
        // }
        float z;
        // if (constrainZ)
        // {
        z = transform.localPosition.z;
        // }
        // else
        // {
        //     var localScale = ExtractScaleFromMatrix(ref matrix);
        //     var uniformScale = Mathf.Sqrt(localScale.x * localScale.y);
        //     z = -referenceZ / uniformScale;
        // }
        var rigLocalPosition = new Vector3(x, y, z);
        transform.localRotation = rigLocalRotation;
        transform.localPosition = rigLocalPosition;
    }

    /// <summary>
    /// Extract translation from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Translation offset.
    /// </returns>
    public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 translate;
        translate.x = matrix.m03;
        translate.y = matrix.m13;
        translate.z = matrix.m23;
        return translate;
    }

    /// <summary>
    /// Extract rotation quaternion from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Quaternion representation of rotation transform.
    /// </returns>
    public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    /// <summary>
    /// Extract scale from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Scale vector.
    /// </returns>
    public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }

    /// <summary>
    /// Extract position, rotation and scale from TRS matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <param name="localPosition">Output position.</param>
    /// <param name="localRotation">Output rotation.</param>
    /// <param name="localScale">Output scale.</param>
    public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
    {
        localPosition = ExtractTranslationFromMatrix(ref matrix);
        localRotation = ExtractRotationFromMatrix(ref matrix);
        localScale = ExtractScaleFromMatrix(ref matrix);
    }
}
