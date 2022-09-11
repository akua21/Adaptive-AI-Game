using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleArena : MonoBehaviour
{

    // Number of edged, the more it has, the smother the border
    [SerializeField] private int _numEdges;

    // Radius of the circle, 0.5     by default
    [SerializeField] private float _radius = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        Vector2[] points = new Vector2[_numEdges];
        for (int i = 0; i < _numEdges; i++)
         {
             float angle = 2 * Mathf.PI * i / _numEdges;
             float x = _radius * Mathf.Cos(angle);
             float y = _radius * Mathf.Sin(angle);
 
             points[i] = new Vector2(x, y);
         }
         GetComponent<EdgeCollider2D>().points = points;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}