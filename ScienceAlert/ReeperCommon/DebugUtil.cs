using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.IO;

namespace ReeperCommon
{
    class PositionSentinel : MonoBehaviour
    {
        public Transform target;

        void LateUpdate()
        {
            if (target == null) return;

            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }

    class LineSentinel : MonoBehaviour
    {
        private List<Transform> targets = new List<Transform>();
        LineRenderer lrender;

        void LateUpdate()
        {
            for (int i = 0; i < targets.Count; ++i)
                lrender.SetPosition(i, targets[i].position);
        }

        public List<Transform> Targets
        {
            set
            {
                if (value != null)
                {
                    targets = value;
                }
                else targets = new List<Transform>();

                lrender = renderer as LineRenderer;
                lrender.SetVertexCount(targets.Count);
                LateUpdate();

            }
        }
    }


    public static class DebugVisualizer
    {


        public static Texture2D Capture(this UnityEngine.Camera cam)
        {
            var oldTarget = cam.targetTexture;
            var oldRt = RenderTexture.active;

            RenderTexture temp = RenderTexture.GetTemporary(Screen.width, Screen.height);
            Texture2D texture = new Texture2D(temp.width, temp.height);

            cam.targetTexture = temp;
            cam.Render();
            cam.targetTexture = oldTarget;

            RenderTexture.active = temp;
            texture.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
            RenderTexture.active = oldRt;

            RenderTexture.ReleaseTemporary(temp);

            return texture;
        }

        public static GameObject Point(Transform target, Color color, float scale = 1f, UnityEngine.PrimitiveType type = PrimitiveType.Sphere)
        {
            var go = Point(target, scale, type);
            go.renderer.material.color = color;
            return go;
        }



        public static GameObject Point(Transform target, float scale = 1f, UnityEngine.PrimitiveType type = PrimitiveType.Sphere)
        {
            var go = GameObject.CreatePrimitive(type);
            PartLoader.StripComponent<Rigidbody>(go);
            PartLoader.StripComponent<Collider>(go);

            go.transform.localScale = Vector3.one * scale;
            var sentinel = go.AddComponent<PositionSentinel>();
            sentinel.target = target;

            go.SetActive(true);

            return go;
        }



        public static GameObject Point(Vector3 position, float scale = 1f, UnityEngine.PrimitiveType type = PrimitiveType.Sphere)
        {
            var go = GameObject.CreatePrimitive(type);
            PartLoader.StripComponent<Rigidbody>(go);
            PartLoader.StripComponent<Collider>(go);

            go.transform.localScale = Vector3.one * scale;
            go.transform.position = position;
            go.SetActive(true);

            return go;
        }



        public static GameObject Line(List<Transform> targets, float startWidth, float endWidth, Color start, Color end)
        {
            var go = new GameObject("DebugVisualizer.Line", typeof(LineRenderer));

            var lsentinel = go.AddComponent<LineSentinel>();
            lsentinel.Targets = new List<Transform>(targets);
            var lrender = lsentinel.renderer as LineRenderer;

            lrender.SetColors(start, end);
            lrender.SetWidth(startWidth, endWidth);
            lrender.useWorldSpace = true;
            lrender.material = new Material(Shader.Find("Particles/Additive"));

            go.SetActive(true);

            return go;
        }



        public static GameObject Line(List<GameObject> targets, float startWidth, float endWidth, Color start, Color end)
        {
            var list = new List<Transform>();
            targets.ForEach(go => list.Add(go.transform));
            return Line(list, startWidth, endWidth, start, end);
        }



        public static GameObject Line(List<Transform> targets, float startWidth, float endWidth)
        {
            return Line(targets, startWidth, endWidth, Color.white, Color.white);
        }


        public static GameObject Line(Vector3 first, Vector3 second, float startWidth, float endWidth, Color start, Color end)
        {
            GameObject firstVertex = new GameObject();
            GameObject secondVertex = new GameObject();

            firstVertex.transform.position = first;
            secondVertex.transform.position = second;

            return Line(new List<Transform> { firstVertex.transform, secondVertex.transform }, startWidth, endWidth, start, end);

        }
        public static GameObject Line(Vector3 first, Vector3 second, float startWidth, float endWidth)
        {
            return Line(first, second, startWidth, endWidth, Color.white, Color.white);
        }



        public static List<GameObject> Line(List<Vector3> points, float startWidth, float endWidth)
        {
            return Line(points, startWidth, endWidth, Color.white, Color.white);
        }



        public static List<GameObject> Line(List<Vector3> points, float startWidth, float endWidth, Color start, Color end)
        {
            if (points.Count < 2)
            {
                Log.Error("DebugVisualizer.Line: supplied list of points, but only one point in list!");
                return new List<GameObject>();
            }

            var list = new List<GameObject>();

            for (int i = 0; i + 1< points.Count; ++i)
            {
                var go = Line(points[i], points[i + 1], startWidth, endWidth, start, end);
                list.Add(go);
            }

            return list;
        }
    }

    public delegate void GameObjectVisitor(GameObject go, int indent);

    public static class DebugExtensions
    {
        //public static void PrintHierarchy(this UnityEngine.Transform t, int indent = 0)
        //{
        //    Log.Normal("{0}Transform: {1}", indent > 0 ? new string('-', indent) + ">" : "", t.name);

        //    for (int i = 0; i < t.childCount; ++i)
        //        t.GetChild(i).PrintHierarchy(indent);
        //}
        private static void internal_PrintHierarchy(GameObject go, int indent)
        {
            Log.Debug("{0} Transform: {1}", indent > 0 ? new string('-', indent) + ">" : "", go.transform.name);
        }

        private static void internal_PrintComponents(GameObject go, int indent)
        {
            Log.Debug("{0}{1} has components:", indent > 0 ? new string('-', indent) + ">" : "", go.name);

            var components = go.GetComponents<Component>();
            foreach (var c in components)
                Log.Debug("{0}: {1}", new string('.', indent + 3) + "c", c.GetType().FullName);
        }

        private static void internal_PrintStatus(GameObject go, int indent)
        {
            internal_PrintHierarchy(go, indent);
            Log.Debug("{0}  :status: {1}", indent > 0 ? new string('-', indent) + ">" : "", go.activeSelf ? "active" : "inactive");
        }

        public static void PrintHierarchyStatus(this UnityEngine.GameObject go)
        {
            go.TraverseHierarchy(internal_PrintStatus);
        }

        public static void PrintHierarchy(this UnityEngine.Transform t)
        {
            t.gameObject.TraverseHierarchy(internal_PrintHierarchy);
        }

        public static void PrintComponents(this UnityEngine.GameObject go)
        {
            go.TraverseHierarchy(internal_PrintComponents);
        }

        public static void TraverseHierarchy(this UnityEngine.GameObject go, GameObjectVisitor visitor, int indent = 0)
        {
            visitor(go, indent);

            for (int i = 0; i < go.transform.childCount; ++i)
                go.transform.GetChild(i).gameObject.TraverseHierarchy(visitor, indent + 3);
        }

        public static List<T> FindComponents<T>(this UnityEngine.GameObject target) where T : Component
        {
            if (target == null)
                return new List<T>();

            var comps = target.GetComponents<T>().ToList();

            Log.Debug("Found {0} comps in {1}", comps.Count, target.name);

            for (int i = 0; i < target.transform.childCount; ++i)
                comps.AddRange(FindComponents<T>(target.transform.GetChild(i).gameObject));

            return comps;
        }

        public static List<T> FindComponents<T>(this UnityEngine.Transform target) where T : Component
        {
            return FindComponents<T>(target.gameObject);
        }

        public static void PrintLocalScales(this UnityEngine.Transform t)
        {
            GameObjectVisitor visitor = delegate(GameObject go, int indent)
            {
                //Log.Debug("{0} = scale {1}", indent > 0 ? new string('-', indent) + ">" : "", go.name, go.transform.localScale.FString());
                Log.Debug("{0}{1} scale: {2}", indent > 0 ? new string('-', indent) + ">" : "", go.name, go.transform.localScale.FString());
            };

            t.gameObject.TraverseHierarchy(visitor);
        }

        public static string FString(this UnityEngine.Vector3 vec, int decimals = 3)
        {
            return vec.ToString(string.Format("F{0}", decimals));
        }

        public static void CaptureSingleFrame(this UnityEngine.Camera cam, string filename)
        {
            cam.gameObject.AddComponent<SingleFrameCapture>().filename = filename;
        }

        public static void RenderSingleFrame(this UnityEngine.Camera cam, string filename)
        {
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
            RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height);
            RenderTexture old = RenderTexture.active;

            RenderTexture.active = rt;
            cam.Render();

            texture.ReadPixels(cam.pixelRect, 0, 0);
            texture.Apply();

            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.active = old;

            texture.SaveToDisk(filename);
        }


        private class SingleFrameCapture : MonoBehaviour
        {
            public string filename;

            void OnPostRender()
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    Log.Write("SingleCapture to " + filename);

                    Texture2D tex = new Texture2D(Mathf.FloorToInt(camera.pixelWidth), Mathf.FloorToInt(camera.pixelHeight), TextureFormat.ARGB32, false);

                    tex.ReadPixels(camera.pixelRect, 0, 0);
                    tex.SaveToDisk(filename);

                    filename = "";
                }

                Component.Destroy(this);
            }
        }


        public static Transform GetTopParent(this Transform transform)
        {
            Transform t = transform;

            while (t.parent != null) t = t.parent;

            return t;
        }

        // http://wiki.unity3d.com/index.php?title=ObjExporter
        public static string MeshToString(string name, Mesh m, Material[] mats)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("g ").Append(name).Append("\n");
            foreach (Vector3 v in m.vertices)
            {
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in m.normals)
            {
                sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in m.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }

            if (mats != null)
            {
                for (int material = 0; material < m.subMeshCount; material++)
                {
                    sb.Append("\n");
                    sb.Append("usemtl ").Append(mats[material % mats.Length].name).Append("\n");
                    sb.Append("usemap ").Append(mats[material % mats.Length].name).Append("\n");

                    int[] triangles = m.GetTriangles(material);
                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                            triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
                    }
                }
            }
            return sb.ToString();
        }

        public static void MeshToFile(Mesh m, string objName, string filename, Material[] mats)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.Write(MeshToString(objName, m, mats));
            }
        }

        public static void SaveToFile(this Mesh m, string objName, string filename, Material[] mats)
        {
            MeshToFile(m, objName, filename, mats);
        }

        public static void Randomize(this Color c)
        {
            c.r = UnityEngine.Random.Range(0f, 1f);
            c.g = UnityEngine.Random.Range(0f, 1f);
            c.b = UnityEngine.Random.Range(0f, 1f);
            c.a = 1f;
           
        }
    }

    class DebugCollider
    {
        #region Debug member variables
        static Texture2D debugTexture;
        static Shader debugShader;
        static Color debugColor;
        #endregion


        /// <summary>
        /// Create visible bounds collider. 
        /// </summary>
        public static void VisualizeCollider(Collider targetColl, GameObject parent)
        {
            if (!targetColl)
                return;

            //Debug.Log("Visualize collider: " + targetColl.ToString());

            if (!debugTexture || !debugShader)
                SetupDebugTexture();

            var collBox = targetColl as BoxCollider;
            var collMesh = targetColl as MeshCollider;
            var collSphere = targetColl as SphereCollider;
            var collCapsule = targetColl as CapsuleCollider;
            //var collWheel = targetColl as WheelCollider;

            GameObject debugCollider = new GameObject();
            debugCollider.transform.position = targetColl.transform.position;
            debugCollider.transform.rotation = targetColl.transform.rotation;
            debugCollider.transform.parent = parent.transform;

            // common rendering parameters
            MeshFilter cmf = debugCollider.AddComponent<MeshFilter>();
            MeshRenderer cmr = debugCollider.AddComponent<MeshRenderer>();

            cmr.material.mainTexture = debugTexture;
            cmr.material.shader = debugShader;
            cmr.material.color = debugColor;

            if (collMesh)
            {
                cmf.sharedMesh = collMesh.sharedMesh;
                cmf.transform.localScale = Vector3.one;
            }
            else if (collSphere)
            {
                GameObject sphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphereGO.transform.parent = debugCollider.transform;
                sphereGO.transform.localPosition = collSphere.center;
                sphereGO.transform.localScale = new Vector3(0.5f / collSphere.radius, 0.5f / collSphere.radius, 0.5f / collSphere.radius);

                Mesh mesh = sphereGO.GetComponent<MeshFilter>().mesh;

                MeshRenderer cmr2 = sphereGO.renderer as MeshRenderer;

                cmr2.material.mainTexture = debugTexture;
                cmr2.material.shader = debugShader;
                cmr2.material.color = debugColor;
            }
            else if (collBox)
            {
                GameObject boxGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boxGO.transform.parent = debugCollider.transform;
                boxGO.transform.position = targetColl.transform.position;
                boxGO.transform.rotation = targetColl.transform.rotation;
                boxGO.transform.localScale = collBox.extents * 2;

                MeshRenderer cmr2 = boxGO.renderer as MeshRenderer;

                cmr2.material.mainTexture = debugTexture;
                cmr2.material.shader = debugShader;
                cmr2.material.color = debugColor;
            } else if (collCapsule)
            {
                GameObject capsuleGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsuleGo.transform.parent = debugCollider.transform;
                capsuleGo.transform.position = targetColl.transform.position;
                capsuleGo.transform.rotation = targetColl.transform.rotation;

                capsuleGo.transform.localScale = new Vector3(1f / 8f / collCapsule.radius, 1f / collCapsule.height, 1f / 8f / collCapsule.radius);
                MeshRenderer cmr2 = capsuleGo.renderer as MeshRenderer;

                cmr2.material.mainTexture = debugTexture;
                cmr2.material.shader = debugShader;
                cmr2.material.color = debugColor;
            }
            else
            {
                Debug.LogError("Unsupported collider type: " + targetColl.GetType().ToString());
            }

            // TODO: wheel, maybe capsule

            /*
             *         private void CloneWheelCollider(ModuleWheel source, GameObject owner)
        {
            foreach (Wheel wheel in source.wheels)
            {
                if (wheel.wheelTransforms.Count != 1)
                {
                    Debug.LogError("CloneWheelCollider: wheel transform count != 1 for part " + source.part.ConstructID + " and wheel " + wheel.wheelName);
                    continue;
                }

                Transform wheelTransform = wheel.wheelTransforms[0]; 

                // we're actually going to be using a flat rotated mesh cylinder
                GameObject wheelGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                MeshCollider mc = wheelGo.AddComponent<MeshCollider>();


                mc.sharedMesh = wheelGo.GetComponent<MeshFilter>().sharedMesh;
                mc.sharedMaterial = wheel.whCollider.sharedMaterial;

                wheelGo.transform.position = wheelTransform.position;
                wheelGo.transform.localScale = new Vector3(wheel.whCollider.radius * wheel.rescaleFactor * 2, 0.125f /* 2 meters by default *//*, wheel.whCollider.radius * 2 * wheel.rescaleFactor);
                wheelGo.transform.rotation = source.transform.rotation * Quaternion.AngleAxis(90.0f, Vector3.forward);
                wheelGo.transform.parent = owner.transform;

                DebugCollider.VisualizeCollider(mc, wheelGo);
            }
        }
*/
        }

        private static void SetupDebugTexture()
        {
            if (debugTexture && debugShader)
                return;
            if (debugTexture)
                Material.Destroy(debugTexture);

            // Debug texture
            debugShader = Shader.Find("Transparent/Diffuse"); // because default shader doesn't support transparency
            if (!debugShader)
                return;


            debugTexture = new Texture2D(16, 16);
            debugColor = new Color(0.945f, 09.45f, 0.3176f, 0.5f);

            for (int x = 0; x < debugTexture.width; ++x)
                for (int y = 0; y < debugTexture.height; ++y)
                    debugTexture.SetPixel(x, y, debugColor);

            debugTexture.Apply();
        }


    }

    class PerformanceTest
    {
        float start = 0f;
        string name;

        internal PerformanceTest(string name)
        {
            Log.Write("new performance test for {0}", name);

            this.name = name;
            start = Time.realtimeSinceStartup;
        }

        internal void Complete()
        {
            Log.Performance("{0} completed in {1} seconds", name, (Time.realtimeSinceStartup - start).ToString("{0:0.#}"));
        }

        ~PerformanceTest()
        {
            Complete();
        }
    }


}


