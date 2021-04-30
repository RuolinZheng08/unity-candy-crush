using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridManager : MonoBehaviour
{
    public List<Sprite> Sprites = new List<Sprite>();
    public GameObject TilePrefab;
    public int GridDimension = 8;
    public float Distance = 1.0f;
    private GameObject[,] Grid;

    public GameObject GameOverMenu;
    public TextMeshProUGUI MovesText;
    public TextMeshProUGUI ScoreText;

    public static GridManager Instance { get; private set; }

    public int StartingMoves = 50;
    private int _numMoves;
    public int NumMoves {
        get { return _numMoves; }
        set {
            _numMoves = value;
            MovesText.text = _numMoves.ToString();
        }
    }

    private int _score;
    public int Score {
        get { return _score; }
        set {
            _score = value;
            ScoreText.text = _score.ToString();
        }
    }

    void Awake() {
        Instance = this;
        Score = 0;
        NumMoves = StartingMoves;
        GameOverMenu.SetActive(false);
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

    SpriteRenderer GetSpriteRendererAt(int row, int col) {
        if (col < 0 || col >= GridDimension || row < 0 || row >= GridDimension) {
            return null;
        }
        GameObject tile = Grid[row, col];
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        return renderer;
    }

    Sprite GetSpriteAt(int row, int col) {
        SpriteRenderer renderer = GetSpriteRendererAt(row, col);
        if (renderer == null) {
            return null;
        }
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
        Sprite sprite = ChooseRandomSprite(possibleSprites);
        return sprite;
    }

    Sprite ChooseRandomSprite(List<Sprite> sprites) {
        Sprite sprite = sprites[Random.Range(0, sprites.Count)];
        return sprite;
    }

    void GameOver() {
        PlayerPrefs.SetInt("score", Score);
        GameOverMenu.SetActive(true);
        SoundManager.Instance.PlaySound(SoundType.TypeGameOver);
    }

    public void SwapTiles(Vector2Int tile1Pos, Vector2Int tile2Pos) {
        GameObject tile1 = Grid[tile1Pos.x, tile1Pos.y];
        SpriteRenderer renderer1 = tile1.GetComponent<SpriteRenderer>();

        GameObject tile2 = Grid[tile2Pos.x, tile2Pos.y];
        SpriteRenderer renderer2 = tile2.GetComponent<SpriteRenderer>();

        Sprite temp = renderer1.sprite;
        renderer1.sprite = renderer2.sprite;
        renderer2.sprite = temp;

        bool hasMatch = CheckMatches();
        if (!hasMatch) { // if no match after swapping, swap back
            SoundManager.Instance.PlaySound(SoundType.TypeMove);
            temp = renderer1.sprite;
            renderer1.sprite = renderer2.sprite;
            renderer2.sprite = temp;
        } else { // some cells have been emptied, re-fill
            SoundManager.Instance.PlaySound(SoundType.TypePop);
            while (hasMatch) {
                FillHoles();
                hasMatch = CheckMatches();
            }
            NumMoves--;
            if (NumMoves <= 0) {
                NumMoves = 0;
                GameOver();
            }
        }
    }

    bool CheckMatches() {
        HashSet<SpriteRenderer> matchedTiles = new HashSet<SpriteRenderer>(); // to remove
        for (int row = 0; row < GridDimension; row++) {
            for (int col = 0; col < GridDimension; col++) {
                SpriteRenderer renderer = GetSpriteRendererAt(row, col);

                List<SpriteRenderer> horizontalMatches = FindColumnMatchForTile(row, col, renderer.sprite);
                if (horizontalMatches.Count >= 2) {
                    matchedTiles.UnionWith(horizontalMatches);
                    matchedTiles.Add(renderer);
                }

                List<SpriteRenderer> verticalMatches = FindRowMatchForTile(row, col, renderer.sprite);
                if (verticalMatches.Count >= 2) {
                    matchedTiles.UnionWith(verticalMatches);
                    matchedTiles.Add(renderer);
                }
            }
        }

        // remove
        foreach (SpriteRenderer renderer in matchedTiles) {
            renderer.sprite = null;
        }
        // accumulate score
        Score += matchedTiles.Count;

        return matchedTiles.Count > 0;
    }

    List<SpriteRenderer> FindColumnMatchForTile(int row, int col, Sprite sprite) {
        List<SpriteRenderer> ret = new List<SpriteRenderer>();
        for (int newCol = col + 1; newCol < GridDimension; newCol++) {
            SpriteRenderer nextCol = GetSpriteRendererAt(row, newCol);
            if (nextCol.sprite != sprite) {
                break;
            }
            ret.Add(nextCol);
        }
        return ret;
    }

    List<SpriteRenderer> FindRowMatchForTile(int row, int col, Sprite sprite) {
        List<SpriteRenderer> ret = new List<SpriteRenderer>();
        for (int newRow = row + 1; newRow < GridDimension; newRow++) {
            SpriteRenderer nextRow = GetSpriteRendererAt(newRow, col);
            if (nextRow.sprite != sprite) {
                break;
            }
            ret.Add(nextRow);
        }
        return ret;
    }

    void FillHoles() {
        int topmostRow = GridDimension - 1;
        for (int row = 0; row < GridDimension; row++) {
            for (int col = 0; col < GridDimension; col++) {
                // while not occupied by a sprite, try grabbing a sprite from the cell above
                while (GetSpriteAt(row, col) == null) {
                    for (int filler = row; filler < topmostRow; filler++) {
                        SpriteRenderer curr = GetSpriteRendererAt(filler, col);
                        SpriteRenderer above = GetSpriteRendererAt(filler + 1, col);
                        curr.sprite = above.sprite;
                    }
                    // put a random sprite at top, it may cascade down
                    SpriteRenderer topmost = GetSpriteRendererAt(topmostRow, col);
                    topmost.sprite = ChooseRandomSprite(Sprites);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Grid = new GameObject[GridDimension, GridDimension];
        InitGrid();
    }

}
