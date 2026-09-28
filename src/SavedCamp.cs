using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using ModData;
using UnityEngine;
using Object=UnityEngine.Object;
namespace SavedCamp;
public sealed class Entry { public string Scene{get;set;}=""; public float[] Position{get;set;}=new float[3]; public float[] Rotation{get;set;}=new float[4]; public bool Door{get;set;}=true; }
public sealed class Main:MelonMod {
 readonly HarmonyLib.Harmony patcher=new HarmonyLib.Harmony("BurebistaTraditionalTipi.Saved"); public bool IsReady=>ready; static Main self; readonly ModDataManager data=new("BurebistaTraditionalTipiSaved",false);
 Dictionary<string,Entry> entries; readonly Dictionary<string,Type> types=new(); string scene=""; bool ready,restoring; int delay; float next;
 const BindingFlags Flags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 public override void OnInitializeMelon(){self=this;}
 static object Call(Type t,string name,params object[] args)=>t.GetMethod(name,Flags).Invoke(null,args);
 static FieldInfo Field(Type t,string name)=>t.GetField(name,Flags);
 static string Root(string kind)=>kind=="tipi"?"tipi":"smoker";
 GameObject Obj(string kind)=>Field(types[kind],Root(kind)).GetValue(null) as GameObject;
 public override void OnSceneWasInitialized(int index,string name){
 if(types.Count==0){foreach(var save in typeof(SaveGameSlots).GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static).Where(m=>m.Name=="WriteSlotToDisk")) patcher.Patch(save,prefix:new HarmonyMethod(typeof(Main),nameof(Changed)));foreach(var pair in new[]{("tipi","BurebistaTraditionalTipi.Main")}){
 var t=AccessTools.TypeByName(pair.Item2);if(t==null)continue;types[pair.Item1]=t;
 
 foreach(var method in pair.Item1=="tipi"?new[]{"PlaceTipi","PackTipi","SetDoorState"}:new[]{"Place","Pack"}) patcher.Patch(AccessTools.Method(t,method),postfix:new HarmonyMethod(typeof(Main),nameof(Changed)));
 }}
 delay=90;ready=false;
 }
 static bool SkipCleanup()=>false;
 static void Changed(){if(self.ready&&!self.restoring)self.Capture();}
 public override void OnSceneWasUnloaded(int index,string name){ready=false;}
 public override void OnSceneWasLoaded(int index,string name){
 ready=false;foreach(var kind in types.Keys){var t=types[kind];var root=Obj(kind);if(root!=null)Object.Destroy(root);Field(t,Root(kind)).SetValue(null,null);Field(t,kind=="tipi"?"centralFire":"fire").SetValue(null,null);}
 entries=null;scene="";delay=90;
 }
 public override void OnUpdate(){
 if(delay-->0)return;
 if(GameManager.GetPlayerTransform()==null||GameManager.GetInventoryComponent()==null)return;
 string current=GameManager.m_ActiveScene;
 if(string.IsNullOrEmpty(current)||current.Contains("MainMenu")||current=="Empty")return;
 try{
 if(!ready){scene=current;var json=data.Load();entries=string.IsNullOrEmpty(json)?new():JsonSerializer.Deserialize<Dictionary<string,Entry>>(json);if(entries==null)throw new Exception("Invalid saved camp data");
 restoring=true;try{foreach(var kind in types.Keys)if(entries.TryGetValue(kind+"|"+scene,out var e)&&Obj(kind)==null)Restore(kind,e);}finally{restoring=false;}ready=true;}
 if(Time.realtimeSinceStartup>=next){next=Time.realtimeSinceStartup+1;Capture();}
 }catch(Exception ex){MelonLogger.Error("Guardado campamento: "+ex);delay=180;}
 }
 void Capture(){if(!ready||entries==null)return;foreach(var kind in types.Keys){string key=kind+"|"+scene;var o=Obj(kind);if(o==null){entries.Remove(key);continue;}var p=o.transform.position;var r=o.transform.rotation;entries[key]=new Entry{Scene=scene,Position=new[]{p.x,p.y,p.z},Rotation=new[]{r.x,r.y,r.z,r.w},Door=kind!="tipi"||(bool)Field(types[kind],"doorClosed").GetValue(null)};}if(!data.Save(JsonSerializer.Serialize(entries)))MelonLogger.Warning("Campamento pendiente: no hay partida activa para guardar.");}
 void Restore(string kind,Entry e){
 if(e.Position.Length!=3||e.Rotation.Length!=4)throw new Exception("Invalid camp coordinates");
 var t=types[kind];Call(t,kind=="tipi"?"CreateMaterials":"Materials");var o=new GameObject(kind=="tipi"?"BurebistaTraditionalTipi":"BurebistaPortableSmoker");Field(t,Root(kind)).SetValue(null,o);
 o.transform.position=new Vector3(e.Position[0],e.Position[1],e.Position[2]);o.transform.rotation=new Quaternion(e.Rotation[0],e.Rotation[1],e.Rotation[2],e.Rotation[3]);
 foreach(var method in kind=="tipi"?new[]{"CreateHideCover","CreateFixedFrontCover","CreatePoles","CreateSmokeFlaps","CreateDoorFlaps","CreateLashing","CreateInteriorProtection"}:new[]{"BuildFrame","BuildCover","BuildRack","BuildFood","BuildSmoke"})Call(t,method,o.transform);
 Fire near=null;foreach(var f in Object.FindObjectsOfType<Fire>())if(Vector3.Distance(f.transform.position,o.transform.position)<.3f){near=f;break;}
 if(near!=null)Field(t,kind=="tipi"?"centralFire":"fire").SetValue(null,near);else Call(t,kind=="tipi"?"CreateCentralCampfire":"CreateNativeFire");
 if(kind=="tipi")Call(t,"SetDoorState",e.Door);Object.DontDestroyOnLoad(o);MelonLogger.Msg("Restaurado "+kind+" en "+scene);
 }
}




