using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TimeFormat
{
    Full,//00:00:00
    FullWithString,//00h:00m:00
    Short,//00:00
    ShortWithString,//00m:00s
    ShortWithFullString,//00 minute : 00 second
    Day,
    IfShortThenMoreDetails,// 1 day, 0 day -> 23h
    DayHourThenMoreDetails,// 1 day 23h -> 23:59:59
    None
};
public enum TimerType
{
    CountDown,
    Elapse,
    None
};

public class Utils : MonoBehaviour
{
    public const int PercentBaseNum = 10000;
    public const int SecondPerday = 86400;

    public static bool IsPointInPolygon(Vector3[] polygon, Vector3 testPoint)
    {
        bool result = false;
        int j = polygon.Length - 1;
        for (int i = 0; i < polygon.Length; i++)
        {
            if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y || polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y)
            {
                if (polygon[i].x + (testPoint.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < testPoint.x)
                {
                    result = !result;
                }
            }
            j = i;
        }
        return result;
    }

    public static bool IsInPolygon(Vector3[] poly, Vector3 p)
    {
        Vector3 p1, p2;
        bool inside = false;

        if (poly.Length < 3)
        {
            return inside;
        }

        var oldVector3 = new Vector3(
            poly[poly.Length - 1].x, poly[poly.Length - 1].z);

        for (int i = 0; i < poly.Length; i++)
        {
            var newVector3 = new Vector3(poly[i].x, poly[i].z);
            if (newVector3.x > oldVector3.x)
            {
                p1 = oldVector3;
                p2 = newVector3;
            }
            else
            {
                p1 = newVector3;
                p2 = oldVector3;
            }
            if ((newVector3.x < p.x) == (p.x <= oldVector3.x)
                && (p.z - (long)p1.z) * (p2.x - p1.x)
                < (p2.z - (long)p1.z) * (p.x - p1.x))
            {
                inside = !inside;
            }
            oldVector3 = newVector3;
        }
        return inside;
    }


    public static void ResizeSpriteToScreen(GameObject theSprite, Camera theCamera, int fitToScreenWidth, int fitToScreenHeight)
    {
        SpriteRenderer sr = theSprite.GetComponent<SpriteRenderer>();
        theSprite.transform.localScale = new Vector3(1, 1, 1);

        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;

        float worldScreenHeight = (float)(theCamera.orthographicSize * 2.0);
        float worldScreenWidth = (float)(worldScreenHeight / Screen.height * Screen.width);

        if (fitToScreenWidth != 0)
        {
            Vector2 sizeX = new Vector2(worldScreenWidth / width / fitToScreenWidth, theSprite.transform.localScale.y);
            theSprite.transform.localScale = sizeX;
        }

        if (fitToScreenHeight != 0)
        {
            Vector2 sizeY = new Vector2(theSprite.transform.localScale.x, worldScreenHeight / height / fitToScreenHeight);
            theSprite.transform.localScale = sizeY;
        }
    }

    public static string GetTwoDigits(int value)
    {
        if (value == 0)
        {
            return "00";
        }
        else if (value < 10)
        {
            return "0" + value;
        }
        else
        {
            return value + "";
        }
    }
    public static string GetTwoDigits(long value)
    {
        if (value == 0)
        {
            return "00";
        }
        else if (value < 10)
        {
            return "0" + value;
        }
        else
        {
            return value + "";
        }
    }

    public static string ConvertSecondToStringFull(int time)
    {
        int second = time % 60;
        int minute = ((time % 3600) / 60);
        int hour = time / 3600;

        string timeFull = "";

        //if (Languages.s_currentLang == Languages.Lang.EN)
        if (true)
        {
            if (hour > 0)
                timeFull += hour.ToString() + (hour > 1 ? " hours " : " hour ");
            if (minute > 0)
                timeFull += minute.ToString() + (minute > 1 ? " mins " : " min ");
            if (second > 0)
                timeFull += second.ToString() + (second > 1 ? " secs " : " sec");
        }
        //else
        //{
        //    if (hour > 0)
        //        timeFull += hour.ToString() + " giờ ";
        //    if (minute > 0)
        //        timeFull += minute.ToString() + " phút ";
        //    if (second > 0)
        //        timeFull += second.ToString() + " giây ";
        //}
        return timeFull;
        //}
    }

    public static string ConvertSecondToStringFull(long time)
    {
        long second = time % 60;
        long minute = ((time % 3600) / 60);
        long hour = time / 3600;

        string timeFull = "";
        
        if (true)
        {
            if (hour > 0)
                timeFull += hour.ToString() + (hour > 1 ? " hours " : " hour ");
            if (minute > 0)
                timeFull += minute.ToString() + (minute > 1 ? " mins " : " min ");
            if (second > 0)
                timeFull += second.ToString() + (second > 1 ? " secs " : " sec");
        }
        //else
        //{
        //    if (hour > 0)
        //        timeFull += hour.ToString() + " giờ ";
        //    if (minute > 0)
        //        timeFull += minute.ToString() + " phút ";
        //    if (second > 0)
        //        timeFull += second.ToString() + " giây ";
        //}
        return timeFull;        
    }

    public static string ConvertSecondToStringShort(int time)
    {
        int second = time % 60;
        int minute = ((time % 3600) / 60);
        int hour = time / 3600;

        string timeFull = "";

        int count = 0;        
        {
            if (hour > 0)
            {
                timeFull += hour.ToString() + "H ";
                count++;
            }
            if (minute > 0)
            {
                timeFull += minute.ToString() + "M ";
                count++;
            }
            if (second > 0 && count <= 1)
            {
                timeFull += second.ToString() + "S";
            }
        }      
        return timeFull;        
    }

    public static string ConvertSecondToString(int time, TimeFormat type = TimeFormat.Full)
    {
        long second = (long)(time % 60);
        long minute = (long)((time % 3600) / 60);
        long hour = (long)time / 3600;
        if (type == TimeFormat.Day)
            return Mathf.CeilToInt((float)hour / 24f).ToString();
        else if (type == TimeFormat.IfShortThenMoreDetails)
        {
            if (hour < 24)
                return ConvertSecondToString(time, TimeFormat.ShortWithFullString);
            else
            {
                return Mathf.CeilToInt((float)hour / 24f).ToString() + " " + "day";
            }
        }
        else if (type == TimeFormat.DayHourThenMoreDetails)
        {
            if (hour < 24)
                return ConvertSecondToString(time);
            else
            {
                int day = Mathf.FloorToInt((float)hour / 24f);
                hour = hour % 24;

                return day + " " + (day > 1 ? "days" : "day") + " " + hour + " " + (hour > 1 ? "hours" : "hour");
            }
        }
        else if (type == TimeFormat.Short)
            return (GetTwoDigits(minute + hour * 60) + ":" + GetTwoDigits(second));
        else if (type == TimeFormat.ShortWithString)
            return (GetTwoDigits(minute + hour * 60) + " " + "min_short" + " " + GetTwoDigits(second) + " " + "sec_short");
        else if (type == TimeFormat.ShortWithFullString)
            return (GetTwoDigits(minute + hour * 60) + " " + "min" + " " + GetTwoDigits(second) + " " + "sec");
        else
            return (GetTwoDigits(hour) + ":" + GetTwoDigits(minute) + ":" + GetTwoDigits(second));
    }
    public static string ConvertTimeToStringDay(int time)
    {
        // 7 ngay 00:00:00
        int second = time % 60;
        int minute = ((time % 3600) / 60);
        int hour = time / 3600;
        int day = hour / 24;
        hour = hour % 24;
        if (day > 0)
            return day + " days" + " " + (GetTwoDigits(hour) + ":" + GetTwoDigits(minute) + ":" + GetTwoDigits(second));
        else
            return (GetTwoDigits(hour) + ":" + GetTwoDigits(minute) + ":" + GetTwoDigits(second));
    }

    public static Vector3 getAnchor(Sprite sprite)
    {
        if (sprite == null)
            return Vector3.zero;
        Bounds bounds = sprite.bounds;
        float pivotX = -bounds.center.x / bounds.extents.x / 2 + 0.5f;
        float pivotY = -bounds.center.y / bounds.extents.y / 2 + 0.5f;
        float pixelsToUnitsX = sprite.textureRect.width * (0.5f - pivotX);
        float pixelsToUnitsY = sprite.textureRect.height * (0.5f - pivotY);
        return new Vector3(pixelsToUnitsX, pixelsToUnitsY, 1f);
    }

    public static Vector3 WorldToUI(float x, float y, float z)
    {
        return new Vector3(x * 100f, y * 100f, z * 100f);
    }
    public static Vector3 UIToWorld(float x, float y, float z)
    {
        return new Vector3(x / 100f, y / 100f, z / 100f);
    }
    
    public static void MakeImageColorDark(Image image)
    {
        image.CrossFadeColor(new Color(.6f, .6f, .6f, 1f), .7f, false, false);
        Image[] childs = image.transform.GetComponentsInChildren<Image>();
        foreach (Image img in childs)
        {
            img.CrossFadeColor(new Color(.6f, .6f, .6f, 1f), .7f, false, false);
        }
    }

    public static void MakeImageColorNormal(Image image)
    {
        image.CrossFadeColor(new Color(1, 1, 1, 1), .7f, false, false);
        Image[] childs = image.transform.GetComponentsInChildren<Image>();
        foreach (Image img in childs)
        {
            img.CrossFadeColor(new Color(1, 1, 1, 1), .7f, false, false);
        }
    }



    public static string ConvertNumberToStringWithCommas(long value)
    {
        if (value < 1000) return value.ToString();
        string str = "";
        //var temp:Int = 0 ;
        while (true)
        {
            long temp = (value % 1000);
            //str =  + str;
            value = (long)(value / 1000);

            if (value > 0)
            {
                if (temp == 0)
                    str = "000" + str;
                else if (temp < 10)
                    str = "00" + temp + str;
                else if (temp < 100)
                    str = "0" + temp + str;
                else
                    str = temp + str;

            }
            else
            {
                str = temp + str;
            }

            if (value > 0)
                str = "," + str;
            else
                break;
        }
        return str;
    }

    public static string ConvertNumberToStringWithCommas(uint value)
    {
        if (value < 1000) return value.ToString();
        string str = "";
        //var temp:Int = 0 ;
        while (true)
        {
            long temp = (value % 1000);
            //str =  + str;
            value = (uint)(value / 1000);

            if (value > 0)
            {
                if (temp == 0)
                    str = "000" + str;
                else if (temp < 10)
                    str = "00" + temp + str;
                else if (temp < 100)
                    str = "0" + temp + str;
                else
                    str = temp + str;

            }
            else
            {
                str = temp + str;
            }

            if (value > 0)
                str = "," + str;
            else
                break;
        }
        return str;
    }

    public static float LevelGold(int gold)
    {

        float number = 1.0f;

        while (gold > 0)
        {
            gold /= 10;
            if (gold > 0) number += 0.4f;
        }


        return number;

    }

    public static void CleanChild(Transform transform)
    {
        Transform[] childs = transform.GetComponentsInChildren<Transform>();
        foreach (Transform trans in childs)
        {
            if (trans != transform)
                Destroy(trans.gameObject);
        }
    }


    public static bool LineIntersectsRect(Vector2 p1, Vector2 p2, Rect r)
    {
        return LineIntersectsLine(p1, p2, new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin)) ||
               LineIntersectsLine(p1, p2, new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax)) ||
               LineIntersectsLine(p1, p2, new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax)) ||
               LineIntersectsLine(p1, p2, new Vector2(r.xMin, r.yMax), new Vector2(r.xMin, r.yMin)) ||
               (r.Contains(p1) && r.Contains(p2));
    }

    private static bool LineIntersectsLine(Vector2 l1p1, Vector2 l1p2, Vector2 l2p1, Vector2 l2p2)
    {
        float q = (l1p1.y - l2p1.y) * (l2p2.x - l2p1.x) - (l1p1.x - l2p1.x) * (l2p2.y - l2p1.y);
        float d = (l1p2.x - l1p1.x) * (l2p2.y - l2p1.y) - (l1p2.y - l1p1.y) * (l2p2.x - l2p1.x);

        if (d == 0)
        {
            return false;
        }

        float r = q / d;

        q = (l1p1.y - l2p1.y) * (l1p2.x - l1p1.x) - (l1p1.x - l2p1.x) * (l1p2.y - l1p1.y);
        float s = q / d;

        if (r < 0 || r > 1 || s < 0 || s > 1)
        {
            return false;
        }

        return true;
    }

    public static double ConvertToUnixTimestamp(DateTime date)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        //Utils.debug("origin " + origin.ToString());
        //Utils.debug("date.ToUniversalTime() " + date.ToUniversalTime().ToString());
        TimeSpan diff = date.ToUniversalTime() - origin;
        //Utils.debug("diff " + Math.Floor(diff.TotalSeconds));
        return Math.Floor(diff.TotalSeconds);
    }  

    public static String GetNameStringEng(int month)
    {
        string name = string.Empty;
        switch (month)
        {
            case 1:
                name = "January"; break;
            case 2:
                name = "February"; break;
            case 3:
                name = "March"; break;
            case 4:
                name = "April"; break;
            case 5:
                name = "May"; break;
            case 6:
                name = "June"; break;
            case 7:
                name = "July"; break;
            case 8:
                name = "August"; break;
            case 9:
                name = "September"; break;
            case 10:
                name = "October"; break;
            case 11:
                name = "November"; break;
            case 12:
                name = "December"; break;
        }
        return name;
    }

    public static string ConvertNumberToStringWithCommas(int value)
    {
        if (value < 1000) return value.ToString();
        string str = "";
        string decimalSplit = ".";
        //if (Languages.s_currentLang == Languages.Lang.EN)
        //{
        //    decimalSplit = ".";
        //}
        //var temp:Int = 0 ;
        while (true)
        {
            int temp = (value % 1000);
            //str =  + str;
            value = (int)(value / 1000);

            if (value > 0)
            {
                if (temp == 0)
                    str = "000" + str;
                else if (temp < 10)
                    str = "00" + temp + str;
                else if (temp < 100)
                    str = "0" + temp + str;
                else
                    str = temp + str;

            }
            else
            {
                str = temp + str;
            }

            if (value > 0)
                str = decimalSplit + str;
            else
                break;
        }
        return str;
    }

    public static int RandomIndexWithArrayRate(int[] arr)
    {
        int idx = 0;
        int rateTotal = 0;
        foreach (int i in arr)
        {
            rateTotal += i;
        }
        int random = UnityEngine.Random.Range(1, rateTotal);
        rateTotal = 0;
        int count = 0;
        foreach (int i in arr)
        {
            rateTotal += i;
            if (random <= rateTotal)
            {
                idx = count;
                return idx;
            }

            count++;
        }
        return idx;
    }  
	
	public static Quaternion RotateFollowDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
        return Quaternion.AngleAxis(angle, Vector3.forward);
    }
	
	public static int GetEnumLength<T>()
    {
        return Enum.GetValues(typeof(T)).Length;
    }

    public static List<string> ParseDescriptionParam(string description)
    {
        List<string> listParam = new List<string>();

        description = description.Substring(description.IndexOf("{"));
        var splitData = description.Split("{");
        for (int i = 0; i < splitData.Length; i++)
        {
            var param = splitData[i].Split("}");
            if (!string.IsNullOrEmpty(param[0]))
            {
                listParam.Add(param[0]);
            }
        }
        return listParam;
    }


    public static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

    public static string ComputeValueColor(float newValue, float baseValue, string format, bool isReverse = false)
    {
        if (newValue > baseValue)
        {
            return !isReverse ? $"<color=green>{format}</color>" : $"<color=red>{format}</color>";
        }
        else if (newValue < baseValue)
        {
            return !isReverse ? $"<color=red>{format}</color>" : $"<color=green>{format}</color>";
        }
        else
        {
            return $"<color=white>{format}</color>";
        }
    }

}