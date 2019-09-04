using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TGS.Lightning.UI
{
    public class WindowController : BaseWindowController<WindowConfig>
    {
        [SerializeField] private Animator _AnimatorController;

        private CustomStateBehaviour _testStateBehaviour;
        
        public override IEnumerator Initialize(WindowConfig windowConfig)
        {
            // Play the Animator
            if (_AnimatorController != null)
            {
                _testStateBehaviour = _AnimatorController.GetBehaviour<CustomStateBehaviour>();
                _AnimatorController.SetTrigger("TransitionIn");
            }
            
            // Initialize
            return base.Initialize(windowConfig);
        }

        public override IEnumerator TransitionIn()
        {
            if (_testStateBehaviour != null)
            {
                bool introFinished = false;
                _testStateBehaviour.OnStateExitCallBack += (animator) => { introFinished = true; };
                
                while (!introFinished)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            
            yield return base.TransitionIn();
        }
    }
}