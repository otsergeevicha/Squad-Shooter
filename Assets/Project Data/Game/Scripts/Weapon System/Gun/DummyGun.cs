namespace Watermelon.SquadShooter
{
    public class DummyGun : BaseGunBehavior
    {
        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            damage = new DuoInt(0, 0);
        }

        public override void OnGunUnloaded()
        {

        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {

        }

        public override void RecalculateDamage()
        {
            damage = new DuoInt(0, 0);
        }

        public override void Reload()
        {

        }

        public override void SetGraphicsState(bool state)
        {

        }
    }
}