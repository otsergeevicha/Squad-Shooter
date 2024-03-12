using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class DropData
    {
        public DropableItemType dropType;

        public CurrencyType currencyType;
        public WeaponType cardType;

        public int amount;

        public DropData() { }

        public DropData Clone()
        {
            var data = new DropData();

            data.dropType = dropType;
            data.currencyType = currencyType;
            data.cardType = cardType;
            data.amount = amount;

            return data;
        }
    }
}