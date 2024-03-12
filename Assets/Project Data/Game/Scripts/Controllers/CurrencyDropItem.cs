using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class CurrencyDropItem : IDropItem
    {
        public DropableItemType DropItemType => DropableItemType.Currency;

        private Currency[] availableCurrencies;

        public GameObject GetDropObject(DropData dropData)
        {
            CurrencyType currencyType = dropData.currencyType;
            for(int i = 0; i < availableCurrencies.Length; i++)
            {
                if(availableCurrencies[i].CurrencyType == currencyType)
                {
                    return availableCurrencies[i].Pool.GetPooledObject();
                }
            }

            return null;
        }

        public void SetCurrencies(Currency[] currencies)
        {
            availableCurrencies = currencies;
        }

        public void Initialise()
        {

        }

        public void Unload()
        {

        }
    }
}
