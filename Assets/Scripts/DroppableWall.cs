using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppableWall : MonoBehaviour
{
    public Transform launchCenter;
    private BoxCollider[] boxColliders;
    private bool[] meshDeads;
    private int liveCount;
    public float explosionForce = 200.0f;
    public float explisionRadius = 1.0f;
    public float upwardsModifier = 3.0f;

    private int[] groupStartIndex = new int[]{
        0,12,24,36,49,57,69,78,87
    };
    private int[] groupEndIndex = new int[]{
        11,23,35,48,56,68,77,86,94
    };

    public void Drop(int index)
    {
        if (!meshDeads[index])
        {
            meshDeads[index] = true;
            var rigidbody = boxColliders[index].gameObject.GetComponent<Rigidbody>();
            var collider = boxColliders[index];
            var timeBody = boxColliders[index].GetComponent<FallTimeBody>();
            timeBody.StartRecord();
            collider.isTrigger = false;
            rigidbody.isKinematic = false;
            rigidbody.AddExplosionForce(explosionForce, launchCenter.position, explisionRadius, upwardsModifier);
            liveCount--;
        }
    }

    public void StartRecordPart(int index)
    {
        
    }

    public void DropGroup(int index)
    {
        for (int i = groupStartIndex[index]; i <= groupEndIndex[index]; i++)
        {
            Drop(i);
        }
    }

    public void RestoreGroup(int index)
    {
        for (int i = groupStartIndex[index]; i <= groupEndIndex[index]; i++)
        {
            Restore(i);
        }
    }

    public void Restore(int index)
    {
        if (meshDeads[index])
        {
            meshDeads[index] = false;
            var rigidbody = boxColliders[index].gameObject.GetComponent<Rigidbody>();
            var collider = boxColliders[index];
            collider.isTrigger = true;
            rigidbody.isKinematic = true;
            rigidbody.transform.localRotation = new Quaternion();
            rigidbody.transform.localPosition = new Vector3();
            rigidbody.transform.localScale = Vector3.one;
            liveCount++;
        }
    }

    public void RestoreAll()
    {
        for (var i = 0; i < boxColliders.Length; i++)
        {
            Restore(i);
        }
    }

    void Awake()
    {
        
    }

    // float startTime;

    // Start is called before the first frame update
    void Start()
    {
        // startTime = Time.time;
        // DropGroup(1);
        boxColliders = GetComponentsInChildren<BoxCollider>();
        meshDeads = new bool[boxColliders.Length];
        liveCount = boxColliders.Length;
    }

    // Update is called once per frame
    void Update()
    {
        // if (Time.time > startTime + 7.0f)
        // {
            // RewindAll(5.0f);
        // }
        UpdateRewind();
    }

    private void UpdateRewind()
    {
        for (int i = 0; i < boxColliders.Length; i++)
        {
            var timeBody = boxColliders[i].GetComponent<FallTimeBody>();
            if (meshDeads[i] && timeBody.rewindDone)
            {
                timeBody.rewindDone = false;
                Restore(i);
            }
        }
    }

    private void RewindPart(int index, float targetLength)
    {
        var timeBody = boxColliders[index].GetComponent<FallTimeBody>();
        timeBody.StartRewind(targetLength);
    }

    public void RewindAll(float targetLength)
    {
        for (int i = 0; i < boxColliders.Length; i++)
        {
            RewindPart(i, targetLength);
        }
    }
}
