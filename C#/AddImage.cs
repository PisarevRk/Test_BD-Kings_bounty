using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using UnityEngine;
using System.Data;
using UnityEngine.UI;

public class AddImage : MonoBehaviour
{
    private string dbName = "URI=file:Map.s3db";

    [Header("Спрайт")]
    public Sprite sprite;
    [Space]
    [Header("TilesCharacteristic")]
    public int id;
    public int isMove;
    public int isUse;
    public int iChange;
    public int isGate;
    [Space]
    [Header("TilesImage")]
    public int OpId;
    public int step;
    [Space]
    [Header("TilesScripts")]
    public int TileId;
    public int ScriptId;
    public string text;

    void Start()
    {
        
    }
    public void GoToTilesCharacteristic()
    {
        byte[] bytes = ImageConversion.EncodeArrayToJPG(sprite.texture.GetRawTextureData(), sprite.texture.graphicsFormat, (uint)sprite.texture.width, (uint)sprite.texture.height);
        InsertToDataTilesCharacteristic(id, bytes, isMove, isUse, iChange, isGate);
    }
    public void GoToTilesImage()
    {
        byte[] bytes = ImageConversion.EncodeArrayToJPG(sprite.texture.GetRawTextureData(), sprite.texture.graphicsFormat, (uint)sprite.texture.width, (uint)sprite.texture.height);
        InsertToDataTilesImage(OpId, step, bytes);
    }
    public void GoToTilesScripts()
    {
        byte[] bytes = ImageConversion.EncodeArrayToJPG(sprite.texture.GetRawTextureData(), sprite.texture.graphicsFormat, (uint)sprite.texture.width, (uint)sprite.texture.height);
        InsertToDataTilesScripts(TileId, ScriptId, bytes, text);
    }

    void InsertToDataTilesCharacteristic(int id, byte[] bytes, int im, int iu, int ic, int ig)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"INSERT INTO TilesCharacteristic (TileId, TilePicture, IsMove, IsUse, IsChange, IsGate)
                                        VALUES (@TileId, @TilePicture, @IsMove, @IsUse, @IsChange, @IsGate)";
            command.Parameters.Add(new SqliteParameter("@TileId", id));
            command.Parameters.Add(new SqliteParameter("@TilePicture", bytes));
            command.Parameters.Add(new SqliteParameter("@IsMove", im));
            command.Parameters.Add(new SqliteParameter("@IsUse", iu));
            command.Parameters.Add(new SqliteParameter("@IsChange", ic));
            command.Parameters.Add(new SqliteParameter("@IsGate", ig));
            command.ExecuteNonQuery();

            connection.Close();
        }
    }
    void InsertToDataTilesImage(int OpId, int step, byte[] bytes)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"INSERT INTO TilesImage (OperationId, Step, Image)
                                        VALUES (@OperationId, @Step, @Image)";
            command.Parameters.Add(new SqliteParameter("@OperationId", OpId));
            command.Parameters.Add(new SqliteParameter("@Step", step));
            command.Parameters.Add(new SqliteParameter("@Image", bytes));
            command.ExecuteNonQuery();

            connection.Close();
        }
    }
    void InsertToDataTilesScripts(int id, int sid, byte[] bytes, string text)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = @"INSERT INTO TilesScripts (TileId, ScriptId, Image, Info)
                                        VALUES (@TileId, @ScriptId, @Image, @Info)";
            command.Parameters.Add(new SqliteParameter("@TileId", id));
            command.Parameters.Add(new SqliteParameter("@ScriptId", sid));
            command.Parameters.Add(new SqliteParameter("@Image", bytes));
            command.Parameters.Add(new SqliteParameter("@Info", text));
            command.ExecuteNonQuery();

            connection.Close();
        }
    }
    void Update()
    {
        
    }
}
