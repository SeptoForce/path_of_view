using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;

namespace PathOfView.GameLogic
{
    public class LevelCameraManager : MonoBehaviour
    {
        [SerializeField] private InputManager inputManager;
        [SerializeField] private GameObject level;
        private float rotationSpeed = 0.2f;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private LevelManager _levelManager;
        private List<GameObject> _cubes;
        private Vector3 center;
        
        [SerializeField] private bool _enableSnapping = true;

        private Vector3 _orientationPoint;
        
        [SerializeField] private float snappingSpeed = 10f;

        private List<Vector3> _snapNodes = new List<Vector3>();
        
        public enum CameraOrientation
        {
            Right,
            Left,
            Back,
            Front,
            Top,
            Bottom
        }
        
        public CameraOrientation cameraOrientation = CameraOrientation.Right;

        private void Start()
        {
            center = CalculateCenterInLevel();
            transform.position = center;
            inputManager.OnDrag += OnDragHandler;
            inputManager.OnTouchLift += OnTouchLiftHandler;

            _orientationPoint = transform.position + transform.forward*3f;
            
            float distanceToCenter = Vector3.Distance(_orientationPoint, center);
            _snapNodes.Add(new Vector3(center.x+distanceToCenter, center.y, center.z));
            _snapNodes.Add(new Vector3(center.x-distanceToCenter, center.y, center.z));
            _snapNodes.Add(new Vector3(center.x, center.y, center.z+distanceToCenter));
            _snapNodes.Add(new Vector3(center.x, center.y, center.z-distanceToCenter));
            _snapNodes.Add(new Vector3(center.x, center.y+distanceToCenter, center.z));
            _snapNodes.Add(new Vector3(center.x, center.y-distanceToCenter, center.z));

            StartCoroutine(SnapCamera());
        }

        private void OnTouchLiftHandler()
        {
            _enableSnapping = true;
            bool enableClicking = _levelManager.enableClicking;
            StartCoroutine(SnapCamera());
            _levelManager.enableClicking = enableClicking;
        }
        
        private void OnDragHandler(Vector2 obj)
        {
            if (_levelManager.isMoving || !_levelManager.enableRotating)
            {
                return;
            }
            _levelManager.enableClicking = false;
            _enableSnapping = false;
            
            var localY = transform.up;
            var localX = transform.right;
            
            transform.Rotate(localY, obj.x * rotationSpeed, Space.World);
            transform.Rotate(localX, -obj.y * rotationSpeed, Space.World);
        }

        private void OnDestroy()
        {
            inputManager.OnDrag -= OnDragHandler;
            inputManager.OnTouchLift -= OnTouchLiftHandler;
        }

        private void FixedUpdate()
        {
            _orientationPoint = transform.position + transform.forward*3;
        }

        private IEnumerator SnapCamera()
        {
            Vector3 closestNode = _snapNodes[0];
            foreach (Vector3 node in _snapNodes)
            {
                if (Vector3.Distance(_orientationPoint, node) < Vector3.Distance(_orientationPoint, closestNode))
                {
                    closestNode = node;
                }
            }
            
            var rotX = transform.rotation.eulerAngles.x;
            var rotY = transform.rotation.eulerAngles.y;
            var rotZ = transform.rotation.eulerAngles.z;

            while (rotX > 180) rotX -= 360;
            while (rotY > 180) rotY -= 360;
            while (rotZ > 180) rotZ -= 360;

            rotX = (float) Math.Round(rotX / 90) * 90;
            rotY = (float) Math.Round(rotY / 90) * 90;
            rotZ = (float) Math.Round(rotZ / 90) * 90;
            
            Quaternion targetRotation = Quaternion.Euler(rotX, rotY, rotZ);

            
            //while transform.rotation and targetRotation difference is bigger than 0.1
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                if (!_enableSnapping)
                {
                    yield break;
                }
                
                Debug.Log("Snapping: " + Quaternion.Angle(transform.rotation, targetRotation));
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, snappingSpeed * Time.deltaTime);
                yield return null;
            }
            
            transform.rotation = targetRotation;
            
            SetCameraOrientation(_snapNodes.IndexOf(closestNode));
            _levelManager.CheckIfGoalReached();
            _levelManager.enableClicking = true;
            yield return null;
        }

        private void SetCameraOrientation(int nodeIndex)
        {
            cameraOrientation = (CameraOrientation) nodeIndex;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(CalculateCenterInLevel(), 0.5f);
            
            Gizmos.color = Color.green;
            foreach (Vector3 node in _snapNodes)
            {
                Gizmos.DrawSphere(node, 0.25f);
            }
            
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_orientationPoint, 0.5f);
        }

        private Vector3 CalculateCenterInLevel()
        {
            _cubes = new List<GameObject>();
            foreach (Transform child in level.transform)
            {
                if (child.CompareTag("Cube"))
                {
                    _cubes.Add(child.gameObject);
                }
            }

            //find center by looking at cubes minimal and maximal positions and calculating the center
            Vector3 min = _cubes[0].transform.position;
            Vector3 max = _cubes[0].transform.position;
            foreach (GameObject cube in _cubes)
            {
                min = Vector3.Min(min, cube.transform.position);
                max = Vector3.Max(max, cube.transform.position);
            }

            Vector3 center = (min + max) / 2;

            return center;
        }
        
        public void SetCameraOrthographicSize(float size)
        {
            StartCoroutine(SetCameraOrthographicSizeCoroutine(size));
        }
        
        private IEnumerator SetCameraOrthographicSizeCoroutine(float size)
        {
            float currentSize = virtualCamera.m_Lens.OrthographicSize;
            while (Math.Abs(currentSize - size) > 0.1f)
            {
                currentSize = Mathf.Lerp(currentSize, size, 10 * Time.deltaTime);
                virtualCamera.m_Lens.OrthographicSize = currentSize;
                yield return null;
            }
            virtualCamera.m_Lens.OrthographicSize = size;
            yield return null;
        }
    }
}