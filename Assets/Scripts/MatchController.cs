using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour
{

    [SerializeField] public Character Player;
    [SerializeField] public Character Bot;

    public static int PlayerLives;
    public static int BotLives;

    void Update()
    {
        PlayerLives = Player.CurrentLives;
        BotLives = Bot.CurrentLives;
    }
}
