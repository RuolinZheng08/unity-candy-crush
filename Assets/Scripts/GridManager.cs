using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public List<Sprite> Sprites = new List<Sprite>();
    public GameObject TilePrefab;
    public int GridDimension = 8;
    public float Distance = 1.0f;
    private GameObject[,] Grid;

    public static GridManager Instance {get; private set;}

    void Awake() {
        Instance = this;
    }

    void InitGrid() {
        Vector3 center = new Vector3(GridDimension * Distance / 2, GridDimension * Distance / 2, 0);
        Vector3 positionOffset = transform.position - center;
        for (int row = 0; row < GridDimension; row++) {
            for (int col = 0; col < GridDimension; col++) {
                GameObject newTile = Instantiate(TilePrefab);
                SpriteRenderer renderer = newTile.GetComponent<SpriteRenderer>();
                renderer.sprite = ChooseSprite(row, col);
                newTile.transform.parent = transform;
                Vector3 position = new Vector3(col * Distance, row * Distance, 0);
                Vector3 offseted = position + positionOffset;
                newTile.transform.position = offseted;
                // logical position
                newTile.GetComponent<Tile>().Position = new Vector2Int(row, col);

                Grid[row, col] = newTile;
            }
        }
    }

    Sprite GetSpriteAt(int row, int col) {
        if (col < 0 || col >= GridDimension || row < 0 || row >= GridDimension) {
            return null;
        }
        GameObject tile = Grid[row, col];
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        return renderer.sprite;
    }

    Sprite ChooseSprite(int row, int col) {
        // avoid three same sprites on startup
        List<Sprite> possibleSprites = new List<Sprite>(Sprites);
        Sprite left1 = GetSpriteAt(row, col - 1);
        Sprite left2 = GetSpriteAt(row, col - 2);
        if (left2 != null && left1 == left2) {
            possibleSprites.Remove(left1);
        }

        Sprite down1 = GetSpriteAt(row - 1, col);
        Sprite down2 = GetSpriteAt(row - 2, col);
        if (down2 != null && down1 == down2) {
            possibleSprites.Remove(down1);
        }
        Sprite sprite = possibleSprites[Random.Range(0, possibleSprites.Count)];
        return sprite;
    }

    public void SwapTiles(Vector2Int tile1Pos, Vector2Int tile2Pos) {
        GameObject tile1 = Grid[tile1Pos.x, tile1Pos.y];
        SpriteRenderer renderer1 = tile1.GetComponent<SpriteRenderer>();

        GameObject tile2 = Grid[tile2Pos.x, tile2Pos.y];
        SpriteRenderer renderer2 = tile2.GetComponent<SpriteRenderer>();

        Sprite temp = renderer1.sprite;
        renderer1.sprite = renderer2.sprite;
        renderer2.sprite = temp;
    }

    // Start is called before the first frame update
    void Start()
    {
        Grid = new GameObject[GridDimension, GridDimension];
        InitGrid();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
