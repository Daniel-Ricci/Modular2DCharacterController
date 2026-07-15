using UnityEngine;

namespace Modular2DCharacterController.SampleScenes.SimplePlatformer.Blocks_Platforms.Scripts
{
    public class BluePlatform : MonoBehaviour
    {
        [SerializeField]
        private Material blueGlowMaterial;
        [SerializeField]
        private Material blueFadeMaterial;

        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;
        private RedBlueState _redBlueState;

        private void OnEnable()
        {
            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            _redBlueState = FindAnyObjectByType<RedBlueState>();
            if (_redBlueState != null)
            {
                _redBlueState.OnStateChanged += UpdateState;
                UpdateState(_redBlueState.isBlue);
            }
        }

        private void UpdateState(bool isBlue)
        {
            if (isBlue)
            {
                _collider.enabled = true;
                _spriteRenderer.material = blueGlowMaterial;
            }
            else
            {
                _collider.enabled = false;
                _spriteRenderer.material = blueFadeMaterial;
            }
        }
    }
}
