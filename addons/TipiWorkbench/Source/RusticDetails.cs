using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TipiWorkbench;

public sealed partial class Main
{
    private static Fire cookingFire;
    private bool pendingCookingOpen;
    private bool cookingConfigured, migrationWarning;
    private float nextCookingScan;

    private Material NativeSurface(string gearName, Material fallback)
    {
        try
        {
            var prefab = GearItem.LoadGearItemPrefab(gearName);
            if (prefab != null)
                foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                    foreach (var source in renderer.sharedMaterials)
                    {
                        if (source == null || source.shader == null || source.mainTexture == null) continue;
                        var mat = new Material(source) { name = "TipiWorkbench_" + gearName };
                        materials.Add(mat);
                        return mat;
                    }
        }
        catch (Exception e) { MelonLogger.Warning("Material " + gearName + ": " + e.Message); }
        return fallback;
    }

    private void OpenCooking()
    {
        try
        {
            if (cookingFire == null)
            {
                Vector3 position = root.transform.TransformPoint(new Vector3(1.45f,.50f,0));
                // On-demand search after normal player input is ready. Reuse the
                // native saved fire instead of duplicating its cooking slots/items.
                foreach (var fire in Object.FindObjectsOfType<Fire>())
                    if (fire != null && fire.GetComponent<Campfire>() != null
                        && Vector3.Distance(fire.transform.position,position) < .35f)
                    { cookingFire = fire; break; }
                if (cookingFire == null)
                {
                    var manager = GameManager.GetFireManagerComponent();
                    if (manager == null) { Tell("El fuego de cocina no esta disponible todavia."); return; }
                    cookingFire = manager.InstantiateCampFire();
                    if (cookingFire == null) { Tell("No se pudo preparar el fuego de cocina."); return; }
                    cookingFire.transform.position = position;
                    cookingFire.transform.rotation = root.transform.rotation;
                    pendingCookingOpen = true;
                    // Keep the native scale, colliders, slots, lighting and serialization.
                    // Do not parent the fire to the disposable workbench.
                    return; // Let native Start initialize before opening the panel next frame.
                }
            }
            var campfire = cookingFire.GetComponent<Campfire>();
            if (campfire == null) { Tell("No se encontro la interfaz de cocina."); return; }
            ConfigureCookingCentre();
            var host = cookingFire.GetComponent<FireplaceInteraction>();
            if (cookingConfigured && cookingFire.IsBurning() && host != null && host.m_CookingSlots != null && host.m_CookingSlots.Length > 0)
                host.m_CookingSlots[0].PerformInteraction();
            else campfire.PerformFireplaceInteraction();
        }
        catch (Exception e) { MelonLogger.Warning("Cocina del brasero: " + e); Tell("No se pudo abrir la cocina. Revisa Latest.log."); }
    }

    private void AddWorkbenchDecorations()
    {
        Decoration("GEAR_Hammer",new Vector3(.64f,1.07f,.10f),.35f);
        Decoration("GEAR_Hacksaw",new Vector3(-.44f,1.07f,.06f),.51f);
        Decoration("GEAR_Knife",new Vector3(.06f,1.07f,-.17f),.30f);
        Decoration("GEAR_Whetstone",new Vector3(-.20f,1.07f,.26f),.14f);
        Decoration("GEAR_Coal",new Vector3(-.50f,.34f,-.10f),.23f);
        Decoration("GEAR_Coal",new Vector3(-.23f,.34f,.12f),.20f);
        Decoration("GEAR_GutDried",new Vector3(-.27f,1.07f,-.22f),.22f);
        Decoration("GEAR_Toolkit",new Vector3(.46f,.34f,-.06f),.44f);
        Decoration("GEAR_Coal",new Vector3(1.89f,.03f,.28f),.22f);
        Decoration("GEAR_Coal",new Vector3(1.78f,.03f,.48f),.18f);
        Decoration("GEAR_ScrapMetal",new Vector3(1.0f,.03f,.54f),.24f);
    }

    private void Decoration(string gearName, Vector3 position, float size)
    {
        GameObject display = null;
        try
        {
            var prefab = GearItem.LoadGearItemPrefab(gearName);
            if (prefab == null) return;
            display = new GameObject("Decoration_" + gearName);
            display.transform.SetParent(root.transform,false);
            Bounds bounds = new Bounds(); bool first = true;
            foreach (var source in prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                var original = source.GetComponent<Renderer>();
                if (source.sharedMesh == null || original == null) continue;
                var mesh = new GameObject("VisualOnly");
                mesh.transform.SetParent(display.transform,false);
                mesh.transform.localPosition = prefab.transform.InverseTransformPoint(source.transform.position);
                mesh.transform.localRotation = Quaternion.Inverse(prefab.transform.rotation) * source.transform.rotation;
                Vector3 a=source.transform.lossyScale,b=prefab.transform.lossyScale;
                mesh.transform.localScale = new Vector3(a.x/b.x,a.y/b.y,a.z/b.z);
                mesh.AddComponent<MeshFilter>().sharedMesh=source.sharedMesh;
                var renderer=mesh.AddComponent<MeshRenderer>();
                renderer.sharedMaterials=original.sharedMaterials;
                renderer.receiveShadows=true;
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
                var box=source.sharedMesh.bounds;
                for(int i=0;i<8;i++)
                {
                    var corner=box.center+Vector3.Scale(box.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                    var point=display.transform.InverseTransformPoint(mesh.transform.TransformPoint(corner));
                    if(first){bounds=new Bounds(point,Vector3.zero);first=false;} else bounds.Encapsulate(point);
                }
            }
            if(first){Object.Destroy(display);return;}
            float factor=size/Mathf.Max(.001f,Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z)));
            display.transform.localScale=Vector3.one*factor;
            display.transform.localPosition=position-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)*factor;
            // Mesh-only decoration: no free items, scripts, colliders or save registrations.
        }
        catch(Exception e)
        {
            if(display!=null)Object.Destroy(display);
            MelonLogger.Warning("Adorno omitido " + gearName + ": " + e.Message);
        }
    }
}
