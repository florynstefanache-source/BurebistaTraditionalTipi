using System.Text.Json;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using ModData;
using UnityEngine;
using Object=UnityEngine.Object;
[assembly:MelonInfo(typeof(BurebistaTipiHideBed.Main),"Burebista Tipi Hide Bed","0.1.1","Burebista")]
[assembly:MelonGame("Hinterland","TheLongDark")]
namespace BurebistaTipiHideBed;
public sealed class BedRecord { public int Rank {get;set;}=-1; public float Condition {get;set;}=1; }
public sealed class Main:MelonMod {
 static readonly string[] Keys=HideRules.Keys;
 static readonly string[] Names=HideRules.Names;
 static readonly float[] Warmth=HideRules.Warmth;
 readonly ModDataManager data=new("BurebistaTipiHideBed",false);
 BedRecord record=new();string slot="";bool loaded,fault,aimed;float next,readyAt;
 GameObject root;Bed bed;static Main self;
 public override void OnInitializeMelon(){self=this;MelonLogger.Msg("Cama del tipi: E al mirar la piel. Mejora automática ciervo > lobo > alce > oso; devuelve la piel anterior.");}
 public override void OnSceneWasLoaded(int index,string name){Clear();loaded=false;fault=false;readyAt=Time.realtimeSinceStartup+15;}
 void Clear(){if(root!=null)Object.Destroy(root);root=null;bed=null;aimed=false;}
 bool Load(){var current=SaveGameSystem.GetCurrentSaveName();if(string.IsNullOrEmpty(current))return false;
 if(current!=slot){Clear();slot=current;loaded=false;fault=false;}
 if(fault)return false;if(loaded)return true;
 try{if(!data.Save("1","ready"))return false;record=JsonSerializer.Deserialize<BedRecord>(data.Load("bed")??"{}")??new();if(record.Rank< -1||record.Rank>3||!float.IsFinite(record.Condition)||record.Condition<=0||record.Condition>1)throw new Exception("Estado de cama inválido");loaded=true;return true;}catch(Exception e){fault=true;MelonLogger.Error(e);return false;}}
 void Save(){if(slot!=SaveGameSystem.GetCurrentSaveName()||!data.Save(JsonSerializer.Serialize(record),"bed"))throw new Exception("No se pudo guardar la cama del tipi");}
 public override void OnUpdate(){
 aimed=false;try{
 var player=GameManager.GetPlayerManagerComponent();if(player==null)return;
 if(Time.realtimeSinceStartup>=next){next=Time.realtimeSinceStartup+3;
 if(Time.realtimeSinceStartup<readyAt||!Load())return;
 var tipi=GameObject.Find("BurebistaTraditionalTipi");
 if(tipi==null){Clear();return;}
 if(root!=null&&root.transform.parent!=tipi.transform)Clear();
 // Do not change the active bed or inventory while the player sleeps or uses menus.
 if(player.GetControlMode()==PlayerControlMode.Normal&&!InterfaceManager.GetInstance().AnyOverlayPanelEnabled()&&GameManager.GetPlayerTransform()!=null&&Vector3.Distance(GameManager.GetPlayerTransform().position,tipi.transform.position)<8)Improve(tipi.transform);
 if(record.Rank>=0&&root==null)Build(tipi.transform,record.Rank);
 }
 if(bed==null||root==null||Time.timeScale<=0||player.GetControlMode()!=PlayerControlMode.Normal||InterfaceManager.GetInstance().AnyOverlayPanelEnabled()||player.IsInPlacementMode())return;
 var camera=GameManager.GetMainCamera();if(camera==null)return;
 if(Physics.Raycast(camera.transform.position,camera.transform.forward,out RaycastHit hit,3.2f,~0,QueryTriggerInteraction.Ignore))aimed=hit.collider!=null&&hit.collider.transform.IsChildOf(root.transform);
 if(aimed&&Input.GetKeyDown(KeyCode.E))InterfaceManager.GetPanel<Panel_Rest>().StartRest(bed);
 }catch(Exception e){next=Time.realtimeSinceStartup+15;MelonLogger.Error("Cama de piel del tipi: "+e);}}
 GearItem FindHide(Inventory inventory,int rank){
 GearItem best=null;float condition=0;foreach(var item in inventory.m_Items){var gear=item?.m_GearItem;if(gear==null)continue;var key=item.m_GearItemName??gear.name;var dataName=gear.m_GearItemData?.name;bool matches=HideRules.Matches(key,rank)||HideRules.Matches(gear.name,rank)||HideRules.Matches(dataName,rank);if(matches&&gear.GetNormalizedCondition()>condition){best=gear;condition=gear.GetNormalizedCondition();}}return best;
 }
 void Improve(Transform parent){
 var inventory=GameManager.GetInventoryComponent();GearItem selected=null;int rank;
 rank=HideRules.Upgrade(record.Rank,i=>{var candidate=FindHide(inventory,i);if(candidate==null||candidate.GetNormalizedCondition()<=0)return false;selected=candidate;return true;});
 if(selected==null)return;
 // Resolve all assets before spending anything. Build failure leaves the old bed intact.
 var previous=record;var previousRoot=root;var previousBed=bed;root=null;bed=null;
 GearItem refund=null;bool removed=false,committed=false;
 try{
 Build(parent,rank);record=new BedRecord{Rank=rank,Condition=selected.GetNormalizedCondition()};
 if(previous.Rank>=0){var prefab=GearItem.LoadGearItemPrefab(Keys[previous.Rank]);if(prefab==null)throw new Exception("Falta la piel anterior para devolverla");refund=Object.Instantiate(prefab.gameObject).GetComponent<GearItem>();refund.gameObject.SetActive(false);refund.SetNormalizedHP(previous.Condition,false);ObjectGuid.MaybeAttachObjectGuidAddOrReplace(refund.gameObject,Guid.NewGuid().ToString());}
 inventory.RemoveGear(selected.gameObject,false);removed=true;
 if(inventory.GameObjectInInventory(selected.gameObject))throw new Exception("No se pudo retirar la piel de la mochila");
 if(refund!=null){refund.gameObject.SetActive(true);var added=GameManager.GetPlayerManagerComponent().AddItemToPlayerInventory(refund,false,true);if(added==null||!added.m_InPlayerInventory)throw new Exception("No se pudo devolver la piel anterior");refund=added;}
 Save();committed=true;
 }finally{
 if(!committed){record=previous;if(refund!=null){if(refund.m_InPlayerInventory)inventory.RemoveGear(refund.gameObject,false);Object.Destroy(refund.gameObject);}if(removed&&!inventory.GameObjectInInventory(selected.gameObject))inventory.AddGear(selected,false);Clear();root=previousRoot;bed=previousBed;}
 }
 Object.Destroy(selected.gameObject);if(previousRoot!=null)Object.Destroy(previousRoot);
 HUDMessage.AddMessage("Cama del tipi: piel de "+Names[rank]+" · +"+Warmth[rank]+" °C"+(previous.Rank>=0?". Piel anterior devuelta.":""));
 }
 void Build(Transform parent,int rank){
 var bedPrefab=GearItem.LoadGearItemPrefab("GEAR_BedRoll");var hidePrefab=GearItem.LoadGearItemPrefab(Keys[rank]);
 if(bedPrefab==null||hidePrefab==null)throw new Exception("No se han cargado las piezas nativas de la cama");
 var built=new GameObject("Burebista_Tipi_HideBed");built.transform.SetParent(parent,false);built.transform.localPosition=new Vector3(0,.08f,-1.75f);
 try{
 var native=Object.Instantiate(bedPrefab.gameObject,built.transform);native.name="NativeSleep";native.SetActive(true);native.transform.localPosition=Vector3.zero;
 var sleeping=native.GetComponent<Bed>();if(sleeping==null)throw new Exception("El saco nativo no contiene el sistema de descanso");
 sleeping.SetState(BedRollState.Placed);DisableGear(native);
 foreach(var renderer in native.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
 sleeping.m_Bedroll=null;sleeping.m_WarmthBonusCelsius=Warmth[rank];sleeping.m_ConditionPercentGainPerHour=3;sleeping.m_UinterruptedRestPercentGainPerHour=12;
 var hide=Object.Instantiate(hidePrefab.gameObject,built.transform);hide.name="CuredHide";hide.transform.localPosition=Vector3.zero;hide.transform.localRotation=Quaternion.identity;hide.SetActive(true);DisableGear(hide);
 // Fit the native pelt to a human-sized bed, preserving its natural proportions.
 var renderers=hide.GetComponentsInChildren<Renderer>(true);bool any=false;Bounds bounds=new();foreach(var renderer in renderers){if(!renderer.enabled)continue;if(!any){bounds=renderer.bounds;any=true;}else bounds.Encapsulate(renderer.bounds);}
 if(!any)throw new Exception("La piel nativa no tiene un modelo visible");
 var size=bounds.size;float length=Mathf.Max(size.x,size.z);if(length<.001f)throw new Exception("Tamaño de piel nativa inválido");
 hide.transform.localScale*=2.15f/length;
 // Normalize orientation and center on the back wall, away from the central fire.
 if(size.z>size.x)hide.transform.localRotation=Quaternion.Euler(0,90,0);
 bounds=renderers[0].bounds;foreach(var renderer in renderers)if(renderer.enabled)bounds.Encapsulate(renderer.bounds);
 var offset=built.transform.InverseTransformVector(built.transform.position-bounds.center);hide.transform.localPosition+=new Vector3(offset.x,0,offset.z);
 float bottom=built.transform.InverseTransformPoint(new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)).y;hide.transform.localPosition+=Vector3.up*(-bottom);
 var collider=built.AddComponent<BoxCollider>();collider.center=new Vector3(0,.07f,0);collider.size=new Vector3(2.2f,.14f,1.15f);
 root=built;bed=sleeping;
 }catch{Object.Destroy(built);throw;}}
 static void DisableGear(GameObject obj){var gear=obj.GetComponent<GearItem>();if(gear!=null){gear.NonInteractive=true;gear.SetInteractive(false,false);gear.ToggleColliders(false);gear.enabled=false;}foreach(var c in obj.GetComponentsInChildren<Collider>(true))c.enabled=false;}
 public override void OnGUI(){if(aimed&&record.Rank>=0)GUI.Box(new Rect(Screen.width/2-230,Screen.height-115,460,40),"E — Dormir · piel de "+Names[record.Rank]+" · +"+Warmth[record.Rank]+" °C");}
 [HarmonyPatch(typeof(Panel_Rest),nameof(Panel_Rest.OnPickUp))]static class PreventPickup {static bool Prefix(Panel_Rest __instance)=>self==null||self.bed==null||__instance.m_Bed!=self.bed;}
 [HarmonyPatch(typeof(Panel_Rest),nameof(Panel_Rest.UpdateVisuals))]static class HidePickup {static void Postfix(Panel_Rest __instance){if(self?.bed!=null&&__instance.m_Bed==self.bed&&__instance.m_PickUpButton!=null)__instance.m_PickUpButton.SetActive(false);}}
 [HarmonyPatch(typeof(Bed),nameof(Bed.Degrade))]static class NoWear {static bool Prefix(Bed __instance)=>self?.bed==null||__instance!=self.bed;}
 [HarmonyPatch(typeof(Bed),nameof(Bed.IsWornOut))]static class NotRuined {static bool Prefix(Bed __instance,ref bool __result){if(self?.bed==null||__instance!=self.bed)return true;__result=false;return false;}}
 [HarmonyPatch(typeof(Bed),nameof(Bed.GetWarmthBonusCelsius))]static class BedWarmth {static bool Prefix(Bed __instance,ref float __result){if(self?.bed==null||__instance!=self.bed||self.record.Rank<0)return true;__result=Warmth[self.record.Rank];return false;}}
}



