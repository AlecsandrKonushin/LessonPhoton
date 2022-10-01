using System.Collections.Generic;
using UnityEngine;

public class MapController : Singleton<MapController>
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private int width, height;

    private GameObject[,] cells;
    private List<PlayerController> players = new List<PlayerController>();

    public void AddPlayer(PlayerController player)
    {
        players.Add(player);

        cells[player.GetGamePosition.x, player.GetGamePosition.y].SetActive(false);
    }

    private void Start()
    {
        cells = new GameObject[width, height];

        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                cells[w, h] = Instantiate(cellPrefab, new Vector3(w, h), Quaternion.identity, transform);
            }
        }
    }
}
