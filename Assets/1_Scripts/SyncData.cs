using System;
using System.Collections;
using UnityEngine;

public class SyncData
{
    public Vector2Int[] Positions;
    public int[] Scores;

    public BitArray MapData;

    public static object Deserialize(byte[] bytes)
    {

    }

    public static byte[] Serialize(object obj)
    {
        SyncData data = (SyncData)obj;

        byte[] result = new byte[
            8 * data.Positions.Length +
            4 * data.Scores.Length +
            Mathf.CeilToInt(data.MapData.Count / 8f)
            ];

        for (int i = 0; i < data.Positions.Length; i++)
        {
            BitConverter.GetBytes(data.Positions[i].x).CopyTo(result, 8 * i);
            BitConverter.GetBytes(data.Positions[i].y).CopyTo(result, 8 * i + 4);
        }

        15-25

        for (int i = 0; i < length; i++)
        {

        }
    }
}
