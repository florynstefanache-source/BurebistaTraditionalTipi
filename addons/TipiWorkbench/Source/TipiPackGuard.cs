using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TipiWorkbench;

public sealed partial class Main
{
    private static FieldInfo deployedTipiField, centralFireField;

    public override void OnLateInitializeMelon()
    {
        // Resolve the optional tipi mod once, after all mod assemblies are loaded.
        var type=AppDomain.CurrentDomain.GetAssemblies()
            .Select(a=>a.GetType("BurebistaTraditionalTipi.Main",false)).FirstOrDefault(t=>t!=null);
        if(type==null){MelonLogger.Warning("No se encontro el tipi para proteger su recogida.");return;}
        const BindingFlags flags=BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public;
        var pack=type.GetMethod("PackTipi",flags);
        deployedTipiField=type.GetField("tipi",flags);
        centralFireField=type.GetField("centralFire",flags);
        if(pack==null || deployedTipiField==null)
        {MelonLogger.Error("No se pudo instalar el bloqueo de recogida: version del tipi incompatible.");return;}
        HarmonyInstance.Patch(pack,prefix:new HarmonyMethod(typeof(Main),nameof(BeforePackTipi)));
        MelonLogger.Msg("Recogida del tipi bloqueada mientras ardan la fogata o la forja.");
    }

    private static bool BeforePackTipi()
    {
        try
        {
            var tipi=deployedTipiField?.GetValue(null) as GameObject;
            if(tipi==null)return true;
            bool liveForge=root!=null && root.transform.IsChildOf(tipi.transform);
            if(liveForge && Time.time<burningUntil)return RefusePacking();
            var central=centralFireField?.GetValue(null) as Component;
            var fire=central==null?null:central.GetComponent<Fire>();
            if(FireActive(fire))return RefusePacking();
            if(liveForge && FireActive(cookingFire))return RefusePacking();

            // Also catch saved native fires before this addon's scan has rebound
            // cookingFire, or when the tipi's centralFire reference is missing.
            Vector3 forge=liveForge
                ?root.transform.TransformPoint(new Vector3(1.45f,.50f,0))
                :tipi.transform.TransformPoint(new Vector3(4.2f,0,1.2f))
                    +tipi.transform.rotation*Quaternion.Euler(0,-25,0)*new Vector3(1.45f,.50f,0);
            foreach(var candidate in Object.FindObjectsOfType<Fire>())
            {
                if(candidate==null || candidate.GetComponent<Campfire>()==null)continue;
                if((NearFireAnchor(candidate.transform.position,tipi.transform.position)
                    || NearFireAnchor(candidate.transform.position,forge)) && FireActive(candidate))
                    return RefusePacking();
            }
            return true; // Let the original packing and save logic run untouched.
        }
        catch(Exception error)
        {
            MelonLogger.Warning("No se pudo verificar el fuego al recoger el tipi: " + error.Message);
            HUDMessage.AddMessage("No se pudo comprobar el fuego. El tipi no se ha recogido.");
            return false;
        }
    }
    private static bool NearFireAnchor(Vector3 position,Vector3 anchor)
    {
        Vector3 delta=position-anchor;
        return delta.x*delta.x+delta.z*delta.z<.45f*.45f && Mathf.Abs(delta.y)<2f;
    }
    private static bool FireActive(Fire fire) => fire!=null && fire.IsBurning();
    private static bool RefusePacking()
    {
        const string text="Apaga la fogata y la forja antes de recoger el tipi.";
        Tell(text);
        HUDMessage.AddMessage(text);
        return false;
    }
}

