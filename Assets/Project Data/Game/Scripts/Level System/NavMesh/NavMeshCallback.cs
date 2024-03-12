namespace Watermelon.LevelSystem
{
    public class NavMeshCallback : INavMeshAgent
    {
        private SimpleCallback onNavMeshInitialised;

        public NavMeshCallback(SimpleCallback onNavMeshInitialised)
        {
            this.onNavMeshInitialised = onNavMeshInitialised;
        }

        public void OnNavMeshUpdated()
        {
            onNavMeshInitialised?.Invoke();
        }
    }
}
