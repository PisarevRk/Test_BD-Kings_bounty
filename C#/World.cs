using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    public float ticTime;
    private float ticTime1;

    public int week;
    public int day;
    public int hour;

    public int hourInDay;
    public int dayInWeek;

    public bool freezTime = false;

    public delegate void Events();
    public event Events onTic;
    public event Events onDay;
    public event Events onWeek;

    public Text hourText;
    public Text dayText;
    public Text weekText;

    public Image slider;

    public Sprite sliderSprite1;
    public Sprite sliderSprite2;
    public Sprite sliderSprite3;
    public Sprite sliderSprite4;
    public Sprite sliderSprite5;
    public Sprite sliderSprite6;
    public Sprite sliderSprite7;
    public Sprite sliderSprite8;
    public Sprite sliderSprite9;

    // Start is called before the first frame update
    void Start()
    {
        ticTime1 = ticTime;
        onTic += OnTic;

        hour = 0;
        day = 0;
        week = 0;
    }
    public void OnTic()
    {
        hour++;
        if (hour == hourInDay)
        {
            OnDay();
            hour = 0;
            day++;
        }
        if (day == dayInWeek)
        {
            OnWeek();
            day = 0;
            week++;
        }

        hourText.text = "Час " + hour.ToString();
        dayText.text = "День " + day.ToString();
        weekText.text = "Неделя " + week.ToString();
    }

    public void OnDay()
    {

    }
    public void OnWeek()
    {

    }
    void FixedUpdate()
    {
        if (!freezTime)
        {
            if (ticTime1 < ticTime / 8) slider.sprite = sliderSprite1;
            else if (ticTime1 > ticTime / 8 && ticTime1 < ticTime / 4) slider.sprite = sliderSprite2;
            else if (ticTime1 > ticTime / 4 && ticTime1 < (ticTime / 8) * 3) slider.sprite = sliderSprite3;
            else if (ticTime1 > (ticTime / 8) * 3 && ticTime1 < ticTime / 2) slider.sprite = sliderSprite4;
            else if (ticTime1 > ticTime / 2 && ticTime1 < (ticTime / 8) * 5) slider.sprite = sliderSprite5;
            else if (ticTime1 > (ticTime / 8) * 5 && ticTime1 < (ticTime / 4) * 3) slider.sprite = sliderSprite6;
            else if (ticTime1 > (ticTime / 4) * 3 && ticTime1 < (ticTime / 8) * 7) slider.sprite = sliderSprite7;
            else if (ticTime1 > (ticTime / 8) * 7 && ticTime1 < ticTime) slider.sprite = sliderSprite8;
            else if (ticTime1 > ticTime) slider.sprite = sliderSprite9;

            if (ticTime1 > 0) ticTime1 -= Time.deltaTime;
            else
            {
                ticTime1 = ticTime;
                onTic();
            }
        }
    }
}
