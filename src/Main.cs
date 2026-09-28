using System;
using Object=UnityEngine.Object;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BurebistaTraditionalTipi;

public sealed class Main : MelonMod
{
	private static GameObject tipi;

	private static GameObject doorFlaps;

	private static GameObject rolledDoor;

	private static Component centralFire;

	private static bool doorClosed = true;
	private static bool showHints = true;

	private static Material hideMaterial;

	private static Material woodMaterial;

	private static Material wolfMaterial;

	private static string assetDirectory;

	private const string PackedTipiGear = "GEAR_BurebistaPackedTipi";

	private static readonly Dictionary<string, Type> Types = new Dictionary<string, Type>();

	private readonly SavedCamp.Main persistence=new();
    public override void OnSceneWasInitialized(int i,string n)=>persistence.OnSceneWasInitialized(i,n);
    public override void OnSceneWasUnloaded(int i,string n)=>persistence.OnSceneWasUnloaded(i,n);
    public override void OnInitializeMelon()
	{
persistence.OnInitializeMelon();
		assetDirectory = Path.Combine(AppContext.BaseDirectory, "Mods", "BurebistaTraditionalTipi");
		MelonLogger.Msg("F3: desplegar | F4: girar | F2: empaquetar | Ctrl+F6: recibir uno para pruebas");
	}

	public override void OnSceneWasLoaded(int index,string name){persistence.OnSceneWasLoaded(index,name);}
public override void OnUpdate()
	{
persistence.OnUpdate(); if(!persistence.IsReady)return;
		if (Playable())
		{
            if (Input.GetKeyDown(KeyCode.F9)) showHints = !showHints;
			if (Input.GetKeyDown((KeyCode)284))
			{
				PlaceTipi();
			}
			if (Input.GetKeyDown(KeyCode.F6) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				GivePackedTipi();
				Message("Prueba: tipi empaquetado añadido a la mochila");
			}
			if (Input.GetKeyDown((KeyCode)285) && (Object)(object)tipi != (Object)null)
			{
				tipi.transform.Rotate(0f, 15f, 0f, (Space)0);
				Message("Tipi girado 15 grados");
			}
			if (Input.GetKeyDown((KeyCode)101) && (Object)(object)tipi != (Object)null && PlayerDistance() <= 4.5f)
			{
				ToggleDoor();
			}
			if (Input.GetKeyDown((KeyCode)283))
			{
				PackTipi(notify: true, returnToInventory: true);
			}
		}
	}

	public override void OnGUI()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (showHints && Playable() && !((Object)(object)tipi == (Object)null) && !(PlayerDistance() > 4.5f))
		{
			GUIStyle val = new GUIStyle(GUI.skin.label);
			val.fontSize = 18;
			val.fontStyle = (FontStyle)1;
			val.alignment = (TextAnchor)4;
			val.normal.textColor = Color.white;
			GUI.Label(new Rect((float)Screen.width * 0.5f - 210f, (float)Screen.height - 170f, 420f, 42f), doorClosed ? "E  ENROLLAR Y ABRIR LA PIEL" : "E  SOLTAR Y CERRAR LA PIEL", val);
		}
	}

	private static void PlaceTipi()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		Transform player = GetPlayer();
		if ((Object)(object)player == (Object)null)
		{
			Message("No se encontró al jugador");
			return;
		}
		if ((Object)(object)tipi != (Object)null)
		{
			Message("El tipi ya está desplegado");
			return;
		}
		if (!TakePackedTipi())
		{
			Message("Necesitas el tipi empaquetado en la mochila");
			return;
		}
		try
		{
			CreateMaterials();
			tipi = new GameObject("BurebistaTraditionalTipi");
			Vector3 forward = player.forward;
			forward.y = 0f;
			if (forward.sqrMagnitude < 0.01f)
			{
				forward = Vector3.forward;
			}
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
			tipi.transform.localScale = Vector3.one;
			CreateCentralCampfire();
			Object.DontDestroyOnLoad((Object)(object)tipi);
			SetDoorState(closed: true);
			Message("Tipi tradicional colocado | F4 girar | F2 retirar");
			MelonLogger.Msg("Tipi creado en " + ((object)tipi.transform.position/*cast due to constrained. prefix*/).ToString());
		}
		catch (Exception ex)
		{
			MelonLogger.Error("No se pudo crear el tipi: " + ex);
			Message("Error creando el tipi; revisa el registro de MelonLoader");
			RemoveTipi(notify: false);
			GivePackedTipi();
		}
	}

	private static void CreateHideCover(Transform parent)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> list = new List<Vector3>();
		List<Vector2> list2 = new List<Vector2>();
		List<int> list3 = new List<int>();
		for (int i = 0; i <= 48; i++)
		{
			float num = (-180f + 360f * (float)i / 48f) * Mathf.Deg2Rad;
			list.Add(new Vector3(Mathf.Sin(num) * 3.05f, 0f, Mathf.Cos(num) * 3.05f));
			list.Add(new Vector3(Mathf.Sin(num) * 0.38f, 4.8f, Mathf.Cos(num) * 0.38f));
			list2.Add(new Vector2((float)i / 48f * 3f, 0f));
			list2.Add(new Vector2((float)i / 48f * 3f, 1f));
		}
		for (int j = 0; j < 48; j++)
		{
			if (!(Mathf.Abs(-180f + 360f * ((float)j + 0.5f) / 48f) < 27f))
			{
				int num2 = j * 2;
				int b = num2 + 1;
				int num3 = num2 + 2;
				int c = num2 + 3;
				AddDouble(list3, num2, b, num3);
				AddDouble(list3, num3, b, c);
			}
		}
		Mesh sharedMesh = NewMesh("TipiHideCover", list, list2, list3);
		GameObject val = new GameObject("Hide covering");
		val.transform.SetParent(parent, false);
		val.AddComponent<MeshFilter>().sharedMesh = sharedMesh;
		((Renderer)val.AddComponent<MeshRenderer>()).sharedMaterial = hideMaterial;
		MeshCollider obj = val.AddComponent<MeshCollider>();
		obj.sharedMesh = sharedMesh;
		obj.convex = false;
	}

	private static void CreatePoles(Transform parent)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		Vector3 start = default(Vector3);
		Vector3 end = default(Vector3);
		for (int i = 0; i < 14; i++)
		{
			float num = 360f * (float)i / 14f;
			if (!(Mathf.Abs((num > 180f) ? (num - 360f) : num) < 34f))
			{
				float num2 = num * Mathf.Deg2Rad;
				start=new Vector3(Mathf.Sin(num2) * 2.9f, 0.03f, Mathf.Cos(num2) * 2.9f);
				end=new Vector3(Mathf.Sin(num2) * 0.16f, 5.95f, Mathf.Cos(num2) * 0.16f);
				CreatePole(parent, start, end, 0.055f, "Lodge pole");
			}
		}
	}

	private static void CreateFixedFrontCover(Transform parent)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Lerp(3.05f, 0.38f, 0.46874997f);
		List<Vector3> list = new List<Vector3>();
		List<Vector2> list2 = new List<Vector2>();
		List<int> list3 = new List<int>();
		for (int i = 0; i <= 10; i++)
		{
			float num2 = Mathf.Lerp(-27f, 27f, (float)i / 10f) * Mathf.Deg2Rad;
			list.Add(new Vector3(Mathf.Sin(num2) * num, 2.25f, Mathf.Cos(num2) * num));
			list.Add(new Vector3(Mathf.Sin(num2) * 0.38f, 4.8f, Mathf.Cos(num2) * 0.38f));
			list2.Add(new Vector2((float)i / 10f * 1.2f, 0.46874997f));
			list2.Add(new Vector2((float)i / 10f * 1.2f, 1f));
		}
		for (int j = 0; j < 10; j++)
		{
			int num3 = j * 2;
			int b = num3 + 1;
			int num4 = num3 + 2;
			int c = num3 + 3;
			AddDouble(list3, num3, b, num4);
			AddDouble(list3, num4, b, c);
		}
		Mesh sharedMesh = NewMesh("Fixed stitched front cover", list, list2, list3);
		GameObject val = new GameObject("Fixed stitched front cover");
		val.transform.SetParent(parent, false);
		val.AddComponent<MeshFilter>().sharedMesh = sharedMesh;
		((Renderer)val.AddComponent<MeshRenderer>()).sharedMaterial = hideMaterial;
		MeshCollider obj = val.AddComponent<MeshCollider>();
		obj.sharedMesh = sharedMesh;
		obj.convex = false;
	}

	private static void CreateSmokeFlaps(Transform parent)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		CreatePanel(parent, "Left smoke flap", new Vector3(-0.38f, 3.95f, 0.16f), new Vector3(-1.55f, 4.88f, 0.24f), new Vector3(-0.34f, 5.45f, 0.1f));
		CreatePanel(parent, "Right smoke flap", new Vector3(0.38f, 3.95f, 0.16f), new Vector3(0.34f, 5.45f, 0.1f), new Vector3(1.55f, 4.88f, 0.24f));
		CreatePole(parent, new Vector3(-1.5f, 4.84f, 0.26f), new Vector3(-2.35f, 0.15f, 2.1f), 0.038f, "Smoke flap pole");
		CreatePole(parent, new Vector3(1.5f, 4.84f, 0.26f), new Vector3(2.35f, 0.15f, 2.1f), 0.038f, "Smoke flap pole");
	}

	private static void CreateDoorFlaps(Transform parent)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		doorFlaps = new GameObject("Closed hide entrance");
		doorFlaps.transform.SetParent(parent, false);
		CreateCurvedEntranceFlap(doorFlaps.transform);
		CreateWolfPainting(doorFlaps.transform);
		rolledDoor = GameObject.CreatePrimitive((PrimitiveType)2);
		((Object)rolledDoor).name = "Rolled tied hide door";
		rolledDoor.transform.SetParent(parent, false);
		rolledDoor.transform.localPosition = new Vector3(0f, 2.32f, 1.82f);
		rolledDoor.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
		rolledDoor.transform.localScale = new Vector3(0.17f, 1.02f, 0.17f);
		rolledDoor.GetComponent<Renderer>().sharedMaterial = hideMaterial;
	}

	private static void CreateCurvedEntranceFlap(Transform parent)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Lerp(3.05f, 0.38f, 0.48749995f) + 0.025f;
		List<Vector3> list = new List<Vector3>();
		List<Vector2> list2 = new List<Vector2>();
		List<int> list3 = new List<int>();
		for (int i = 0; i <= 12; i++)
		{
			float num2 = (float)i / 12f;
			float num3 = Mathf.Lerp(-32f, 32f, num2) * Mathf.Deg2Rad;
			list.Add(new Vector3(Mathf.Sin(num3) * 3.075f, 0.025f, Mathf.Cos(num3) * 3.075f));
			list.Add(new Vector3(Mathf.Sin(num3) * num, 2.34f, Mathf.Cos(num3) * num));
			list2.Add(new Vector2(num2 * 1.35f, 0f));
			list2.Add(new Vector2(num2 * 1.35f, 0.48749995f));
		}
		for (int j = 0; j < 12; j++)
		{
			int num4 = j * 2;
			int b = num4 + 1;
			int num5 = num4 + 2;
			int c = num4 + 3;
			AddDouble(list3, num4, b, num5);
			AddDouble(list3, num5, b, c);
		}
		Mesh sharedMesh = NewMesh("Curved overlapping hide entrance", list, list2, list3);
		GameObject val = new GameObject("Curved overlapping hide entrance");
		val.transform.SetParent(parent, false);
		val.AddComponent<MeshFilter>().sharedMesh = sharedMesh;
		((Renderer)val.AddComponent<MeshRenderer>()).sharedMaterial = hideMaterial;
		MeshCollider obj = val.AddComponent<MeshCollider>();
		obj.sharedMesh = sharedMesh;
		obj.convex = false;
	}

	private static void CreateWolfPainting(Transform parent)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Lerp(3.05f, 0.38f, 0.099999994f) + 0.055f;
		float num2 = Mathf.Lerp(3.05f, 0.38f, 0.39166665f) + 0.055f;
		List<Vector3> list = new List<Vector3>();
		List<Vector2> list2 = new List<Vector2>();
		List<int> list3 = new List<int>();
		for (int i = 0; i <= 10; i++)
		{
			float num3 = (float)i / 10f;
			float num4 = Mathf.Lerp(-19f, 19f, num3) * Mathf.Deg2Rad;
			list.Add(new Vector3(Mathf.Sin(num4) * num, 0.48f, Mathf.Cos(num4) * num));
			list.Add(new Vector3(Mathf.Sin(num4) * num2, 1.88f, Mathf.Cos(num4) * num2));
			list2.Add(new Vector2(num3, 0f));
			list2.Add(new Vector2(num3, 1f));
		}
		for (int j = 0; j < 10; j++)
		{
			int num5 = j * 2;
			int b = num5 + 1;
			int num6 = num5 + 2;
			int c = num5 + 3;
			AddDouble(list3, num5, b, num6);
			AddDouble(list3, num6, b, c);
		}
		Mesh sharedMesh = NewMesh("Painted wolf decal", list, list2, list3);
		GameObject val = new GameObject("Painted wolf on closed hide");
		val.transform.SetParent(parent, false);
		val.AddComponent<MeshFilter>().sharedMesh = sharedMesh;
		((Renderer)val.AddComponent<MeshRenderer>()).sharedMaterial = wolfMaterial;
	}

	private static void CreateInteriorProtection(Transform parent)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("Tipi interior warmth and wind protection");
		val.transform.SetParent(parent, false);
		val.transform.localPosition = new Vector3(0f, 1.25f, 0f);
		SphereCollider val2 = val.AddComponent<SphereCollider>();
		((Collider)val2).isTrigger = true;
		val2.radius = 2.18f;
		HeatSource obj = val.AddComponent<HeatSource>();
		obj.m_MaxTempIncrease = 10f;
		obj.m_MaxTempIncreaseInnerRadius = 2.05f;
		obj.m_MaxTempIncreaseOuterRadius = 2.3f;
		obj.m_TimeToReachMaxTempMinutes = 0f;
		obj.m_StartingTemp = 10f;
		obj.m_StartOn = true;
		obj.TurnOn();
		val.AddComponent<WindKiller>().m_Collider = (Collider)(object)val2;
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
		if ((Object)(object)doorFlaps != (Object)null)
		{
			doorFlaps.SetActive(closed);
		}
		if ((Object)(object)rolledDoor != (Object)null)
		{
			rolledDoor.SetActive(!closed);
		}
	}

	private static float PlayerDistance()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Transform player = GetPlayer();
		if (!((Object)(object)player != (Object)null) || !((Object)(object)tipi != (Object)null))
		{
			return 999f;
		}
		return Vector3.Distance(player.position, tipi.transform.position);
	}

	private static void CreateLashing(Transform parent)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = GameObject.CreatePrimitive((PrimitiveType)2);
		((Object)obj).name = "Pole lashing";
		obj.transform.SetParent(parent, false);
		obj.transform.localPosition = new Vector3(0f, 5.02f, 0f);
		obj.transform.localScale = new Vector3(0.31f, 0.065f, 0.31f);
		obj.GetComponent<Renderer>().sharedMaterial = woodMaterial;
	}

	private static void CreateCentralCampfire()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		object obj = FindType("GameManager")?.GetMethod("GetFireManagerComponent", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, null);
		if (obj == null)
		{
			throw new Exception("No se encontró FireManager");
		}
		object obj2 = obj.GetType().GetMethod("InstantiateCampFire", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(obj, null);
		centralFire = (Component)((obj2 is Component) ? obj2 : null);
		if ((Object)(object)centralFire == (Object)null)
		{
			throw new Exception("El juego no pudo crear la fogata nativa");
		}
		centralFire.transform.position = tipi.transform.position + Vector3.up * 0.04f;
		centralFire.transform.rotation = Quaternion.identity;
		MethodInfo method = obj2.GetType().GetMethod("TurnOffImmediate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (method != null)
		{
			method.Invoke(obj2, null);
		}
		MelonLogger.Msg("Fogata nativa apagada creada en el centro del tipi");
	}

	private static void CreatePanel(Transform parent, string name, Vector3 a, Vector3 b, Vector3 c)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> vertices = new List<Vector3> { a, b, c };
		List<Vector2> uvs = new List<Vector2>
		{
			new Vector2(0.5f, 1f),
			Vector2.zero,
			Vector2.right
		};
		List<int> list = new List<int>();
		AddDouble(list, 0, 1, 2);
		Mesh sharedMesh = NewMesh(name, vertices, uvs, list);
		GameObject val = new GameObject(name);
		val.transform.SetParent(parent, false);
		val.AddComponent<MeshFilter>().sharedMesh = sharedMesh;
		((Renderer)val.AddComponent<MeshRenderer>()).sharedMaterial = hideMaterial;
	}

	private static void CreatePole(Transform parent, Vector3 start, Vector3 end, float radius, string name)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = GameObject.CreatePrimitive((PrimitiveType)2);
		((Object)obj).name = name;
		obj.transform.SetParent(parent, false);
		Vector3 val = end - start;
		obj.transform.localPosition = (start + end) * 0.5f;
		obj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, val.normalized);
		obj.transform.localScale = new Vector3(radius, val.magnitude * 0.5f, radius);
		obj.GetComponent<Renderer>().sharedMaterial = woodMaterial;
	}

	private static Mesh NewMesh(string name, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		Mesh val = new Mesh
		{
			name = name
		};
		int count = vertices.Count;
		List<Vector3> val2 = new List<Vector3>();
		foreach (Vector3 vertex in vertices)
		{
			val2.Add(vertex);
		}
		foreach (Vector3 vertex2 in vertices)
		{
			val2.Add(vertex2);
		}
		List<Vector2> val3 = new List<Vector2>();
		foreach (Vector2 uv in uvs)
		{
			val3.Add(uv);
		}
		foreach (Vector2 uv2 in uvs)
		{
			val3.Add(uv2);
		}
		List<int> val4 = new List<int>();
		foreach (int triangle in triangles)
		{
			val4.Add(triangle);
		}
		for (int i = 0; i + 2 < triangles.Count; i += 3)
		{
			val4.Add(triangles[i] + count);
			val4.Add(triangles[i + 2] + count);
			val4.Add(triangles[i + 1] + count);
		}
		val.vertices=val2.ToArray();
		val.uv=val3.ToArray();
		val.triangles=val4.ToArray();
		val.RecalculateNormals();
		val.RecalculateBounds();
		return val;
	}

	private static void AddDouble(List<int> values, int a, int b, int c)
	{
		values.Add(a);
		values.Add(c);
		values.Add(b);
	}

	private static void CreateMaterials()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Expected O, but got Unknown
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Expected O, but got Unknown
		Shader val = Shader.Find("Legacy Shaders/Diffuse") ?? Shader.Find("Standard") ?? Shader.Find("Unlit/Texture");
		if ((Object)(object)val == (Object)null)
		{
			throw new Exception("No hay shader compatible");
		}
		hideMaterial = new Material(val)
		{
			name = "Tipi hide",
			color = Color.white
		};
		if (hideMaterial.HasProperty("_Cull"))
		{
			hideMaterial.SetInt("_Cull", 2);
		}
		string text = Path.Combine(assetDirectory, "tipi_hide.png");
		if (!File.Exists(text))
		{
			throw new FileNotFoundException("Falta tipi_hide.png", text);
		}
		Texture2D val2 = new Texture2D(2, 2, (TextureFormat)4, false);
		if (!ImageConversion.LoadImage(val2, (File.ReadAllBytes(text))))
		{
			throw new Exception("No se pudo leer la textura");
		}
		((Texture)val2).wrapMode = (TextureWrapMode)0;
		((Texture)val2).filterMode = (FilterMode)1;
		hideMaterial.mainTexture = (Texture)(object)val2;
		woodMaterial = new Material(Shader.Find("Legacy Shaders/Diffuse") ?? Shader.Find("Standard") ?? val)
		{
			name = "Tipi poles",
			color = new Color(0.24f, 0.13f, 0.055f, 1f)
		};
		string text2 = Path.Combine(assetDirectory, "tipi_wolf.png");
		if (!File.Exists(text2))
		{
			throw new FileNotFoundException("Falta tipi_wolf.png", text2);
		}
		Texture2D val3 = new Texture2D(2, 2, (TextureFormat)4, false);
		if (!ImageConversion.LoadImage(val3, (File.ReadAllBytes(text2))))
		{
			throw new Exception("No se pudo leer el dibujo del lobo");
		}
		((Texture)val3).wrapMode = (TextureWrapMode)1;
		((Texture)val3).filterMode = (FilterMode)1;
		wolfMaterial = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse") ?? Shader.Find("Unlit/Transparent") ?? val)
		{
			name = "Painted wolf decal",
			color = Color.white,
			mainTexture = (Texture)(object)val3
		};
		wolfMaterial.SetOverrideTag("RenderType", "Transparent");
		wolfMaterial.renderQueue = 3000;
		// Keep the original stitched-hide texture loaded above.
		woodMaterial = ApplyGameMaterial(woodMaterial, "GEAR_Stick");
	}

    // Clone the COMPLETE native material, retaining its shader and texture bindings.
    // Do not instantiate GPU-only textures: their cloned contents may render black.
    // Never mutate the game's shared textures or original material.
    private static Material ApplyGameMaterial(Material fallback, string gearName)
    {
        try
        {
            GearItem prefab = GearItem.LoadGearItemPrefab(gearName);
            if (prefab != null)
                foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
                    foreach (Material source in renderer.sharedMaterials)
                    {
                        if (source == null || source.shader == null || source.mainTexture == null) continue;
                        Material material = new Material(source);
                        material.name = "Tipi_TLD_" + gearName;
                        // The cover is a solid skin, not a cutout billboard.
                        if (material.HasProperty("_Cull")) material.SetInt("_Cull", 2);
                        if (material.HasProperty("_Cutoff")) material.SetFloat("_Cutoff", 0f);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_EMISSION");
                        if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", Color.black);
                        MelonLogger.Msg("Material TLD completo para " + gearName + ": " + source.shader.name + ", textura " + source.mainTexture.name);
                        // Only the old locally-loaded fallback owns its texture.
                        Texture oldTexture = fallback.mainTexture;
                        Object.Destroy(fallback);
                        if (oldTexture != null) Object.Destroy(oldTexture);
                        return material;
                    }
            MelonLogger.Warning("Material TLD no disponible: " + gearName + "; se conserva el acabado original.");
        }
        catch (Exception error)
        {
            MelonLogger.Warning("Se conserva el acabado original del tipi: " + error.Message);
        }
        return fallback;
    }

	private static Vector3 GroundPoint(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(position + Vector3.up * 3f, Vector3.down, out val, 10f))
		{
			position.y = val.point.y + 0.02f;
		}
		else
		{
			position.y -= 1.6f;
		}
		return position;
	}

	private static void RemoveTipi(bool notify)
	{
		if ((Object)(object)centralFire != (Object)null)
		{
			try
			{
				object obj = centralFire;
				MethodInfo method = obj.GetType().GetMethod("IsBurning", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (!(method != null) || !(bool)method.Invoke(obj, null))
				{
					Object.Destroy((Object)(object)centralFire.gameObject);
				}
				else if (notify)
				{
					Message("La fogata encendida permanece en el terreno");
				}
			}
			catch
			{
			}
		}
		centralFire = null;
		if ((Object)(object)tipi != (Object)null)
		{
			Object.Destroy((Object)(object)tipi);
		}
		tipi = null;
		doorFlaps = null;
		rolledDoor = null;
		doorClosed = true;
		if (notify)
		{
			Message("Tipi retirado");
		}
	}

	private static void PackTipi(bool notify, bool returnToInventory)
	{
		if ((Object)(object)tipi == (Object)null)
		{
			if (notify)
			{
				Message("No hay ningún tipi desplegado para empaquetar");
			}
			return;
		}
		if (IsCentralFireBurning())
		{
			if (notify)
			{
				Message("Apaga la fogata antes de empaquetar el tipi");
			}
			return;
		}
		RemoveTipi(notify: false);
		if (returnToInventory)
		{
			GivePackedTipi();
		}
		if (notify)
		{
			Message("Tipi empaquetado y guardado en la mochila");
		}
	}

	private static bool IsCentralFireBurning()
	{
		if ((Object)(object)centralFire == (Object)null)
		{
			return false;
		}
		try
		{
			MethodInfo method = ((object)centralFire).GetType().GetMethod("IsBurning", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return method != null && (bool)method.Invoke(centralFire, null);
		}
		catch
		{
			return false;
		}
	}

	private static object GetGameComponent(string methodName)
	{
		try
		{
			return FindType("GameManager")?.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, null);
		}
		catch
		{
			return null;
		}
	}

	private static bool HasPackedTipi()
	{
		object gameComponent = GetGameComponent("GetInventoryComponent");
		if (gameComponent == null)
		{
			return false;
		}
		try
		{
			MethodInfo method = gameComponent.GetType().GetMethod("NumGearInInventory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
			{
				typeof(string),
				typeof(bool)
			}, null);
			return method != null && Convert.ToInt32(method.Invoke(gameComponent, new object[2] { "GEAR_BurebistaPackedTipi", false })) > 0;
		}
		catch
		{
			return false;
		}
	}

	private static bool TakePackedTipi()
	{
		object gameComponent = GetGameComponent("GetInventoryComponent");
		if (gameComponent == null || !HasPackedTipi())
		{
			return false;
		}
		try
		{
			MethodInfo method = gameComponent.GetType().GetMethod("RemoveGearFromInventory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[3]
			{
				typeof(string),
				typeof(int),
				typeof(bool)
			}, null);
			if (method == null)
			{
				return false;
			}
			method.Invoke(gameComponent, new object[3] { "GEAR_BurebistaPackedTipi", 1, true });
			return true;
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("No se pudo consumir el tipi empaquetado: " + ex.Message);
			return false;
		}
	}

	private static void GivePackedTipi()
	{
		object gameComponent = GetGameComponent("GetPlayerManagerComponent");
		if (gameComponent == null)
		{
			return;
		}
		try
		{
			MethodInfo? method = gameComponent.GetType().GetMethod("AddItemCONSOLE", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[3]
			{
				typeof(string),
				typeof(int),
				typeof(float)
			}, null);
			if (method == null)
			{
				throw new MissingMethodException("AddItemCONSOLE");
			}
			method.Invoke(gameComponent, new object[3] { "GEAR_BurebistaPackedTipi", 1, 100f });
		}
		catch (Exception ex)
		{
			MelonLogger.Error("No se pudo entregar el tipi empaquetado: " + ex);
		}
	}

	private static Transform GetPlayer()
	{
		try
		{
			object? obj = FindType("GameManager")?.GetMethod("GetPlayerTransform", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, null);
			return (Transform)((obj is Transform) ? obj : null);
		}
		catch
		{
			return null;
		}
	}

	private static Type FindType(string name)
	{
		if (Types.TryGetValue(name, out var value))
		{
			return value;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			value = assembly.GetType(name, throwOnError: false) ?? assembly.GetType("Il2Cpp." + name, throwOnError: false);
			if (value != null)
			{
				break;
			}
		}
		Types[name] = value;
		return value;
	}

	private static bool Playable()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
		string text = activeScene.name ?? "";
		if (text.Length > 0 && text.IndexOf("MainMenu", StringComparison.OrdinalIgnoreCase) < 0)
		{
			return text.IndexOf("Boot", StringComparison.OrdinalIgnoreCase) < 0;
		}
		return false;
	}

	private static void Message(string text)
	{
		MelonLogger.Msg(text);
		try
		{
			FindType("HUDMessage")?.GetMethod("AddMessage", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(string) }, null)?.Invoke(null, new object[1] { text });
		}
		catch
		{
		}
	}
}








