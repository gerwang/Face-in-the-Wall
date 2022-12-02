using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallTimeBody : MonoBehaviour
{
    public float recordTime = 5f;
    private bool isRecording = false;
    private float recordStartTime, recordEndTime;
    private List<FallRecordEvent> recordedEvents = new List<FallRecordEvent>();
    private bool isRewinding = false;
    private float rewindStartTime, rewindEndTime;
    private Rigidbody rb;
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

    public void StartRewindScaled(float scaleFactor)
    {
        StopRecord();
        float targetLength = (recordEndTime - recordStartTime) * scaleFactor;
        StartRewind(targetLength);
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
            rb.isKinematic = true;
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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Rewind()
    {
        if (recordedEvents.Count == 0)
        {
            StopRewind();
        }
        else
        {
            float rewindProcess = (Time.time - rewindStartTime) / (rewindEndTime - rewindStartTime);
            FallRecordEvent lastEvent = null;
            while (recordedEvents.Count > 0)
            {
                var tmpEvent = recordedEvents[recordedEvents.Count - 1];
                float eventProcess = (recordEndTime - tmpEvent.time) / (recordEndTime - recordStartTime);
                if (eventProcess > rewindProcess)
                {
                    break;
                }
                lastEvent = tmpEvent;
                recordedEvents.RemoveAt(recordedEvents.Count - 1);
            }
            if (lastEvent != null)
            {
                lastEvent.Apply(transform);
            }
        }
    }

    private void Record()
    {
        if (Time.time > recordStartTime + recordTime)
        {
            StopRecord();
        }
        else
        {
            recordedEvents.Add(new FallRecordEvent(transform.position, transform.rotation, Time.time));
        }
    }

    void FixedUpdate()
    {
        if (isRewinding)
        {
            Rewind();
        }
        else if (isRecording)
        {
            Record();
        }
    }
}
