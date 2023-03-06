using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perception : MonoBehaviour
{
    public Dictionary<GameObject, MemoryRecord> memoryMap = new();

    public GameObject[] sensedObjects;
    public MemoryRecord[] sensedRecord;

    public void ClearFOV()
    {
        foreach(KeyValuePair<GameObject, MemoryRecord> memory in memoryMap)
        {
            memory.Value.withinFOV = false;
        }
    }
    public void AddMemory(GameObject target)
    {
        MemoryRecord record = new(target.transform.position, true);
        if(memoryMap.ContainsKey(target))
            memoryMap[target] = record;
        else
            memoryMap.Add(target, record);
    }
    private void Update()
    {
        sensedObjects = new GameObject[memoryMap.Keys.Count];
        sensedRecord = new MemoryRecord[memoryMap.Values.Count];
        memoryMap.Keys.CopyTo(sensedObjects, 0);
        memoryMap.Values.CopyTo(sensedRecord, 0);
    }
}

[Serializable]
public class MemoryRecord
{

    [SerializeField]
    public Vector3 lastSensedPosition;

    [SerializeField]
    public bool withinFOV;


    public MemoryRecord()
    {
        lastSensedPosition = Vector3.zero;
        withinFOV = false;
    }
    public MemoryRecord(Vector3 _pos, bool _fov)
    {
        lastSensedPosition = _pos;
        withinFOV = _fov;
    }
}