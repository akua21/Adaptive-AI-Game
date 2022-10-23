using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BehaviourEnum { player, bot, botInputs };
public enum CharacterState { idle, attack, block, dash, hitted, recoil };
public enum BotMovementState { idle, follow, wander };


public class GameInputs
{
    public int PlayerHP { get; set; }
    public int BotHP { get; set; }
    public float DistanceToPlayer { get; set; }
    public Vector2 DirectionToPlayer { get; set; }
    public CharacterState PlayerState { get; set; }
    public CharacterState BotState { get; set; }
    public float PlayerRotation { get; set; }
    public float BotRotation { get; set; }

    public GameInputs()
    {
        PlayerHP = 0;
        BotHP = 0;
        DistanceToPlayer = 0f;
        DirectionToPlayer = new Vector2(0, 0);
        PlayerState = CharacterState.idle;
        BotState = CharacterState.idle;
        PlayerRotation = 0f;
        BotRotation = 0f;
    }

    public GameInputs(
        int playerHP,
        int botHP,
        float distanceToPlayer,
        Vector2 directionToPlayer,
        CharacterState playerState,
        CharacterState botState,
        float playerRotation,
        float botRotation
    )
    {
        PlayerHP = playerHP;
        BotHP = botHP;
        DistanceToPlayer = distanceToPlayer;
        DirectionToPlayer = directionToPlayer;
        PlayerState = playerState;
        BotState = botState;
        PlayerRotation = playerRotation;
        BotRotation = botRotation;
    }

}

public class MatchController : MonoBehaviour
{

    [SerializeField] public Character Player;
    [SerializeField] public Character Bot;

    public static int PlayerLives;
    public static int BotLives;

    private static int _currentDifficulty;
    public static int CurrentDifficulty {
        get {
            return _currentDifficulty;
        }
        set {
            Debug.Log("Change");
            if (value > 1)
            {
                _currentDifficulty = 1;
            }
            else if (value < -1)
            {
                _currentDifficulty = -1;
            }
            else{
                _currentDifficulty = value;
            }
            UpdateGenesFromDifficulty(_currentDifficulty);
        }
    } 
    public static float[] CurrentGenes;

    public static readonly float[] BOTEASY = new float[8] {
        0.07f,
        0.03f,
        0.10f,
        0.10f,
        0.02f,
        0.02f,
        0.02f,
        0.01f
    };

    public static readonly float[] BOTMEDIUM = new float[8] {
        0.09f,
        0.03f,
        0.08f,
        0.12f,
        0.04f,
        0.04f,
        0.04f,
        0.02f
    };

    public static readonly float[] BOTHARD = new float[8] {
        0.12f,
        0.02f,
        0.05f,
        0.15f,
        0.06f,
        0.06f,
        0.06f,
        0.02f
    };

    public static bool WarmUp;

    private static void UpdateGenesFromDifficulty(int difficulty)
    {
        switch (difficulty)
        {
            case -1:
                CurrentGenes = BOTEASY;
                break;
            case 0:
                CurrentGenes = BOTMEDIUM;
                break;
            case 1:
                CurrentGenes = BOTHARD;
                break;
            default:
                break;
        }
    }

    public static void UpdateGenes(float[] genes)
    {
        CurrentGenes = genes;
    }

    public static void UpdateWarmUp(bool warmUp)
    {
        WarmUp = warmUp;
    }

    void Update()
    {
        PlayerLives = Player.CurrentLives;
        BotLives = Bot.CurrentLives;
    }
}
