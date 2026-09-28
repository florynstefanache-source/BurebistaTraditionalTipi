using Il2Cpp;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TipiWorkbench;

public sealed partial class Main
{
    // Campfire.Update may reposition its original rock-based placement points.
    // Reapply only our single cooking anchor after native Update.
    public override void OnLateUpdate()
    {
        if(root==null || cookingFire==null || !cookingConfigured)return;
        try { AlignCentralCookware(); }
        catch(Exception e) { cookingConfigured=false; MelonLoader.MelonLogger.Warning("Ajuste de olla: " + e.Message); }
    }

    private void AlignCentralCookware()
    {
        var host=cookingFire.GetComponent<FireplaceInteraction>();
        if(host==null || host.m_CookingSlots==null || host.m_CookingSlots.Length!=1)return;
        var slot=host.m_CookingSlots[0];
        if(slot==null || slot.m_GearPlacePoint==null)return;
        var point=slot.m_GearPlacePoint;
        Vector3 centre=root.transform.TransformPoint(new Vector3(1.45f,.620f,0));
        slot.transform.position=centre;
        slot.transform.rotation=root.transform.rotation;
        point.transform.position=centre;
        point.transform.rotation=root.transform.rotation;
        var gear=point.m_PlacedGear;
        if(gear==null || gear.m_InPlayerInventory || !gear.gameObject.activeInHierarchy || !gear.IsAttachedToPlacePoint())return;
        // Match the actual vessel bottom, not its authored pivot or a campfire rock.
        Bounds bounds=new Bounds();bool found=false;
        foreach(var renderer in gear.GetComponentsInChildren<Renderer>())
        {
            if(renderer.TryCast<MeshRenderer>()==null && renderer.TryCast<SkinnedMeshRenderer>()==null)continue;
            if(!renderer.enabled)continue;
            if(!found){bounds=renderer.bounds;found=true;}else bounds.Encapsulate(renderer.bounds);
        }
        if(!found)return;
        Vector3 delta=centre-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
        if(delta.sqrMagnitude>.000001f)gear.transform.position+=delta;
    }

    private void UpdateCookingCentre()
    {
        if (Time.unscaledTime < nextCookingScan) return;
        nextCookingScan = Time.unscaledTime + .5f;
        if (cookingFire == null)
        {
            var position = root.transform.TransformPoint(new Vector3(1.45f,.50f,0));
            foreach (var fire in Object.FindObjectsOfType<Fire>())
                if (fire != null && fire.GetComponent<Campfire>() != null
                    && Vector3.Distance(position, fire.transform.position) < .35f)
                { cookingFire=fire; cookingConfigured=false; break; }
            if (cookingFire == null) { nextCookingScan=Time.unscaledTime+3f; return; }
        }
        ConfigureCookingCentre();
    }

    private void ConfigureCookingCentre()
    {
        if (cookingFire == null) return;
        // Retain the native fire ONLY as the cooking/heat/save backend.
        // Hide its logs, rock ring and effects without deactivating slot logic.
        // Never hide or disable pots, pans or food attached by the player.
        foreach (var renderer in cookingFire.GetComponentsInChildren<Renderer>(true))
            if (renderer.GetComponentInParent<GearItem>() == null) renderer.enabled=false;
        foreach (var light in cookingFire.GetComponentsInChildren<Light>(true))
            if (light.GetComponentInParent<GearItem>() == null) light.enabled=false;

        var campfire=cookingFire.GetComponent<Campfire>();
        if(campfire!=null)campfire.m_ValidatePlacePoints=false;
        if (cookingConfigured) { AlignCentralCookware(); return; }
        var host=cookingFire.GetComponent<FireplaceInteraction>();
        if(host==null || host.m_CookingSlots==null || host.m_CookingSlots.Length==0) return;
        var slots=host.m_CookingSlots;
        for(int oldIndex=1;oldIndex<slots.Length;oldIndex++)
            if(slots[oldIndex] is var slot && slot != null && slot.m_GearPlacePoint != null && slot.m_GearPlacePoint.m_PlacedGear != null)
            {
                if(!migrationWarning){Tell("Recoge los recipientes del fuego anterior para centrar la cocina del brasero.");migrationWarning=true;}
                return; // Preserve occupied old slots and their saved items until emptied.
            }
        var centre=slots[0];
        if(centre==null || centre.m_GearPlacePoint==null) return;
        foreach(var collider in cookingFire.GetComponentsInChildren<Collider>(true))
            if(collider.GetComponentInParent<GearItem>()==null) collider.enabled=false;
        Vector3 position=root.transform.TransformPoint(new Vector3(1.45f,.635f,0));
        centre.gameObject.SetActive(true);
        centre.enabled=true;
        centre.transform.position=position;
        centre.transform.rotation=root.transform.rotation;
        centre.m_GearPlacePoint.transform.position=position;
        centre.m_GearPlacePoint.transform.rotation=root.transform.rotation;
        centre.m_GearPlacePoint.enabled=true;
        centre.m_GearPlacePoint.gameObject.SetActive(true);
        // A single target above the existing iron supports, no campfire geometry.
        var target=centre.gameObject.GetComponent<BoxCollider>();
        if(target==null)target=centre.gameObject.AddComponent<BoxCollider>();
        target.center=Vector3.zero;
        target.size=new Vector3(.36f,.035f,.36f);
        target.isTrigger=false;
        target.enabled=true;
        for(int i=1;i<slots.Length;i++)
            if(slots[i]!=null)
            {
                slots[i].enabled=false;
                if(slots[i].m_GearPlacePoint!=null)slots[i].m_GearPlacePoint.enabled=false;
            }
        host.m_CookingSlots=new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<CookingSlot>(new[]{centre});
        cookingConfigured=true;
        AlignCentralCookware();
    }
}
