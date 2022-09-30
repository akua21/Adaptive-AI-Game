using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class ControllerVisuals : MonoBehaviour
{

    // Character control inputs
    private PlayerControl controls;

    

    private void Awake() {
        controls = new PlayerControl();

        GameObject northInner = GameObject.Find("NorthInner");
        GameObject southInner = GameObject.Find("SouthInner");
        GameObject eastInner = GameObject.Find("EastInner");
        GameObject westInner = GameObject.Find("WestInner");


        northInner.SetActive(false);
        southInner.SetActive(false);
        eastInner.SetActive(false);
        westInner.SetActive(false);

        // Show button used
        controls.Gameplay.EmptyAction.performed += ctx => northInner.SetActive(true);
        controls.Gameplay.EmptyAction.canceled += ctx => northInner.SetActive(false);

        controls.Gameplay.Block.performed += ctx => southInner.SetActive(true);
        controls.Gameplay.Block.canceled += ctx => southInner.SetActive(false);

        controls.Gameplay.Dash.performed += ctx => westInner.SetActive(true);
        controls.Gameplay.Dash.canceled += ctx => westInner.SetActive(false);

        controls.Gameplay.Attack.performed += ctx => eastInner.SetActive(true);
        controls.Gameplay.Attack.canceled += ctx => eastInner.SetActive(false);


    }

    private void OnEnable() {
        controls.Gameplay.Enable();
    }

    private void OnDisable() {
        controls.Gameplay.Disable();
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
