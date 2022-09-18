using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPunObservable
{
    private PhotonView photonView;
    private Material material;

    private bool isRed;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKey(KeyCode.LeftArrow)) transform.Translate(-Time.deltaTime * 5, 0, 0);
            if (Input.GetKey(KeyCode.RightArrow)) transform.Translate(Time.deltaTime * 5, 0, 0);

            if (Input.GetKey(KeyCode.Space))
            {
                isRed = !isRed;
            }
        }

        if (isRed)
        {
            material.color = Color.red;
        }
        else
        {
            material.color = Color.green;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isRed);
        }
        else
        {
            isRed = (bool)stream.ReceiveNext();
        }
    }
}
