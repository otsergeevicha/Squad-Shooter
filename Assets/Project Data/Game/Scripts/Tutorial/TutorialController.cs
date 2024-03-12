using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class TutorialController : MonoBehaviour
    {
        private static TutorialController tutorialController;
        private static List<ITutorial> registeredTutorials = new List<ITutorial>();

        [SerializeField] TutorialCanvasController tutorialCanvasController;
        [SerializeField] NavigationArrowController navigationArrowController;

        [Space]
        [SerializeField] GameObject labelPrefab;

        private static Pool labelPool;

        private static bool isTutorialSkipped;

        public void Initialise()
        {
            tutorialController = this;

            isTutorialSkipped = TutorialHelper.IsTutorialSkipped();

            // Create pools
            labelPool = new Pool(new PoolSettings(labelPrefab.name, labelPrefab, 0, true));

            navigationArrowController.Initialise();
            tutorialCanvasController.Initialise();
        }

        private void LateUpdate()
        {
            navigationArrowController.LateUpdate();
        }

        public static ITutorial GetTutorial(TutorialID tutorialID)
        {
            for(int i = 0; i < registeredTutorials.Count; i++)
            {
                if (registeredTutorials[i].TutorialID == tutorialID)
                {
                    if (!registeredTutorials[i].IsInitialised)
                        registeredTutorials[i].Initialise();

                    if (isTutorialSkipped)
                        registeredTutorials[i].FinishTutorial();

                    return registeredTutorials[i];
                }
            }

            return null;
        }

        public static void ActivateTutorial(ITutorial tutorial)
        {
            if (!tutorial.IsInitialised)
                tutorial.Initialise();

            if (isTutorialSkipped)
                tutorial.FinishTutorial();
        }

        public static void RegisterTutorial(ITutorial tutorial)
        {
            if (registeredTutorials.FindIndex(x => x == tutorial) != -1)
                return;

            // Add tutorial to list
            registeredTutorials.Add(tutorial);
        }

        public static void RemoveTutorial(ITutorial tutorial)
        {
            int tutorialIndex = registeredTutorials.FindIndex(x => x == tutorial);
            if (tutorialIndex != -1)
            {
                // Remove tutorial from list
                registeredTutorials.RemoveAt(tutorialIndex);
            }
        }

        public static TutorialLabelBehaviour CreateTutorialLabel(string text, Transform parentTransform, Vector3 offset)
        {
            GameObject labelObject = labelPool.GetPooledObject();
            labelObject.transform.position = parentTransform.position + offset;

            TutorialLabelBehaviour tutorialLabelBehaviour = labelObject.GetComponent<TutorialLabelBehaviour>();
            tutorialLabelBehaviour.Activate(text, parentTransform, offset);

            return tutorialLabelBehaviour;
        }

        public static void Unload()
        {
            labelPool.ReturnToPoolEverything(true);

            tutorialController.navigationArrowController.Unload();
        }
    }
}