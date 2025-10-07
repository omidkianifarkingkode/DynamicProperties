using System;
using UnityEditor;
using UnityEngine;

internal static class PropertyDrawerUtil
{
    // ---------- Layout helpers ----------
    public static void SplitKeyValueRects(Rect position, out Rect keyRect, out Rect valueRect, float keyPortion = 1f / 3f, float pad = 5f)
    {
        float keyW = position.width * keyPortion;
        keyRect = new Rect(position.x, position.y, keyW, position.height);
        valueRect = new Rect(position.x + keyW + pad, position.y, position.width - keyW - pad, position.height);
    }

    public static (Rect text, Rect[] buttons) ReserveButtonsOnRight(Rect valueRect, params float[] widths)
    {
        float pad = 3f;
        float xMax = valueRect.xMax;
        var rects = new Rect[widths.Length];
        for (int i = widths.Length - 1; i >= 0; --i)
        {
            float w = widths[i];
            xMax -= (w + pad);
            rects[i] = new Rect(xMax + pad, valueRect.y, w, valueRect.height);
        }
        var text = new Rect(valueRect.x, valueRect.y, xMax - valueRect.x, valueRect.height);
        return (text, rects);
    }

    // ---------- Typed drawers (return the new value) ----------
    public static float DrawFloat(Rect r, float v, float? min, float? max, float? step)
    {
        v = (min.HasValue && max.HasValue) ? EditorGUI.Slider(r, v, min.Value, max.Value)
                                           : EditorGUI.FloatField(r, v);
        if (step.HasValue && step.Value > 0f)
        {
            v = Mathf.Round(v / step.Value) * step.Value;
        }
        return v;
    }

    public static int DrawInt(Rect r, int v, float? min, float? max, float? step)
    {
        if (min.HasValue && max.HasValue)
        {
            int iMin = Mathf.RoundToInt(min.Value);
            int iMax = Mathf.RoundToInt(max.Value);
            v = (iMax - iMin <= 1000) ? EditorGUI.IntSlider(r, v, iMin, iMax)
                                      : Mathf.Clamp(EditorGUI.IntField(r, v), iMin, iMax);
        }
        else v = EditorGUI.IntField(r, v);

        if (step.HasValue && step.Value > 0f)
            v = Mathf.RoundToInt(Mathf.Round(v / step.Value) * step.Value);
        return v;
    }

    public static bool DrawBool(Rect r, bool v) => EditorGUI.Toggle(r, v);

    public static int DrawEnum(Rect r, int raw, Type enumType)
    {
        if (enumType == null)
        {
            EditorGUI.HelpBox(r, "Enum type not defined!", MessageType.Warning);
            return raw;
        }

        bool isFlags = enumType.IsDefined(typeof(FlagsAttribute), false);
        if (isFlags)
        {
            int mask = EditorGUI.MaskField(r, raw, Enum.GetNames(enumType));
            // clamp to defined bits
            int all = 0;
            foreach (var v in Enum.GetValues(enumType))
                all |= Convert.ToInt32(v);
            return mask & all;
        }
        else
        {
            var names = Enum.GetNames(enumType);
            Array values = Enum.GetValues(enumType);
            int[] iv = new int[values.Length];
            for (int i = 0; i < iv.Length; i++) iv[i] = Convert.ToInt32(values.GetValue(i));
            return EditorGUI.IntPopup(r, raw, names, iv);
        }
    }

    // DateTime as ticks in long, UI "yyyy-MM-dd HH:mm:ss" + [-][+][Now]
    public static long DrawDateTimeTicks(Rect r, long ticks, float stepSeconds)
    {
        DateTime dt;
        try { dt = new DateTime(ticks, DateTimeKind.Utc); }
        catch { dt = DateTime.UnixEpoch; }

        var (textR, btns) = ReserveButtonsOnRight(r, 25f, 25f, 60f); // -, +, Now
        string text = dt.ToString("yyyy-MM-dd HH:mm:ss");
        EditorGUI.BeginChangeCheck();
        string newText = EditorGUI.DelayedTextField(textR, text);
        if (EditorGUI.EndChangeCheck() && DateTime.TryParse(newText, out var parsed))
            dt = DateTime.SpecifyKind(parsed, DateTimeKind.Utc).ToUniversalTime();

        if (GUI.Button(btns[0], "-")) dt = dt.AddSeconds(-Mathf.Max(0.0001f, stepSeconds));
        if (GUI.Button(btns[1], "+")) dt = dt.AddSeconds(Mathf.Max(0.0001f, stepSeconds));
        if (GUI.Button(btns[2], "Now")) dt = DateTime.UtcNow;

        return dt.Ticks;
    }

    // TimeSpan as ticks in long, UI "d.hh:mm:ss" or "hh:mm:ss" + [-][+]
    public static long DrawTimeSpanTicks(Rect r, long ticks, float stepSeconds, float? minSec, float? maxSec)
    {
        TimeSpan ts;
        try { ts = new TimeSpan(ticks); }
        catch { ts = TimeSpan.Zero; }

        var (textR, btns) = ReserveButtonsOnRight(r, 25f, 25f); // -, +
        string fmt = ts.Days != 0 ? @"d\.hh\:mm\:ss" : @"hh\:mm\:ss";
        string text = ts.ToString(fmt, System.Globalization.CultureInfo.InvariantCulture);

        EditorGUI.BeginChangeCheck();
        string newText = EditorGUI.DelayedTextField(textR, text);
        if (EditorGUI.EndChangeCheck() && TimeSpan.TryParse(newText, out var parsed))
            ts = parsed;

        if (GUI.Button(btns[0], "-")) ts -= TimeSpan.FromSeconds(stepSeconds);
        if (GUI.Button(btns[1], "+")) ts += TimeSpan.FromSeconds(stepSeconds);

        if (minSec.HasValue && maxSec.HasValue)
        {
            double s = Mathf.Clamp((float)ts.TotalSeconds, minSec.Value, maxSec.Value);
            ts = TimeSpan.FromSeconds(s);
        }

        return ts.Ticks;
    }
}
