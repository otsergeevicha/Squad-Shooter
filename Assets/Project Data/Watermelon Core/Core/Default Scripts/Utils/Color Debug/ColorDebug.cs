using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon
{



    public static class ColorDebug
    {
        public static void Log(ColoredText extendedFormatter)
        {
            Debug.Log(extendedFormatter.text);
        }

        #region Overrides Color param
        public static void Log(Color color, string value)
        {
            Debug.Log(new ColoredText(color, value).text);
        }

        public static void Log(Color color, int value)
        {
            Debug.Log(new ColoredText(color, value.ToString()).text);
        }

        public static void Log(Color color, string stringLabel, int value)
        {
            ColoredText log = new ColoredText(color, stringLabel + " = ").Append(value);
            Debug.Log(log.text);
        }

        #endregion

        #region Overrides CustomColor param
        public static void Log(CustomColor color, string value)
        {
            Debug.Log(new ColoredText(color, value).text);
        }

        public static void Log(CustomColor color, string stringLabel, int value)
        {
            ColoredText log = new ColoredText(color, stringLabel + " = ").Append(value);
            Debug.Log(log.text);
        }
        #endregion

    }

    public class ColoredText
    {
        public string text;

        public ColoredText(Color color, string text)
        {
            this.text = text;

            ApllyColor(color);
        }

        public ColoredText(string hashCode, string text)
        {
            this.text = text;

            ApllyColor(hashCode);
        }

        public ColoredText(CustomColor colorEnum, string text)
        {
            this.text = text;

            ApllyColor(ColorDebugSettings.GetColorByEnum(colorEnum));
        }

        // Styles
        public ColoredText MakeBold()
        {
            text = $"<b>{text}</b>";

            return this;
        }

        public ColoredText MakeItalic()
        {
            text = $"<i>{text}</i>";

            return this;
        }

        // Register

        public ColoredText ToUpper()
        {
            text = text.ToUpper();
            return this;
        }

        public ColoredText ToLower()
        {
            text = text.ToLower();
            return this;
        }

        // Appends
        public ColoredText Append(int value)
        {
            text += value.ToString();

            return this;
        }

        public ColoredText Append(float value)
        {
            text += value.ToString();

            return this;
        }

        public ColoredText Append(double value)
        {
            text += value.ToString();

            return this;
        }

        public ColoredText Append(string value)
        {
            text += value;

            return this;
        }

        public ColoredText Append(Vector3 value)
        {
            text += value;

            return this;
        }

        public ColoredText Append(ColoredText coloredText)
        {
            text += coloredText.text;

            return this;
        }

        private void ApllyColor(Color color)
        {
            text = $"<color={color.ToHex()}>{text}</color>";
        }

        private void ApllyColor(string hashCode)
        {
            text = $"<color={hashCode}>{text}</color>";
        }
    }
}