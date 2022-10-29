using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text logText;
    [SerializeField] private InputField nickNameField;

    private const string SAVE_NICK = "NickName";

    private void Start()
    {
        string nickName = PlayerPrefs.GetString(SAVE_NICK, $"Player {Random.Range(0, 100)}");
        PhotonNetwork.NickName = nickName;
        nickNameField.text = nickName;
        Log(PhotonNetwork.NickName);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Log("Connect to master");
    }

    public void CreateRoom()
    {
        PhotonNetwork.NickName = nickNameField.text;
        PlayerPrefs.SetString(SAVE_NICK, nickNameField.text);

        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 20, CleanupCacheOnLeave = false });
    }

    public void JoinRoom()
    {
        PhotonNetwork.NickName = nickNameField.text;
        PlayerPrefs.SetString(SAVE_NICK, nickNameField.text);

        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined to room");

        PhotonNetwork.LoadLevel("Game");
    }

    private void Log(string message)
    {
        Debug.Log(message);
        logText.text += $"\n{message}";
    }
}
