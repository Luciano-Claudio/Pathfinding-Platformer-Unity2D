using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MouseTarget : MonoBehaviour
{
    [SerializeField] private Vector3 mousePos;
    [SerializeField] private List<BotAI> Bots;
    [SerializeField] private Transform circle;

    private void Awake()
    {

        Bots = FindObjectsOfType<BotAI>().ToList();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 aux = new Vector3(mousePos.x, mousePos.y, 0);
            circle.position = aux;
            foreach (var bot in Bots)
            {
                bot.Target = circle;
            }
        }
    }
}
