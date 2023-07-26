using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PathOfView.GameLogic
{
    public class LevelManager : MonoBehaviour
    {
        private int _levelWidth;
        private int _levelHeight;
        private int _levelDepth;

        private List<bool[,]> _viewMaps;
        private List<GameObject> _cubes;

        private Vector3Int _offsetCube;
        
        private GameObject _goalCube;
        private GameObject _player;

        public bool isMoving = false;
        public bool enableRotating = true;
        public bool enableClicking = true;

        [SerializeField] private LevelCameraManager cameraManager;
        [SerializeField] private Material goalMaterial;

        private void Awake()
        {
            _cubes = new List<GameObject>();
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Cube"))
                {
                    _cubes.Add(child.gameObject);
                }
            }

            _offsetCube = new Vector3Int((int)_cubes[0].transform.position.x, (int)_cubes[0].transform.position.y,
                (int)_cubes[0].transform.position.z);
            foreach (GameObject cube in _cubes)
            {
                _offsetCube.x = (int)Math.Round(Mathf.Min(_offsetCube.x, cube.transform.position.x));
                _offsetCube.y = (int)Math.Round(Mathf.Min(_offsetCube.y, cube.transform.position.y));
                _offsetCube.z = (int)Math.Round(Mathf.Min(_offsetCube.z, cube.transform.position.z));
            }
            
            _goalCube = GameObject.Find("Goal");
            _player = GameObject.Find("Player");
        }

        private void Start()
        {
            // Calculate Level Size
            int maxHeight = (int)Math.Round(_cubes[0].transform.position.y);
            int maxWidth = (int)Math.Round(_cubes[0].transform.position.x);
            int maxDepth = (int)Math.Round(_cubes[0].transform.position.z);
            int minHeight = (int)Math.Round(_cubes[0].transform.position.y);
            int minWidth = (int)Math.Round(_cubes[0].transform.position.x);
            int minDepth = (int)Math.Round(_cubes[0].transform.position.z);
            foreach (GameObject cube in _cubes)
            {
                maxHeight = (maxHeight < (int)Math.Round(cube.transform.position.y))
                    ? (int)Math.Round(cube.transform.position.y)
                    : maxHeight;
                maxWidth = (maxWidth < (int)Math.Round(cube.transform.position.x))
                    ? (int)Math.Round(cube.transform.position.x)
                    : maxWidth;
                maxDepth = (maxDepth < (int)Math.Round(cube.transform.position.z))
                    ? (int)Math.Round(cube.transform.position.z)
                    : maxDepth;
                minHeight = (minHeight > (int)Math.Round(cube.transform.position.y))
                    ? (int)Math.Round(cube.transform.position.y)
                    : minHeight;
                minWidth = (minWidth > (int)Math.Round(cube.transform.position.x))
                    ? (int)Math.Round(cube.transform.position.x)
                    : minWidth;
                minDepth = (minDepth > (int)Math.Round(cube.transform.position.z))
                    ? (int)Math.Round(cube.transform.position.z)
                    : minDepth;
            }

            _levelWidth = maxWidth - minWidth + 1;
            _levelHeight = maxHeight - minHeight + 1;
            _levelDepth = maxDepth - minDepth + 1;

            // Set Camera Orthographic Size to biggest level dimension
            cameraManager.SetCameraOrthographicSize(Mathf.Max(_levelWidth, _levelHeight, _levelDepth) + 4);

            // Create View Maps
            _viewMaps = new List<bool[,]>();
            _viewMaps.Add(new bool[_levelDepth, _levelHeight]); // Right
            _viewMaps.Add(new bool[_levelDepth, _levelHeight]); // Left
            _viewMaps.Add(new bool[_levelWidth, _levelHeight]); // Back
            _viewMaps.Add(new bool[_levelWidth, _levelHeight]); // Front
            _viewMaps.Add(new bool[_levelWidth, _levelDepth]); // Top
            _viewMaps.Add(new bool[_levelWidth, _levelDepth]); // Bottom

            // Fill View Maps
            foreach (GameObject cube in _cubes)
            {
                int x = (int)Math.Round(cube.transform.position.x) - minWidth;
                int y = (int)Math.Round(cube.transform.position.y) - minHeight;
                int z = (int)Math.Round(cube.transform.position.z) - minDepth;
                _viewMaps[0][z, y] = true;
                _viewMaps[1][z, y] = true;
                _viewMaps[2][x, y] = true;
                _viewMaps[3][x, y] = true;
                _viewMaps[4][x, z] = true;
                _viewMaps[5][x, z] = true;
            }
        }

        public bool CheckIfGoalReached()
        {
            enableRotating = false;
            LevelCameraManager.CameraOrientation _cameraOrientation = cameraManager.cameraOrientation;
            Vector2Int goalPosition = new Vector2Int();
            Vector2Int playerPosition = new Vector2Int();
            switch (_cameraOrientation)
            {
                case LevelCameraManager.CameraOrientation.Right:
                case LevelCameraManager.CameraOrientation.Left:
                    goalPosition = new Vector2Int((int)Math.Round(_goalCube.transform.position.z),
                        (int)Math.Round(_goalCube.transform.position.y));
                    playerPosition = new Vector2Int((int)Math.Round(_player.transform.position.z),
                        (int)Math.Round(_player.transform.position.y));
                    break;
                case LevelCameraManager.CameraOrientation.Front:
                case LevelCameraManager.CameraOrientation.Back:
                    goalPosition = new Vector2Int((int)Math.Round(_goalCube.transform.position.x),
                        (int)Math.Round(_goalCube.transform.position.y));
                    playerPosition = new Vector2Int((int)Math.Round(_player.transform.position.x),
                        (int)Math.Round(_player.transform.position.y));
                    break;
                case LevelCameraManager.CameraOrientation.Top:
                case LevelCameraManager.CameraOrientation.Bottom:
                    goalPosition = new Vector2Int((int)Math.Round(_goalCube.transform.position.x),
                        (int)Math.Round(_goalCube.transform.position.z));
                    playerPosition = new Vector2Int((int)Math.Round(_player.transform.position.x),
                        (int)Math.Round(_player.transform.position.z));
                    break;
            }

            if (goalPosition == playerPosition)
            {
                StartCoroutine(TransitionToNextScene());
                enableClicking = false;
                return true;
            }
            enableRotating = true;
            enableClicking = true;
            return false;
        }

        public Animator fadeAnimator;
        private IEnumerator TransitionToNextScene()
        {
            fadeAnimator.SetTrigger("FadeOut");
            yield return new WaitForSeconds(fadeAnimator.GetCurrentAnimatorStateInfo(0).length);
            int nextSceneIndex = (SceneManager.sceneCountInBuildSettings - 1 == SceneManager.GetActiveScene().buildIndex)
                ? 0
                : SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(nextSceneIndex);
        }

        public LevelCameraManager.CameraOrientation GetCameraOrientation()
        {
            return cameraManager.cameraOrientation;
        }

        public bool[,] GetCurrentViewMap()
        {
            return _viewMaps[(int)cameraManager.cameraOrientation];
        }

        public bool FindPath(Vector3 currentPosition, Vector3 targetPosition, out List<Vector2Int> path)
        {
            bool[,] viewMap = GetCurrentViewMap();

            Vector2Int currentPos = new Vector2Int();
            Vector2Int targetPos = new Vector2Int();
            Vector2Int offset = new Vector2Int();

            switch (cameraManager.cameraOrientation)
            {
                case LevelCameraManager.CameraOrientation.Right:
                case LevelCameraManager.CameraOrientation.Left:
                    currentPos.x = (int)Math.Round(currentPosition.z);
                    currentPos.y = (int)Math.Round(currentPosition.y);
                    targetPos.x = (int)Math.Round(targetPosition.z);
                    targetPos.y = (int)Math.Round(targetPosition.y);
                    offset.x = (int)_offsetCube.z;
                    offset.y = (int)_offsetCube.y;
                    break;
                case LevelCameraManager.CameraOrientation.Front:
                case LevelCameraManager.CameraOrientation.Back:
                    currentPos.x = (int)Math.Round(currentPosition.x);
                    currentPos.y = (int)Math.Round(currentPosition.y);
                    targetPos.x = (int)Math.Round(targetPosition.x);
                    targetPos.y = (int)Math.Round(targetPosition.y);
                    offset.x = (int)_offsetCube.x;
                    offset.y = (int)_offsetCube.y;
                    break;
                case LevelCameraManager.CameraOrientation.Top:
                case LevelCameraManager.CameraOrientation.Bottom:
                    currentPos.x = (int)Math.Round(currentPosition.x);
                    currentPos.y = (int)Math.Round(currentPosition.z);
                    targetPos.x = (int)Math.Round(targetPosition.x);
                    targetPos.y = (int)Math.Round(targetPosition.z);
                    offset.x = (int)_offsetCube.x;
                    offset.y = (int)_offsetCube.z;
                    break;
            }

            path = Pathfinder.FindPath(currentPos, targetPos, viewMap, offset);

            return path.Count > 0;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(_offsetCube, Vector3.one * 0.5f);
            
        }
    }
}