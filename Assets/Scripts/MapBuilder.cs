using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public enum TileType
{
    Wall,
    Floor
}

public class TileInfo
{
    public Vector3Int Position;
    public TileType TileType;

    public TileInfo(Vector3Int position, TileType tileType)
    {
        Position = position;
        TileType = tileType;
    }
}

public class MapBuilder : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] TileBase FloorTile;
    [SerializeField] TileBase WallTile;
    [SerializeField] int step;
    [SerializeField] Vector2Int mapSize;
    [SerializeField] int wallRatio;

    List<TileInfo> mapArray;

    [Space]
    [SerializeField] TileBase BottomLeftTile;
    [SerializeField] TileBase BottomMiddleTile;
    [SerializeField] TileBase BottomRightTile;
    [Space]
    [SerializeField] TileBase MiddleLeftTile;
    [SerializeField] TileBase MiddleRightTile;
    [Space]
    [SerializeField] TileBase TopLeftTile;
    [SerializeField] TileBase TopMiddleTile;
    [SerializeField] TileBase TopRightTile;

    [Space(20)]
    [SerializeField] GameObject playerPrefab;
    GameObject playerObject;

    [Header("Enemy")]
    [SerializeField] GameObject airEnemy;
    [SerializeField] Vector2 offset = new Vector2(0.5f, 0.5f);
    [SerializeField] GameObject groundEnemy;

    int round;

    // Start is called before the first frame update
    void Start()
    {
        round = 0;
        mapArray = new List<TileInfo>();

        BuildMap();

        for (int i = 0; i < 4; i++)
        {
            round++;
            ImproveMap();
        }

        BuildInsideTiles();
        BuildTile();
        SpawnEnemy();

        if (playerObject != null)
        {
            Destroy(playerObject);
        }

        if (D.Get<Player>().gameObject)
        {
            foreach (var tile in mapArray)
            {
                if (tile.TileType == TileType.Floor)
                {
                    playerObject = D.Get<Player>().gameObject;
                    playerObject.transform.position = tile.Position + new Vector3(0.5f, 0.5f, 0);
                    break;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (playerObject != null)
            {
                Destroy(playerObject);
            }

            foreach (var tile in mapArray)
            {
                if (tile.TileType == TileType.Floor)
                {
                    playerObject = Instantiate(playerPrefab);
                    playerObject.transform.position = tile.Position + new Vector3(0.5f, 0.5f, 0);
                    break;
                }
            }
        }
    }

    void BuildMap()
    {
        mapArray.Clear();
        tilemap.ClearAllTiles();

        System.Random random = new System.Random();

        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                int temp = random.Next(0, 100);
                TileType currentType = (temp > wallRatio ? TileType.Floor : TileType.Wall);
                Vector3Int currentPosition = new Vector3Int(x, y, 0);

                TileInfo currentInfo = new TileInfo(currentPosition, currentType);
                mapArray.Add(currentInfo);
            }
        }
    }

    int NeighborWallsCount(TileInfo info, Vector2Int range)
    {
        int count = 0;
        int index = info.Position.y * mapSize.x + info.Position.x;

        if (index > range.x * mapSize.x + (range.x - 1)
            && ((float)index < mapArray.Count - range.x * mapSize.x - range.x)
            && ((float)index >= (info.Position.y * mapSize.x + range.x))
            && ((float)index < (info.Position.y + 1) * mapSize.x - range.x)
            )
        {
            for (int i = 1; i < range.x + 1; i++)
            {
                // o x
                if (mapArray[index + i].TileType == TileType.Floor) { count++; }
                // x o
                if (mapArray[index - i].TileType == TileType.Floor) { count++; }

                if (mapArray[index - i * mapSize.x].TileType == TileType.Floor) { count++; }
                if (mapArray[index + i * mapSize.x].TileType == TileType.Floor) { count++; }

                for (int j = 1; j < range.y + 1; j++)
                {
                    //Debug.Log($"[{info.Position.x}, {info.Position.y}] {index}");
                    if (mapArray[index + mapSize.x + j].TileType == TileType.Floor) { count++; }
                    if (mapArray[index + mapSize.x - j].TileType == TileType.Floor) { count++; }

                    if (mapArray[index - j - i * mapSize.x].TileType == TileType.Floor) { count++; }
                    if (mapArray[index + j - i * mapSize.x].TileType == TileType.Floor) { count++; }
                }
            }
        }

        return count;
    }

    void ImproveMap()
    {
        foreach (var tile in mapArray)
        {
            if (tile.TileType == TileType.Floor)
            {
                tile.TileType = (NeighborWallsCount(tile, Vector2Int.one) >= 4) ? TileType.Floor : TileType.Wall;
            }
            else
            {
                tile.TileType = (NeighborWallsCount(tile, Vector2Int.one) >= 5) ? TileType.Floor : TileType.Wall;
            }
        }
    }

    void BuildTile()
    {
        foreach (var tile in mapArray)
        {
            //Debug.Log(tile.TileType);
            if (tile.TileType == TileType.Wall)
            {
                tilemap.SetTile(tile.Position, WallTile);
            }

            if (tile.TileType == TileType.Floor)
            {
                tilemap.SetTile(tile.Position, null);
            }
        }

        for (int i = 0; i < mapArray.Count - 1; i++)
        {
            TileInfo currentInfo = mapArray[i];
            if (currentInfo.TileType == TileType.Wall)
            {
                if ((i > 0 && mapArray[i - 1].TileType == TileType.Wall)
                    && (i > 0 && mapArray[i + 1].TileType == TileType.Wall)
                    && (i + mapSize.x < mapArray.Count && mapArray[i + mapSize.x].TileType == TileType.Floor)
                    )
                {
                    tilemap.SetTile(currentInfo.Position, TopMiddleTile);
                }
                else if ((i > 0 && mapArray[i - 1].TileType == TileType.Floor)
                && (i >= mapSize.x && mapArray[i - mapSize.x].TileType == TileType.Wall)
                && (i + mapSize.x < mapArray.Count && mapArray[i + mapSize.x].TileType == TileType.Wall)
                )
                {
                    tilemap.SetTile(currentInfo.Position, MiddleLeftTile);
                }
                else if ((i + 1 < mapArray.Count && mapArray[i + 1].TileType == TileType.Floor)
                    && (i >= mapSize.x && mapArray[i - mapSize.x].TileType == TileType.Wall)
                    && (i + mapSize.x < mapArray.Count && mapArray[i + mapSize.x].TileType == TileType.Wall)
                    )
                {
                    tilemap.SetTile(currentInfo.Position, MiddleRightTile);
                }
                else if ((i + mapSize.x < mapArray.Count && mapArray[i + mapSize.x].TileType == TileType.Floor)
                    && (i > 0 && mapArray[i - 1].TileType == TileType.Floor)
                    )
                {
                    tilemap.SetTile(currentInfo.Position, TopLeftTile);
                }
                else if ((i + mapSize.x < mapArray.Count && mapArray[i + mapSize.x].TileType == TileType.Floor)
                    && (i > 0 && mapArray[i + 1].TileType == TileType.Floor)
                    )
                {
                    tilemap.SetTile(currentInfo.Position, TopRightTile);
                }
                else if ((i >= mapSize.x && mapArray[i - mapSize.x].TileType == TileType.Floor)
                    && (i > 0 && mapArray[i - 1].TileType == TileType.Floor)
                    )
                {
                    tilemap.SetTile(currentInfo.Position, BottomLeftTile);
                }
                else if ((i >= mapSize.x && mapArray[i - mapSize.x].TileType == TileType.Floor)
                    && (i + 1 < mapArray.Count && mapArray[i + 1].TileType == TileType.Floor)
                    )
                {
                    tilemap.SetTile(currentInfo.Position, BottomRightTile);
                }
                else if ((i > 0 && mapArray[i - 1].TileType == TileType.Wall)
                    && (i > 0 && mapArray[i + 1].TileType == TileType.Wall)
                    && (i >= mapSize.x && mapArray[i - mapSize.x].TileType == TileType.Floor)
                    )
                {
                    tilemap.SetTile(currentInfo.Position, BottomMiddleTile);
                }
            }
        }
    }

    void BuildInsideTiles()
    {
        System.Random random = new System.Random();


        for (int i = 0; i < mapArray.Count; i++)
        {
            int temp = random.Next(1, 100);
            int y = random.Next(3, 5);
            int x = random.Next(3, 5);
            if ((mapArray[i].Position.y % y == 1) && mapArray[i].Position.x % x == 1 && mapArray[i].TileType != TileType.Wall && temp < 20)
            {
                int sizeX = random.Next(1, 5);
                if (i < mapArray.Count - sizeX)
                {
                    for (int j = 0; j < sizeX; j++)
                    {
                        mapArray[i + j].TileType = TileType.Wall;
                    }
                }
            }
        }
    }

    void SpawnEnemy()
    {
        System.Random random = new System.Random();
        foreach(var tile in mapArray)
        {
            if(tile.TileType != TileType.Wall)
            {
                bool spawnEnemy = random.Next(1, 100) < 5;
                if (spawnEnemy)
                {
                    if(random.Next(1, 2) == 1)
                    {
                        Instantiate(airEnemy).transform.position = new Vector2(tile.Position.x + offset.x, tile.Position.y + offset.y);
                    }
                    else
                    {
                        Instantiate(groundEnemy).transform.position = new Vector2(tile.Position.x + offset.x, tile.Position.y + offset.y);
                    }
                }
            }
        }
    }
}