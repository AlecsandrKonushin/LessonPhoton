using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPunObservable
{
    [SerializeField] private Sprite enemySprite;
    [SerializeField] private Transform ladder;

    private PhotonView photonView;
    private SpriteRenderer spriteRenderer;

    private Vector2Int direction;

    private float speedMove = 10;

    public Vector2Int GamePosition;
    public PhotonView GetPhotonView { get => photonView; }
    public Vector2Int Direction { get => direction; set => direction = value; }

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GamePosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        MapController.Instance.AddPlayer(this);

        if (!photonView.IsMine) spriteRenderer.sprite = enemySprite;
    }

    private void Update()
    {
        if (photonView.IsMine)
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
}
