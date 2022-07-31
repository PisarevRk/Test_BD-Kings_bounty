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
            player.statusText.text = "�������� �����";
            player.doingState = Player.DoingState.MoveUp;
        }
        if (Input.GetKey(moveDown))
        {
            player.statusText.text = "�������� ����";
            player.doingState = Player.DoingState.MoveDown;
        }
        if (Input.GetKey(moveRight))
        {
            player.statusText.text = "�������� ������";
            player.doingState = Player.DoingState.MoveRight;
        }
        if (Input.GetKey(moveLeft))
        {
            player.statusText.text = "�������� �����";
            player.doingState = Player.DoingState.MoveLeft;
        }
        if (Input.GetKey(ChangeUp))
        {
            player.statusText.text = "������� ��� ����� ������";
            player.doingState = Player.DoingState.ChangeUp;
        }
        if (Input.GetKey(ChangeDown))
        {
            player.statusText.text = "������� ��� ����� �����";
            player.doingState = Player.DoingState.ChangeDown;
        }
        if (Input.GetKey(ChangeRight))
        {
            player.statusText.text = "������� ��� ����� ������";
            player.doingState = Player.DoingState.ChangeRight;
        }
        if (Input.GetKey(ChangeLeft))
        {
            player.statusText.text = "������� ��� ����� �����";
            player.doingState = Player.DoingState.ChangeLeft;
        }
    }
}
