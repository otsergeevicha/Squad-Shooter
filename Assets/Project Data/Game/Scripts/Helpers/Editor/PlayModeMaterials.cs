using System.IO;
using System.Linq;
using UnityEditor;

namespace Watermelon.SquadShooter
{
    public class PlayModeMaterials : AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            if (EditorApplication.isPlaying)
            {
                return paths.Where(path => Path.GetExtension(path) != ".mat").ToArray();
            }
            else
            {
                return paths;
            }
        }
    }
}