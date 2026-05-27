using System.Collections.Generic;
using CaveTogether.Minigames;
using UnityEngine;

namespace CaveTogether.Game
{
    public class GameSceneHider : MonoBehaviour
    {
        private readonly List<Renderer> _renderers = new();
        private readonly List<Collider> _colliders = new();
        private readonly List<Canvas> _canvases = new();

        private void Start()
        {
            var mm = FindAnyObjectByType<MinigameManager>();
            mm.MinigameBegan += Hide;
            mm.MinigameEnded += Show;
        }

        private void OnDestroy()
        {
            var mm = FindAnyObjectByType<MinigameManager>();
            if (mm != null)
            {
                mm.MinigameBegan -= Hide;
                mm.MinigameEnded -= Show;
            }
        }

        private void Hide()
        {
            var thisScene = gameObject.scene;
            _renderers.Clear();
            _colliders.Clear();
            _canvases.Clear();

            foreach (var r in FindObjectsByType<Renderer>())
                if (r.gameObject.scene == thisScene) _renderers.Add(r);

            foreach (var col in FindObjectsByType<Collider>())
                if (col.gameObject.scene == thisScene) _colliders.Add(col);

            foreach (var c in FindObjectsByType<Canvas>())
                if (c.gameObject.scene == thisScene && c.renderMode == RenderMode.ScreenSpaceOverlay)
                    _canvases.Add(c);

            foreach (var r in _renderers) r.enabled = false;
            foreach (var col in _colliders) col.enabled = false;
            foreach (var c in _canvases) c.enabled = false;
        }

        private void Show()
        {
            foreach (var r in _renderers) if (r) r.enabled = true;
            foreach (var col in _colliders) if (col) col.enabled = true;
            foreach (var c in _canvases) if (c) c.enabled = true;
            _renderers.Clear();
            _colliders.Clear();
            _canvases.Clear();
        }
    }
}
