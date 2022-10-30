using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Individual
{
    // Size: 8
    public float[] Genes { get; }
    public Character Bot { get; }
    public int Score { get; set; }

    public Individual(Character character, float[] multiplier, bool isStatic = false)
    {
        if (!isStatic)
        {
            Genes = new float[8] {
            Random.value * multiplier[0],
            Random.value * multiplier[1],
            Random.value * multiplier[2],
            Random.value * multiplier[3],
            Random.value * multiplier[4],
            Random.value * multiplier[5],
            Random.value * multiplier[6],
            Random.value * multiplier[7]
        };
        }
        else
        {
            Genes = MatchController.CurrentGenes;

        }
        Bot = character;
        Bot.Init(
            Genes[0],
            Genes[1],
            Genes[2],
            Genes[3],
            Genes[4],
            Genes[5],
            Genes[6],
            Genes[7],
            true,
            BehaviourEnum.botGenetic
        );

    
        Score = 0;
    }

    public Individual(Character character, bool isStatic = false, float multiplier = 0.1f)
    {
        if (!isStatic)
        {
            Genes = new float[8] {
            Random.value * multiplier,
            Random.value * multiplier,
            Random.value * multiplier,
            Random.value * multiplier,
            Random.value * multiplier,
            Random.value * multiplier,
            Random.value * multiplier,
            Random.value * multiplier
        };
        }
        else
        {
            Genes = MatchController.CurrentGenes;

        }
        Bot = character;
        Bot.Init(
            Genes[0],
            Genes[1],
            Genes[2],
            Genes[3],
            Genes[4],
            Genes[5],
            Genes[6],
            Genes[7],
            true,
            BehaviourEnum.botGenetic
        );

    
        Score = 0;

    }

    public Individual(Character character, Individual parent1, Individual parent2)
    {

        Genes = new float[8] {
            (parent1.Genes[0] + parent2.Genes[0]) / 2 + Random.value * 0.2f - 0.1f,
            (parent1.Genes[1] + parent2.Genes[1]) / 2 + Random.value * 0.2f - 0.1f,
            (parent1.Genes[2] + parent2.Genes[2]) / 2 + Random.value * 0.2f - 0.1f,
            (parent1.Genes[3] + parent2.Genes[3]) / 2 + Random.value * 0.2f - 0.1f,
            (parent1.Genes[4] + parent2.Genes[4]) / 2 + Random.value * 0.2f - 0.1f,
            (parent1.Genes[5] + parent2.Genes[5]) / 2 + Random.value * 0.2f - 0.1f,
            (parent1.Genes[6] + parent2.Genes[6]) / 2 + Random.value * 0.2f - 0.1f,
            (parent1.Genes[7] + parent2.Genes[7]) / 2 + Random.value * 0.2f - 0.1f
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
            Genes[7],
            true,
            BehaviourEnum.botGenetic
        );

        Score = 0;
    }

    public override string ToString()
    {
        string text = "{Score: " + Score.ToString() + ", Genes: [";


        for (int i = 0; i < 7; i++)
        {
            text += Genes[i].ToString() + ", ";
        }

        text += Genes[7].ToString() + "]}";

        return text;
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
    [SerializeField] private bool _canMoveCamera;

    [Header("Canvas")]
    [SerializeField] private GameObject _canvas;
    [SerializeField] private Image _loadingBar;

    [Header("Battles configuration")]
    [SerializeField] private int _numberGenerations; // Number of generations
    [SerializeField] private int _numberRounds; // Number of rounds in a generation
    [SerializeField] private float _battleTimeLimit;
    [SerializeField] private int _numberBattles; // Number of simultaneous battles

    private int _numberFinishedRounds;
    private List<BattleArena> _mapList;
    private List<Individual> _population;
    private List<Individual> _staticPopulation; // Population from the hardcoded bot. Does not evolve

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

        if (ctx.performed && cameraInput != new Vector2(0, 0) && _canMoveCamera)
        {
            _canvas.SetActive(false);
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
        if (ctx.performed && _camera.orthographicSize >= 5 && _canMoveCamera)
        {
            _camera.orthographicSize -= 5;
            _canvas.SetActive(false);
        }
    }

    public void OnZoomOut(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && _canMoveCamera)
        {
            _camera.orthographicSize += 5;
            _canvas.SetActive(false);
        }
    }

    public void OnHide(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && _canMoveCamera)
        {
            _canvas.SetActive(!_canvas.activeSelf);
            if (_canvas.activeSelf)
            {
                _camera.orthographicSize = 5;        
                _camera.transform.position = new Vector3(0, 0, _camera.transform.position.z);
            }
        }
    }

    public void OnLoadingUpdate() {
        _numberFinishedRounds += 1;
        _loadingBar.fillAmount = (float)_numberFinishedRounds / (float)(_numberGenerations * _numberRounds);
    }

    private void Update()
    {
        Time.timeScale = _timeScale;
    }

    private IEnumerator BattleTimer()
    {

        for (int i = 0; i < _numberGenerations-1; i++)
        {
            for (int j = 0; j < _numberRounds; j++)
            {

                MixPlayers();
                ResetPlayers();

                yield return new WaitForSeconds(_battleTimeLimit);

                // Debug.Log("FINISH BATTLE " + j.ToString() + " of ROUND " + i.ToString());
                foreach (Individual individual in _population)
                {
                    individual.Bot.Die();
                }

                foreach (Individual individual in _staticPopulation)
                {
                    individual.Bot.Die();
                }

                EvaluatePopulation();
                OnLoadingUpdate();
            }
            NewGeneration();
        }

        // Debug.Log("---"); 
        // Debug.Log("FINAL BATTLE");
        // Debug.Log("---");

        for (int j = 0; j < _numberRounds; j++)
        {

            MixPlayers();
            ResetPlayers();

            yield return new WaitForSeconds(_battleTimeLimit);

            // Debug.Log("FINISH BATTLE " + j.ToString() + " of the LAST ROUND");
            foreach (Individual individual in _population)
            {
                individual.Bot.Die();
            }

            foreach (Individual individual in _staticPopulation)
            {
                individual.Bot.Die();
            }

            EvaluatePopulation();
            OnLoadingUpdate();
        }
        DisplayBestBot();
        // Save the best bot so it plays against the player
        SaveBestBot();
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }

    private void DisplayBestBot()
    {
        Individual bestIndividual = _population[0];

        foreach (Individual individual in _population)
        {
            if (individual.Score > bestIndividual.Score)
            {
                bestIndividual = individual;
            }
        }

        Debug.Log(bestIndividual.ToString());
    }

    private void SaveBestBot()
    {
       Individual bestIndividual = _population[0];

        foreach (Individual individual in _population)
        {
            if (individual.Score > bestIndividual.Score)
            {
                bestIndividual = individual;
            }
        }
        MatchController.UpdateGenes(bestIndividual.Genes);
    }

    private void StartUpSimulation()
    {
        _mapList = new List<BattleArena>();
        _population = new List<Individual>();

        _staticPopulation = new List<Individual>();

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
                    string p1Name = "Evolving Player - " + arenaCount.ToString();
                    GameObject p1GameObject = Instantiate(
                        _characterPrefab,
                        new Vector3(0, 0, 0),
                        Quaternion.identity
                    );
                    p1GameObject.name = p1Name;
                    Character p1 = p1GameObject.GetComponent<Character>();


                    // Character 2
                    string p2Name = "Static Player - " + arenaCount.ToString();
                    GameObject p2GameObject = Instantiate(
                        _characterPrefab,
                        new Vector3(0, 0, 0),
                        Quaternion.identity
                    );
                    p2GameObject.name = p2Name;
                    Character p2 = p2GameObject.GetComponent<Character>();

                    // Add to population
                    Individual individual1 = new Individual(p1);
                    Individual individual2 = new Individual(p2, isStatic: true);

                    _population.Add(individual1);
                    _staticPopulation.Add(individual2);
                }
            } 
        } 
    }

    private void EvaluatePopulation()
    {
        for (int i = 0; i < _numberBattles; i++)
        {   
            int botResult = _population[i].Bot.HP - _staticPopulation[i].Bot.HP;
            
            int playerResult = MatchController.WinnerHP;

            if (!MatchController.PlayerWon)
            {
                playerResult = -playerResult;
            }

            int score = Mathf.Abs(botResult - playerResult);
            _population[i].Score += (10 - score);
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

        for (int i = 0; i < _numberBattles; i++)
        {
            // Reset the characters
            _population[i].Bot.ResetCharacter();
            _staticPopulation[i].Bot.ResetCharacter();

            // Set position
            _population[i].Bot.transform.position = _mapList[i].transform.position + new Vector3(-2, 0, -1);

            _staticPopulation[i].Bot.transform.position = _mapList[i].transform.position + new Vector3(+2, 0, -1);

            // Put the center point in the correct position
            _population[i].Bot.CenterPoint = _mapList[i].transform.position;
            _staticPopulation[i].Bot.CenterPoint = _mapList[i].transform.position;

            // Make them enemies
            _population[i].Bot.ChangeEnemyCharacter(_staticPopulation[i].Bot);
            _staticPopulation[i].Bot.ChangeEnemyCharacter(_population[i].Bot);
        }
    }

    private void NewGeneration() {

        // Calculate the total ammount of score
        int rouletteSize = 0;

        foreach (Individual individual in _population)
        {
            rouletteSize += individual.Score;
        }

        Individual[] roulette = new Individual[rouletteSize];

        int roulettePos = 0;

        // Create roulette so better individuals occupy more space
        foreach (Individual individual in _population)
        {
            for (int i = 0; i < individual.Score; i++)
            {
                roulette[roulettePos] = individual;
                roulettePos += 1;
            }
        }

        List<Individual> newPopulation = new List<Individual>();


        for (int i = 0; i < _population.Count; i++)
        {
            Individual p1 = roulette[Random.Range(0, rouletteSize)];
            Individual p2 = roulette[Random.Range(0, rouletteSize)];

            GameObject childGameObject = Instantiate(
                _characterPrefab,
                new Vector3(0, 0, 0),
                Quaternion.identity
            );
            Character child = childGameObject.GetComponent<Character>();

            // Add to population
            Individual childIndividual = new Individual(child, p1, p2);
            newPopulation.Add((childIndividual));
        }

        // Destroy previous population
        foreach (Individual individual in _population)
        {
            Destroy(individual.Bot.gameObject);
        }

        _population = newPopulation;
    }
}