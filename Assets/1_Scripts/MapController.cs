using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapController : Singleton<MapController>, IOnEventCallback
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private int width, height;

    private GameObject[,] cells;
    private List<PlayerController> players = new List<PlayerController>();

    private double lastTickTime;

    public void AddPlayer(PlayerController player)
    {
        players.Add(player);

        cells[player.GamePosition.x, player.GamePosition.y].SetActive(false);
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

    private void Update()
    {
        if (PhotonNetwork.Time > lastTickTime + 1 && PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Vector2Int[] directions = players
                .OrderBy(p => p.GetPhotonView.Owner.ActorNumber)
                .Select(p => p.Direction)
                .ToArray();

            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(42, directions, options, sendOptions);

            PerformTick(directions);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case 42:
                Vector2Int[] directions = (Vector2Int[])photonEvent.CustomData;

                PerformTick(directions);

                break;
        }
    }

    private void PerformTick(Vector2Int[] directions)
    {
        if (players.Count != directions.Length) return;

        PlayerController[] sortedPlayers = players
            .OrderBy(p => p.GetPhotonView.Owner.ActorNumber)
            .ToArray();

        int i = 0;

        foreach (var player in sortedPlayers)
        {
            player.Direction = directions[i++];

            MinePlayerBlock(player);
        }

        foreach (var player in sortedPlayers)
        {
            MovePlayer(player);
        }

        lastTickTime = PhotonNetwork.Time;
    }

    private void MinePlayerBlock(PlayerController player)
    {
        Vector2Int targetPosition = player.GamePosition + player.Direction;

        if (targetPosition.x < 0) return;
        if (targetPosition.y < 0) return;
        if (targetPosition.x >= width - 1) return;
        if (targetPosition.y >= height - 1) return;

        cells[targetPosition.x, targetPosition.y].SetActive(false);

        Vector2Int pos = targetPosition;
        PlayerController minePlayer = players.First(p => p.GetPhotonView.IsMine);

        if (minePlayer != player)
        {
            while (pos.y < cells.GetLength(1) && !cells[pos.x, pos.y].activeSelf)
            {
                if(pos == minePlayer.GamePosition)
                {
                    PhotonNetwork.LeaveRoom();
                    break;
                }

                pos.y++;
            }
        }
    }

    private void MovePlayer(PlayerController player)
    {
        player.GamePosition += player.Direction;

        if (player.GamePosition.x < 0) player.GamePosition.x = 0;
        if (player.GamePosition.y < 0) player.GamePosition.y = 0;
        if (player.GamePosition.x >= width - 1) player.GamePosition.x = width - 1;
        if (player.GamePosition.y >= height - 1) player.GamePosition.y = height - 1;

        int ladderLength = 0;
        Vector2Int pos = player.GamePosition;
        while (pos.y > 0 && !cells[pos.x, pos.y - 1].activeSelf)
        {
            ladderLength++;
            pos.y--;
        }

        player.SetLadderLength(ladderLength);
    }

    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
