using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon {
    [CreateAssetMenu(fileName = "Color Debug Settings", menuName = "Settings/Color Debug Settings")]
    [HelpURL("https://docs.google.com/document/d/1K2vJIKtMALoe5-TmcHHlAb4aR2HRFc7msFRkgItrH90/edit?usp=sharing")]
    public partial class ColorDebugSettings : ScriptableObject
    {
        private static ColorDebugSettings instance;

        [SerializeField] ColorInfo[] colorInfos;

        public void Init()
        {
            instance = this;

            ColorDebug.Log(new ColoredText("#FFFEB7", "ColorDebug => ").MakeBold().Append("Inited"));
        }  
        
        public static Color GetColorByEnum(CustomColor color)
        {
            if (instance.colorInfos == null && instance.colorInfos.Length == 0)
                return Color.black; // By default

            return instance.colorInfos[(int)color].colorValue;
        }


        [System.Serializable]
        private class ColorInfo
        {
            public CustomColor colorEnum; //key can be used as index in settingInfos array
            public Color colorValue;           
        }
    }   


   
}