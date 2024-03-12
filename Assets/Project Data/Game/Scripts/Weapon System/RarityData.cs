using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class RarityData
    {
        [SerializeField] Rarity rarity;
        public Rarity Rarity => rarity;

        [SerializeField] string name;
        public string Name => name;

        [SerializeField] Color mainColor;
        public Color MainColor => mainColor;

        [SerializeField] Color textColor;
        public Color TextColor => textColor;
    }
}