using System.Collections;
using CaveTogether.Game.Entities;
using CaveTogether.Services;
using UnityEngine;

namespace CaveTogether.Game
{
    public class GameSceneManager : MonoBehaviour
    {
        private IEnumerator Start()
        {
            CharacterManager cm = null;
            while (cm == null)
            {
                cm = FindAnyObjectByType<CharacterManager>();
                yield return null;
            }

            Character clientCharacter = null;
            while (clientCharacter == null)
            {
                clientCharacter = cm.GetClientCharacter();
                yield return null;
            }

            FindAnyObjectByType<CameraManager>().FocusOn(clientCharacter.transform.position);

            ServiceLocator.Get<TransitionService>().StartTransition(false);
        }
    }
}