using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorAnimation : MonoBehaviour
{
    [SerializeField]
    private Image[] tri;

    [SerializeField]
    private Image[] sqr;

    [SerializeField]
    private Image[] hex;

    private enum GridType
    {
        Tri,
        Sqr,
        Hex
    }

    private GridType[] curTypes;
    private bool[] status;
    private const float swapDeltaTime = 0.005f;
    private void Awake()
    {
        curTypes = new GridType[4];
        curTypes[0] = GridType.Tri; // red
        curTypes[1] = GridType.Tri; // green
        curTypes[2] = GridType.Tri; // blue
        curTypes[3] = GridType.Tri; // yellow

        status = new bool[4] { true, true, true, true };
        
    }
    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (status[i])
            {
                SwitchColor(i);
            }
        }
    }

    private void SwitchColor(int i)
    {
        GridType prev = curTypes[i];
        GridType t = GetRandomType(i);
        //Debug.Log(i + " " + prev + " " + t);
        if (prev == GridType.Tri)
        {
            if (t == GridType.Sqr)
            {
                StartCoroutine(triToSqr(i));
            }
            else
            {
                StartCoroutine(triToHex(i));
            }
        }
        else if (prev == GridType.Sqr)
        {
            if (t == GridType.Tri)
            {
                StartCoroutine(sqrToTri(i));
            }
            else
            {
                StartCoroutine(sqrToHex(i));
            }
        }
        else 
        {
            if (t == GridType.Tri)
            {
                StartCoroutine(hexToTri(i));
            }
            else
            {
                StartCoroutine(hexToSqr(i));
            }
        }
        curTypes[i] = t;
        status[i] = false;
    }
    private GridType GetRandomType(int i)
    {
        int rnd = Random.Range(1, 4);
        switch (rnd)
        {
            case 1:
                if (curTypes[i] == GridType.Tri)
                {
                    return GridType.Sqr;
                }
                return GridType.Tri;
            case 2:
                if (curTypes[i] == GridType.Sqr)
                {
                    return GridType.Hex;
                }
                return GridType.Sqr;
            case 3:
                if (curTypes[i] == GridType.Hex)
                {
                    return GridType.Tri;
                }
                return GridType.Hex;
        }
        return GridType.Tri;
    }

    private IEnumerator triToSqr(int index)
    {
        for (int i = 0; i < 100; i++)
        {
            Color col = sqr[index].color;
            col.a += 0.01f;
            sqr[index].color = col;
            yield return new WaitForSeconds(swapDeltaTime);
        }
        yield return new WaitForSeconds(3);
        status[index] = true;
    }
    private IEnumerator sqrToTri(int index)
    {
        for (int i = 0; i < 100; i++)
        {
            Color col = sqr[index].color;
            col.a -= 0.01f;
            sqr[index].color = col;
            yield return new WaitForSeconds(swapDeltaTime);
        }
        yield return new WaitForSeconds(3);
        status[index] = true;
    }
    private IEnumerator sqrToHex(int index)
    {
        for (int i = 0; i < 100; i++)
        {
            Color col = hex[index].color;
            col.a += 0.01f;
            hex[index].color = col;
            yield return new WaitForSeconds(swapDeltaTime);
        }
        yield return new WaitForSeconds(3);
        status[index] = true;
    }
    private IEnumerator hexToSqr(int index)
    {
        for (int i = 0; i < 100; i++)
        {
            Color col = hex[index].color;
            col.a -= 0.01f;
            hex[index].color = col;
            yield return new WaitForSeconds(swapDeltaTime);
        }
        yield return new WaitForSeconds(3);
        status[index] = true;
    }
    private IEnumerator triToHex(int index)
    {
        for (int i = 0; i < 100; i++)
        {
            Color col = sqr[index].color;
            col.a += 0.01f;
            sqr[index].color = col;
            col = hex[index].color;
            col.a += 0.01f;
            hex[index].color = col;
            yield return new WaitForSeconds(swapDeltaTime);
        }
        yield return new WaitForSeconds(3);
        status[index] = true;
    }
    private IEnumerator hexToTri(int index)
    {
        for (int i = 0; i < 100; i++)
        {
            Color col = sqr[index].color;
            col.a -= 0.01f;
            sqr[index].color = col;
            col = hex[index].color;
            col.a -= 0.01f;
            hex[index].color = col;
            yield return new WaitForSeconds(swapDeltaTime);
        }
        yield return new WaitForSeconds(3);
        status[index] = true;
    }
}
