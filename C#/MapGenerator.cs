using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using UnityEngine;
using System.Data;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    private string dbName = "URI=file:Map.s3db";
    public Sprite image;
    public GameObject tile;

    public GameObject[,] mapRender;
    public int[,] map;
    public int worldId = 0;
    public int mapXsize;
    public int mapYsize;
    public int renderXsize;
    public int renderYsize;

    public int xChangedCoordinate = -1;
    public int yChangedCoordinate = -1;

    public float yTileOffset;
    public GameObject parent;
    public Player player;
    public World world;


    void Start()
    {
        world.onTic += OnTic;
        mapRender = new GameObject[renderXsize * 2 + 1, renderYsize * 2 + 1];
    }
    public void OnTic()
    {

        // Чистим отрендеренную карту
        for (int i = 0; i < renderXsize * 2 + 1; i++)
        {
            for (int j = 0; j < renderYsize * 2 + 1; j++)
            {
                if (mapRender[i, j] != null && i != xChangedCoordinate && j != yChangedCoordinate)
                {
                    Destroy(mapRender[i, j].gameObject);
                    mapRender[i, j] = null;
                }
            }
        }

        // Извлекаем карту из бд
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Max(XCoordinate) FROM Tiles WHERE WorldId = " + worldId;
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.GetValue(0) != DBNull.Value)
                    {
                        mapXsize = Int32.Parse(reader.GetValue(0).ToString());
                        mapXsize += 1;
                    }
                    else mapXsize = 0;
                    reader.Close();
                }

                command.CommandText = "SELECT Max(YCoordinate) FROM Tiles WHERE WorldId = " + worldId;
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.GetValue(0) != DBNull.Value)
                    {
                        mapYsize = Int32.Parse(reader.GetValue(0).ToString());
                        mapYsize += 1;
                    }
                    else mapYsize = 0;
                    reader.Close();
                }

                map = new int[mapXsize, mapYsize];

                command.CommandText = "SELECT * FROM Tiles WHERE WorldId = " + worldId; 
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        map[Int32.Parse(reader["XCoordinate"].ToString()), Int32.Parse(reader["YCoordinate"].ToString())] = Int32.Parse(reader["TileId"].ToString());
                    }
                    reader.Close();
                }
            }
            connection.Close();
        }

        // определяем границы рендера
        int renderXstart = (int)(player.transform.position.x - renderXsize);
        int renderYstart = (int)(player.transform.position.y / (1 - yTileOffset) - renderYsize);
        int renderXend = (int)(player.transform.position.x + renderXsize);
        int renderYend = (int)(player.transform.position.y / (1 - yTileOffset) + renderYsize);

        if (renderXstart < 0) renderXstart = 0;
        else if (renderXend > mapXsize) renderXend = mapXsize;

        if (renderYstart < 0) renderYstart = 0;
        else if (renderYend > mapYsize) renderYend = mapYsize;

        

        // рендерим карту
        for (int i = renderXstart; i < renderXend; i++)
        {
            for (int j = renderYstart; j < renderYend; j++)
            {
                if (mapRender[i, j] == null)
                {
                    using (var connection = new SqliteConnection(dbName))
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT TilePicture FROM TilesCharacteristic INNER JOIN Tiles ON TilesCharacteristic.TileId = Tiles.TileId WHERE Tiles.XCoordinate = " + i + " AND Tiles.YCoordinate = " + j + " AND WorldId = " + worldId;
                            using (IDataReader reader = command.ExecuteReader())
                            {
                                if (reader["TilePicture"] != DBNull.Value)
                                {
                                    var tex = new Texture2D(1, 1);
                                    tex.LoadImage((byte[])reader["TilePicture"]);
                                    image = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));                  
                                    tile.GetComponent<SpriteRenderer>().sprite = image;
                                    tile.transform.localScale = new Vector3(2.08f, 2.08f, 1);
                                    mapRender[i, j] = Instantiate(tile, new Vector3(i, j - (yTileOffset * j), tile.transform.position.z), Quaternion.identity, parent.transform);
                                }
                            }
                        }
                        connection.Close();
                    }
                    
                }
            }
        }
    }
    void Update()
    {

    }
}
