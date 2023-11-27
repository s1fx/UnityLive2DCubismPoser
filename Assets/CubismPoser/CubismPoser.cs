#if UNITY_EDITOR
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using System.Threading;
using UnityEngine;

namespace SF.Cubism.Poser
{
    [RequireComponent(typeof(CubismModel))]
    [ExecuteInEditMode]
    public class CubismPoser : MonoBehaviour
    {
        private readonly CancellationTokenSource _cts = new();
        private CancellationToken _token;

        private CubismModel _model;
        private CubismUpdateController _updateController;
        private bool _initialized;

        public CubismModel Model => _model;
        public CancellationToken Token => _token;

        private void Awake()
        {
            _token = _cts.Token;

            if (!Application.isPlaying)
            {
                _model = GetComponent<CubismModel>();
                _updateController = GetComponent<CubismUpdateController>();                
                
                if (TryGetComponent<CubismParametersInspector>(out var cubismInspector))
                    DestroyImmediate(cubismInspector);

                if (!TryGetComponent(out _updateController))
                    _updateController = gameObject.AddComponent<CubismUpdateController>();

                CubismUpdater(false);

                _updateController.enabled = true;
                _initialized = true;
            }
            else
                Destroy(this);
        }

        private void Update()
        {
            if (_model != null)
                _model.ForceUpdateNow();
        }

        private void OnDestroy()
        {
            _cts.Cancel();
            _cts.Dispose();

            if (_initialized && gameObject != null)
            {
                CubismUpdater(true);
                gameObject.AddComponent<CubismParametersInspector>();
            }
        }

        private void CubismUpdater(bool state)
        {
            if (_model != null)
                _model.enabled = state;
        }
    }
}
#endif