using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using UnityEngine;
using System.Data;
using UnityEngine.UI;
public class WorldEditor : MonoBehaviour
{
    public Sprite image;
    public int worldId;
    public int mapXdesiredSize;
    public int mapYdesiredSize;
    GameObject[,] mapRender;
    public GameObject tile;
    public GameObject parent;
    public Camera mainCamera;
    public float cameraSpeed;

    public int mapXsize;
    public int mapYsize;

    public float yTileOffset;

    public int xMousPos;
    public int yMousPos;

    public float xMousPosFloat;
    public float yMousPosFloat;

    public float yMousPosConvert;
    public bool onUI;
    int[,] map;
    public int id = 0;

    private string dbName = "URI=file:Map.s3db";

    void Start()
    {
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

                if (mapXsize < mapXdesiredSize) mapXsize = mapXdesiredSize;
                if (mapYsize < mapYdesiredSize) mapYsize = mapYdesiredSize;

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
        mapRender = new GameObject[mapXsize, mapYsize];

        for (int i = 0; i < mapXsize; i++)
        {
            for (int j = 0; j < mapYsize; j++)
            {
                mapRender[i, j] = null;
            }
        }

        // рендерим карту
        for (int i = 0; i < mapXsize; i++)
        {
            for (int j = 0; j < mapYsize; j++)
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

    public void swithPref(int x)
    {
        id = x;
    }
    float Convert(float yMouse)
    {
        float y1 = 0;
        while (y1 < yMouse)
            y1 += (1 - yTileOffset);
        return y1;
    }
    public void End()
    {
        for (int i = 0; i < mapXsize; i++)
        {
            for (int j = 0; j < mapYsize; j++)
            {
                Debug.Log(map[i, j]);
                UpdateData(i, j, map[i, j]);
            }
        }
    }   
    void UpdateData(int x, int y, int id)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT TileId FROM Tiles WHERE XCoordinate = '"+x+ "' AND YCoordinate = '"+y+ "' AND WorldId = " + worldId;
                int k = 0;
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (id == -1 && reader.GetValue(0) != DBNull.Value) k = 1;
                    else if (id != -1 && reader.GetValue(0) != DBNull.Value) k = 2;
                    else if (id != -1 && reader.GetValue(0) == DBNull.Value) k = 3;
                    reader.Close();
                }
                if (k == 1)
                {
                    command.CommandText = "DELETE FROM Tiles WHERE XCoordinate = " + x + " AND YCoordinate = " + y + " AND WorldId = " + worldId;
                    command.ExecuteNonQuery();
                }
                else if (k == 2)
                {
                    command.CommandText = "UPDATE Tiles SET TileId = '" + id + "' WHERE XCoordinate = '" + x + "' AND YCoordinate = '" + y + "' AND WorldId = " + worldId;
                    try { command.ExecuteNonQuery(); }
                    catch { Debug.Log("Нарушина уникальность бд, отмена операции"); }
                }
                else if (k == 3)
                {
                    command.CommandText = "INSERT INTO Tiles (XCoordinate, YCoordinate, TileId, WorldId) VALUES ('" + x + "','" + y + "','" + id + "','" + worldId + "')";
                    command.ExecuteNonQuery();
                }

            }
            connection.Close();
        }
    }

    void Update()
    {
        xMousPosFloat = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        yMousPosFloat = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;

        xMousPos = (int)Math.Ceiling(xMousPosFloat);
        yMousPosConvert = Convert(yMousPosFloat);
        yMousPos = (int)Math.Round(yMousPosConvert / (1 - yTileOffset));

        xMousPos -= 1;
        yMousPos -= 1;

        if (!onUI)
        {
            if (Input.GetMouseButton(0))
            {
                if (xMousPos < mapXsize && xMousPos >= 0 && yMousPos < mapYsize && yMousPos >= 0)
                    if (mapRender[xMousPos, yMousPos] == null)
                    {

                        using (var connection = new SqliteConnection(dbName))
                        {
                            connection.Open();
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = "SELECT TilePicture FROM TilesCharacteristic WHERE TileId = '"+id+"';";
                                using (IDataReader reader = command.ExecuteReader())
                                {
                                    if (reader["TilePicture"] != DBNull.Value)
                                    {
                                        var tex = new Texture2D(1, 1);
                                        tex.LoadImage((byte[])reader["TilePicture"]);
                                        image = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
                                        tile.GetComponent<SpriteRenderer>().sprite = image;
                                        tile.transform.localScale = new Vector3(2.08f, 2.08f, 1);
                                        mapRender[xMousPos, yMousPos] = Instantiate(tile, new Vector3(xMousPos, yMousPosConvert-(1-yTileOffset), tile.transform.position.z), Quaternion.identity, parent.transform);
                                        map[xMousPos, yMousPos] = id;
                                    }
                                }
                            }
                            connection.Close();
                        }

                        
                    }
            }
            if (Input.GetMouseButton(1))
            {
                if (xMousPos < mapXsize && xMousPos >= 0 && yMousPos < mapYsize && yMousPos >= 0)
                    if (mapRender[xMousPos, yMousPos] != null)
                    {
                        Destroy(mapRender[xMousPos, yMousPos].gameObject);
                        mapRender[xMousPos, yMousPos] = null;
                        map[xMousPos, yMousPos] = -1;
                    }
            }
            if (Input.GetMouseButton(2))
            {
                Vector3 targetPosition = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, mainCamera.transform.position.z);
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, cameraSpeed * Time.deltaTime);
            }

            

            mainCamera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * 2;
            if (mainCamera.orthographicSize < 3)
            {
                mainCamera.orthographicSize = 3;
            }
            if (mainCamera.orthographicSize > 13)
            {
                mainCamera.orthographicSize = 13;
            }
        }
    }
}
