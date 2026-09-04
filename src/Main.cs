using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(BurebistaTraditionalTipi.Main), "Burebista Traditional Tipi", "1.1.0", "Burebista")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace BurebistaTraditionalTipi
{
    public sealed class Main : MelonMod
    {
        private static GameObject tipi;
        private static GameObject doorFlaps;
        private static GameObject rolledDoor;
        private static Component centralFire;
        private static bool doorClosed = true;
        private static Material hideMaterial;
        private static Material woodMaterial;
        private static Material wolfMaterial;
        private static string assetDirectory;
        private const string PackedTipiGear = "GEAR_BurebistaPackedTipi";
        private static readonly Dictionary<string, Type> Types = new Dictionary<string, Type>();

        public override void OnInitializeMelon()
        {
            assetDirectory = Path.Combine(AppContext.BaseDirectory, "Mods", "BurebistaTraditionalTipi");
            MelonLogger.Msg("F3: desplegar | F4: girar | F2: empaquetar | F6: recibir uno para pruebas");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (tipi != null) PackTipi(false, true);
            else RemoveTipi(false);
        }

        public override void OnUpdate()
        {
            if (!Playable()) return;
            if (Input.GetKeyDown(KeyCode.F3)) PlaceTipi();
            if (Input.GetKeyDown(KeyCode.F6))
            {
                GivePackedTipi();
                Message("Prueba: tipi empaquetado añadido a la mochila");
            }
            if (Input.GetKeyDown(KeyCode.F4) && tipi != null)
            {
                tipi.transform.Rotate(0f, 15f, 0f, Space.World);
                Message("Tipi girado 15 grados");
            }
            if (Input.GetKeyDown(KeyCode.E) && tipi != null && PlayerDistance() <= 4.5f)
                ToggleDoor();
            if (Input.GetKeyDown(KeyCode.F2)) PackTipi(true, true);
        }

        public override void OnGUI()
        {
            if (!Playable() || tipi == null || PlayerDistance() > 4.5f) return;
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 18; style.fontStyle = FontStyle.Bold; style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(Screen.width * 0.5f - 210f, Screen.height - 170f, 420f, 42f),
                doorClosed ? "E  ENROLLAR Y ABRIR LA PIEL" : "E  SOLTAR Y CERRAR LA PIEL", style);
        }

        private static void PlaceTipi()
        {
            Transform player = GetPlayer();
            if (player == null) { Message("No se encontró al jugador"); return; }
            if (tipi != null) { Message("El tipi ya está desplegado"); return; }
            if (!TakePackedTipi()) { Message("Necesitas el tipi empaquetado en la mochila"); return; }
            try
            {
                CreateMaterials();
                tipi = new GameObject("BurebistaTraditionalTipi");
                Vector3 forward = player.forward; forward.y = 0f;
                if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
                forward.Normalize();
                tipi.transform.position = GroundPoint(player.position + forward * 4.5f);
                tipi.transform.rotation = Quaternion.Euler(0f, player.eulerAngles.y + 180f, 0f);

                CreateHideCover(tipi.transform);
                CreateFixedFrontCover(tipi.transform);
                CreatePoles(tipi.transform);
                CreateSmokeFlaps(tipi.transform);
                CreateDoorFlaps(tipi.transform);
                CreateLashing(tipi.transform);
                CreateInteriorProtection(tipi.transform);
                // Dimensiones ya expresadas en metros; no deformar entrada ni postes.
                tipi.transform.localScale = Vector3.one;
                CreateCentralCampfire();
                Object.DontDestroyOnLoad(tipi);
                SetDoorState(true);
                Message("Tipi tradicional colocado | F4 girar | F2 retirar");
                MelonLogger.Msg("Tipi creado en " + tipi.transform.position);
            }
            catch (Exception ex)
            {
                MelonLogger.Error("No se pudo crear el tipi: " + ex);
                Message("Error creando el tipi; revisa el registro de MelonLoader");
                RemoveTipi(false);
                GivePackedTipi();
            }
        }

        private static void CreateHideCover(Transform parent)
        {
            const int segments = 48;
            const float radius = 3.05f, topRadius = 0.38f, height = 4.80f;
            const float opening = 27f;
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float angle = -180f + 360f * i / segments;
                float radians = angle * Mathf.Deg2Rad;
                vertices.Add(new Vector3(Mathf.Sin(radians) * radius, 0f, Mathf.Cos(radians) * radius));
                vertices.Add(new Vector3(Mathf.Sin(radians) * topRadius, height, Mathf.Cos(radians) * topRadius));
                uvs.Add(new Vector2(i / (float)segments * 3f, 0f));
                uvs.Add(new Vector2(i / (float)segments * 3f, 1f));
            }
            for (int i = 0; i < segments; i++)
            {
                float middle = -180f + 360f * (i + 0.5f) / segments;
                if (Mathf.Abs(middle) < opening) continue;
                int a = i * 2, b = a + 1, c = a + 2, d = a + 3;
                AddDouble(triangles, a, b, c); AddDouble(triangles, c, b, d);
            }
            var mesh = NewMesh("TipiHideCover", vertices, uvs, triangles);
            var shell = new GameObject("Hide covering"); shell.transform.SetParent(parent, false);
            shell.AddComponent<MeshFilter>().sharedMesh = mesh;
            shell.AddComponent<MeshRenderer>().sharedMaterial = hideMaterial;
            var collider = shell.AddComponent<MeshCollider>(); collider.sharedMesh = mesh; collider.convex = false;
        }

        private static void CreatePoles(Transform parent)
        {
            const int count = 14;
            for (int i = 0; i < count; i++)
            {
                float angle = 360f * i / count;
                float signedAngle = angle > 180f ? angle - 360f : angle;
                // Mantener completamente libre la entrada frontal.
                if (Mathf.Abs(signedAngle) < 34f) continue;
                float r = angle * Mathf.Deg2Rad;
                Vector3 bottom = new Vector3(Mathf.Sin(r) * 2.90f, 0.03f, Mathf.Cos(r) * 2.90f);
                Vector3 top = new Vector3(Mathf.Sin(r) * 0.16f, 5.95f, Mathf.Cos(r) * 0.16f);
                CreatePole(parent, bottom, top, 0.055f, "Lodge pole");
            }
        }

        private static void CreateFixedFrontCover(Transform parent)
        {
            const int segments = 10;
            const float radius = 3.05f, topRadius = 0.38f, height = 4.80f;
            const float doorTop = 2.25f, opening = 27f;
            float lowerRadius = Mathf.Lerp(radius, topRadius, doorTop / height);
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Lerp(-opening, opening, i / (float)segments) * Mathf.Deg2Rad;
                vertices.Add(new Vector3(Mathf.Sin(angle) * lowerRadius, doorTop, Mathf.Cos(angle) * lowerRadius));
                vertices.Add(new Vector3(Mathf.Sin(angle) * topRadius, height, Mathf.Cos(angle) * topRadius));
                uvs.Add(new Vector2(i / (float)segments * 1.2f, doorTop / height));
                uvs.Add(new Vector2(i / (float)segments * 1.2f, 1f));
            }
            for (int i = 0; i < segments; i++)
            {
                int a = i * 2, b = a + 1, c = a + 2, d = a + 3;
                AddDouble(triangles, a, b, c);
                AddDouble(triangles, c, b, d);
            }
            Mesh mesh = NewMesh("Fixed stitched front cover", vertices, uvs, triangles);
            GameObject panel = new GameObject("Fixed stitched front cover");
            panel.transform.SetParent(parent, false);
            panel.AddComponent<MeshFilter>().sharedMesh = mesh;
            panel.AddComponent<MeshRenderer>().sharedMaterial = hideMaterial;
            MeshCollider collider = panel.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh; collider.convex = false;
        }

        private static void CreateSmokeFlaps(Transform parent)
        {
            CreatePanel(parent, "Left smoke flap",
                new Vector3(-0.38f, 3.95f, 0.16f), new Vector3(-1.55f, 4.88f, 0.24f), new Vector3(-0.34f, 5.45f, 0.10f));
            CreatePanel(parent, "Right smoke flap",
                new Vector3(0.38f, 3.95f, 0.16f), new Vector3(0.34f, 5.45f, 0.10f), new Vector3(1.55f, 4.88f, 0.24f));
            CreatePole(parent, new Vector3(-1.50f, 4.84f, 0.26f), new Vector3(-2.35f, 0.15f, 2.10f), 0.038f, "Smoke flap pole");
            CreatePole(parent, new Vector3(1.50f, 4.84f, 0.26f), new Vector3(2.35f, 0.15f, 2.10f), 0.038f, "Smoke flap pole");
        }

        private static void CreateDoorFlaps(Transform parent)
        {
            doorFlaps = new GameObject("Closed hide entrance");
            doorFlaps.transform.SetParent(parent, false);
            CreateCurvedEntranceFlap(doorFlaps.transform);
            CreateWolfPainting(doorFlaps.transform);

            rolledDoor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rolledDoor.name = "Rolled tied hide door";
            rolledDoor.transform.SetParent(parent, false);
            rolledDoor.transform.localPosition = new Vector3(0f, 2.32f, 1.82f);
            rolledDoor.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            rolledDoor.transform.localScale = new Vector3(0.17f, 1.02f, 0.17f);
            rolledDoor.GetComponent<Renderer>().sharedMaterial = hideMaterial;
        }

        private static void CreateCurvedEntranceFlap(Transform parent)
        {
            const int segments = 12;
            const float radius = 3.05f, topRadius = 0.38f, height = 4.80f;
            const float flapTop = 2.34f, overlapAngle = 32f;
            float upperRadius = Mathf.Lerp(radius, topRadius, flapTop / height) + 0.025f;
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float fraction = i / (float)segments;
                float angle = Mathf.Lerp(-overlapAngle, overlapAngle, fraction) * Mathf.Deg2Rad;
                vertices.Add(new Vector3(Mathf.Sin(angle) * (radius + 0.025f), 0.025f, Mathf.Cos(angle) * (radius + 0.025f)));
                vertices.Add(new Vector3(Mathf.Sin(angle) * upperRadius, flapTop, Mathf.Cos(angle) * upperRadius));
                uvs.Add(new Vector2(fraction * 1.35f, 0f));
                uvs.Add(new Vector2(fraction * 1.35f, flapTop / height));
            }
            for (int i = 0; i < segments; i++)
            {
                int a = i * 2, b = a + 1, c = a + 2, d = a + 3;
                AddDouble(triangles, a, b, c);
                AddDouble(triangles, c, b, d);
            }
            Mesh mesh = NewMesh("Curved overlapping hide entrance", vertices, uvs, triangles);
            GameObject flap = new GameObject("Curved overlapping hide entrance");
            flap.transform.SetParent(parent, false);
            flap.AddComponent<MeshFilter>().sharedMesh = mesh;
            flap.AddComponent<MeshRenderer>().sharedMaterial = hideMaterial;
            MeshCollider collider = flap.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh; collider.convex = false;
        }

        private static void CreateWolfPainting(Transform parent)
        {
            const int segments = 10;
            const float radius = 3.05f, topRadius = 0.38f, height = 4.80f;
            const float bottom = 0.48f, top = 1.88f, angleWidth = 19f;
            float lowerRadius = Mathf.Lerp(radius, topRadius, bottom / height) + 0.055f;
            float upperRadius = Mathf.Lerp(radius, topRadius, top / height) + 0.055f;
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float fraction = i / (float)segments;
                float angle = Mathf.Lerp(-angleWidth, angleWidth, fraction) * Mathf.Deg2Rad;
                vertices.Add(new Vector3(Mathf.Sin(angle) * lowerRadius, bottom, Mathf.Cos(angle) * lowerRadius));
                vertices.Add(new Vector3(Mathf.Sin(angle) * upperRadius, top, Mathf.Cos(angle) * upperRadius));
                uvs.Add(new Vector2(fraction, 0f));
                uvs.Add(new Vector2(fraction, 1f));
            }
            for (int i = 0; i < segments; i++)
            {
                int a = i * 2, b = a + 1, c = a + 2, d = a + 3;
                AddDouble(triangles, a, b, c);
                AddDouble(triangles, c, b, d);
            }
            Mesh mesh = NewMesh("Painted wolf decal", vertices, uvs, triangles);
            GameObject painting = new GameObject("Painted wolf on closed hide");
            painting.transform.SetParent(parent, false);
            painting.AddComponent<MeshFilter>().sharedMesh = mesh;
            painting.AddComponent<MeshRenderer>().sharedMaterial = wolfMaterial;
        }

        private static void CreateInteriorProtection(Transform parent)
        {
            GameObject volume = new GameObject("Tipi interior warmth and wind protection");
            volume.transform.SetParent(parent, false);
            volume.transform.localPosition = new Vector3(0f, 1.25f, 0f);

            SphereCollider trigger = volume.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 2.18f;

            HeatSource heat = volume.AddComponent<HeatSource>();
            heat.m_MaxTempIncrease = 10f;
            heat.m_MaxTempIncreaseInnerRadius = 2.05f;
            heat.m_MaxTempIncreaseOuterRadius = 2.30f;
            heat.m_TimeToReachMaxTempMinutes = 0f;
            heat.m_StartingTemp = 10f;
            heat.m_StartOn = true;
            heat.TurnOn();

            WindKiller wind = volume.AddComponent<WindKiller>();
            wind.m_Collider = trigger;
            MelonLogger.Msg("Protección interior activa: +10 C y bloqueo de viento");
        }

        private static void ToggleDoor()
        {
            SetDoorState(!doorClosed);
            Message(doorClosed ? "Piel de entrada cerrada" : "Piel enrollada: entrada abierta");
        }

        private static void SetDoorState(bool closed)
        {
            doorClosed = closed;
            if (doorFlaps != null) doorFlaps.SetActive(closed);
            if (rolledDoor != null) rolledDoor.SetActive(!closed);
        }

        private static float PlayerDistance()
        {
            Transform player = GetPlayer();
            return player != null && tipi != null ? Vector3.Distance(player.position, tipi.transform.position) : 999f;
        }

        private static void CreateLashing(Transform parent)
        {
            GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            band.name = "Pole lashing"; band.transform.SetParent(parent, false);
            band.transform.localPosition = new Vector3(0f, 5.02f, 0f);
            band.transform.localScale = new Vector3(0.31f, 0.065f, 0.31f);
            band.GetComponent<Renderer>().sharedMaterial = woodMaterial;
        }

        private static void CreateCentralCampfire()
        {
            Type managerType = FindType("GameManager");
            object fireManager = managerType?.GetMethod("GetFireManagerComponent", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, null);
            if (fireManager == null) throw new Exception("No se encontró FireManager");
            object fire = fireManager.GetType().GetMethod("InstantiateCampFire", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(fireManager, null);
            centralFire = fire as Component;
            if (centralFire == null) throw new Exception("El juego no pudo crear la fogata nativa");
            centralFire.transform.position = tipi.transform.position + Vector3.up * 0.04f;
            centralFire.transform.rotation = Quaternion.identity;

            // La fogata nativa nace apagada; nunca se llama TurnOn ni StartFireLit.
            MethodInfo turnOff = fire.GetType().GetMethod("TurnOffImmediate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (turnOff != null) turnOff.Invoke(fire, null);
            MelonLogger.Msg("Fogata nativa apagada creada en el centro del tipi");
        }

        private static void CreatePanel(Transform parent, string name, Vector3 a, Vector3 b, Vector3 c)
        {
            var vertices = new List<Vector3> { a, b, c };
            var uvs = new List<Vector2> { new Vector2(0.5f, 1f), Vector2.zero, Vector2.right };
            var tris = new List<int>(); AddDouble(tris, 0, 1, 2);
            var mesh = NewMesh(name, vertices, uvs, tris);
            var panel = new GameObject(name); panel.transform.SetParent(parent, false);
            panel.AddComponent<MeshFilter>().sharedMesh = mesh;
            panel.AddComponent<MeshRenderer>().sharedMaterial = hideMaterial;
        }

        private static void CreatePole(Transform parent, Vector3 start, Vector3 end, float radius, string name)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = name; pole.transform.SetParent(parent, false);
            Vector3 delta = end - start;
            pole.transform.localPosition = (start + end) * 0.5f;
            pole.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
            pole.transform.localScale = new Vector3(radius, delta.magnitude * 0.5f, radius);
            pole.GetComponent<Renderer>().sharedMaterial = woodMaterial;
        }

        private static Mesh NewMesh(string name, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
        {
            var mesh = new Mesh { name = name };
            // Duplicar vértices para la cara interior. Compartir los mismos vértices
            // haría que las normales opuestas se anulasen y la piel quedase negra.
            int offset = vertices.Count;
            var vv = new Il2CppSystem.Collections.Generic.List<Vector3>();
            foreach (var v in vertices) vv.Add(v);
            foreach (var v in vertices) vv.Add(v);
            var uu = new Il2CppSystem.Collections.Generic.List<Vector2>();
            foreach (var v in uvs) uu.Add(v);
            foreach (var v in uvs) uu.Add(v);
            var tt = new Il2CppSystem.Collections.Generic.List<int>();
            foreach (var t in triangles) tt.Add(t);
            for (int i = 0; i + 2 < triangles.Count; i += 3)
            {
                tt.Add(triangles[i] + offset);
                tt.Add(triangles[i + 2] + offset);
                tt.Add(triangles[i + 1] + offset);
            }
            mesh.SetVertices(vv); mesh.SetUVs(0, uu); mesh.SetTriangles(tt, 0, true);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        private static void AddDouble(List<int> values, int a, int b, int c)
        {
            // El material desactiva el descarte de caras, por lo que una sola cara
            // conserva normales limpias y se ilumina correctamente por ambos lados.
            values.Add(a); values.Add(c); values.Add(b);
        }

        private static void CreateMaterials()
        {
            Shader shader = Shader.Find("Legacy Shaders/Diffuse") ?? Shader.Find("Standard") ?? Shader.Find("Unlit/Texture");
            if (shader == null) throw new Exception("No hay shader compatible");
            hideMaterial = new Material(shader) { name = "Tipi hide", color = Color.white };
            if (hideMaterial.HasProperty("_Cull")) hideMaterial.SetInt("_Cull", 2);
            string texturePath = Path.Combine(assetDirectory, "tipi_hide.png");
            if (!File.Exists(texturePath)) throw new FileNotFoundException("Falta tipi_hide.png", texturePath);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(texturePath))) throw new Exception("No se pudo leer la textura");
            texture.wrapMode = TextureWrapMode.Repeat; texture.filterMode = FilterMode.Bilinear; hideMaterial.mainTexture = texture;
            Shader woodShader = Shader.Find("Legacy Shaders/Diffuse") ?? Shader.Find("Standard") ?? shader;
            woodMaterial = new Material(woodShader) { name = "Tipi poles", color = new Color(0.24f, 0.13f, 0.055f, 1f) };

            string wolfPath = Path.Combine(assetDirectory, "tipi_wolf.png");
            if (!File.Exists(wolfPath)) throw new FileNotFoundException("Falta tipi_wolf.png", wolfPath);
            var wolfTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(wolfTexture, File.ReadAllBytes(wolfPath))) throw new Exception("No se pudo leer el dibujo del lobo");
            wolfTexture.wrapMode = TextureWrapMode.Clamp; wolfTexture.filterMode = FilterMode.Bilinear;
            Shader wolfShader = Shader.Find("Legacy Shaders/Transparent/Diffuse") ?? Shader.Find("Unlit/Transparent") ?? shader;
            wolfMaterial = new Material(wolfShader) { name = "Painted wolf decal", color = Color.white, mainTexture = wolfTexture };
            wolfMaterial.SetOverrideTag("RenderType", "Transparent");
            wolfMaterial.renderQueue = 3000;
        }

        private static Vector3 GroundPoint(Vector3 position)
        {
            RaycastHit hit;
            Vector3 origin = position + Vector3.up * 3f;
            if (Physics.Raycast(origin, Vector3.down, out hit, 10f)) position.y = hit.point.y + 0.02f;
            else position.y -= 1.6f;
            return position;
        }

        private static void RemoveTipi(bool notify)
        {
            if (centralFire != null)
            {
                try
                {
                    object fire = centralFire;
                    MethodInfo burningMethod = fire.GetType().GetMethod("IsBurning", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    bool burning = burningMethod != null && (bool)burningMethod.Invoke(fire, null);
                    if (!burning) Object.Destroy(centralFire.gameObject);
                    else if (notify) Message("La fogata encendida permanece en el terreno");
                }
                catch { }
            }
            centralFire = null;
            if (tipi != null) Object.Destroy(tipi);
            tipi = null;
            doorFlaps = null; rolledDoor = null; doorClosed = true;
            if (notify) Message("Tipi retirado");
        }

        private static void PackTipi(bool notify, bool returnToInventory)
        {
            if (tipi == null)
            {
                if (notify) Message("No hay ningún tipi desplegado para empaquetar");
                return;
            }
            if (IsCentralFireBurning())
            {
                if (notify) Message("Apaga la fogata antes de empaquetar el tipi");
                return;
            }
            RemoveTipi(false);
            if (returnToInventory) GivePackedTipi();
            if (notify) Message("Tipi empaquetado y guardado en la mochila");
        }

        private static bool IsCentralFireBurning()
        {
            if (centralFire == null) return false;
            try
            {
                MethodInfo method = centralFire.GetType().GetMethod("IsBurning", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return method != null && (bool)method.Invoke(centralFire, null);
            }
            catch { return false; }
        }

        private static object GetGameComponent(string methodName)
        {
            try { return FindType("GameManager")?.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, null); }
            catch { return null; }
        }

        private static bool HasPackedTipi()
        {
            object inventory = GetGameComponent("GetInventoryComponent");
            if (inventory == null) return false;
            try
            {
                MethodInfo method = inventory.GetType().GetMethod("NumGearInInventory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(bool) }, null);
                return method != null && Convert.ToInt32(method.Invoke(inventory, new object[] { PackedTipiGear, false })) > 0;
            }
            catch { return false; }
        }

        private static bool TakePackedTipi()
        {
            object inventory = GetGameComponent("GetInventoryComponent");
            if (inventory == null || !HasPackedTipi()) return false;
            try
            {
                MethodInfo method = inventory.GetType().GetMethod("RemoveGearFromInventory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(int), typeof(bool) }, null);
                if (method == null) return false;
                method.Invoke(inventory, new object[] { PackedTipiGear, 1, true });
                return true;
            }
            catch (Exception ex) { MelonLogger.Warning("No se pudo consumir el tipi empaquetado: " + ex.Message); return false; }
        }

        private static void GivePackedTipi()
        {
            object playerManager = GetGameComponent("GetPlayerManagerComponent");
            if (playerManager == null) return;
            try
            {
                MethodInfo method = playerManager.GetType().GetMethod("AddItemCONSOLE", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(int), typeof(float) }, null);
                if (method == null) throw new MissingMethodException("AddItemCONSOLE");
                method.Invoke(playerManager, new object[] { PackedTipiGear, 1, 100f });
            }
            catch (Exception ex) { MelonLogger.Error("No se pudo entregar el tipi empaquetado: " + ex); }
        }

        private static Transform GetPlayer()
        {
            try
            {
                Type type = FindType("GameManager");
                return type?.GetMethod("GetPlayerTransform", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, null) as Transform;
            }
            catch { return null; }
        }

        private static Type FindType(string name)
        {
            Type type; if (Types.TryGetValue(name, out type)) return type;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(name, false) ?? assembly.GetType("Il2Cpp." + name, false);
                if (type != null) break;
            }
            Types[name] = type; return type;
        }

        private static bool Playable()
        {
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name ?? "";
            return scene.Length > 0 && scene.IndexOf("MainMenu", StringComparison.OrdinalIgnoreCase) < 0 && scene.IndexOf("Boot", StringComparison.OrdinalIgnoreCase) < 0;
        }

        private static void Message(string text)
        {
            MelonLogger.Msg(text);
            try { FindType("HUDMessage")?.GetMethod("AddMessage", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null)?.Invoke(null, new object[] { text }); }
            catch { }
        }
    }
}
