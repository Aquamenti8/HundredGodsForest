
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveMap (worldGenerator world)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/map1.map";
        FileStream stream = new FileStream(path, FileMode.Create);

        MapData data = new MapData(world);

        formatter.Serialize(stream, data);
        stream.Close();
    }
    public static MapData LoadMap()
    {
        string path = Application.persistentDataPath + "/map1.map";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            MapData data = formatter.Deserialize(stream) as MapData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("save file not found in " + path);
            return null;
        }
    }
}
