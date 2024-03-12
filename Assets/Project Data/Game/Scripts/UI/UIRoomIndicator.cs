using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class UIRoomIndicator : MonoBehaviour
    {
        [SerializeField] GameObject roomCompleteIndicator;

        public void Init()
        {
            roomCompleteIndicator.SetActive(false);
        }

        public void SetAsReached()
        {
            roomCompleteIndicator.SetActive(true);
        }
    }
}