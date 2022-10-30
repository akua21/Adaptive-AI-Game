using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ManyBotsController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _mapPrefab;
    [SerializeField] private GameObject _characterPrefab;

    [Header("Simulation Inputs")]
    [SerializeField] private int _numberBots;
    [SerializeField] private string _filename;


    [Header("Time control")]
    [Range(0, 10)] [SerializeField] float _timeScale;


    private BattleArena _map;

    private List<Individual> _botList;

    void Awake()
    {
        // Create map
        string mapName = "Battle Arena";                    
        GameObject mapGameObject = Instantiate(
            _mapPrefab,
            new Vector3(0, 0, 0),
            Quaternion.identity
        );
        mapGameObject.name = mapName;
        _map = mapGameObject.GetComponent<BattleArena>();
        _map.ChangeName(mapName);
 
        // Create players
        _botList = new List<Individual>();

        for (int i = 0; i < _numberBots; i++)
        {
            string botName = "Bot - " + i.ToString();
            GameObject botGameObject = Instantiate(
                _characterPrefab,
                new Vector3(0, 0, -1),
                Quaternion.identity
            );
            botGameObject.name = botName;
            Character bot = botGameObject.GetComponent<Character>();
            bot.gameObject.SetActive(false);
            Individual individual = new Individual(bot, new float[8] {
                0.2f, 0.2f, 0.1f, 0.1f, 0.1f, 0.2f, 0.05f, 0.1f               
            });

            _botList.Add(individual);
        }
    }

    void Start()
    {
        // File
        if (_filename != "")
        {
            string path = Application.streamingAssetsPath + "/ManyBots/";
            _filename = path + _filename + ".csv";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllText(_filename, "probs,matchesWon\n");
        }

        // Fights
        StartCoroutine(SimulateFights());
    }

    private IEnumerator SimulateFights()
    {
        for (int i = 0; i < _botList.Count; i++)
        {
            for (int j = i+1; j < _botList.Count; j++)
            {
                Fight(_botList[i], _botList[j]);       
                Debug.Log(i.ToString() + "-" + j.ToString());
                yield return new WaitForSeconds(120);
                EndFight(_botList[i], _botList[j]);
                yield return new WaitForSeconds(1);
            }

            Debug.Log(_botList[i].Score);
        }

        foreach (Individual individual in _botList)
        {
            SaveInfo(individual);
        }
    }

    private void Fight(Individual fighter1, Individual fighter2)
    {
        fighter1.Bot.gameObject.SetActive(true);
        fighter2.Bot.gameObject.SetActive(true);

        fighter1.Bot.ResetCharacter();
        fighter2.Bot.ResetCharacter();

        fighter1.Bot.transform.position = _map.transform.position + new Vector3(-2, 0, -1);
        fighter2.Bot.transform.position = _map.transform.position + new Vector3(+2, 0, -1);

        fighter1.Bot.CenterPoint = _map.transform.position;
        fighter2.Bot.CenterPoint = _map.transform.position;

        fighter1.Bot.ChangeEnemyCharacter(fighter2.Bot);
        fighter2.Bot.ChangeEnemyCharacter(fighter1.Bot);
    }

    private void EndFight(Individual fighter1, Individual fighter2)
    {
        fighter1.Bot.gameObject.SetActive(false);
        fighter2.Bot.gameObject.SetActive(false);

        if (fighter1.Bot.HP == 0)
        {
            fighter2.Score += 1;
        }

        if (fighter2.Bot.HP == 0)
        {
            fighter1.Score += 1;
        }
    }

    private void SaveInfo(Individual individual)
    {
        if (_filename != "")
        {
            File.AppendAllText(_filename, "[");
            for (int i = 0; i < individual.Genes.Length-1; i++)
            {
                File.AppendAllText(_filename, individual.Genes[i].ToString() + ",");
            }
            File.AppendAllText(_filename, individual.Genes[individual.Genes.Length-1].ToString() + "],");
            File.AppendAllText(_filename, individual.Score.ToString() + "\n");
        }
    }

    private void Update()
    {
        Time.timeScale = _timeScale;
    }
}
