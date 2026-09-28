using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;
[assembly: MelonInfo(typeof(TipiCuringRack.Main), "Tipi Curing Rack", "0.1.1", "Burebista")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
namespace TipiCuringRack;
public sealed class Main : MelonMod
{
    private sealed class Hung
    {
        public GearItem Item; public int Slot; public Vector3 Scale; public Quaternion Rotation;
        public bool Kinematic;
    }
    private static readonly List<Hung> hung = new();
    private static GameObject rack, tipi;
    private static Material wood, rope;
    private float scanAt;
    private string message = "";
    private float messageUntil;
    private static readonly Vector3[] slots = { new(-.57f, .9f, 0), new(-.19f, .9f, 0), new(.19f, .9f, 0), new(.57f, .9f, 0), new(0, .48f, .38f), new(0, .48f, .62f) };
    public override void OnInitializeMelon() => MelonLogger.Msg("Tipi curing rack: deploy the tipi; drop fresh hides/guts beside the frame and press G to hang them.");
    private static GameObject Bar(Vector3 a, Vector3 b, float thickness, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "RackPole"; go.transform.SetParent(rack.transform, false);
        go.transform.localPosition = (a + b) * .5f;
        go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
        go.transform.localScale = new Vector3(thickness, (b - a).magnitude * .5f, thickness);
        var col = go.GetComponent<Collider>(); col.enabled = false; Object.Destroy(col);
        go.GetComponent<Renderer>().sharedMaterial = material; return go;
    }
    private static Material CreateWoodMaterial(Shader fallbackShader)
    {
        try
        {
            var prefab = GearItem.LoadGearItemPrefab("GEAR_Stick");
            if (prefab != null)
                foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                    foreach (var source in renderer.sharedMaterials)
                    {
                        if (source == null || source.shader == null || source.mainTexture == null) continue;
                        // Keep native shader and original texture bindings. Never clone
                        // GPU-only textures or alter the game's shared materials.
                        var material = new Material(source) { name = "TipiRack_TLD_Wood" };
                        MelonLogger.Msg("Bastidor: madera TLD aplicada (" + source.shader.name + ").");
                        return material;
                    }
        }
        catch (Exception error) { MelonLogger.Warning("Madera TLD no disponible: " + error.Message); }
        return new Material(fallbackShader) { color = new Color(.28f, .18f, .09f) };
    }
    private static void Build()
    {
        var shader = Shader.Find("Standard") ?? Shader.Find("Unlit/Color");
        wood = CreateWoodMaterial(shader);
        rope = new Material(shader) { color = new Color(.55f, .42f, .24f) };
        rack = new GameObject("Burebista_Tipi_CuringRack");
        rack.transform.SetParent(tipi.transform, false);
        rack.transform.localPosition = new Vector3(-1.7f, .08f, -.55f);
        rack.transform.localRotation = Quaternion.Euler(0, 65, 0);
        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * .84f;
            Bar(new(x, 0, -.3f), new(x, 1.55f, 0), .065f, wood);
            Bar(new(x, 0, .3f), new(x, 1.55f, 0), .065f, wood);
        }
        Bar(new(-.92f, 1.4f, 0), new(.92f, 1.4f, 0), .07f, wood);
        Bar(new(-.84f, .35f, 0), new(.84f, .35f, 0), .04f, wood);
        foreach (var slot in slots.Take(4)) Bar(new(slot.x, 1.4f, 0), new(slot.x, 1.14f, 0), .009f, rope);
        // Separate low trestle for the two long saplings.
        foreach (float x in new float[] { -.65f, .65f })
        {
            Bar(new(x, 0, .25f), new(x, .46f, .5f), .05f, wood);
            Bar(new(x, 0, .75f), new(x, .46f, .5f), .05f, wood);
            Bar(new(x, .46f, .25f), new(x, .46f, .75f), .05f, wood);
        }
        MelonLogger.Msg("Tipi curing frame created: 4 hides/guts + 2 saplings.");
    }
    private static bool Eligible(GearItem item)
    {
        if (item == null || item.m_InPlayerInventory || item.m_EvolveItem == null) return false;
        string name = item.name;
        return !name.Contains("Dried", StringComparison.OrdinalIgnoreCase)
            && (name.Contains("Hide", StringComparison.OrdinalIgnoreCase) || name.Contains("Pelt", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Gut", StringComparison.OrdinalIgnoreCase) || name.Contains("Sapling", StringComparison.OrdinalIgnoreCase));
    }
    private void HangNearby()
    {
        int added = 0;
        foreach (var item in Object.FindObjectsOfType<GearItem>())
        {
            if (hung.Count >= slots.Length) break;
            if (!Eligible(item) || hung.Any(h => h.Item == item) || Vector3.Distance(item.transform.position, rack.transform.position) > 1.8f) continue;
                        bool sapling = item.name.Contains("Sapling", StringComparison.OrdinalIgnoreCase);
            var free = Enumerable.Range(sapling ? 4 : 0, sapling ? 2 : 4).Where(i => !hung.Any(h => h.Slot == i)).ToArray();
            if (free.Length == 0) continue;
            int index = free[0];
            var entry = new Hung { Item = item, Slot = index, Scale = item.transform.localScale,
                Rotation = item.transform.rotation, Kinematic = item.m_RigidBody != null && item.m_RigidBody.isKinematic };
            hung.Add(entry);
            // Gear remains a native world item (not a child of the disposable frame).
            if (item.m_RigidBody != null) { item.m_RigidBody.velocity = Vector3.zero; item.m_RigidBody.isKinematic = true; }
            var renderers = item.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                float extent = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
                if (extent > .01f) item.transform.localScale = entry.Scale * Mathf.Clamp((sapling ? 1.5f : .55f) / extent, .05f, 3);
            }
            added++;
        }
        message = added > 0 ? $"Colgados: {added}. Curado a velocidad normal." : "Deja pieles, tripas o retoños frescos al pie. Capacidad: 4 pieles/tripas + 2 retoños.";
        messageUntil = Time.unscaledTime + 5;
    }
    private static void Release(Hung entry, bool ground)
    {
        if (entry.Item == null) return;
        entry.Item.transform.localScale = entry.Scale;
        if (entry.Item.m_InPlayerInventory) return;
        entry.Item.transform.rotation = entry.Rotation;
        if (ground && rack != null) entry.Item.transform.position = rack.transform.TransformPoint(new Vector3(slots[entry.Slot].x, .12f, .4f));
        if (entry.Item.m_RigidBody != null) entry.Item.m_RigidBody.isKinematic = entry.Kinematic;
    }
    private static void Clear()
    {
        foreach (var entry in hung) Release(entry, true);
        hung.Clear();
        if (rack != null) Object.Destroy(rack);
        if (wood != null) Object.Destroy(wood);
        if (rope != null) Object.Destroy(rope);
        rack = null; tipi = null; wood = null; rope = null;
    }
    public override void OnSceneWasLoaded(int i, string scene) { Clear(); scanAt = 0; }
    public override void OnDeinitializeMelon() => Clear();
    public override void OnUpdate()
    {
        if (!GameManager.HasPlayerObject() || GameManager.IsMainMenuActive()) return;
        try
        {
            if (rack == null && Time.time > scanAt)
            {
                if (hung.Count > 0) Clear();
                scanAt = Time.time + 2;
                tipi = GameObject.Find("BurebistaTraditionalTipi");
                if (tipi != null) Build();
            }
            if (rack == null) return;
            foreach (var entry in hung.ToArray())
            {
                if (entry.Item == null || entry.Item.m_InPlayerInventory)
                { Release(entry, false); hung.Remove(entry); continue; }
                var pos = rack.transform.TransformPoint(slots[entry.Slot]);
                entry.Item.transform.position = pos;
                entry.Item.transform.rotation = rack.transform.rotation * (entry.Slot < 4 ? Quaternion.Euler(90, 0, Mathf.Sin(Time.time * 1.3f + entry.Slot) * 2) : Quaternion.Euler(0, 90, 0));
            }
            if (Time.timeScale > 0 && !GameManager.ControlsLocked() && Input.GetKeyDown(KeyCode.G)
                && Vector3.Distance(GameManager.GetPlayerTransform().position, rack.transform.position) < 2.5f)
                HangNearby();
        }
        catch (Exception ex) { MelonLogger.Warning("Tipi rack: " + ex.Message); }
    }
    internal static bool IsHanging(GearItem item) => rack != null && item != null && !item.m_InPlayerInventory && hung.Any(h => h.Item == item);
    public override void OnGUI()
    {
        if (rack == null || !GameManager.HasPlayerObject() || GameManager.IsMainMenuActive()) return;
        if (Vector3.Distance(GameManager.GetPlayerTransform().position, rack.transform.position) > 2.5f) return;
        GUI.Label(new Rect(24, Screen.height - 235, 900, 28), $"G: colocar materiales cercanos | Secadero {hung.Count}/6 | Recoger: selecciona el material");
        if (Time.unscaledTime < messageUntil) GUI.Label(new Rect(24, Screen.height - 265, 900, 28), message);
    }
}
[HarmonyPatch(typeof(EvolveItem), nameof(EvolveItem.IsIndoorScene))]
internal static class RackCuringShelter
{
    private static void Postfix(EvolveItem __instance, ref bool __result)
    { if (Main.IsHanging(__instance.m_GearItem)) __result = true; }
}

[HarmonyPatch(typeof(EvolveItem), nameof(EvolveItem.CanEvolve))]
internal static class RackNativeCuring
{
    private static void Postfix(EvolveItem __instance, ref bool __result)
    {
        if (Main.IsHanging(__instance.m_GearItem) && __instance.m_TimeToEvolveGameDays > 0
            && !__instance.m_GearItem.IsWornOut()) __result = true;
    }
}
