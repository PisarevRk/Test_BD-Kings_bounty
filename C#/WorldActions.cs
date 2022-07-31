using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.Sqlite;
using System.Data;

public class WorldActions : MonoBehaviour
{
    private string dbName = "URI=file:Map.s3db";

    public GameObject UiPanel;
    public Image EventImage;
    public Text EventText;

    public Player player;
    
    void Start()
    {
        UiPanel.SetActive(false);
    }

    public void Cave(int id)
    {
        // ScriptId - 0

        player.freezPlayer = true;
        UiPanel.SetActive(true);

        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM TilesScripts WHERE ScriptId = " + id;
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader["Image"] != DBNull.Value)
                    {
                        var tex = new Texture2D(1, 1);
                        tex.LoadImage((byte[])reader["Image"]);
                        EventImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));

                        EventText.text = reader["Info"].ToString();
                    }
                    reader.Close();
                }
            }
            connection.Close();
        }
    }
    public void OK()
    {
        player.freezPlayer = false;
        UiPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
