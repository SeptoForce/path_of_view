using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathOfView.GameLogic
{
    public class CharacterManager : MonoBehaviour
    {
        [SerializeField] private InputManager inputManager;
        [SerializeField] private GameObject level;
        private LevelManager _levelManager;

        [SerializeField] private LayerMask cubeLayerMask;

        private bool _isMoving = false;

        private void Awake()
        {
        }

        private void Start()
        {
            _levelManager = level.GetComponent<LevelManager>();
            inputManager.OnTap += OnTapHandler;
        }

        private Vector3 _targetPosition;

        private void OnTapHandler(Vector2 obj)
        {
            if(!_levelManager.enableClicking) return;
            Ray ray = Camera.main.ScreenPointToRay(obj);

            if (!Physics.Raycast(ray, out RaycastHit hit, 100f, cubeLayerMask)) return;

            if (hit.collider.CompareTag("Cube"))
            {
                _targetPosition = hit.collider.transform.position;

                if (_isMoving) return;
                if (_levelManager.FindPath(transform.position, _targetPosition, out List<Vector2Int> path))
                {
                    StartCoroutine(Move(path));
                }
            }
        }

        private IEnumerator Move(List<Vector2Int> path)
        {
            _levelManager.enableClicking = false;
            _levelManager.isMoving = true;
            _isMoving = true;

            var movementSpeed = 5f;

            foreach (Vector2Int node in path)
            {
                LevelCameraManager.CameraOrientation orientation = _levelManager.GetCameraOrientation();
                Vector3 targetPosition = Vector3.zero;

                var nodeIndex = path.IndexOf(node);
                if (nodeIndex == 0 || nodeIndex == path.Count - 1)
                {
                    movementSpeed = 5f;
                }
                else if (nodeIndex == 1 || nodeIndex == path.Count - 2)
                {
                    movementSpeed = 7f;
                }
                else
                {
                    movementSpeed = 10f;
                }

                switch (orientation)
                {
                    case LevelCameraManager.CameraOrientation.Right:
                    case LevelCameraManager.CameraOrientation.Left:
                        targetPosition = new Vector3(transform.position.x, node.y, node.x);
                        break;
                    case LevelCameraManager.CameraOrientation.Front:
                    case LevelCameraManager.CameraOrientation.Back:
                        targetPosition = new Vector3(node.x, node.y, transform.position.z);
                        break;
                    case LevelCameraManager.CameraOrientation.Top:
                    case LevelCameraManager.CameraOrientation.Bottom:
                        targetPosition = new Vector3(node.x, transform.position.y, node.y);
                        break;
                }

                while (transform.position != targetPosition)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition,
                        movementSpeed * Time.deltaTime);
                    yield return null;
                }
            }

            _isMoving = false;
            _levelManager.CheckIfGoalReached();
            _levelManager.isMoving = false;
        }

        private void OnDestroy()
        {
            inputManager.OnTap -= OnTapHandler;
        }
    }
}