using UnityEngine;

namespace Watermelon.SquadShooter
{
    public interface GridItem
    {
        void InitGridItem(int id);
        RectTransform GetRectTransform();
    }
}