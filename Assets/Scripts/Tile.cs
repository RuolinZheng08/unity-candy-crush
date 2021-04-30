using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private static Tile selected;
    private SpriteRenderer Renderer;
    public Vector2Int Position;

    // Start is called before the first frame update
    void Start()
    {
        Renderer = GetComponent<SpriteRenderer>();
    }

    public void Select() {
        Renderer.color = Color.grey;
    }

    public void Unselect() {
        Renderer.color = Color.white;
    }

    void OnMouseDown() {
        if (selected == null) {
            selected = this;
            Select();
        } else {
            if (selected == this) {
                return;
            }
            selected.Unselect();
            float distance = Vector2Int.Distance(Position, selected.Position);
            if (distance == 1) {
                GridManager.Instance.SwapTiles(Position, selected.Position);
                selected = null;
            } else {
                selected = this;
                Select();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
