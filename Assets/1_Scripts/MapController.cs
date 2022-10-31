using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapController : Singleton<MapController>, IOnEventCallback
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private int width, height;
    [SerializeField] private PlayersTop playerTop;

    private GameObject[,] cells;
    private List<PlayerController> players = new List<PlayerController>();

    private double lastTickTime;

    public List<PlayerController> GetPlayerControllers { get => players; }

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
        if (PhotonNetwork.Time > lastTickTime + 0.5f && PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 1)
        {
            Vector2Int[] directions = players
                .Where(p => !p.GetIsDead)
                .OrderBy(p => p.GetPhotonView.Owner.ActorNumber)
                .Select(p => p.Direction)
                .ToArray();

            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(42, directions, options, sendOptions);

            PerformTick(directions);
        }
    }

    public void SendSyncData(Player player)
    {
        SyncData data = new SyncData();

        RaiseEventOptions options = new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        data.Positions = new Vector2Int[players.Count];
        data.Scores = new int[players.Count];

        PlayerController[] sortedPlayers = players
            .Where(p => !p.GetIsDead)
            .OrderBy(p => p.GetPhotonView.Owner.ActorNumber)
            .ToArray();

        for (int i = 0; i < sortedPlayers.Length; i++)
        {
            data.Positions[i] = sortedPlayers[i].GamePosition;
            data.Scores[i] = sortedPlayers[i].GetScore;
        }

        data.MapData = new BitArray(width * height);
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                data.MapData.Set(w + h * cells.GetLength(0), cells[w, h].activeSelf);
            }
        }

        PhotonNetwork.RaiseEvent(43, data, options, sendOptions);
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case 42:
                Vector2Int[] directions = (Vector2Int[])photonEvent.CustomData;

                PerformTick(directions);

                break;

            case 43:
                SyncData data = (SyncData)photonEvent.CustomData;

                OnSyncData(data);

                break;
        }
    }

    private void OnSyncData(SyncData data)
    {
        PlayerController[] sortedPlayers = players
           .Where(p => !p.GetIsDead)
           .Where(p => !p.GetPhotonView.IsMine)
           .OrderBy(p => p.GetPhotonView.Owner.ActorNumber)
           .ToArray();

        for (int i = 0; i < sortedPlayers.Length; i++)
        {
            sortedPlayers[i].GamePosition = data.Positions[i];
            sortedPlayers[i].SetScore = data.Scores[i];

            sortedPlayers[i].transform.position = (Vector2)sortedPlayers[i].GamePosition;
        }

        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                bool cellActive = data.MapData.Get(w + h * cells.GetLength(0));
                cells[w, h].SetActive(cellActive);
            }
        }
    }

    private void PerformTick(Vector2Int[] directions)
    {
        if (players.Count != directions.Length) return;

        PlayerController[] sortedPlayers = players
            .Where(p => !p.GetIsDead)
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

        foreach (var player in players.Where(p => p.GetIsDead))
        {
            Vector2Int pos = player.GamePosition;
            while (pos.y > 0 && !cells[pos.x, pos.y - 1].activeSelf)
            {
                pos.y--;
            }

            player.GamePosition = pos;
        }

        playerTop.SetTexts(players);
        lastTickTime = PhotonNetwork.Time;
    }

    private void MinePlayerBlock(PlayerController player)
    {
        Vector2Int targetPosition = player.GamePosition + player.Direction;

        if (targetPosition.x < 0) return;
        if (targetPosition.y < 0) return;
        if (targetPosition.x >= width - 1) return;
        if (targetPosition.y >= height - 1) return;

        if (cells[targetPosition.x, targetPosition.y].activeSelf)
        {
            cells[targetPosition.x, targetPosition.y].SetActive(false);
            player.AddScore = 1;
        }

        Vector2Int pos = targetPosition;
        PlayerController minePlayer = players.First(p => p.GetPhotonView.IsMine);

        if (minePlayer != player)
        {
            while (pos.y < cells.GetLength(1) && !cells[pos.x, pos.y].activeSelf)
            {
                if (pos == minePlayer.GamePosition)
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
