/*
 using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class GridSwapper : MonoBehaviour
{
    [SerializeField]
    TriangularGrid triangGrid;

    [SerializeField]
    SquareGrid squareGrid;

    [SerializeField]
    HexGrid hexGrid;

    [SerializeField]
    TileMaterial tileMaterial;


    List<List<Tile>> tiles;
    TransformAccessArray transAccArr;

    NativeArray<Vector3> triToSqr;
    NativeArray<Vector3> triToHex;
    NativeArray<Vector3> sqrToHex;

    NativeArray<Vector3> sqrToTri;
    NativeArray<Vector3> hexToTri;
    NativeArray<Vector3> hexToSqr;
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
                    break;
                case State.Sqr:
                    squareGrid.enabled = true;
                    triangGrid.enabled = false;
                    hexGrid.enabled = false;
                    break;
                case State.Hex:
                    hexGrid.enabled = true;
                    triangGrid.enabled = false;
                    squareGrid.enabled = false;
                    break;
            }
            state = value;
        }
    }
    #endregion


    private const float deltaTime = 0.0001f;

    //private const float hexAdj = 1.8f;
    //private const float hexInner = 0.72f;
    //private const float hexRow = 1;

    private void Initialize()
    {
        tiles = GridController.tiles;

        int c = tiles.Count * tiles[0].Count;
        Transform[] transforms = new Transform[c];
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                int index = (i * tiles[0].Count) + j;
                transforms[index] = tiles[i][j].transform;
            }
        }
        TransformAccessArray transAccArr = new TransformAccessArray(transforms);


        triToSqr = new NativeArray<Vector3>(c, Allocator.Persistent);
        triToHex = new NativeArray<Vector3>(c, Allocator.Persistent);
        sqrToHex = new NativeArray<Vector3>(c, Allocator.Persistent);

        sqrToTri = new NativeArray<Vector3>(c, Allocator.Persistent);
        hexToTri = new NativeArray<Vector3>(c, Allocator.Persistent);
        hexToSqr = new NativeArray<Vector3>(c, Allocator.Persistent);
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
        jobSwap.delta = triToSqr;
        for (int z = 0; z < 100; z++)
        {
            tileMaterial.IncSqrAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(deltaTime);
        }
        EventSystem.Instance.SpectatorOff();
    }
    private IEnumerator SwappingSqrToTri()
    {
        JobSwapGrid jobSwap = new JobSwapGrid();
        jobSwap.delta = sqrToTri;


        for (int z = 0; z < 100; z++)
        {
            tileMaterial.DecSqrAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(deltaTime);
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableSqrPart();
            }
        }
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
        jobSwap.delta = sqrToHex;


        for (int z = 0; z < 100; z++)
        {
            tileMaterial.IncHexAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(deltaTime);
        }
        EventSystem.Instance.SpectatorOff();
    }

    private IEnumerator SwappingHexToSqr()
    {
        JobSwapGrid jobSwap = new JobSwapGrid();
        jobSwap.delta = hexToSqr;


        for (int z = 0; z < 100; z++)
        {
            tileMaterial.DecHexAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(deltaTime);
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableHexPart();
            }
        }
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
        jobSwap.delta = triToHex;


        for (int z = 0; z < 100; z++)
        {
            tileMaterial.IncSqrAlpha();
            tileMaterial.IncHexAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(deltaTime);
        }
        //transAccArr.Dispose();
        //deltaData.Dispose();
        EventSystem.Instance.SpectatorOff();
    }

    private IEnumerator SwappingHexToTri()
    {
        JobSwapGrid jobSwap = new JobSwapGrid();
        jobSwap.delta = hexToTri;


        for (int z = 0; z < 100; z++)
        {
            tileMaterial.DecSqrAlpha();
            tileMaterial.DecHexAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(deltaTime);
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableSqrPart();
                tiles[i][j].DisableHexPart();
            }
        }
        EventSystem.Instance.SpectatorOff();
    }

    #endregion



}

 */


/*
 * using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class GridSwapper : MonoBehaviour
{
    [SerializeField]
    TriangularGrid triangGrid;

    [SerializeField]
    SquareGrid squareGrid;

    [SerializeField]
    HexGrid hexGrid;

    [SerializeField]
    TileMaterial tileMaterial;


    List<List<Tile>> tiles;
    List<List<Vector3>> triToSqr;
    List<List<Vector3>> triToHex;
    List<List<Vector3>> sqrToHex;

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
                    break;
                case State.Sqr:
                    squareGrid.enabled = true;
                    triangGrid.enabled = false;
                    hexGrid.enabled = false;
                    break;
                case State.Hex:
                    hexGrid.enabled = true;
                    triangGrid.enabled = false;
                    squareGrid.enabled = false;
                    break;
            }
            state = value;
        }
    }
    #endregion


    private const float deltaTime = 0.0001f;

    //private const float hexAdj = 1.8f;
    //private const float hexInner = 0.72f;
    //private const float hexRow = 1;

    private void Initialize()
    {
        tiles = GridController.tiles;

        #region triToSqr
        triToSqr = new List<List<Vector3>>();

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
            triToSqr.Add(new List<Vector3>());
        }
        for (int i = 0; i < tiles.Count / 2; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                triToSqr[i].Add((tiles[i + 1][j].transform.position
                              + tiles[i][j].transform.position) / 2);
                Vector3 t = triToSqr[i][j];
                t.x *= 1.7f;
                triToSqr[i][j] = t;
                //tiles[i][j].transform.position = newPos[i][j];
                //yield return new WaitForSeconds(0.1f);
            }
        }
        for (int i = tiles.Count - 1; i > tiles.Count / 2; i--)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                triToSqr[i].Add((tiles[i - 1][j].transform.position
                              + tiles[i][j].transform.position) / 2);
                Vector3 t = triToSqr[i][j];
                t.x *= 1.7f;
                triToSqr[i][j] = t;
            }
            middleZTri = (triToSqr[i][0].z
                           + triToSqr[i][1].z) / 2;
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Vector3 t = triToSqr[i][j];
                t.z = middleZTri - deltaZ;
                triToSqr[i][j] = t;
            }
        }
        for (int j = 0; j < tiles[middleTri + 1].Count; j++)
        {
            triToSqr[middleTri + 1].Add((bufferMiddle[j]
                          + tiles[middleTri + 1][j].transform.position) / 2);
            Vector3 t = triToSqr[middleTri + 1][j];
            t.x *= 1.7f;
            triToSqr[middleTri + 1][j] = t;
        }
        middleZTri = (triToSqr[middleTri + 1][0].z
                       + triToSqr[middleTri + 1][1].z) / 2;
        for (int j = 0; j < tiles[middleTri + 1].Count; j++)
        {
            Vector3 t = triToSqr[middleTri + 1][j];
            t.z = middleZTri - deltaZ;
            triToSqr[middleTri + 1][j] = t;
        }
        deltaZ = triToSqr[middleTri + 1][0].z - triToSqr[middleTri][0].z;
        middleTri += 1;
        for (int i = middleTri + 1; i < tiles.Count; i++)
        {
            float currDeltaZ = triToSqr[i][0].z - triToSqr[i - 1][0].z;
            float secondDelta = currDeltaZ - deltaZ;
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Vector3 t = triToSqr[i][j];
                t.z -= secondDelta;
                triToSqr[i][j] = t;
            }
        }
        middleTri -= 1;
        for (int i = middleTri - 1; i >= 0; i--)
        {
            float currDeltaZ = triToSqr[i + 1][0].z - triToSqr[i][0].z;
            float secondDelta = currDeltaZ - deltaZ;
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Vector3 t = triToSqr[i][j];
                t.z += secondDelta;
                triToSqr[i][j] = t;
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
                deltaPos[i].Add((triToSqr[i][j] - tiles[i][j].transform.position) / 100);
            }
        }
        triToSqr = deltaPos;
        #endregion



        List<List<Vector3>> triTiles = new List<List<Vector3>>();
        for (int i = 0; i < tiles.Count; i++)
        {
            triTiles.Add(new List<Vector3>());
            for (int j = 0; j < tiles[i].Count; j++)
            {
                triTiles[i].Add(triToSqr[i][j] * 100 + tiles[i][j].transform.position);
            }
        }

        #region sqrToHex
        sqrToHex = new List<List<Vector3>>();
        for (int i = 0; i < tiles.Count; i++)
        {
            sqrToHex.Add(new List<Vector3>());
            for (int j = 0; j < tiles[i].Count; j++)
            {
                sqrToHex[i].Add(triTiles[i][j]);
                Vector3 newPos = sqrToHex[i][j];
                newPos.x *= 1.35f;
                newPos.z *= 1.4f;//0.7f;
                sqrToHex[i][j] = newPos;
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            float middleZ = (sqrToHex[i][0].z + sqrToHex[i][1].z) / 2;
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Vector3 newPos = sqrToHex[i][j];
                newPos.z = middleZ;//0.7f;
                sqrToHex[i][j] = newPos;
            }
        }
        for (int i = 0; i < tiles[0].Count; i++)
        {
            if (i % 2 == 1)
            {
                for (int j = 0; j < tiles.Count; j++)
                {
                    Vector3 newPos = sqrToHex[j][i];
                    newPos.z -= 0.91f;//0.7f;
                    sqrToHex[j][i] = newPos;
                }
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                sqrToHex[i][j] = (sqrToHex[i][j] - triTiles[i][j]) / 100;
            }
        }
        #endregion






        #region triToHex
        triToHex = new List<List<Vector3>>();
        for (int i = 0; i < tiles.Count; i++)
        {
            triToHex.Add(new List<Vector3>());
            for (int j = 0; j < tiles[i].Count; j++)
            {
                triToHex[i].Add((triToSqr[i][j] + sqrToHex[i][j]));
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
        for (int z = 0; z < 100; z++)
        {
            tileMaterial.IncSqrAlpha();
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].transform.position += triToSqr[i][j];
                }
                yield return new WaitForSeconds(deltaTime);
            }
        }
        EventSystem.Instance.SpectatorOff();
    }
    private IEnumerator SwappingSqrToTri()
    {
        for (int z = 0; z < 100; z++)
        {
            tileMaterial.DecSqrAlpha();
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].transform.position += -triToSqr[i][j];
                    
                }
                yield return new WaitForSeconds(deltaTime);
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableSqrPart();
            }
        }
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
        for (int z = 0; z < 100; z++)
        {
            tileMaterial.IncHexAlpha();
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].transform.position += sqrToHex[i][j];
                }
                yield return new WaitForSeconds(deltaTime);
            }
        }

        EventSystem.Instance.SpectatorOff();
    }

    private IEnumerator SwappingHexToSqr()
    {

        for (int z = 0; z < 100; z++)
        {
            tileMaterial.DecHexAlpha();
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].transform.position -= sqrToHex[i][j];

                }
                yield return new WaitForSeconds(deltaTime);
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableHexPart();
            }
        }
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
        int c = triToHex.Count * triToHex[0].Count;
        Transform[] transforms = new Transform[c];
        NativeArray<Vector3> deltaData = new NativeArray<Vector3>(c, Allocator.TempJob);
        for (int i = 0; i < triToHex.Count; i++)
        {
            for (int j = 0; j < triToHex[i].Count; j++)
            {
                int index = (i * triToHex[0].Count) + j;
                transforms[index] = tiles[i][j].transform;
                deltaData[index] = triToHex[i][j];
            }
        }
        JobSwapGrid jobSwap = new JobSwapGrid();
        jobSwap.delta = deltaData;
        TransformAccessArray transAccArr = new TransformAccessArray(transforms);
        for (int z = 0; z < 100; z++)
        {
            tileMaterial.IncSqrAlpha();
            tileMaterial.IncHexAlpha();
            JobHandle jobHandle = jobSwap.Schedule(transAccArr);
            jobHandle.Complete();
            yield return new WaitForSeconds(deltaTime);
        }
        deltaData.Dispose();
        transAccArr.Dispose();
        EventSystem.Instance.SpectatorOff();
    }

    private IEnumerator SwappingHexToTri()
    {
        for (int z = 0; z < 100; z++)
        {
            tileMaterial.DecSqrAlpha();
            tileMaterial.DecHexAlpha();
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].transform.position -= triToHex[i][j];

                }
                yield return new WaitForSeconds(deltaTime);
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableSqrPart();
                tiles[i][j].DisableHexPart();
            }
        }
        EventSystem.Instance.SpectatorOff();
    }

    #endregion



}

*/
/*
 * using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSwapper : MonoBehaviour
{
    [SerializeField]
    TriangularGrid triangGrid;

    [SerializeField]
    SquareGrid squareGrid;

    [SerializeField]
    HexGrid hexGrid;

    [SerializeField]
    TileMaterial tileMaterial;


    List<List<Tile>> tiles;
    List<List<Vector3>> triToSqr;
    List<List<Vector3>> triToHex;
    List<List<Vector3>> sqrToHex;

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
                    break;
                case State.Sqr:
                    squareGrid.enabled = true;
                    triangGrid.enabled = false;
                    hexGrid.enabled = false;
                    break;
                case State.Hex:
                    hexGrid.enabled = true;
                    triangGrid.enabled = false;
                    squareGrid.enabled = false;
                    break;
            }
            state = value;
        }
    }
    #endregion


    private const float deltaTime = 0.0001f;

    //private const float hexAdj = 1.8f;
    //private const float hexInner = 0.72f;
    //private const float hexRow = 1;

    private void Initialize()
    {
        tiles = GridController.tiles;

        #region triToSqr
        triToSqr = new List<List<Vector3>>();

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
            triToSqr.Add(new List<Vector3>());
        }
        for (int i = 0; i < tiles.Count / 2; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                triToSqr[i].Add((tiles[i + 1][j].transform.position
                              + tiles[i][j].transform.position) / 2);
                Vector3 t = triToSqr[i][j];
                t.x *= 1.7f;
                triToSqr[i][j] = t;
                //tiles[i][j].transform.position = newPos[i][j];
                //yield return new WaitForSeconds(0.1f);
            }
        }
        for (int i = tiles.Count - 1; i > tiles.Count / 2; i--)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                triToSqr[i].Add((tiles[i - 1][j].transform.position
                              + tiles[i][j].transform.position) / 2);
                Vector3 t = triToSqr[i][j];
                t.x *= 1.7f;
                triToSqr[i][j] = t;
            }
            middleZTri = (triToSqr[i][0].z
                           + triToSqr[i][1].z) / 2;
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Vector3 t = triToSqr[i][j];
                t.z = middleZTri - deltaZ;
                triToSqr[i][j] = t;
            }
        }
        for (int j = 0; j < tiles[middleTri + 1].Count; j++)
        {
            triToSqr[middleTri + 1].Add((bufferMiddle[j]
                          + tiles[middleTri + 1][j].transform.position) / 2);
            Vector3 t = triToSqr[middleTri + 1][j];
            t.x *= 1.7f;
            triToSqr[middleTri + 1][j] = t;
        }
        middleZTri = (triToSqr[middleTri + 1][0].z
                       + triToSqr[middleTri + 1][1].z) / 2;
        for (int j = 0; j < tiles[middleTri + 1].Count; j++)
        {
            Vector3 t = triToSqr[middleTri + 1][j];
            t.z = middleZTri - deltaZ;
            triToSqr[middleTri + 1][j] = t;
        }
        deltaZ = triToSqr[middleTri + 1][0].z - triToSqr[middleTri][0].z;
        middleTri += 1;
        for (int i = middleTri + 1; i < tiles.Count; i++)
        {
            float currDeltaZ = triToSqr[i][0].z - triToSqr[i - 1][0].z;
            float secondDelta = currDeltaZ - deltaZ;
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Vector3 t = triToSqr[i][j];
                t.z -= secondDelta;
                triToSqr[i][j] = t;
            }
        }
        middleTri -= 1;
        for (int i = middleTri - 1; i >= 0; i--)
        {
            float currDeltaZ = triToSqr[i + 1][0].z - triToSqr[i][0].z;
            float secondDelta = currDeltaZ - deltaZ;
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Vector3 t = triToSqr[i][j];
                t.z += secondDelta;
                triToSqr[i][j] = t;
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
                deltaPos[i].Add(triToSqr[i][j] - tiles[i][j].transform.position);
            }
        }
        triToSqr = deltaPos;
        #endregion

        List<List<Vector3>> triTiles = new List<List<Vector3>>();
        for (int i = 0; i < tiles.Count; i++)
        {
            triTiles.Add(new List<Vector3>());
            for (int j = 0; j < tiles[i].Count; j++)
            {
                triTiles[i].Add(triToSqr[i][j] + tiles[i][j].transform.position);
            }
        }
        #region sqrToHex
        sqrToHex = new List<List<Vector3>>();
        for (int i = 0; i < tiles.Count; i++)
        {
            sqrToHex.Add(new List<Vector3>());
            for (int j = 0; j < tiles[i].Count; j++)
            {
                sqrToHex[i].Add(triTiles[i][j]);
                Vector3 newPos = sqrToHex[i][j];
                newPos.x *= 1.35f;
                newPos.z *= 1.4f;//0.7f;
                sqrToHex[i][j] = newPos;
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            float middleZ = (sqrToHex[i][0].z + sqrToHex[i][1].z) / 2;
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Vector3 newPos = sqrToHex[i][j];
                newPos.z = middleZ;//0.7f;
                sqrToHex[i][j] = newPos;
            }
        }
        for (int i = 0; i < tiles[0].Count; i++)
        {
            if (i % 2 == 1)
            {
                for (int j = 0; j < tiles.Count; j++)
                {
                    Vector3 newPos = sqrToHex[j][i];
                    newPos.z -= 0.91f;//0.7f;
                    sqrToHex[j][i] = newPos;
                }
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                sqrToHex[i][j] = sqrToHex[i][j] - triTiles[i][j];
            }
        }
        #endregion
        #region triToHex
        triToHex = new List<List<Vector3>>();
        for (int i = 0; i < tiles.Count; i++)
        {
            triToHex.Add(new List<Vector3>());
            for (int j = 0; j < tiles[i].Count; j++)
            {
                triToHex[i].Add(triToSqr[i][j] + sqrToHex[i][j]);
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
        for (int z = 0; z < 100; z++)
        {
            tileMaterial.IncSqrAlpha();
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].transform.position += triToSqr[i][j] / 100;
                    
                }
                yield return new WaitForSeconds(deltaTime);
            }
        }
        EventSystem.Instance.SpectatorOff();
    }
    private IEnumerator SwappingSqrToTri()
    {
        for (int z = 0; z < 100; z++)
        {
            tileMaterial.DecSqrAlpha();
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].transform.position += -triToSqr[i][j] / 100;
                    
                }
                yield return new WaitForSeconds(deltaTime);
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableSqrPart();
            }
        }
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
        for (int z = 0; z < 100; z++)
        {
            tileMaterial.IncHexAlpha();
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].transform.position += sqrToHex[i][j] / 100;
                }
                yield return new WaitForSeconds(deltaTime);
            }
        }

        EventSystem.Instance.SpectatorOff();
    }

    private IEnumerator SwappingHexToSqr()
    {

        for (int z = 0; z < 100; z++)
        {
            tileMaterial.DecHexAlpha();
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].transform.position -= sqrToHex[i][j] / 100;

                }
                yield return new WaitForSeconds(deltaTime);
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableHexPart();
            }
        }
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
        for (int z = 0; z < 100; z++)
        {
            tileMaterial.IncSqrAlpha();
            tileMaterial.IncHexAlpha();
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].transform.position += triToHex[i][j] / 100;
                }
                yield return new WaitForSeconds(deltaTime);
            }
        }

        EventSystem.Instance.SpectatorOff();
    }

    private IEnumerator SwappingHexToTri()
    {
        for (int z = 0; z < 100; z++)
        {
            tileMaterial.DecSqrAlpha();
            tileMaterial.DecHexAlpha();
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int j = 0; j < tiles[i].Count; j++)
                {
                    tiles[i][j].transform.position -= triToHex[i][j] / 100;

                }
                yield return new WaitForSeconds(deltaTime);
            }
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].DisableSqrPart();
                tiles[i][j].DisableHexPart();
            }
        }
        EventSystem.Instance.SpectatorOff();
    }

    #endregion



}

*/
/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour, IComparable<Tile>
{
    [SerializeField]
    private MeshRenderer trianglePart;
    [SerializeField]
    private MeshRenderer squarePart;
    [SerializeField]
    private GameObject hexUp;
    [SerializeField]
    private GameObject hexDown;

    private MeshRenderer[] hexParts;

    [SerializeField]
    private Material defaultMaterial;
    [SerializeField]
    private Material highlightMaterial;
    [SerializeField]
    private Material[] groupHighlightMaterials;
    [SerializeField]
    private Material selectedMaterial;
    [SerializeField]
    private Material groupSelectMaterial;
    [SerializeField]
    private Material placedMaterial;

    public enum OrientType
    {
        Up,
        Down
    }
    public OrientType Orientation { get; private set; }

    //[SerializeField]
    //private DelTrig delTrig;
    private bool isPlaced;

    private bool isSquarePartEnabled;
    private bool isHexPartEnabled;
    private bool areReflinesEnabled;

    private float sqrAlpha;
    private float hexAlpha;

    public void EnableSqrPart()
    {
        isSquarePartEnabled = true;
        squarePart.gameObject.SetActive(true);
        ItsState = ItsState;
        sqrAlpha = 0;
        Color prev = squarePart.material.color;
        prev.a = sqrAlpha;
        squarePart.material.color = prev;
    }

    public void IncSqrAlpha()
    {
        sqrAlpha += 0.01f;
        Color prev = squarePart.material.color;
        prev.a = sqrAlpha;
        squarePart.material.color = prev;
    }

    public void DisableSqrPart()
    {
        isSquarePartEnabled = false;
        squarePart.gameObject.SetActive(false);
    }

    public void DecSqrAlpha()
    {
        sqrAlpha -= 0.01f;
        Color prev = squarePart.material.color;
        prev.a = sqrAlpha;
        squarePart.material.color = prev;
    }

    public void EnableHexPart()
    {
        isHexPartEnabled = true;
        foreach (MeshRenderer mRend in hexParts)
        {
            mRend.gameObject.SetActive(true);
        }
        ItsState = ItsState;
        hexAlpha = 0;
    }
    public void IncHexAlpha()
    {
        hexAlpha += 0.01f;
        foreach (MeshRenderer mRend in hexParts)
        {
            Color prev = mRend.material.color;
            prev.a = hexAlpha;
            mRend.material.color = prev;
        }
    }
    public void DisableHexPart()
    {
        isSquarePartEnabled = false;
        foreach (MeshRenderer mRend in hexParts)
        {
            mRend.gameObject.SetActive(false);
        }
    }
    public void DecHexAlpha()
    {
        hexAlpha -= 0.01f;
        foreach (MeshRenderer mRend in hexParts)
        {
            Color prev = mRend.material.color;
            prev.a = hexAlpha;
            mRend.material.color = prev;
        }
    }
    private void Awake()
    {
        state = State.Default;
        isPlaced = false;
        isSquarePartEnabled = false;
        isHexPartEnabled = false;
        areReflinesEnabled = true;
    }

    #region TileMode
    public void Listen()
    {
        EventSystem.Instance.OnTileCreated += Created;
        EventSystem.Instance.OnTileDeselected += TileDeselected;
    }

    #region State
    public enum State
    {
        Default,
        Highlight,
        HighlightGroup,
        Selected, // does not used ?
        SelectedGroup,
        Placed
    }
    private State state;

    public State ItsState
    {
        get { return state; }
        set
        {
            switch (value)
            {
                case State.Default:
                    trianglePart.material.color = defaultMaterial.color;
                    if (isSquarePartEnabled) squarePart.material.color = defaultMaterial.color;
                    if (isHexPartEnabled)
                    {
                        foreach (MeshRenderer mr in hexParts)
                        {
                            mr.material.color = defaultMaterial.color;
                        }
                    }
                    state = value;
                    break;
                case State.Highlight:
                    trianglePart.material.color = highlightMaterial.color;
                    if (isSquarePartEnabled) squarePart.material.color = highlightMaterial.color;
                    if (isHexPartEnabled)
                    {
                        foreach (MeshRenderer mr in hexParts)
                        {
                            mr.material.color = highlightMaterial.color;
                        }
                    }
                    state = value;
                    break;
                case State.HighlightGroup:
                    trianglePart.material.color = groupHighlightMaterials[HighlightGroup].color;
                    if (isSquarePartEnabled) squarePart.material.color = groupHighlightMaterials[HighlightGroup].color;
                    if (isHexPartEnabled)
                    {
                        foreach (MeshRenderer mr in hexParts)
                        {
                            mr.material.color = groupHighlightMaterials[HighlightGroup].color;
                        }
                    }
                    state = value;
                    break;
                case State.Selected:
                    trianglePart.material.color = selectedMaterial.color;
                    if (isSquarePartEnabled) squarePart.material.color = selectedMaterial.color;
                    if (isHexPartEnabled)
                    {
                        foreach (MeshRenderer mr in hexParts)
                        {
                            mr.material.color = selectedMaterial.color;
                        }
                    }
                    state = value;
                    break;
                case State.Placed:
                    if (!isPlaced)
                    {
                        isPlaced = true;
                        //delTrig.OnPlacement();
                    }
                    trianglePart.material.color = placedMaterial.color;
                    if (isSquarePartEnabled) squarePart.material.color = placedMaterial.color;
                    if (isHexPartEnabled)
                    {
                        foreach (MeshRenderer mr in hexParts)
                        {
                            mr.material.color = placedMaterial.color;
                        }
                    }
                    state = value;
                    break;
            }
        }
    }
    #endregion
    public int HighlightGroup { get; set; }

    #region RefLines
    private int horizFirstRefLineIndex = -1;
    private int horizSecondRefLineIndex = -1;
    public (int, int) HorizRefLines
    {
        get { return (horizFirstRefLineIndex, horizSecondRefLineIndex); }
        set
        {
            if (horizFirstRefLineIndex == -1)
            {
                horizFirstRefLineIndex = value.Item1;
            }
            if (horizSecondRefLineIndex == -1)
            {
                horizSecondRefLineIndex = value.Item2;
            }
        }
    }

    private int vertFirstRefLineIndex = -1;
    private int vertSecondRefLineIndex = -1;
    public (int, int) VertRefLines
    {
        get { return (vertFirstRefLineIndex, vertSecondRefLineIndex); }
        set
        {
            if (vertFirstRefLineIndex == -1)
            {
                vertFirstRefLineIndex = value.Item1;
            }
            if (vertSecondRefLineIndex == -1)
            {
                vertSecondRefLineIndex = value.Item2;
            }
        }
    }

    private int topLeftFirstRefLineIndex = -1;
    private int topLeftSecondRefLineIndex = -1;

    public (int, int) TopLeftRefLines
    {
        get { return (topLeftFirstRefLineIndex, topLeftSecondRefLineIndex); }
        set
        {
            if (topLeftFirstRefLineIndex == -1)
            {
                topLeftFirstRefLineIndex = value.Item1;
            }
            if (topLeftSecondRefLineIndex == -1)
            {
                topLeftSecondRefLineIndex = value.Item2;
            }
        }
    }

    private int botLeftFirstRefLineIndex = -1;
    private int botLeftSecondRefLineIndex = -1;
    public (int, int) BotLeftRefLines
    {
        get { return (botLeftFirstRefLineIndex, botLeftSecondRefLineIndex); }
        set
        {
            if (botLeftFirstRefLineIndex == -1)
            {
                botLeftFirstRefLineIndex = value.Item1;
            }
            if (botLeftSecondRefLineIndex == -1)
            {
                botLeftSecondRefLineIndex = value.Item2;
            }
        }
    }
    #endregion

    private int row;
    private int column;

    public int Row { get; private set; }
    public int Column { get; private set; }

    private void Created(int r, int c, OrientType orient)
    {
        Row = r;
        Column = c;
        Orientation = orient;
        if (orient == OrientType.Up)
        {
            hexParts = hexUp.GetComponentsInChildren<MeshRenderer>();
            hexUp.SetActive(true);
            hexDown.SetActive(false);
        }
        else
        {
            hexParts = hexDown.GetComponentsInChildren<MeshRenderer>();
            hexDown.SetActive(true);
            hexUp.SetActive(false);
        }
        EventSystem.Instance.OnTileCreated -= Created;
    }
    private void TileDeselected()
    {
        switch (state)
        {
            case State.Highlight:
            case State.HighlightGroup:
                ItsState = State.Default;
                break;
            case State.Selected:
                ItsState = State.Placed;
                break;
        }
    }

    public void Log()
    {
        Debug.Log("Tile " + row + " " + column + ".");
    }

    #endregion

    private State prevState;

    public int CompareTo(Tile other)
    {
        int comp = Column.CompareTo(other.Column);
        if (comp != 0)
        {
            return comp;
        }
        else
        {
            comp = Row.CompareTo(other.Row);
            return comp;
        }
    }
}
*/