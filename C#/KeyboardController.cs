using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : MonoBehaviour
{
    public Player player;

    public KeyCode moveUp;
    public KeyCode moveDown;
    public KeyCode moveRight;
    public KeyCode moveLeft;
    public KeyCode ChangeUp;
    public KeyCode ChangeDown;
    public KeyCode ChangeRight;
    public KeyCode ChangeLeft;

    void Update()
    {
        if (Input.GetKey(moveUp))
        {
            player.statusText.text = "Двигаюсь вверх";
            player.doingState = Player.DoingState.MoveUp;
        }
        if (Input.GetKey(moveDown))
        {
            player.statusText.text = "Двигаюсь вниз";
            player.doingState = Player.DoingState.MoveDown;
        }
        if (Input.GetKey(moveRight))
        {
            player.statusText.text = "Двигаюсь вправо";
            player.doingState = Player.DoingState.MoveRight;
        }
        if (Input.GetKey(moveLeft))
        {
            player.statusText.text = "Двигаюсь влево";
            player.doingState = Player.DoingState.MoveLeft;
        }
        if (Input.GetKey(ChangeUp))
        {
            player.statusText.text = "Работаю над целью сверху";
            player.doingState = Player.DoingState.ChangeUp;
        }
        if (Input.GetKey(ChangeDown))
        {
            player.statusText.text = "Работаю над целью снизу";
            player.doingState = Player.DoingState.ChangeDown;
        }
        if (Input.GetKey(ChangeRight))
        {
            player.statusText.text = "Работаю над целью справа";
            player.doingState = Player.DoingState.ChangeRight;
        }
        if (Input.GetKey(ChangeLeft))
        {
            player.statusText.text = "Работаю над целью слева";
            player.doingState = Player.DoingState.ChangeLeft;
        }
    }
}
