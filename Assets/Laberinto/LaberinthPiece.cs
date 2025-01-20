using UnityEngine;

public class LaberinthPiece : MonoBehaviour
{
    public int x, z, chance;
    public bool n, s, e, w;
    
    void Start()
    {
        int num = Random.Range(0, 100);
        if (num < chance && z < Generator.gen.zMax - 1)
        {
            n = true;
        }
        num = Random.Range(0, 100);
        if (num < chance && z > 0)
        {
            s = true;
        }
        num = Random.Range(0, 100);
        if (num < chance && x < Generator.gen.xMax - 1)
        {
            e = true;
        }
        num = Random.Range(0, 100);
        if (num < chance && x > 0)
        {
            w = true;
        }
        GenerateNeightbours();
        if(z < Generator.gen.zMax - 1 && Generator.gen.map[x, z + 1] != null)
        {
            n = true;
        }
        if(z > 0 && Generator.gen.map[x, z - 1] != null)
        {
            s = true;
        }
        if(x < Generator.gen.xMax - 1 && Generator.gen.map[x + 1, z] != null)
        {
            e = true;
        }
        if(x > 0 && Generator.gen.map[x - 1, z] != null)
        {
            w = true;
        }
        CheckWalls();
    }

    public void GenerateNeightbours()
    {
        if (n)
        {
            Generator.gen.GenerateNextPiece(x, z + 1);
        }
        if (s)
        {
            Generator.gen.GenerateNextPiece(x, z - 1);
        }
        if (e)
        {
            Generator.gen.GenerateNextPiece(x + 1, z);
        }
        if (w)
        {
            Generator.gen.GenerateNextPiece(x - 1, z);
        }
    }

    public void CheckWalls()
    {
        if (w)
        {
            transform.GetChild(3).gameObject.SetActive(false);
        }
        if (e)
        {
            transform.GetChild(2).gameObject.SetActive(false);
        }
        if (s)
        {
            transform.GetChild(1).gameObject.SetActive(false);
        }
        if (n)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
