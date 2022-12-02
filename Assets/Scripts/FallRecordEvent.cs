using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallRecordEvent
{
    public Vector3 position;
    public Quaternion rotation;
    public float time;

    public FallRecordEvent(Vector3 position, Quaternion rotation, float time)
    {
        this.position = position;
        this.rotation = rotation;
        this.time = time;
    }

    public void Apply(Transform transform)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
