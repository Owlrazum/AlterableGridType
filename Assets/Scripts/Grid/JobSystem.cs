using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public struct JobSwapGrid : IJobParallelForTransform
{
    [ReadOnly]
    public NativeArray<Vector3> delta;
    public void Execute(int index, TransformAccess transform)
    {
        transform.position += delta[index];
    }
}
