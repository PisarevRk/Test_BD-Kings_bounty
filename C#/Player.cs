using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private string dbName = "URI=file:Map.s3db";

    public Sprite workSprite;
    public Sprite sprite;
    public SpriteRenderer render;
    public DoingState doingState = DoingState.StandStill;
    public Text statusText;
    public World world;
    public WorldActions actions;
    public Camera mainCamera;
    public MapGenerator mapGenerator;
    public Vector2 targetPosition;

    private bool isMoving = false;
    private bool isUses = false;
    private bool isEnters = false;
    private bool isChangeWorld = false;

    public bool freezPlayer = false;
    public int freezTime = 0;

    public int xChangedCoordinate = -1;
    public int yChangedCoordinate = -1;

    int changeOperationId;

    public int worldId = 0;

    public float cameraSpeed;

    void Start()
    {
        world.onTic += OnTic;
        transform.position = new Vector3(0.5f, (1 - mapGenerator.yTileOffset)/2, transform.position.z);
    }
    public void OnTic()
    {
        //================================================================================================
        // Вызываются раз в промежуток времени, заданный в классе 

        isMoving = false;
        isUses = false;
        isEnters = false;
        isChangeWorld = false;

        // Выбор действия на основе состояния игрока
        if (doingState == DoingState.MoveUp) MoveUp();        
        else if (doingState == DoingState.MoveDown) MoveDown();      
        else if (doingState == DoingState.MoveRight) MoveRight();       
        else if (doingState == DoingState.MoveLeft) MoveLeft();      
        else if (doingState == DoingState.ChangeUp) ChangeUp();    
        else if (doingState == DoingState.ChangeDown) ChangeDown();    
        else if (doingState == DoingState.ChangeRight) ChangeRight();      
        else if (doingState == DoingState.ChangeLeft) ChangeLeft();     
        doingState = DoingState.StandStill;
        statusText.text = "";

        // Если выбрана цель для пути
        if (isMoving)
        {
            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            targetPosition = new Vector2(0, 0);
        }
        else if (isEnters)
        {
            int joint = 0;
            using (var connection = new SqliteConnection(dbName))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT JointId FROM WorldInJoint WHERE XCoordinate = '" + targetPosition.x + "' AND YCoordinate = '" + targetPosition.y + "' AND WorldId = '" + mapGenerator.worldId + "';";
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader["JointId"] != DBNull.Value)
                        {
                            joint = Int32.Parse(reader["JointId"].ToString());
                        }
                        reader.Close();
                    }
                    command.CommandText = "SELECT * FROM WorldOutJoint WHERE JointId = " + joint;
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader["WorldId"] != DBNull.Value)
                        {
                            mapGenerator.worldId = Int32.Parse(reader["WorldId"].ToString());
                            transform.position = new Vector3(Int32.Parse(reader["XCoordinate"].ToString()) + 0.5f, (Int32.Parse(reader["YCoordinate"].ToString())) * (1 - mapGenerator.yTileOffset) + (1 - mapGenerator.yTileOffset)/2, transform.position.z);
                            targetPosition = new Vector2(0, 0);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
                isEnters = false;
            }
        }
        else if (isUses)
        {
            int id = 0;
            using (var connection = new SqliteConnection(dbName))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT TileId FROM Tiles WHERE XCoordinate = '" + targetPosition.x + "' AND YCoordinate = '" + targetPosition.y + "' AND WorldId = '" + mapGenerator.worldId + "';";
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader["TileId"] != DBNull.Value)
                        {
                            id = Int32.Parse(reader["TileId"].ToString());
                        }
                        reader.Close();
                    }
                    command.CommandText = "SELECT ScriptId FROM TilesScripts WHERE TileId = " + id;
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        if (reader["ScriptId"] != DBNull.Value)
                        {
                            id = Int32.Parse(reader["ScriptId"].ToString());
                            if (id == 0) actions.Cave(id);  
                        }
                        reader.Close();
                    }
                }
                connection.Close();
                isUses = false;
            }
        }


        // Если игрок заморожен
        if (freezTime > 0)
        {
            // Если игрок меняет объект
            if (isChangeWorld)
            {
                render.sprite = workSprite;
                ChangeWordProcess(freezTime);
            }
            freezTime -= 1;

            if (freezTime == 0)
            {
                if (isChangeWorld) 
                {
                    render.sprite = sprite;
                    EndChange(); 
                }
                freezPlayer = false;
            }
            

        }
    }
    public void MoveUp()
    {
        //================================================================================================
        // Нажата клавиша "вверх" 

        // Если игроку можно двигаться
        if (freezPlayer) return;

        // проверка на границу карты
        if (transform.position.y + (1 - mapGenerator.yTileOffset) != mapGenerator.mapYsize)
        {

            // если по цели можно пройти
            if (isMove((int)(transform.position.x), (int)((transform.position.y + (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset))))
            {
                isMoving = true;
                targetPosition = new Vector2(transform.position.x, transform.position.y + (1 - mapGenerator.yTileOffset));
            }

            // если цель можно использовать
            else if (isUse((int)(transform.position.x), (int)((transform.position.y + (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset))))
            {
                isUses = true;
                targetPosition = new Vector2(transform.position.x - 0.5f, (transform.position.y + (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset) - 0.5f);
            }

            // если в цель можно войти
            else if (isGate((int)(transform.position.x), (int)((transform.position.y + (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset))))
            {
                isEnters = true;
                targetPosition = new Vector2(transform.position.x - 0.5f, (transform.position.y + (1 - mapGenerator.yTileOffset))/ (1 - mapGenerator.yTileOffset) - 0.5f);
            }
        }
        //================================================================================================
    }
    public void MoveDown()
    {
        //================================================================================================
        // Нажата клавиша "вниз" 

        // *смотри функцию MoveUp()
        if (freezPlayer) return;

        if (transform.position.y - (1 - mapGenerator.yTileOffset) >= 0.1)
        {
            if (isMove((int)(transform.position.x), (int)((transform.position.y - (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset))))
            {
                isMoving = true;
                targetPosition = new Vector2(transform.position.x, transform.position.y - (1 - mapGenerator.yTileOffset));
            }
            else if (isUse((int)(transform.position.x), (int)((transform.position.y - (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset))))
            {
                isUses = true;
                targetPosition = new Vector2(transform.position.x - 0.5f, (transform.position.y - (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset) - 0.5f);
            }
            else if (isGate((int)(transform.position.x), (int)((transform.position.y - (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset))))
            {
                isEnters = true;
                targetPosition = new Vector2(transform.position.x - 0.5f, (transform.position.y - (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset) - 0.5f);
                //statusText.text = "Захожу в " + target.Name;
            }
        }
        //================================================================================================
    }
    public void MoveRight()
    {
        //================================================================================================
        // Нажата клавиша "вправо" 

        // *смотри функцию MoveUp()
        if (freezPlayer) return;

        if ((transform.position.x + 1) < mapGenerator.mapXsize)
        { 
            if (isMove((int)(transform.position.x + 1), (int)(transform.position.y / (1 - mapGenerator.yTileOffset))))
            {
                isMoving = true;
                targetPosition = new Vector2(transform.position.x + 1, transform.position.y);
            }
            else if (isUse((int)(transform.position.x + 1), (int)(transform.position.y / (1 - mapGenerator.yTileOffset))))
            {
                isUses = true;
                float y = transform.position.y - 0.355f;
                if (y < 0.355) y = 0;
                targetPosition = new Vector2(transform.position.x + 1 - 0.5f, y / (1 - mapGenerator.yTileOffset));
            }
            else if (isGate((int)(transform.position.x + 1), (int)(transform.position.y / (1 - mapGenerator.yTileOffset))))
            {
                isEnters = true;
                float y = transform.position.y - 0.355f;
                if (y < 0.355) y = 0;
                targetPosition = new Vector2(transform.position.x + 1 - 0.5f, y / (1 - mapGenerator.yTileOffset));
            }
        }
        //================================================================================================
    }
    public void MoveLeft()
    {
        //================================================================================================
        // Нажата клавиша "влево" 

        // *смотри функцию MoveUp()
        if (freezPlayer) return;

        if ((transform.position.x - 1) > 0)
        {
            if (isMove((int)(transform.position.x - 1), (int)(transform.position.y / (1 - mapGenerator.yTileOffset))))
            {
                isMoving = true;
                targetPosition = new Vector2(transform.position.x - 1, transform.position.y);
            }
            else if (isUse((int)(transform.position.x - 1), (int)(transform.position.y / (1 - mapGenerator.yTileOffset))))
            {
                isUses = true;
                float y = transform.position.y - 0.355f;
                if (y < 0.355) y = 0;
                targetPosition = new Vector2(transform.position.x - 1 - 0.5f, y / (1 - mapGenerator.yTileOffset));
            }
            else if (isGate((int)(transform.position.x - 1), (int)(transform.position.y / (1 - mapGenerator.yTileOffset))))
            {
                isEnters = true;
                float y = transform.position.y - 0.355f;
                if (y < 0.355) y = 0;
                targetPosition = new Vector2(transform.position.x - 1 - 0.5f, y / (1 - mapGenerator.yTileOffset));
            }
        }
        //================================================================================================
    }
    public void ChangeUp() 
    {
        // проверка на границу карты
        if (transform.position.y + (1 - mapGenerator.yTileOffset) != mapGenerator.mapYsize)
        {
            // если цель можно изменить
            if (isChange((int)(transform.position.x), (int)((transform.position.y + (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset))))
            {
                xChangedCoordinate = (int)(transform.position.x);
                yChangedCoordinate = (int)((transform.position.y + (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset));
                mapGenerator.xChangedCoordinate = (int)(transform.position.x);
                mapGenerator.yChangedCoordinate = (int)((transform.position.y + (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset));
                
                StartChange(HowLongToChange((int)(transform.position.x), (int)((transform.position.y + (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset))));
            }
        }
    }
    public void ChangeDown() 
    {
        if (transform.position.y - (1 - mapGenerator.yTileOffset) >= 0.1)
        {
            // если цель можно изменить
            if (isChange((int)(transform.position.x), (int)((transform.position.y - (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset))))
            {
                xChangedCoordinate = (int)(transform.position.x);
                yChangedCoordinate = (int)((transform.position.y - (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset));
                mapGenerator.xChangedCoordinate = (int)(transform.position.x);
                mapGenerator.yChangedCoordinate = (int)((transform.position.y - (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset));

                StartChange(HowLongToChange((int)(transform.position.x), (int)((transform.position.y - (1 - mapGenerator.yTileOffset)) / (1 - mapGenerator.yTileOffset))));               
            }
        }
    }
    public void ChangeRight() 
    {
        if ((transform.position.x + 1) < mapGenerator.mapXsize)
        {
            // если цель можно изменить
            if (isChange((int)(transform.position.x + 1), (int)(transform.position.y / (1 - mapGenerator.yTileOffset))))
            {
                xChangedCoordinate = (int)(transform.position.x + 1);
                yChangedCoordinate = (int)(transform.position.y / (1 - mapGenerator.yTileOffset));
                mapGenerator.xChangedCoordinate = (int)(transform.position.x + 1);
                mapGenerator.yChangedCoordinate = (int)(transform.position.y / (1 - mapGenerator.yTileOffset));

                StartChange(HowLongToChange((int)(transform.position.x + 1), (int)(transform.position.y / (1 - mapGenerator.yTileOffset))));                
            }
        }
    }
    public void ChangeLeft() 
    {
        if ((transform.position.x - 1) > 0)
        {
            // если цель можно изменить
            if (isChange((int)(transform.position.x - 1), (int)(transform.position.y / (1 - mapGenerator.yTileOffset))))
            {
                xChangedCoordinate = (int)(transform.position.x - 1);
                yChangedCoordinate = (int)(transform.position.y / (1 - mapGenerator.yTileOffset));
                mapGenerator.xChangedCoordinate = (int)(transform.position.x - 1);
                mapGenerator.yChangedCoordinate = (int)(transform.position.y / (1 - mapGenerator.yTileOffset));

                StartChange(HowLongToChange((int)(transform.position.x - 1), (int)(transform.position.y / (1 - mapGenerator.yTileOffset))));     
            }
        }
    }

    private void StartChange(int inf)
    {
        if (inf == -1)
        {
            statusText.text = "Что-то пошло не так";
            return;
        }
        freezTime = inf;
        freezPlayer = true;
        isChangeWorld = true;
    }
    private void ChangeWordProcess(int step)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT OperationId FROM TilesChange INNER JOIN Tiles ON TilesChange.TileId = Tiles.TileId WHERE Tiles.XCoordinate = " + xChangedCoordinate + " AND Tiles.YCoordinate = " + yChangedCoordinate + ";";
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader["OperationId"] != DBNull.Value)
                    {
                        changeOperationId = Int32.Parse(reader["OperationId"].ToString());
                    }
                }
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Image FROM TilesImage WHERE TilesImage.OperationId = " + changeOperationId + " AND TilesImage.Step = " + step + ";";
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader["Image"] != DBNull.Value)
                    {
                        var tex = new Texture2D(1, 1);
                        tex.LoadImage((byte[])reader["Image"]);
                        Debug.Log("Работаю");
                        mapGenerator.mapRender[xChangedCoordinate, yChangedCoordinate].GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
                    }
                }
            }
            connection.Close();
        }

        
            
        
    }
    private void EndChange()
    {
        int newId = 0;
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT ChangeTileId FROM TilesChange WHERE OperationId = " + changeOperationId + ";";
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader["ChangeTileId"] != DBNull.Value)
                    {
                        newId =  Int32.Parse(reader["ChangeTileId"].ToString());
                    }
                }
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE Tiles SET TileId = '" + newId + "' WHERE XCoordinate = '" + xChangedCoordinate + "' AND YCoordinate = '" + yChangedCoordinate + "' AND WorldId = '" + worldId + ";";
                command.ExecuteNonQuery();
            }
            connection.Close();
        }

        xChangedCoordinate = -1;
        yChangedCoordinate = -1;
        mapGenerator.xChangedCoordinate = -1;
        mapGenerator.yChangedCoordinate = -1;

        Debug.Log("Закончил");
    }

    bool isMove(int x, int y)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT IsMove FROM TilesCharacteristic INNER JOIN Tiles ON TilesCharacteristic.TileId = Tiles.TileId WHERE Tiles.XCoordinate = " + x + " AND Tiles.YCoordinate = " + y + " AND Tiles.WorldId = '" + mapGenerator.worldId + "';";
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader["IsMove"] != DBNull.Value)
                    {
                        if (Int32.Parse(reader["IsMove"].ToString()) == 1) return true;
                        else return false;
                    }
                }
            }
            connection.Close();
            return false;
        }
    }
    bool isUse(int x, int y)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT IsUse FROM TilesCharacteristic INNER JOIN Tiles ON TilesCharacteristic.TileId = Tiles.TileId WHERE Tiles.XCoordinate = " + x + " AND Tiles.YCoordinate = " + y + " AND Tiles.WorldId = '" + mapGenerator.worldId + "';";
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader["IsUse"] != DBNull.Value)
                    {
                        if (Int32.Parse(reader["IsUse"].ToString()) == 1) return true;
                        else return false;
                    }
                }
            }
            connection.Close();
            return false;
        }
    }
    bool isChange(int x, int y)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT IsChange FROM TilesCharacteristic INNER JOIN Tiles ON TilesCharacteristic.TileId = Tiles.TileId WHERE Tiles.XCoordinate = " + x + " AND Tiles.YCoordinate = " + y + " AND Tiles.WorldId = '" + mapGenerator.worldId + "';";
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader["IsChange"] != DBNull.Value)
                    {
                        if (Int32.Parse(reader["IsChange"].ToString()) == 1) return true;
                        else return false;
                    }
                }
            }
            connection.Close();
            return false;
        }
    }
    bool isGate(int x, int y)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT IsGate FROM TilesCharacteristic INNER JOIN Tiles ON TilesCharacteristic.TileId = Tiles.TileId WHERE Tiles.XCoordinate = " + x + " AND Tiles.YCoordinate = " + y + " AND Tiles.WorldId = '" + mapGenerator.worldId + "';";
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader["IsGate"] != DBNull.Value)
                    {
                        if (Int32.Parse(reader["IsGate"].ToString()) == 1) return true;
                        else return false;
                    }
                }
            }
            connection.Close();
            return false;
        }
    }

    int HowLongToChange(int x, int y)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Times FROM TilesChange INNER JOIN Tiles ON TilesChange.TileId = Tiles.TileId WHERE Tiles.XCoordinate = " + x + " AND Tiles.YCoordinate = " + y + ";";
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader["Times"] != DBNull.Value)
                    {
                        return Int32.Parse(reader["Times"].ToString());
                    }
                }
            }
            connection.Close();
            return -1;
        }
    }

    float Convert(float y)
    {
        float y1 = 0;
        while (y1 < y)
            y1 += (1 - mapGenerator.yTileOffset);
        return y1;
    }

    void Update()
    {
        // Двигаем камеру
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, cameraSpeed * Time.deltaTime);
    }

    public enum DoingState
    {
        StandStill,
        MoveUp,
        MoveDown,
        MoveRight,
        MoveLeft,
        ChangeUp,
        ChangeDown,
        ChangeRight,
        ChangeLeft
    }
}
