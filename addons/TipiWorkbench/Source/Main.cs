using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppTLD.Gear;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;
[assembly: MelonInfo(typeof(TipiWorkbench.Main), "Tipi Exterior Workbench", "0.3.4", "Burebista")]
[assembly: MelonGame("Hinterland", "TheLongDark")]
namespace TipiWorkbench;
public sealed partial class Main : MelonMod
{
    internal static WorkBench Bench;
    private static GameObject root;
    private readonly List<Material> materials = new();
    private Light glow;
    private Texture2D atlas;
    private Material embers;
    private static bool paidSession;
    internal static bool Lit => root != null && (Time.time < burningUntil || (cookingFire != null && cookingFire.IsBurning()));
    internal static bool Credit => paidSession && root != null && Time.time < burningUntil;
    private float scanAt;
    internal static float burningUntil;
    internal static string Message = "";
    internal static float MessageUntil;
    internal static bool Ours(Panel_Crafting panel) => Bench != null && panel?.m_CurrentLocation != null && panel.m_CurrentLocation.Pointer == Bench.Pointer;
    internal static bool HasFuel()
    {
        var inv = GameManager.GetInventoryComponent();
        return inv != null && (inv.NumGearInInventory("GEAR_Coal", false) >= 1 || inv.NumGearInInventory("GEAR_Charcoal", false) >= 3);
    }
    internal static void Tell(string text) { Message = text; MessageUntil = Time.unscaledTime + 6; MelonLogger.Msg(text); }
    internal static bool BurnFuel()
    {
        var inv = GameManager.GetInventoryComponent();
        if (inv == null) return false;
        string fuel; int count;
        if (inv.NumGearInInventory("GEAR_Coal", false) >= 1) { fuel = "GEAR_Coal"; count = 1; }
        else if (inv.NumGearInInventory("GEAR_Charcoal", false) >= 3) { fuel = "GEAR_Charcoal"; count = 3; }
        else { Tell("Necesitas 1 carbon o 3 carboncillos para iniciar el trabajo."); return false; }
        int before = inv.NumGearInInventory(fuel, false);
        inv.RemoveGearFromInventory(fuel, count, false);
        if (inv.NumGearInInventory(fuel, false) != before - count)
        { Tell("No se pudo confirmar el combustible. Trabajo detenido."); return false; }
        burningUntil = Time.time + 300;
        paidSession = true;
        Tell("Brasero encendido: 5 minutos. Incluye combustible para una sesion de trabajo.");
        return true;
    }
    internal static bool PayForWork()
    {
        if (!Credit && !BurnFuel()) return false;
        paidSession = false;
        burningUntil = Time.time + 300;
        return true;
    }
    private Material Surface(int column, int row)
    {
        var mat = Mat(Color.white);
        mat.mainTexture = atlas;
        mat.mainTextureScale = new Vector2(.48f, .48f);
        mat.mainTextureOffset = new Vector2(column * .5f + .01f, row * .5f + .01f);
        return mat;
    }
    private Material Mat(Color color)
    {
        var shader = Shader.Find("Standard") ?? Shader.Find("Unlit/Color");
        var mat = new Material(shader) { color = color };
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", .08f);
        materials.Add(mat); return mat;
    }
    private GameObject Part(string name, PrimitiveType shape, Vector3 pos, Vector3 scale, Material mat, bool solid = false)
    {
        var go = GameObject.CreatePrimitive(shape); go.name = name;
        go.transform.SetParent(root.transform, false); go.transform.localPosition = pos; go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        if (!solid) { var col = go.GetComponent<Collider>(); col.enabled = false; Object.Destroy(col); }
        return go;
    }
    private void Build(GameObject tipi)
    {
        root = new GameObject("Burebista_Exterior_Workbench"); root.transform.SetParent(tipi.transform, false);
        root.transform.localPosition = new Vector3(4.2f, 0, 1.2f);
        root.transform.localRotation = Quaternion.Euler(0, -25, 0);
        if (Physics.Raycast(root.transform.position + Vector3.up * 3, Vector3.down, out var ground, 8, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) root.transform.position = ground.point;
        atlas = new Texture2D(2, 2, TextureFormat.RGBA32, true);
        using (var stream = typeof(Main).Assembly.GetManifestResourceStream("TipiWorkbench.Atlas.png"))
        using (var bytes = new System.IO.MemoryStream())
        {
            if (stream == null) throw new InvalidOperationException("Missing embedded workbench textures.");
            stream.CopyTo(bytes);
            if (!ImageConversion.LoadImage(atlas, bytes.ToArray(), false)) throw new InvalidOperationException("Workbench texture load failed.");
        }
        atlas.filterMode = FilterMode.Trilinear; atlas.anisoLevel = 4;
        var wood = NativeSurface("GEAR_ReclaimedWoodB", Surface(0, 1)); var iron = NativeSurface("GEAR_ScrapMetal", Surface(1, 1));
        var poleWood = NativeSurface("GEAR_Stick", wood);
        var stone = NativeSurface("GEAR_Stone", Surface(0, 0)); var coal = NativeSurface("GEAR_Coal", Surface(1, 0));
        var leather = NativeSurface("GEAR_LeatherHideDried", Mat(new Color(.38f,.25f,.14f)));
        for(int i=0;i<5;i++) Part("Plank",PrimitiveType.Cube,new Vector3(0,1.02f,(i-2)*.16f),new Vector3(1.8f,.09f,.15f),wood,true);
        foreach(float x in new float[]{-.65f,.65f}) foreach(float z in new float[]{-.28f,.28f})
            Part("Leg",PrimitiveType.Cylinder,new Vector3(x,.5f,z),new Vector3(.10f,.5f,.10f),poleWood);
        Part("LowerShelf",PrimitiveType.Cube,new Vector3(0,.3f,0),new Vector3(1.6f,.07f,.65f),wood);
        for(int i=0;i<9;i++) {float a=i*Mathf.PI*2/9; Part("HearthStone",PrimitiveType.Sphere,new Vector3(1.45f+Mathf.Cos(a)*.34f,.18f,Mathf.Sin(a)*.34f),new Vector3(.25f,.30f,.23f),stone);}
        Part("Brazier",PrimitiveType.Cylinder,new Vector3(1.45f,.38f,0),new Vector3(.64f,.12f,.64f),iron);
        embers = Mat(new Color(.06f,.045f,.03f));
        var emberShader = Shader.Find("Unlit/Color");
        if (emberShader != null) embers.shader = emberShader;
        for(int i=0;i<12;i++)
        {
            float a=i*Mathf.PI*2/12;
            var pos=new Vector3(1.45f+Mathf.Cos(a)*.19f,.51f,Mathf.Sin(a)*.19f);
            var chunk=Part("CharcoalChunk",PrimitiveType.Cube,pos,new Vector3(.115f,.075f,.095f),coal);
            chunk.transform.localRotation=Quaternion.Euler(i*13%25,i*47,i*7%20);
            Part("GlowingCrack",PrimitiveType.Cube,pos+Vector3.up*.043f,new Vector3(.065f,.008f,.012f),embers).transform.localRotation=Quaternion.Euler(0,i*47,0);
            var rim=Part("IronRim",PrimitiveType.Cube,new Vector3(1.45f+Mathf.Cos(a)*.31f,.53f,Mathf.Sin(a)*.31f),new Vector3(.04f,.06f,.17f),iron);
            rim.transform.localRotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg,0);
        }
        var lamp = new GameObject("BrazierGlow"); lamp.transform.SetParent(root.transform,false); lamp.transform.localPosition=new Vector3(1.45f,.7f,0);
        glow=lamp.AddComponent<Light>(); glow.color=new Color(1,.32f,.06f); glow.range=3; glow.intensity=0;
        Bench=root.AddComponent<WorkBench>(); Bench.m_CraftingAndRepairTimeModifier=1; Bench.m_CraftingAndRepairSkillModifier=0;
        for(int i=-1;i<=1;i++)
            Part("CookingSupport",PrimitiveType.Cube,new Vector3(1.45f,.60f,i*.13f),new Vector3(.50f,.035f,.025f),iron,true);
        AddWorkbenchDecorations();
        nextCookingScan = Time.unscaledTime + 3f;
        MelonLogger.Msg("Exterior tipi workbench ready. B to open; 1 coal or 3 charcoal per crafting session.");
    }
    private void Clear()
    {
        cookingConfigured = false; migrationWarning = false;
        pendingCookingOpen = false;
        cookingFire = null; // Native fire and its cooking items belong to the game save.
        if(root!=null) Object.Destroy(root);
        foreach(var mat in materials) if(mat!=null) Object.Destroy(mat);
        if(atlas!=null) Object.Destroy(atlas);
        materials.Clear(); root=null; Bench=null; glow=null; embers=null; atlas=null; burningUntil=0; paidSession=false;
    }
    public override void OnSceneWasLoaded(int i,string scene) {Clear();scanAt=0;}
    public override void OnDeinitializeMelon()=>Clear();
    public override void OnUpdate()
    {
        if(!GameManager.HasPlayerObject() || GameManager.IsMainMenuActive())return;
        try
        {
            if(root==null && Time.time>scanAt)
            { Clear();scanAt=Time.time+2;var tipi=GameObject.Find("BurebistaTraditionalTipi");if(tipi!=null)Build(tipi); }
            if(root==null)return;
            float flicker=.85f+.12f*Mathf.Sin(Time.time*13)+.07f*Mathf.Sin(Time.time*21);
            if(glow!=null)glow.intensity=Lit ? flicker*1.5f:0;
            if(embers!=null)embers.color=Lit ? new Color(1,.22f+.10f*flicker,.015f) : new Color(.04f,.03f,.02f);
            if(Time.timeScale<=0 || GameManager.ControlsLocked())return;
            UpdateCookingCentre();
            if(pendingCookingOpen) { pendingCookingOpen=false; OpenCooking(); }
            else if(Input.GetKeyDown(KeyCode.C) && Vector3.Distance(GameManager.GetPlayerTransform().position,root.transform.position)<2.5f) OpenCooking();
            if(Input.GetKeyDown(KeyCode.N) && Vector3.Distance(GameManager.GetPlayerTransform().position,root.transform.position)<2.5f)
            {
                if(Time.time < burningUntil){burningUntil=0;paidSession=false;Tell("Brasero apagado. El combustible quemado no se devuelve.");}
                else BurnFuel();
            }
            if(Input.GetKeyDown(KeyCode.B) && Vector3.Distance(GameManager.GetPlayerTransform().position,root.transform.position)<2.5f)
            {
                var panel=Resources.FindObjectsOfTypeAll<Panel_Crafting>().FirstOrDefault();
                if(panel==null){Tell("Panel de fabricacion no disponible.");return;}
                panel.EnableCraftingAtLocation(Bench.Cast<CraftingLocationInterface>());
            }
        }
        catch(Exception ex){MelonLogger.Warning("Tipi workbench: "+ex);}
    }
    public override void OnGUI()
    {
        if(root==null || !GameManager.HasPlayerObject())return;
        if(Vector3.Distance(GameManager.GetPlayerTransform().position,root.transform.position)>2.5f)return;
        GUI.Label(new Rect(24,Screen.height-205,1000,28),"B: banco | C: cocinar | N: "+(Time.time < burningUntil?"apagar":"encender")+" brasero | 1 carbon o 3 carboncillos");
        if(Time.unscaledTime<MessageUntil) GUI.Label(new Rect(24,Screen.height-235,1000,28),Message);
    }
}
[HarmonyPatch(typeof(Panel_Crafting),nameof(Panel_Crafting.OnBeginCrafting))]
internal static class FuelCheck
{
    private static bool Prefix(Panel_Crafting __instance)
    {
        if(!Main.Ours(__instance) || Main.Credit || Main.HasFuel())return true;
        Main.Tell("Falta combustible: 1 carbon o 3 carboncillos.");return false;
    }
}
[HarmonyPatch(typeof(Panel_Crafting),nameof(Panel_Crafting.CraftingStart))]
internal static class FuelPayment
{
    private static bool Prefix(Panel_Crafting __instance)=>!Main.Ours(__instance)||Main.PayForWork();
}

