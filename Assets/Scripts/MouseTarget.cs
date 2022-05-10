using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTarget : MonoBehaviour
{
    [SerializeField] private Vector3 mousePos;
    [SerializeField] private BotAI Bot;
    [SerializeField] private Transform circle;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 aux = new Vector3(mousePos.x, mousePos.y, 0);
            circle.position = aux;
            Bot.Target = circle;
        }
    }
}
