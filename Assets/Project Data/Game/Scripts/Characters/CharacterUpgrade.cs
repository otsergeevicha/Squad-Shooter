using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterUpgrade
    {
        [SerializeField] CurrencyType currencyType;
        public CurrencyType CurrencyType => currencyType;

        [SerializeField] int price;
        public int Price => price;

        [Space]
        [SerializeField] CharacterStats stats;
        public CharacterStats Stats => stats;

        [Space]
        [SerializeField] bool changeStage;
        public bool ChangeStage => changeStage;

        [SerializeField] int stageIndex = -1;
        public int StageIndex => stageIndex;
    }
}