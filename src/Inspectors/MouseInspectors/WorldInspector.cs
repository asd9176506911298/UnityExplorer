using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityExplorer.UI;
using UnityExplorer.UI.Panels;

namespace UnityExplorer.Inspectors.MouseInspectors
{
    public class WorldInspector : MouseInspectorBase
    {
        private static Camera MainCamera;
        public static readonly List<GameObject> LastHitObjects = new();
        private static readonly List<GameObject> currentHitObjects = new();

        public override void OnBeginMouseInspect()
        {
            foreach (var camera in Camera.allCameras)
            {
                if (camera.name == "SceneCamera")
                    MainCamera = camera;
            }

            if (!MainCamera)
            {
                ExplorerCore.LogWarning("No MainCamera found! Cannot inspect world!");
                return;
            }
        }

        public override void ClearHitData()
        {
            currentHitObjects.Clear();
        }

        public override void OnSelectMouseInspect()
        {
            LastHitObjects.Clear();
            LastHitObjects.AddRange(currentHitObjects);
            RuntimeHelper.StartCoroutine(SetPanelActiveCoro());
        }

        IEnumerator SetPanelActiveCoro()
        {
            yield return null;
            WorldInspectorResultsPanel panel = UIManager.GetPanel<WorldInspectorResultsPanel>(UIManager.Panels.WorldInspectorResults);
            panel.SetActive(true);
            panel.ShowResults();
        }

        public override void UpdateMouseInspect(Vector2 mousePos)
        {
            currentHitObjects.Clear();

            if (!MainCamera)
            {
                foreach (var camera in Camera.allCameras)
                {
                    if (camera.name == "SceneCamera")
                        MainCamera = camera;
                }
            }

            if (!MainCamera)
            {
                ExplorerCore.LogWarning("No Main Camera was found, unable to inspect world!");
                MouseInspector.Instance.StopInspect();
                return;
            }

            Ray ray = MainCamera.ScreenPointToRay(mousePos);
            var tmp = new Vector3(ray.origin.x, ray.origin.y, 0f);
            ray.origin = tmp;

            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, Physics2D.DefaultRaycastLayers);

            if (hits.Length > 0)
            {   
                foreach (var hit in hits)
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.gameObject)
                        {
                            currentHitObjects.Add(hit.collider.gameObject);
                            ExplorerCore.LogWarning(hit.collider.gameObject);
                        }
                    }
                }
            }
            
            OnHitGameObject();
        }

        internal void OnHitGameObject()
        {
            if (currentHitObjects.Any())
                MouseInspector.Instance.objNameLabel.text = $"Click to view World Objects under mouse: {currentHitObjects.Count}";
            else
                MouseInspector.Instance.objNameLabel.text = $"No World objects under mouse.";
        }

        public override void OnEndInspect()
        {
            //currentHitObjects.Clear();
            //LastHitObjects.Clear();
        }
    }
}
