using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TGS.Lightning.UI
{
    public class WindowConfig
    {
        public string _SceneName;
        public bool _IsAdditive = false;
    }
    
    [RequireComponent(typeof(CanvasGroup))]
    public class BaseWindowController<T> : MonoBehaviour, IWindowController where T : WindowConfig
    {
        [SerializeField] private TopBarConfig _TopBarConfig;
        [SerializeField] protected CanvasGroup _CanvasGroup;
        protected T _Config;

        public TopBarConfig TopBarConfig
        {
            get { return _TopBarConfig; }
        }

        public CanvasGroup CanvasGroup
        {
            get { return _CanvasGroup; }
        }

        public virtual void Awake()
        {
            if (_CanvasGroup == null)
            {
                _CanvasGroup = gameObject.GetComponent<CanvasGroup>();
            }

            _CanvasGroup.interactable = false;

            // TODO (victor): Maybe we need to hide the screen, but I don't know.
            UnityEngine.Debug.LogError("Awake");
        }

        public virtual IEnumerator TransitionIn()
        {
            // We enable interactability after the scene has finished loading the data.
            _CanvasGroup.interactable = true;

            UnityEngine.Debug.LogError("Transition In");
            yield return null;
        }

        public virtual IEnumerator Initialize(WindowConfig windowConfig)
        {
            _Config = windowConfig as T;

            UnityEngine.Debug.LogError("Initialize");
            yield return null;
        }

        public virtual IEnumerator Unload()
        {
            // We disable interactability, just in case.
            _CanvasGroup.interactable = false;

            UnityEngine.Debug.LogError("Unload");
            yield return null;
        }

        public virtual IEnumerator TransitionOut()
        {
            UnityEngine.Debug.LogError("Transition Out");
            yield return null;
        }

        public virtual void OnDestroy()
        {
            UnityEngine.Debug.LogError("Destroy");
        }

        public virtual void OnBackButtonClicked()
        {
            TGSSceneManager.Instance.CloseScene();
        }
        
    }
}