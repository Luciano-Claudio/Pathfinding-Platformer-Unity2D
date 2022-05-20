using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MouseTarget : MonoBehaviour
{
    [SerializeField] private Vector3 mousePos;
    [SerializeField] private List<AIController> Bots;
    [SerializeField] private Transform circle;
    [SerializeField] private Transform square;

    private void Awake()
    {

        Bots = FindObjectsOfType<AIController>().ToList();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 aux = new Vector3(mousePos.x, mousePos.y, 0);
            circle.position = aux;
            Bots.First().Target = circle;
        }
        //if (Input.GetMouseButtonDown(1))
        //{
        //    mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    Vector3 aux = new Vector3(mousePos.x, mousePos.y, 0);
        //    square.position = aux;
        //    Bots.Last().Target = square;
        //}
    }
}
