using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPunObservable
{
    [SerializeField] private Sprite enemySprite, deadSprite;
    [SerializeField] private Transform ladder;
    [SerializeField] TextMeshProUGUI nickNameText;

    private PhotonView photonView;
    private SpriteRenderer spriteRenderer;

    private Vector2Int direction;
    private float speedMove = 10;
    private bool isDead;
    private int score;

    public Vector2Int GamePosition;
    public PhotonView GetPhotonView { get => photonView; }
    public Vector2Int Direction { get => direction; set => direction = value; }
    public bool GetIsDead { get => isDead; }
    public int GetScore { get => score; }
    public int AddScore { set => score += value; }

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GamePosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        MapController.Instance.AddPlayer(this);

        nickNameText.SetText(photonView.Owner.NickName);

        if (!photonView.IsMine) 
        { 
            spriteRenderer.sprite = enemySprite;
            nickNameText.color = Color.green;
        }
    }

    private void Update()
    {
        if (photonView.IsMine && !isDead)
        {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) direction = Vector2Int.left;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) direction = Vector2Int.right;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) direction = Vector2Int.up;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) direction = Vector2Int.down;
        }

        //if (direction == Vector2Int.left) spriteRenderer.flipX = false;
        //if (direction == Vector2Int.right) spriteRenderer.flipX = true;

        transform.position = Vector3.MoveTowards(transform.position, (Vector2)GamePosition, Time.deltaTime * speedMove);
    }

    public void SetLadderLength(int length)
    {
        for (int i = 0; i < ladder.childCount; i++)
        {
            ladder.GetChild(i).gameObject.SetActive(i < length);
        }

        while (ladder.childCount < length)
        {
            Transform lastTile = ladder.GetChild(ladder.childCount - 1);
            Instantiate(lastTile, lastTile.position + Vector3.down, Quaternion.identity, ladder);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(direction);
        }
        else
        {
            direction = (Vector2Int)stream.ReceiveNext();
        }
    }

    public void Kill()
    {
        isDead = true;
        spriteRenderer.sprite = deadSprite;

        SetLadderLength(0);
    }
}
