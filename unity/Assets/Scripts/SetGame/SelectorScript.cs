using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class SelectorScript : MonoBehaviour
{
    public Image img;

    public PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction selectAction;

    private Vector2Int gridPos = Vector2Int.zero;
    private int gridWidth = 0;
    private int gridHeight = 0;

    private float moveDelay = 0.2f;
    private float lastMoveTime = 0f;

    void Start()
    {
        Debug.Log("Starting Selector");
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        moveAction.Enable();
        selectAction = playerInput.actions.FindAction("Jump");
        selectAction.Enable();

        gridPos = Vector2Int.zero;
        Transform card = GridScript.gridScript.transform.GetChild(0);
        transform.position = card.position;
    }

    public void SetColor(Color c)
    {
        img.color = c;
    }

    void Update()
    {
        if (GridScript.cardObjects.Count < 3) gridWidth = GridScript.cardObjects.Count;
        else gridWidth = 3;

        gridHeight = GridScript.cardObjects.Count / gridWidth;

        Vector2 move = moveAction.ReadValue<Vector2>();
        if (Time.time - lastMoveTime >= moveDelay)
        {
            Vector2Int delta = Vector2Int.zero;
            if (move.x > 0.5f) delta.x = 1;
            else if (move.x < -0.5f) delta.x = -1;
            if (move.y > 0.5f) delta.y = -1;
            else if (move.y < -0.5f) delta.y = 1;

            if (delta != Vector2Int.zero)
            {
                TryMove(delta);
                lastMoveTime = Time.time;
            }
        }

        if (selectAction.WasPressedThisFrame())
        {
            Debug.Log("Selector pressed");
            int index = gridPos.y * gridWidth + gridPos.x;
            Debug.Log("Card " + gridPos);
            Debug.Log("Index " + index);
            if (index >= 0 && index < GridScript.cardObjects.Count)
            {
                SetCard selected = GridScript.cardObjects[index];
                GridScript.CardSelected(selected, playerInput);
            }
        }
    }

    void TryMove(Vector2Int delta)
    {
        gridPos.x = (gridPos.x + gridWidth + delta.x) % gridWidth;
        gridPos.y = (gridPos.y + gridHeight + delta.y) % gridHeight;
        Debug.Log("Moving Cursor to " + gridPos);
        UpdateSelectorPosition();
    }

    void UpdateSelectorPosition()
    {
        int index = gridPos.y * gridWidth + gridPos.x;
        if (index >= 0 && index < GridScript.gridScript.transform.childCount)
        {
            Transform card = GridScript.gridScript.transform.GetChild(index);
            transform.position = card.position;
        }
    }
}
