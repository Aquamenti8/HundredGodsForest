using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData
{
	// pour sauvegarder la geo il me faut un tableau qui contient toutes les valeurs de chaques points de l'espace c'est notre terrain map
	// lors de la sauvegarde je créé un tableau de chunks 2D et dans chaque chunk un terrain map float [,,]

	public float[,,][,,] mapData;

	public MapData (worldGenerator world)
    {
		mapData = new float[world.WorldSizeInChunks+1,0, world.WorldSizeInChunks+1][,,];

        float[,,] terrainMap = world.chunks[new Vector3Int(0,0,0)].terrainMap;
        mapData[0, 0, 0] = terrainMap;
        Debug.Log("Array bound " + new Vector3(world.WorldSizeInChunks + 1, 0, world.WorldSizeInChunks + 1));
        for (int x = 0; x < world.WorldSizeInChunks; x++)
        {
            for (int z = 0; z < world.WorldSizeInChunks; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x * GameData.ChunkWidth - world.TerrainCenterOffset, 0, z * GameData.ChunkWidth - world.TerrainCenterOffset);
                terrainMap = world.chunks[chunkPos].terrainMap;
                Debug.Log("Chunk bounds " + new Vector3(x, 0, z));
                Debug.Log("terrainMap " + terrainMap[0,0,0]);
                mapData[x, 0, z] = terrainMap;
            }
        }
    }
}
