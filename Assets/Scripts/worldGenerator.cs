using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class worldGenerator : MonoBehaviour
{
    public int WorldSizeInChunks = 10;

    public float TerrainHeightRange = 10f; 
    public float BaseTerrainHeight = 60f;
    int TerrainCenterOffset;

    public GameObject loadingScreen;

    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    // Start is called before the first frame update
    void Start()
    {

    }
    public void Generate()
    {
        GameData.BaseTerrainHeight = BaseTerrainHeight;
        GameData.TerrainHeightRange = TerrainHeightRange;

        loadingScreen.SetActive(true);
        for (int x = 0; x < WorldSizeInChunks; x++)
        {
            for (int  z = 0;  z < WorldSizeInChunks;  z++)
            {
                TerrainCenterOffset = WorldSizeInChunks * GameData.ChunkWidth / 2;
                Vector3Int chunkPos = new Vector3Int(x * GameData.ChunkWidth - TerrainCenterOffset, 0, z * GameData.ChunkWidth - TerrainCenterOffset);
                chunks.Add(chunkPos, new Chunk(chunkPos));
                chunks[chunkPos].chunkObject.transform.SetParent(transform);
            }
        }

        Debug.Log(string.Format("{0} x {0} world generated.", WorldSizeInChunks * GameData.ChunkWidth));
        loadingScreen.SetActive(false);
    }

    

    public Chunk GetChunkFromVector3 (Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        return chunks[new Vector3Int(x, y, z)];
    }
}
