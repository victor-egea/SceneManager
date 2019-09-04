using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TGS.Lightning.UI
{
    public interface IWindowController
    {
        TopBarConfig TopBarConfig { get; }
        CanvasGroup CanvasGroup { get; }
        void Awake();
        IEnumerator TransitionIn();
        IEnumerator Initialize(WindowConfig windowConfig);
        IEnumerator Unload();
        IEnumerator TransitionOut();
        void OnDestroy();
        void OnBackButtonClicked();
    }
}