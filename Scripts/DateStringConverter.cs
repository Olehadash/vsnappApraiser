using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DateStringConverter
{
    private static System.Text.StringBuilder stringBuilder;

    static DateStringConverter()
    {
        stringBuilder = new System.Text.StringBuilder(50);
    }

    public static string GetDMYHMDate()
    {
        stringBuilder.Clear();
        stringBuilder.
    Append(System.DateTime.Now.Day).Append("/").
    Append(System.DateTime.Now.Month).Append("/").
    Append(System.DateTime.Now.Year).Append(" - ").
    Append(System.DateTime.Now.Hour).Append(":").
    Append(System.DateTime.Now.Minute);
        return stringBuilder.ToString();
    }

    public static string GetMDHMSMDate()
    {
        stringBuilder.Clear();
        stringBuilder.
    Append(System.DateTime.Now.Month).Append("-").
    Append(System.DateTime.Now.Day).Append("-").
    Append(System.DateTime.Now.Hour).Append("-").
    Append(System.DateTime.Now.Minute).Append("-").
    Append(System.DateTime.Now.Second).Append("-").
    Append(System.DateTime.Now.Millisecond);
        return stringBuilder.ToString();
    }

    public static string FromRawDateToParsedDate(string rawDate)
    {
        string[] date = rawDate.Split('-');
        string[] time = date[1].Split(':');

        int hours = int.Parse(time[0]);
        time[0] = hours < 10 ? "0" + hours : hours.ToString();

        int minutes = int.Parse(time[1]);
        time[1] = minutes < 10 ? "0" + minutes : minutes.ToString();

        return date[0] + " - " + time[0] + ":" + time[1];
    }
}
