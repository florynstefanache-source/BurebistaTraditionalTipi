using System;
using System.IO;
using System.Reflection;
using ModComponent.Components;
using ModComponent.SDK.Components;
using ModComponent.Utilities;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public static class BuildPackedTipi
{
    private const string Root = "Assets/BurebistaPackedTipi";

    public static void Build()
    {
        AssetDatabase.DeleteAsset(Root);
        Directory.CreateDirectory(Root);

        // The game uses the built-in Standard shader.  URP materials export from
        // Unity 6 but render as black geometry in The Long Dark.
        Shader gameShader = Shader.Find("Standard") ?? Shader.Find("Legacy Shaders/Diffuse");
        Material hide = new Material(gameShader);
        hide.color = new Color(0.43f, 0.27f, 0.12f, 1f);
        if (hide.HasProperty("_Glossiness")) hide.SetFloat("_Glossiness", 0.08f);
        AssetDatabase.CreateAsset(hide, Root + "/PackedHide.mat");

        Material leather = new Material(gameShader);
        leather.color = new Color(0.12f, 0.055f, 0.018f, 1f);
        if (leather.HasProperty("_Glossiness")) leather.SetFloat("_Glossiness", 0.15f);
        AssetDatabase.CreateAsset(leather, Root + "/PackedStraps.mat");

        Material wood = new Material(gameShader);
        wood.color = new Color(0.25f, 0.13f, 0.055f, 1f);
        if (wood.HasProperty("_Glossiness")) wood.SetFloat("_Glossiness", 0.03f);
        AssetDatabase.CreateAsset(wood, Root + "/PackedPoles.mat");

        GameObject item = new GameObject("GEAR_BurebistaPackedTipi");
        GameObject model = new GameObject("PackedTipiModel");
        model.transform.SetParent(item.transform, false);

        GameObject roll = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        roll.name = "PackedTipiRoll";
        roll.transform.SetParent(model.transform, false);
        roll.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        roll.transform.localScale = new Vector3(0.28f, 0.53f, 0.28f);
        roll.GetComponent<Renderer>().sharedMaterial = hide;
        UnityEngine.Object.DestroyImmediate(roll.GetComponent<Collider>());

        // Three tipi poles pass through the hide bundle and remain visible at
        // both ends, so the dropped object reads immediately as a packed tipi.
        Vector2[] poleOffsets =
        {
            new Vector2(-0.13f, -0.08f),
            new Vector2( 0.12f, -0.07f),
            new Vector2( 0.00f,  0.14f)
        };
        for (int p = 0; p < poleOffsets.Length; p++)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "TipiPole" + (p + 1);
            pole.transform.SetParent(model.transform, false);
            pole.transform.localPosition = new Vector3(0f, poleOffsets[p].x, poleOffsets[p].y);
            pole.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            pole.transform.localScale = new Vector3(0.035f, 0.82f, 0.035f);
            pole.GetComponent<Renderer>().sharedMaterial = wood;
            UnityEngine.Object.DestroyImmediate(pole.GetComponent<Collider>());
        }

        for (int i = -1; i <= 1; i += 2)
        {
            // Thin cylinders form proper bands around the bundle instead of the
            // two detached rectangular blocks used by the first version.
            GameObject strap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            strap.name = i < 0 ? "LeatherStrapLeft" : "LeatherStrapRight";
            strap.transform.SetParent(model.transform, false);
            strap.transform.localPosition = new Vector3(i * 0.36f, 0f, 0f);
            strap.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            strap.transform.localScale = new Vector3(0.292f, 0.035f, 0.292f);
            strap.GetComponent<Renderer>().sharedMaterial = leather;
            UnityEngine.Object.DestroyImmediate(strap.GetComponent<Collider>());
        }

        // ModComponent expects the pickup collider on the prefab root. One root
        // collider keeps every visible part together when the item is dropped.
        BoxCollider bundleCollider = item.AddComponent<BoxCollider>();
        bundleCollider.center = Vector3.zero;
        bundleCollider.size = new Vector3(1.70f, 0.60f, 0.60f);

        ModGenericComponent generic = item.AddComponent<ModGenericComponent>();
        generic.DisplayNameLocalizationId = "GAMEPLAY_BurebistaPackedTipi";
        generic.DescriptionLocalizatonId = "GAMEPLAY_BurebistaPackedTipiDescription";
        generic.WeightKG = 8f;
        generic.DaysToDecay = 0;
        generic.MaxHP = 100f;
        generic.InitialCondition = InitialCondition.Perfect;
        generic.InventoryCategory = InventoryCategory.Tool;
        generic.InspectOnPickup = true;
        generic.InspectDistance = 1.7f;
        generic.InspectAngles = new Vector3(12f, 30f, 0f);
        generic.InspectScale = Vector3.one;
        // These fields must reference an actual rendered mesh. An empty parent
        // makes the complete model disappear in the game.
        generic.NormalModel = roll;
        generic.InspectModel = roll;

        string prefabPath = Root + "/GEAR_BurebistaPackedTipi.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(item, prefabPath);
        UnityEngine.Object.DestroyImmediate(item);

        string iconPath = Root + "/ico_GearItem__BurebistaPackedTipi.png";
        CreateTransparentIcon(
            Path.Combine(Application.dataPath, "TipiIconSource.png"),
            Path.GetFullPath(iconPath));
        AssetDatabase.ImportAsset(iconPath, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(iconPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        ModLocalization localization = ScriptableObject.CreateInstance<ModLocalization>();
        localization.localizationEntries.Add(Entry("GAMEPLAY_BurebistaPackedTipi", "Packed Traditional Tipi", "Tipi tradicional empaquetado"));
        localization.localizationEntries.Add(Entry("GAMEPLAY_BurebistaPackedTipiDescription",
            "A portable hide tipi. Deploy it with F3 and pack it again with F2 after extinguishing the campfire.",
            "Un tipi portátil de piel. Despliégalo con F3 y vuelve a empaquetarlo con F2 después de apagar la fogata."));
        AssetDatabase.CreateAsset(localization, Root + "/BurebistaPackedTipiLocalization.asset");

        ModDefinition definition = ScriptableObject.CreateInstance<ModDefinition>();
        definition.Name = "BurebistaTraditionalTipi";
        definition.Author = "Burebista";
        definition.Version = "1.1.0";
        definition.RequiredMods = Array.Empty<string>();
        definition.modLocalization = localization;
        definition.Items = new[] { prefab };
        definition.Icons = new[] { AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath) };
        AssetDatabase.CreateAsset(definition, Root + "/BurebistaTraditionalTipi.asset");
        AssetDatabase.SaveAssets();

        AddressableAssetSettings addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
        if (addressableSettings != null)
        {
            addressableSettings.EnableJsonCatalog = true;
            addressableSettings.BuildRemoteCatalog = true;
            addressableSettings.OverridePlayerVersion = "BurebistaTraditionalTipi";
            EditorUtility.SetDirty(addressableSettings);
            AssetDatabase.SaveAssets();
        }

        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../release/Mods/BurebistaTraditionalTipi.modcomponent"));
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        Type manager = typeof(ModDefinition).Assembly.GetType("ModComponent.SDK.Components.ModManager");
        MethodInfo export = manager.GetMethod("ExportModAsModComponent", BindingFlags.Static | BindingFlags.NonPublic);
        export.Invoke(null, new object[] { definition, output });
        Debug.Log("BUILT_MODCOMPONENT=" + output);
        EditorApplication.Exit(0);
    }

    private static ModLocalization.LocalizationEntry Entry(string key, string english, string spanish)
    {
        return new ModLocalization.LocalizationEntry
        {
            localizationKey = key,
            languages = new ModLocalization.Languages { English = english, Spanish = spanish }
        };
    }

    private static void CreateTransparentIcon(string sourcePath, string outputPath)
    {
        Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        source.LoadImage(File.ReadAllBytes(sourcePath));
        int width = source.width;
        int height = source.height;
        Color32[] pixels = source.GetPixels32();
        bool[] background = new bool[pixels.Length];
        int[] queue = new int[pixels.Length];
        int head = 0;
        int tail = 0;

        Action<int> enqueue = index =>
        {
            if (index < 0 || index >= pixels.Length || background[index]) return;
            Color32 color = pixels[index];
            // Remove only dark pixels connected to the outside. Dark details
            // enclosed by the tipi, including the wolf, stay intact.
            if (Math.Max(color.r, Math.Max(color.g, color.b)) > 38) return;
            background[index] = true;
            queue[tail++] = index;
        };

        for (int x = 0; x < width; x++)
        {
            enqueue(x);
            enqueue((height - 1) * width + x);
        }
        for (int y = 0; y < height; y++)
        {
            enqueue(y * width);
            enqueue(y * width + width - 1);
        }

        while (head < tail)
        {
            int index = queue[head++];
            int x = index % width;
            int y = index / width;
            if (x > 0) enqueue(index - 1);
            if (x + 1 < width) enqueue(index + 1);
            if (y > 0) enqueue(index - width);
            if (y + 1 < height) enqueue(index + width);
        }

        // Image-generation residue can leave isolated dark dots that are not
        // connected to the outer background. Keep only the largest visible
        // connected component (the complete tipi) and discard those specks.
        bool[] visited = new bool[pixels.Length];
        System.Collections.Generic.List<int> largest = new System.Collections.Generic.List<int>();
        for (int seed = 0; seed < pixels.Length; seed++)
        {
            if (background[seed] || visited[seed]) continue;
            System.Collections.Generic.List<int> component = new System.Collections.Generic.List<int>();
            head = 0;
            tail = 0;
            queue[tail++] = seed;
            visited[seed] = true;
            while (head < tail)
            {
                int index = queue[head++];
                component.Add(index);
                int x = index % width;
                int y = index / width;
                int left = index - 1;
                int right = index + 1;
                int down = index - width;
                int up = index + width;
                if (x > 0 && !background[left] && !visited[left]) { visited[left] = true; queue[tail++] = left; }
                if (x + 1 < width && !background[right] && !visited[right]) { visited[right] = true; queue[tail++] = right; }
                if (y > 0 && !background[down] && !visited[down]) { visited[down] = true; queue[tail++] = down; }
                if (y + 1 < height && !background[up] && !visited[up]) { visited[up] = true; queue[tail++] = up; }
            }
            if (component.Count > largest.Count) largest = component;
        }
        bool[] keep = new bool[pixels.Length];
        for (int i = 0; i < largest.Count; i++) keep[largest[i]] = true;

        for (int i = 0; i < pixels.Length; i++)
        {
            if (!keep[i]) pixels[i].a = 0;
        }
        source.SetPixels32(pixels);
        source.Apply();
        File.WriteAllBytes(outputPath, source.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(source);
    }

    private static Texture2D CreateIcon()
    {
        const int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color hide = new Color(0.55f, 0.36f, 0.18f, 1f);
        Color edge = new Color(0.16f, 0.09f, 0.035f, 1f);
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) texture.SetPixel(x, y, clear);
        for (int y = 70; y < 186; y++)
        {
            float dy = (y - 128f) / 58f;
            for (int x = 34; x < 222; x++)
            {
                float dx = (x - 128f) / 94f;
                if (dx * dx + dy * dy <= 1f) texture.SetPixel(x, y, hide);
            }
        }
        for (int x = 34; x < 222; x++)
        {
            texture.SetPixel(x, 69, edge); texture.SetPixel(x, 70, edge);
            texture.SetPixel(x, 185, edge); texture.SetPixel(x, 186, edge);
        }
        for (int strap = 0; strap < 2; strap++)
        {
            int cx = strap == 0 ? 78 : 178;
            for (int x = cx - 5; x <= cx + 5; x++) for (int y = 66; y <= 190; y++) texture.SetPixel(x, y, edge);
        }
        texture.Apply();
        return texture;
    }
}
