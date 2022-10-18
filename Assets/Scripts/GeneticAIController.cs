using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Individual
{
    // Size: 8
    public float[] Genes { get; }
    public Character Bot { get; }
    public int Score { get; set; }

    public Individual(Character character)
    {
        Genes = new float[8] {
            Random.value * 0.1f,
            Random.value * 0.1f,
            Random.value * 0.1f,
            Random.value * 0.1f,
            Random.value * 0.1f,
            Random.value * 0.1f,
            Random.value * 0.1f,
            Random.value * 0.1f
        };

        Bot = character;
        Bot.Init(
            Genes[0],
            Genes[1],
            Genes[2],
            Genes[3],
            Genes[4],
            Genes[5],
            Genes[6],
            Genes[7]
        );

        Score = 0;
    }

    public Individual(Character character, Character parent1, Character parent2)
    {

    }

    public void AddScore(int addedValue)
    {
        Score += addedValue;
    }
}

public class GeneticAIController : MonoBehaviour
{
    // Prefabs
    [Header("Prefabs")]
    [SerializeField] private GameObject _mapPrefab;
    [SerializeField] private GameObject _characterPrefab;

    // Camera
    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("Battles configuration")]
    [SerializeField] private int _numberBattles;
    [SerializeField] float _battleTimeLimit;
    [SerializeField] int _numberRounds;

    private List<BattleArena> _mapList;
    private List<Individual> _population;

    [Header("Time control")]
    [Range(0, 100)] [SerializeField] float _timeScale;


    void Awake()
    {
        StartUpSimulation();

        StartCoroutine(BattleTimer());
    }

    public void OnCameraMove(InputAction.CallbackContext ctx)
    {
        Vector2 cameraInput = ctx.ReadValue<Vector2>();

        if (ctx.performed && cameraInput != new Vector2(0, 0))
        {
            if (Mathf.Abs(cameraInput.x) > Mathf.Abs(cameraInput.y))
            {
                _camera.transform.position = new Vector3(
                    _camera.transform.position.x + 10 * Mathf.Sign(cameraInput.x),
                    _camera.transform.position.y,
                    _camera.transform.position.z
                );
            }
            else {
                _camera.transform.position = new Vector3(
                    _camera.transform.position.x,
                    _camera.transform.position.y + 10 * Mathf.Sign(cameraInput.y),
                    _camera.transform.position.z
                );
            }
        }
    }

    public void OnZoomIn(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && _camera.orthographicSize >= 5)
        {
            _camera.orthographicSize -= 5;
        }
    }

    public void OnZoomOut(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _camera.orthographicSize += 5;
        }
    }

    private void Update()
    {
        Time.timeScale = _timeScale;
    }

    private IEnumerator BattleTimer()
    {

        for (int i = 0; i < _numberRounds; i++)
        {

            MixPlayers();
            ResetPlayers();

            yield return new WaitForSeconds(_battleTimeLimit);

            Debug.Log("FINISH BATTLE " + i.ToString());
            foreach (Individual individual in _population)
            {
                individual.Bot.Die();
            }

            EvaluatePopulation();
        }
    }

    private void StartUpSimulation()
    {
        _mapList = new List<BattleArena>();
        _population = new List<Individual>();

        int numberOfMapsPerRow = (int) Mathf.Ceil(Mathf.Sqrt(_numberBattles));

        int arenaCount = 0;
        for (int i = 0; i < numberOfMapsPerRow; i++)
        {  
            for (int j = 0; j < numberOfMapsPerRow; j++)
            {

                if (arenaCount < _numberBattles)
                {
                    arenaCount += 1;

                    // Map
                    string mapName = "Arena " + arenaCount.ToString();                    
                    GameObject mapGameObject = Instantiate(
                        _mapPrefab,
                        new Vector3(10*j, 10*i, 0),
                        Quaternion.identity
                    );
                    mapGameObject.name = mapName;
                    BattleArena map = mapGameObject.GetComponent<BattleArena>();
                    map.ChangeName(mapName);

                    _mapList.Add(map);  


                    // Character 1
                    string p1Name = "Player 1 - " + arenaCount.ToString();
                    GameObject p1GameObject = Instantiate(
                        _characterPrefab,
                        new Vector3(0, 0, 0),
                        Quaternion.identity
                    );
                    p1GameObject.name = p1Name;
                    Character p1 = p1GameObject.GetComponent<Character>();


                    // Character 2
                    string p2Name = "Player 2 - " + arenaCount.ToString();
                    GameObject p2GameObject = Instantiate(
                        _characterPrefab,
                        new Vector3(0, 0, 0),
                        Quaternion.identity
                    );
                    p2GameObject.name = p2Name;
                    Character p2 = p2GameObject.GetComponent<Character>();

                    // Add to population
                    Individual individual1 = new Individual(p1);
                    Individual individual2 = new Individual(p2);

                    _population.Add(individual1);
                    _population.Add(individual2);
                }
            } 
        } 
    }

    private void EvaluatePopulation()
    {
        for (int i = 0; i < _numberBattles; i++)
        {
            int score = _population[2*i].Bot.HP - _population[2*i+1].Bot.HP;

            _population[2*i].AddScore(score);
            _population[2*i+1].AddScore(-score);
        }
    }

    private void MixPlayers()
    {
        List<Individual> newPopulation = new List<Individual>();

        // Move at in random order
        while (_population.Count > 0)
        {
            int removeAt = Random.Range(0, _population.Count);
            newPopulation.Add(_population[removeAt]);
            _population.RemoveAt(removeAt);
        }

        _population = newPopulation;
    }

    private void ResetPlayers()
    {
        int numberOfMapsPerRow = (int) Mathf.Ceil(Mathf.Sqrt(_numberBattles));
        int arenaCount = 0;

        for (int i = 0; i < numberOfMapsPerRow; i++)
        {  
            for (int j = 0; j < numberOfMapsPerRow; j++)
            {
                if (arenaCount < _numberBattles)
                {
                    // Reset the characters
                    _population[2*arenaCount].Bot.ResetCharacter();
                    _population[2*arenaCount+1].Bot.ResetCharacter();

                    // Set position
                    _population[2*arenaCount].Bot.transform.position = _mapList[arenaCount].transform.position - new Vector3(-2, 0, 0);

                    _population[2*arenaCount+1].Bot.transform.position = _mapList[arenaCount].transform.position - new Vector3(+2, 0, 0);

                    // Put the center point in the correct position
                    _population[2*arenaCount].Bot.CenterPoint = _mapList[arenaCount].transform.position;
                    _population[2*arenaCount+1].Bot.CenterPoint = _mapList[arenaCount].transform.position;


                    // Make them enemies
                    _population[2*arenaCount].Bot.ChangeEnemyCharacter(_population[2*arenaCount+1].Bot);
                    _population[2*arenaCount+1].Bot.ChangeEnemyCharacter(_population[2*arenaCount].Bot);

                    arenaCount += 1;
                }
            }
        }
    }

    // private float FitnessFunction(Character char)
    // {
    //     return 0.0f
    // }

    // private Character Crossing(Character p1, Character p2)
    // {

    // }

    // private Character Mutation(Character char)
    // {

    // }
}
