using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayersTop : MonoBehaviour
{
    [SerializeField] private Text[] texts;

    private void Start()
    {
        foreach (var text in texts)
        {
            text.text = "";
        }
    }

    public void SetTexts(List<PlayerController> players)
    {
        PlayerController[] top = players
            .Where(p => !p.GetIsDead)
            .OrderByDescending(p => p.GetScore)
            .Take(5)
            .ToArray();

        for (int i = 0; i < top.Length; i++)
        {
            texts[i].text = $"{i + 1}. {top[i].GetPhotonView.Owner.NickName} - {top[i].GetScore}";
        }
    }
}
