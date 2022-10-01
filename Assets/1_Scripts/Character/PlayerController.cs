using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPunObservable
{
    private PhotonView photonView;
    private SpriteRenderer spriteRenderer;

    private Vector2Int direction;
    private Vector2Int gamePosition;

    private float speedMove = 10;

    public Vector2Int GetGamePosition { get => gamePosition; }

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        gamePosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        MapController.Instance.AddPlayer(this);
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

        if (direction == Vector2Int.left) spriteRenderer.flipX = false;
        if (direction == Vector2Int.right) spriteRenderer.flipX = true;

        transform.position = Vector3.MoveTowards(transform.position, (Vector2)gamePosition, Time.deltaTime * speedMove);
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
