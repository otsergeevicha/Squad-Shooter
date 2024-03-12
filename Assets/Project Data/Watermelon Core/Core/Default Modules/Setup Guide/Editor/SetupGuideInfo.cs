using UnityEngine;

namespace Watermelon
{
    [SetupTab("Info", texture = "icon_info", priority = -1)]
    [CreateAssetMenu(fileName = "Setup Guide Info", menuName = "Settings/Editor/Setup Guide Info")]
    [HelpURL("https://docs.google.com/document/d/1gLEKKtJkUQpD056Ei2Yla_0HYlcdx2Yny1_otKuFqW8")]
    public class SetupGuideInfo : ScriptableObject
    {
        public string gameName = "ProjectName";
        public string documentationURL = "#";

        public SetupButtonWindow[] windowButtons;
        public SetupButtonFolder[] folderButtons;
        public SetupButtonFile[] fileButtons;
    }
}

// -----------------
// Setup Guide v 1.0.2
// -----------------