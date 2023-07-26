using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PathOfView.GameLogic
{
    public class LevelTransition : MonoBehaviour
    {
        public int sceneIndex;
        public Animator fadeAnimator;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                StartCoroutine(TransitionToNextScene());
            }
        }

        private IEnumerator TransitionToNextScene()
        {
            fadeAnimator.SetTrigger("FadeOut");
            yield return new WaitForSeconds(fadeAnimator.GetCurrentAnimatorStateInfo(0).length);
            SceneManager.LoadScene(sceneIndex);
        }
    }
}
