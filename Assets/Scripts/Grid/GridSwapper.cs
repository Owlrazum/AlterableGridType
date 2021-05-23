using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class GridSwapper : MonoBehaviour
{
    public enum GridType
    {
        Tri,
        Sqr,
        Hex
    }
    public static GridType currentGrid;

    [SerializeField]
    TriangularGrid triangGrid;

    [SerializeField]
    SquareGrid squareGrid;

    [SerializeField]
    HexGrid hexGrid;

    [SerializeField]
    TileMaterial tileMaterial;


    List<List<Tile>> tiles;
    Transform[] transforms;

    Vector3[] triToSqr;
    Vector3[] triToHex;
    Vector3[] sqrToHex;

    Vector3[] sqrToTri;
    Vector3[] hexToTri;
    Vector3[] hexToSqr;
    private void Start()
    {
        PlayerInput.Instance.OnChangeGrid += SwapGrids;
        EventSystem.Instance.OnTilesInitialized += Initialize;
        EventSystem.Instance.GridSwapperReady();
    }

    #region State
    private enum State
    {
        Tri,
        Sqr,
        Hex
    }

    private State state;
    private State ItsState
    {
        get { return state; }
        set
        {
            if (state == value)
            {
                return;
            }
            switch (value)
            {
                case State.Tri:
                    triangGrid.enabled = true;
                    squareGrid.enabled = false;
                    hexGrid.enabled = false;
                    currentGrid = GridType.Tri;
                    EventSystem.Instance.GridSwapped(currentGrid);
                    break;
                case State.Sqr:
                    squareGrid.enabled = true;
                    triangGrid.enabled = false;
                    hexGrid.enabled = false;
                    currentGrid = GridType.Sqr;
                    EventSystem.Instance.GridSwapped(currentGrid);
                    break;
                case State.Hex:
                    hexGrid.enabled = true;
                    triangGrid.enabled = false;
                    squareGrid.enabled = false;
                    currentGrid = GridType.Hex;
                    EventSystem.Instance.GridSwapped(currentGrid);
                    break;
            }
            state = value;
        }
    }
    #endregion


    private const float swapDeltaTime = 1.5f;

    //private const float hexAdj = 1.8f;
    //private const float hexInner = 0.72f;
    //private const float hexRow = 1;

    private void Initialize()
    {
        tiles = GridController.tiles;

        currentGrid = GridType.Tri;

        int c = tiles.Count * tiles[0].Count;
        transforms = new Transform[c];
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                transforms[i * tiles[0].Count + j] = tiles[i][j].transform;
            }
        }
        
        triToSqr = new Vector3[c];
        triToHex = new Vector3[c];
        sqrToHex = new Vector3[c];

        sqrToTri = new Vector3[c];
        hexToTri = new Vector3[c];
        hexToSqr = new Vector3[c];

        #region triToSqr
        List<List<Vector3>> triToSqrList = new List<List<Vector3>>();

        List<Vector3> bufferMiddle = new List<Vector3>();
        int middleTri = tiles.Count / 2 - 1;
        for (int i = 0; i < tiles[middleTri].Count; i++)
        {
            bufferMiddle.Add(tiles[middleTri][i].transform.position);
        }
        float middleZTri;
        float deltaZ = -1.3f; // first usage for displace of one half rows
        for (int i = 0; i < tiles.Count; i++)
        {
            triToSqrList.Add(new List<Vector3>());
        }
        for (int i = 0; i < tiles.Count / 2; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                triToSqrList[i].Add((tiles[i + 1][j].transform.position
                              + tiles[i][j].transform.position) / 2);
                Vector3 t = triToSqrList[i][j];
                t.x *= 1.7f;
                triToSqrList[i][j] = t;
                //tiles[i][j].transform.position = newPos[i][j];
                //yield return new WaitForSeconds(0.1f);
            }
        }
        for (int i = tiles.Count - 1; i > tiles.Count / 2; i--)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                triToSqrList[i].Add((tiles[i - 1][j].transform.position
                              + tiles[i][j].transform.position) / 2);
                Vector3 t = triToSqrList[i][j];
                t.x *= 1.7f;
                triToSqrList[i][j] = t;
            }
            middleZTri = (triToSqrList[i][0].z
                           + triToSqrList[i][1].z) / 2;
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Vector3 t = triToSqrList[i][j];
                t.z = middleZTri - deltaZ;
                triToSqrList[i][j] = t;
            }
        }
        for (int j = 0; j < tiles[middleTri + 1].Count; j++)
        {
            triToSqrList[middleTri + 1].Add((bufferMiddle[j]
                          + tiles[middleTri + 1][j].transform.position) / 2);
            Vector3 t = triToSqrList[middleTri + 1][j];
            t.x *= 1.7f;
            triToSqrList[middleTri + 1][j] = t;
        }
        middleZTri = (triToSqrList[middleTri + 1][0].z
                       + triToSqrList[middleTri + 1][1].z) / 2;
        for (int j = 0; j < tiles[middleTri + 1].Count; j++)
        {
            Vector3 t = triToSqrList[middleTri + 1][j];
            t.z = middleZTri - deltaZ;
            triToSqrList[middleTri + 1][j] = t;
        }
        deltaZ = triToSqrList[middleTri + 1][0].z - triToSqrList[middleTri][0].z;
        middleTri += 1;
        for (int i = middleTri + 1; i < tiles.Count; i++)
        {
            float currDeltaZ = triToSqrList[i][0].z - triToSqrList[i - 1][0].z;
            float secondDelta = currDeltaZ - deltaZ;
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Vector3 t = triToSqrList[i][j];
                t.z -= secondDelta;
                triToSqrList[i][j] = t;
            }
        }
        middleTri -= 1;
        for (int i = middleTri - 1; i >= 0; i--)
        {
            float currDeltaZ = triToSqrList[i + 1][0].z - triToSqrList[i][0].z;
            float secondDelta = currDeltaZ - deltaZ;
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Vector3 t = triToSqrList[i][j];
                t.z += secondDelta;
                triToSqrList[i][j] = t;
            }
        }
        List<List<Vector3>> deltaPos = new List<List<Vector3>>();
        for (int i = 0; i < tiles.Count; i++)
        {
            deltaPos.Add(new List<Vector3>());
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                deltaPos[i].Add((triToSqrList[i][j] - tiles[i][j].transform.position) / 100);
            }
        }
        triToSqrList = deltaPos;

        for (int i = 0; i < triToSqrList.Count; i++)
        {
            for (int j = 0; j < triToSqrList[i].Count; j++)
            {
                int index = (i * triToSqrList[0].Count) + j;
                triToSqr[index] = triToSqrList[i][j];
                sqrToTri[index] = -triToSqrList[i][j]; 
            }
        }
        #endregion



        List<List<Vector3>> triTiles = new List<List<Vector3>>();
        for (int i = 0; i < tiles.Count; i++)
        {
            triTiles.Add(new List<Vector3>());
            for (int j = 0; j < tiles[i].Count; j++)
            {
                triTiles[i].Add(triToSqrList[i][j] * 100 + tiles[i][j].transform.position);
            }
        }

        #region sqrToHex
        List<List<Vector3>> sqrToHexList = new List<List<Vector3>>();
        for (int i = 0; i < tiles.Count; i++)
        {
            sqrToHexList.Add(new List<Vector3>());
            for (int j = 0; j < tiles[i].Count; j++)
            {
                sqrToHexList[i].Add(triTiles[i][j]);
                Vector3 newPos = sqrToHexList[i][j];
                newPos.x *= 1.35f;
                newPos.z *= 1.4f;//0.7f;
                sqrToHexList[i][j] = newPos;
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            float middleZ = (sqrToHexList[i][0].z + sqrToHexList[i][1].z) / 2;
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Vector3 newPos = sqrToHexList[i][j];
                newPos.z = middleZ;//0.7f;
                sqrToHexList[i][j] = newPos;
            }
        }
        for (int i = 0; i < tiles[0].Count; i++)
        {
            if (i % 2 == 1)
            {
                for (int j = 0; j < tiles.Count; j++)
                {
                    Vector3 newPos = sqrToHexList[j][i];
                    newPos.z -= 0.91f;//0.7f;
                    sqrToHexList[j][i] = newPos;
                }
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                sqrToHexList[i][j] = (sqrToHexList[i][j] - triTiles[i][j]) / 100;
            }
        }
        for (int i = 0; i < sqrToHexList.Count; i++)
        {
            for (int j = 0; j < sqrToHexList[i].Count; j++)
            {
                int index = (i * sqrToHexList[0].Count) + j;
                sqrToHex[index] = sqrToHexList[i][j];
                hexToSqr[index] = -sqrToHexList[i][j];
            }
        }

        #endregion






        #region triToHex
        List<List<Vector3>> triToHexList = new List<List<Vector3>>();
        for (int i = 0; i < tiles.Count; i++)
        {
            triToHexList.Add(new List<Vector3>());
            for (int j = 0; j < tiles[i].Count; j++)
            {
                triToHexList[i].Add((triToSqrList[i][j] + sqrToHexList[i][j]));
            }
        }
        for (int i = 0; i < triToHexList.Count; i++)
        {
            for (int j = 0; j < triToHexList[i].Count; j++)
            {
                int index = (i * triToHexList[0].Count) + j;
                triToHex[index] = triToHexList[i][j];
                hexToTri[index] = -triToHexList[i][j];
            }
        }
        #endregion
    }

    private void SwapGrids()
    {
        if (ItsState == State.Tri)
        {
            ItsState = State.Hex;
            triangGrid.DisableReflines();
            StartCoroutine(SwappingTriToHex());
        }
        else if (ItsState == State.Sqr)
        {
            ItsState = State.Tri;
            StartCoroutine(SwappingSqrToTri());
        }
        else if (ItsState == State.Hex)
        {
            ItsState = State.Sqr;
            StartCoroutine(SwappingHexToSqr());
        }
        else 
        {
            Debug.Log("How?!");
        }
    }

    #region TriToSqr
    private IEnumerator SwappingTriToSqr()
        {
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].EnableSqrPart();
            }
        }
        JobSwapGrid jobSwap = new JobSwapGrid();
        NativeArray<Vector3> deltaData = new NativeArray<Vector3>(triToSqr, Allocator.TempJob);
        jobSwap.delta = deltaData;
        TransformAccessArray transAccArr = new TransformAccessArray(transforms);

        for (int z = 0; z < 100; z++)
        {
            tileMaterial.IncSqrAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(swapDeltaTime * Time.deltaTime);
        }

        deltaData.Dispose();
        transAccArr.Dispose();
        EventSystem.Instance.SpectatorOff();
    }
    private IEnumerator SwappingSqrToTri()
    {
        JobSwapGrid jobSwap = new JobSwapGrid();
        NativeArray<Vector3> deltaData = new NativeArray<Vector3>(sqrToTri, Allocator.TempJob);
        jobSwap.delta = deltaData;
        TransformAccessArray transAccArr = new TransformAccessArray(transforms);

        for (int z = 0; z < 100; z++)
        {
            tileMaterial.DecSqrAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(swapDeltaTime * Time.deltaTime);
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableSqrPart();
            }
        }
        deltaData.Dispose();
        transAccArr.Dispose();
        EventSystem.Instance.SpectatorOff();
    }
    #endregion

    #region SqrToHex
    private IEnumerator SwappingSqrToHex()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].EnableHexPart();
            }
        }
        JobSwapGrid jobSwap = new JobSwapGrid();
        NativeArray<Vector3> deltaData = new NativeArray<Vector3>(sqrToHex, Allocator.TempJob);
        jobSwap.delta = deltaData;
        TransformAccessArray transAccArr = new TransformAccessArray(transforms);

        for (int z = 0; z < 100; z++)
        {
            tileMaterial.IncHexAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(swapDeltaTime * Time.deltaTime);
        }
        deltaData.Dispose();
        transAccArr.Dispose();
        EventSystem.Instance.SpectatorOff();
    }

    private IEnumerator SwappingHexToSqr()
    {
        JobSwapGrid jobSwap = new JobSwapGrid();
        NativeArray<Vector3> deltaData = new NativeArray<Vector3>(hexToSqr, Allocator.TempJob);
        jobSwap.delta = deltaData;
        TransformAccessArray transAccArr = new TransformAccessArray(transforms);

        for (int z = 0; z < 100; z++)
        {
            tileMaterial.DecHexAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(swapDeltaTime * Time.deltaTime);
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableHexPart();
            }
        }
        deltaData.Dispose();
        transAccArr.Dispose();
        EventSystem.Instance.SpectatorOff();
    }
    #endregion

    #region TriToHex
    private IEnumerator SwappingTriToHex()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].EnableSqrPart();
                tiles[i][j].EnableHexPart();
            }
        }

        JobSwapGrid jobSwap = new JobSwapGrid();
        NativeArray<Vector3> deltaData = new NativeArray<Vector3>(triToHex, Allocator.TempJob);
        jobSwap.delta = deltaData;
        TransformAccessArray transAccArr = new TransformAccessArray(transforms);

        for (int z = 0; z < 100; z++)
        {
            tileMaterial.IncSqrAlpha();
            tileMaterial.IncHexAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(swapDeltaTime * Time.deltaTime);
        }
        deltaData.Dispose();
        transAccArr.Dispose();
        EventSystem.Instance.SpectatorOff();
    }

    private IEnumerator SwappingHexToTri()
    {
        JobSwapGrid jobSwap = new JobSwapGrid();
        NativeArray<Vector3> deltaData = new NativeArray<Vector3>(hexToTri, Allocator.TempJob);
        jobSwap.delta = deltaData;
        TransformAccessArray transAccArr = new TransformAccessArray(transforms);

        for (int z = 0; z < 100; z++)
        {
            tileMaterial.DecSqrAlpha();
            tileMaterial.DecHexAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(swapDeltaTime * Time.deltaTime);
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableSqrPart();
                tiles[i][j].DisableHexPart();
            }
        }
        deltaData.Dispose();
        transAccArr.Dispose();
        EventSystem.Instance.SpectatorOff();
    }

    #endregion



}
