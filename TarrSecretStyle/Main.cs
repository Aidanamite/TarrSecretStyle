using HarmonyLib;
using SRML;
using SRML.SR;
using SRML.Console;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using MonomiPark.SlimeRancher;
using DLCPackage;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine.Experimental.Rendering;
using System.Linq;
using SRML.Utils.Enum;
using SRML.SR.Patches;
using SRML.Utils;
using MonomiPark.SlimeRancher.Regions;
using Secret_Style_Things.Utils;
using System;
using AssetsLib;
using static AssetsLib.TextureUtils;

using Object = UnityEngine.Object;
using Console = SRML.Console.Console;

namespace TarrSecretStyle
{
    public class Main : ModEntryPoint
    {
        public const string VERSION = "1.1.3";
        internal static Assembly modAssembly = Assembly.GetExecutingAssembly();
        internal static string modName = $"{modAssembly.GetName().Name}";
        internal static string modDir = $"{Environment.CurrentDirectory}\\SRML\\Mods\\{modName}";
        internal static Sprite tarrSprite = LoadImage("tarr_ss.png").CreateSprite();
        internal static Sprite digitarrSprite = LoadImage("digitarr_ss.png").CreateSprite();
        internal static Texture2D digitarrBackground = LoadImage("matrix2.png");
        internal static Sprite tarrGordoSprite = LoadImage("tarr_gordo_ss.png").CreateSprite();
        internal static Sprite digitarrGordoSprite = LoadImage("digitarr_gordo_ss.png").CreateSprite();
        internal static Sprite tarrPlortSprite = LoadImage("tarr_plort_ss.png").CreateSprite();
        internal static GameObject tarrDeathFx;
        internal static GameObject tarrExoticDeathFx;
        internal static GameObject tarrTrailFx;
        internal static GameObject tarrExoticTrailFx;
        internal static Material stemMat;
        internal static Mesh stemMesh;
        internal static bool started;

        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
            //Console.RegisterCommand(new CustomCommand());
        }
        public override void Load()
        {
            string id = SRML.SR.SaveSystem.ModdedStringRegistry.ClaimID("pod", "TarrPumpkin");
            DLCDirector.SECRET_STYLE_TREASURE_PODS.Add(id);
            string id2 = SRML.SR.SaveSystem.ModdedStringRegistry.ClaimID("pod", "TarrCorrupt");
            DLCDirector.SECRET_STYLE_TREASURE_PODS.Add(id2);
            GameContext.Instance.DLCDirector.onPackageInstalled += style =>
            {
                if (style != Id.SECRET_STYLE)
                    return;
                if (!started)
                {
                    started = true;
                    var def = GameContext.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.TARR_SLIME);
                    var a = def.GetAppearanceForSet(SlimeAppearance.AppearanceSaveSet.CLASSIC);
                    a.NameXlateKey = "l.classic_appearance";
                    var na = ScriptableObject.CreateInstance<SlimeAppearance>();
                    na.CopyFields(a);
                    na.Icon = tarrSprite;
                    na.NameXlateKey = "l.secret_style_tarr";
                    na.SaveSet = SlimeAppearance.AppearanceSaveSet.SECRET_STYLE;
                    na.Structures = new SlimeAppearanceStructure[a.Structures.Length];
                    for (int i = 0; i < a.Structures.Length; i++)
                        na.Structures[i] = new SlimeAppearanceStructure(a.Structures[i]);
                    na.name = "TarrExotic";
                    def.AppearancesDynamic.Add(na);
                    na.ColorPalette.Top = new Color(1, 0.5f, 0);
                    na.ColorPalette.Middle = new Color(1, 0.5f, 0);
                    na.ColorPalette.Bottom = new Color(1, 0.5f, 0);
                    // ------- Body Material -------
                    var m = na.Structures[0].DefaultMaterials[0].Clone();
                    m.name = m.name.Replace("(Clone)", "Exotic");
                    na.Structures[0].DefaultMaterials[0] = m;
                    na.Structures[1].DefaultMaterials[0] = m;
                    var t = (m.GetTexture("_Stripes") as Texture2D).GetReadable();
                    t.ModifyTexturePixels((x) =>
                    {
                        var v = x.grayscale;
                        x.r = 0;
                        x.g = 0;
                        x.b = v;
                        return x;
                    });
                    m.SetTexture("_Stripes", t);
                    m.SetColor("_TopColor", new Color(0.8f, 0.45f, 0, 0.01f));
                    m.SetColor("_MiddleColor", new Color(0.8f, 0.45f, 0, 0.01f));
                    m.SetColor("_BottomColor", new Color(0.8f, 0.45f, 0, 0.01f));
                    // ------- Face Materials -------
                    var cg = new ColorGroup();
                    cg.AddColor(Color.white, 1);
                    cg.AddColor(Color.clear, 0.99f);
                    cg.AddColor(Color.clear, 0.2f);
                    cg.AddColor(new Color(0.5f, 0, 0.5f), 0.05f);
                    cg.AddColor(Color.white, 0);
                    na.Face = Object.Instantiate(na.Face);
                    var mm = new Dictionary<Material, Material>();
                    for (int i = 0; i < na.Face.ExpressionFaces.Length; i++)
                    {
                        var u = na.Face.ExpressionFaces[i];
                        var e = u.Eyes;
                        if (e != null)
                        {
                            if (!mm.ContainsKey(e))
                            {
                                m = e.Clone();
                                mm[e] = m;
                                m.name = m.name.Replace("(Clone)", "Exotic");
                                if (m.HasProperty("_ColorRamp"))
                                {
                                    t = (m.GetTexture("_ColorRamp") as Texture2D).GetReadable();
                                    t.ModifyTexturePixels((c, x, y) => cg.GetColor(y));
                                    m.SetTexture("_ColorRamp", t);
                                }
                                if (m.HasProperty("_ScaleAmplitude"))
                                    m.SetFloat("_ScaleAmplitude", 0);
                                if (m.HasProperty("_WiggleAmplitude"))
                                    m.SetFloat("_WiggleAmplitude", 0);
                            }
                            u.Eyes = mm[e];
                        }
                        e = u.Mouth;
                        if (e != null)
                        {
                            if (!mm.ContainsKey(e))
                            {
                                m = e.Clone();
                                mm[e] = m;
                                m.name = m.name.Replace("(Clone)", "Exotic");
                                if (m.HasProperty("_ColorRamp"))
                                {
                                    t = (m.GetTexture("_ColorRamp") as Texture2D).GetReadable();
                                    t.ModifyTexturePixels((c, x, y) => cg.GetColor(y));
                                    m.SetTexture("_ColorRamp", t);
                                }
                                if (m.HasProperty("_ScaleAmplitude"))
                                    m.SetFloat("_ScaleAmplitude", 0);
                                if (m.HasProperty("_WiggleAmplitude"))
                                    m.SetFloat("_WiggleAmplitude", 0);
                            }
                            u.Mouth = mm[e];
                        }
                        na.Face.ExpressionFaces[i] = u;
                    }
                    na.Face.OnEnable();
                    // ------- Open Mouth Material -------
                    m = na.Structures[1].DefaultMaterials[2].Clone();
                    m.name = m.name.Replace("(Clone)", "Exotic");
                    na.Structures[1].DefaultMaterials[2] = m;
                    t = (m.GetTexture("_ColorRamp") as Texture2D).GetReadable();
                    t.ModifyTexturePixels((c, x, y) => cg.GetColor(x));
                    m.SetTexture("_ColorRamp", t);
                    // ------- Stem Prefab -------
                    var stem = new GameObject("PumpkinStem", typeof(MeshRenderer), typeof(MeshFilter), typeof(SlimeAppearanceObject));
                    stemMat = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(go => go.name == "envMossGrowths01");
                    stemMesh = Object.Instantiate(Resources.FindObjectsOfTypeAll<Mesh>().FirstOrDefault(go => go.name == "growthStem01"));
                    var verts = stemMesh.vertices;
                    for (int i = 0; i < verts.Length; i++)
                        verts[i] = verts[i] * 0.1f + Vector3.down * 0.08f;
                    stemMesh.vertices = verts;
                    stem.GetComponent<MeshRenderer>().sharedMaterial = stemMat;
                    stem.GetComponent<MeshFilter>().sharedMesh = stemMesh;
                    var apObj = stem.GetComponent<SlimeAppearanceObject>();
                    apObj.IgnoreLODIndex = true;
                    apObj.ParentBone = SlimeAppearance.SlimeBone.JiggleTop;
                    na.Structures = na.Structures.AddToArray(new SlimeAppearanceStructure(na.Structures[1]));
                    na.Structures[2].DefaultMaterials = new Material[] { stemMat };
                    na.Structures[2].Element = ScriptableObject.CreateInstance<SlimeAppearanceElement>();
                    na.Structures[2].Element.Name = "Stem";
                    na.Structures[2].Element.Prefabs = new SlimeAppearanceObject[] { PrefabUtils.CopyPrefab(stem).GetComponent<SlimeAppearanceObject>() };
                    na.Structures[2].ElementMaterials = new SlimeAppearanceMaterials[] { new SlimeAppearanceMaterials() { OverrideDefaults = false } };
                    na.Structures[2].SupportsFaces = false;
                    Object.Destroy(stem);

                    if (SRModLoader.IsModPresent("tarrrancher"))
                        GameContext.Instance.SlimeDefinitions.GetSlimeByIdentifiableId((Identifiable.Id)System.Enum.Parse(typeof(Identifiable.Id), "CALMED_TARR_SLIME")).AppearancesDynamic.Add(na);



                    def = GameContext.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.GLITCH_TARR_SLIME);
                    a = def.GetAppearanceForSet(SlimeAppearance.AppearanceSaveSet.CLASSIC);
                    a.NameXlateKey = "l.classic_appearance";
                    a.Icon = Resources.FindObjectsOfTypeAll<Sprite>().First((x) => x.name == "tut_digitarr2");
                    na = ScriptableObject.CreateInstance<SlimeAppearance>();
                    na.CopyFields(a);
                    na.Icon = digitarrSprite;
                    na.NameXlateKey = "l.secret_style_glitch_tarr";
                    na.SaveSet = SlimeAppearance.AppearanceSaveSet.SECRET_STYLE;
                    na.Structures = new SlimeAppearanceStructure[a.Structures.Length];
                    for (int i = 0; i < a.Structures.Length; i++)
                        na.Structures[i] = new SlimeAppearanceStructure(a.Structures[i]);
                    na.name = "GlitchTarrExotic";
                    def.AppearancesDynamic.Add(na);
                    na.ColorPalette.Top = new Color(1, 0, 1);
                    na.ColorPalette.Middle = new Color(0, 0, 0);
                    na.ColorPalette.Bottom = new Color(1, 0, 1);
                    // ------- Body Material -------
                    m = na.Structures[0].DefaultMaterials[0].Clone();
                    m.name = "slimeGlitchTarrExotic";
                    na.Structures[0].DefaultMaterials[0] = m;
                    na.Structures[1].DefaultMaterials[0] = m;
                    m.SetTexture("_Pixels", digitarrBackground);
                    m.SetFloat("_GlitchAmount", m.GetFloat("_GlitchAmount") * 5);
                    m.SetFloat("_GridMultiply", m.GetFloat("_GridMultiply") * 5);
                    // ------- Face Material -------
                    cg = new ColorGroup();
                    cg.AddColor(Color.black, 0.3f);
                    cg.AddColor(Color.green, 0);
                    na.Face = Object.Instantiate(na.Face);
                    mm = new Dictionary<Material, Material>();
                    for (int i = 0; i < na.Face.ExpressionFaces.Length; i++)
                    {
                        var u = na.Face.ExpressionFaces[i];
                        var e = u.Eyes;
                        if (e != null)
                        {
                            if (!mm.ContainsKey(e))
                            {
                                m = e.Clone();
                                mm[e] = m;
                                m.name = m.name.Replace("(Clone)", "GlitchExotic");
                                if (m.HasProperty("_ColorRamp"))
                                {
                                    t = (m.GetTexture("_ColorRamp") as Texture2D).GetReadable();
                                    t.ModifyTexturePixels((c, x, y) => cg.GetColor(x));
                                    m.SetTexture("_ColorRamp", t);
                                }
                            }
                            u.Eyes = mm[e];
                        }
                        e = u.Mouth;
                        if (e != null)
                        {
                            if (!mm.ContainsKey(e))
                            {
                                m = e.Clone();
                                mm[e] = m;
                                m.name = m.name.Replace("(Clone)", "GlitchExotic");
                                if (m.HasProperty("_ColorRamp"))
                                {
                                    t = (m.GetTexture("_ColorRamp") as Texture2D).GetReadable();
                                    t.ModifyTexturePixels((c, x, y) => cg.GetColor(x));
                                    m.SetTexture("_ColorRamp", t);
                                }
                            }
                            u.Mouth = mm[e];
                        }
                        na.Face.ExpressionFaces[i] = u;
                    }
                    na.Face.OnEnable();
                    // ------- Open Mouth Material -------
                    m = na.Structures[1].DefaultMaterials[2].Clone();
                    m.name = m.name.Replace("(Clone)", "GlitchExotic");
                    na.Structures[1].DefaultMaterials[2] = m;
                    t = (m.GetTexture("_ColorRamp") as Texture2D).GetReadable();
                    t.ModifyTexturePixels((c, x, y) => cg.GetColor(x));
                    m.SetTexture("_ColorRamp", t);
                }
                if (started && !Levels.isMainMenu())
                {
                    var def = GameContext.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.TARR_SLIME);
                    var a = def.GetAppearanceForSet(SlimeAppearance.AppearanceSaveSet.SECRET_STYLE);
                    var pod = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.name == "treasurePodCosmetic").CreateInactive();
                    pod.name = "treasurePodCosmetic";
                    TreasurePod treasurePod = pod.GetComponent<TreasurePod>();
                    pod.transform.SetParent(Resources.FindObjectsOfTypeAll<Region>().First((x) => x.name == "cellReef_SandTrap").transform, true);
                    treasurePod.director = pod.GetComponentInParent<IdDirector>();
                    treasurePod.director.persistenceDict.Add(treasurePod, id);
                    treasurePod.unlockedSlimeAppearanceDefinition = def;
                    treasurePod.unlockedSlimeAppearance = a;
                    //pod.transform.SetParent(vPod.transform.parent, true);
                    pod.transform.position = new Vector3(-208, 14.7f, -75);
                    var r = pod.transform.localEulerAngles;
                    r.y += 90;
                    pod.transform.localEulerAngles = r;
                    pod.SetActive(true);



                    def = GameContext.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.GLITCH_TARR_SLIME);
                    a = def.GetAppearanceForSet(SlimeAppearance.AppearanceSaveSet.SECRET_STYLE);
                    pod = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.name == "treasurePodCosmetic").CreateInactive();
                    pod.name = "treasurePodCosmetic";
                    treasurePod = pod.GetComponent<TreasurePod>();
                    pod.transform.SetParent(Resources.FindObjectsOfTypeAll<Region>().First((x) => x.name == "cellSlimulationQuarry_MirrorIsland").transform, true);
                    treasurePod.director = pod.GetComponentInParent<IdDirector>();
                    treasurePod.director.persistenceDict.Add(treasurePod, id2);
                    treasurePod.unlockedSlimeAppearanceDefinition = def;
                    treasurePod.unlockedSlimeAppearance = a;
                    pod.transform.position = new Vector3(1173.1f, -0.5f, 1260.3f);
                    r = pod.transform.localEulerAngles;
                    r.y += 90;
                    pod.transform.localEulerAngles = r;
                    pod.SetActive(true);
                }
            };
        }

        public override void PostLoad()
        {
            SceneContext.Instance.SlimeAppearanceDirector.onSlimeAppearanceChanged += (x, y) =>
            {
                if (x.IdentifiableId == Identifiable.Id.TARR_SLIME && SRModLoader.IsModPresent("tarrrancher") && System.Enum.TryParse("CALMED_TARR_SLIME", out Identifiable.Id tarrSlimeId))
                {
                    var def = GameContext.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(tarrSlimeId);
                    if (def != null)
                    {
                        var a = def.GetAppearanceForSet(y.SaveSet);
                        if (a != null)
                            SceneContext.Instance.SlimeAppearanceDirector.UpdateChosenSlimeAppearance(def, a);
                    }
                }
                if (x.IdentifiableId == Identifiable.Id.TARR_SLIME)
                {
                    foreach(var ident in Resources.FindObjectsOfTypeAll<Identifiable>())
                    {
                        if (ident.id == x.IdentifiableId)
                        {
                            var death = ident.GetComponent<DestroyAfterTime>();
                            if (!tarrDeathFx)
                                tarrDeathFx = death.destroyFX;
                            if (!tarrExoticDeathFx && tarrDeathFx && y.SaveSet == SlimeAppearance.AppearanceSaveSet.SECRET_STYLE)
                            {
                                var d = Object.Instantiate(tarrDeathFx);
                                foreach (var renderer in d.GetComponentsInChildren<ParticleSystemRenderer>())
                                    if (renderer.sharedMaterial && renderer.sharedMaterial.name.Contains("slimeTarr"))
                                        renderer.sharedMaterial = y.Structures[0].DefaultMaterials[0];
                                tarrExoticDeathFx = PrefabUtils.CopyPrefab(d);
                                Object.DestroyImmediate(d);
                            }
                            death.destroyFX = y.SaveSet == SlimeAppearance.AppearanceSaveSet.SECRET_STYLE ? tarrExoticDeathFx : tarrDeathFx;

                            var spawn = ident.GetComponent<TarrSpawnFX>();
                            if (!tarrTrailFx)
                                tarrTrailFx = spawn.SpawnFX;
                            if (!tarrExoticTrailFx && tarrTrailFx && y.SaveSet == SlimeAppearance.AppearanceSaveSet.SECRET_STYLE)
                            {
                                var d = Object.Instantiate(tarrTrailFx);
                                foreach (var renderer in d.GetComponentsInChildren<ParticleSystemRenderer>())
                                    if (renderer.sharedMaterial && renderer.sharedMaterial.name.Contains("slimeTarr"))
                                        renderer.sharedMaterial = y.Structures[0].DefaultMaterials[0];
                                tarrExoticTrailFx = PrefabUtils.CopyPrefab(d);
                                Object.DestroyImmediate(d);
                            }
                            spawn.SpawnFX = y.SaveSet == SlimeAppearance.AppearanceSaveSet.SECRET_STYLE ? tarrExoticTrailFx: tarrTrailFx;
                        }
                    }
                }
            };
            if (SSTInteractions.HasSST())
            {
                if (Enum.TryParse("TARR_GORDO", true, out Identifiable.Id tarrGordo))
                    SSTInteractions.SetupTarrGordo(tarrGordo);
                if (Enum.TryParse("GLITCH_TARR_GORDO", true, out Identifiable.Id glitchTarrGordo))
                    SSTInteractions.SetupGlitchTarrGordo(glitchTarrGordo);
                if (Enum.TryParse("TARR_PLORT", true, out Identifiable.Id tarrPlort))
                    SSTInteractions.SetupTarrPlort(tarrPlort);
            }
        }

        public static void Log(string message) => Console.Log($"[{modName}]: " + message);
        public static void LogError(string message) => Console.LogError($"[{modName}]: " + message);
        public static void LogWarning(string message) => Console.LogWarning($"[{modName}]: " + message);
        public static void LogSuccess(string message) => Console.LogSuccess($"[{modName}]: " + message);
    }

    [HarmonyPatch(typeof(PediaDirector), "GetPediaId")]
    class Patch_GetPediaId
    {
        static void Postfix(Identifiable.Id identId, ref PediaDirector.Id? __result)
        {
            if (identId == Identifiable.Id.GLITCH_TARR_SLIME)
                __result = Ids.GLITCH_TARR_SLIME;
        }
    }

    [HarmonyPatch(typeof(SlimeAppearanceUI), "ShouldShowSlimeInList")]
    class Patch_ShouldShowSlimeInList
    {
        static void Postfix(SlimeDefinition slime, ref bool __result)
        {
            if (slime.IdentifiableId.ToString() == "CALMED_TARR_SLIME")
                __result = false;
        }
    }

    [HarmonyPatch(typeof(AppearancesModel), "ShouldPersistSlimeAppearanceInfo")]
    class Patch_ShouldSaveSS
    {
        static void Postfix(Identifiable.Id slimeId, ref bool __result)
        {
            if (slimeId == Identifiable.Id.GLITCH_TARR_SLIME || slimeId == Identifiable.Id.TARR_SLIME)
                __result = true;
        }
    }

    [HarmonyPatch(typeof(TargetingUI), "GetIdentifiableTarget")]
    class Patch_GetTarget
    {
        public static GameObject calling;
        static void Prefix(GameObject gameObject) => calling = gameObject;
        static void Postfix() => calling = null;
    }

    [HarmonyPatch(typeof(Identifiable), "GetName")]
    class Patch_GetName
    {
        static void Prefix(ref Identifiable.Id id)
        {
            if (Patch_GetTarget.calling)
            {
                var id2 = Identifiable.GetId(Patch_GetTarget.calling);
                if (id2 == Identifiable.Id.GLITCH_TARR_SLIME)
                    id = id2;
            }
        }
    }

    [EnumHolder]
    static class Ids
    {
        public static PediaDirector.Id GLITCH_TARR_SLIME;
    }

    [HarmonyPatch(typeof(TentacleGrapple), "MaybeGrapple")]
    class Patch_TryGrapple
    {
        public static Identifiable calling = null;
        static void Prefix(TentacleGrapple __instance) => calling = __instance.GetComponent<Identifiable>();
        static void Postfix() => calling = null;
    }

    [HarmonyPatch(typeof(TentacleHook), "Awake")]
    class Patch_StartGrapple
    {
        static Material TarrArm;
        static void Prefix(TentacleHook __instance)
        {
            if (!Patch_TryGrapple.calling)
                return;
            var ap = Patch_TryGrapple.calling.GetComponent<SlimeAppearanceApplicator>();
            if (Patch_TryGrapple.calling && ap && ap.Appearance.SaveSet == SlimeAppearance.AppearanceSaveSet.SECRET_STYLE)
            {
                Material m = null;
                Renderer r = __instance.tentacleObject.GetComponent<Renderer>();
                if (!r)
                    return;
                if (Patch_TryGrapple.calling.id == Identifiable.Id.TARR_SLIME)
                {
                    if (!TarrArm) //fix
                    {
                        TarrArm = r.sharedMaterial.Clone();
                        TarrArm.name = TarrArm.name.Replace("(Clone)", "").Replace(" (Instance)", "") + "Exotic";
                        var t = (TarrArm.GetTexture("_Stripes") as Texture2D).GetReadable();
                        t.ModifyTexturePixels((x) =>
                        {
                            var v = x.grayscale;
                            x.r = 0;
                            x.g = v;
                            x.b = 0;
                            return x;
                        });
                        TarrArm.SetTexture("_Stripes", t);
                        TarrArm.SetColor("_TopColor", new Color(0.2f, 0.45f, 0, 0.01f));
                        TarrArm.SetColor("_MiddleColor", new Color(0.2f, 0.45f, 0, 0.01f));
                        TarrArm.SetColor("_BottomColor", new Color(0.2f, 0.45f, 0, 0.01f));
                    }
                    m = TarrArm;
                }
                else if (Patch_TryGrapple.calling.id == Identifiable.Id.GLITCH_TARR_SLIME)
                    m = ap.Appearance.Structures[0].DefaultMaterials[0];
                else
                    return;
                r.sharedMaterial = m;
            }
        }
    }

    [HarmonyPatch(typeof(ResourceBundle), "LoadFromText")]
    class Patch_LoadResources
    {
        static void Postfix(string path, Dictionary<string,string> __result)
        {
            if (path != "actor")
                return;
            var lang = GameContext.Instance.MessageDirector.GetCultureLang();
            if (lang == MessageDirector.Lang.EN)
            {
                __result["l.glitch_tarr_slime"] = "Digitarr Slime";
                __result["l.secret_style_tarr"] = "Jack-o-Tarr";
                __result["l.secret_style_glitch_tarr"] = "Corruption";
            }
            else if (lang == MessageDirector.Lang.DE)
            {
                __result["l.glitch_tarr_slime"] = "Digiterr-Slime";
                __result["l.secret_style_tarr"] = "KürbisTerr";
                __result["l.secret_style_glitch_tarr"] = "Korruption";
            }
            else if (lang == MessageDirector.Lang.ES)
            {
                __result["l.glitch_tarr_slime"] = "Slime Digialquitrrán";
                __result["l.secret_style_tarr"] = "Jack-o-Alquitrrán";
                __result["l.secret_style_glitch_tarr"] = "Corrupción";
            }
            else if (lang == MessageDirector.Lang.FR)
            {
                __result["l.glitch_tarr_slime"] = "Slime Digigoudrron";
                __result["l.secret_style_tarr"] = "Citrouille Goudrron";
                __result["l.secret_style_glitch_tarr"] = "Corruption";
            }
            else if (lang == MessageDirector.Lang.RU)
            {
                __result["l.glitch_tarr_slime"] = "цифраbapp-слайм";
                __result["l.secret_style_tarr"] = "Джек-о-вapp";
                __result["l.secret_style_glitch_tarr"] = "Искажение";
            }
            else if (lang == MessageDirector.Lang.SV)
            {
                __result["l.glitch_tarr_slime"] = "Digitjärslime";
                __result["l.secret_style_tarr"] = "Pumpatjär";
                __result["l.secret_style_glitch_tarr"] = "Korruption";
            }
            else if (lang == MessageDirector.Lang.ZH)
            {
                __result["l.glitch_tarr_slime"] = "数字焦油怪";
                __result["l.secret_style_tarr"] = "南瓜焦油";
                __result["l.secret_style_glitch_tarr"] = "腐败";
            }
            else if (lang == MessageDirector.Lang.JA)
            {
                __result["l.glitch_tarr_slime"] = "デジタールスライム";
                __result["l.secret_style_tarr"] = "ジャック・オー・タール";
                __result["l.secret_style_glitch_tarr"] = "腐敗";
            }
            else if (lang == MessageDirector.Lang.PT)
            {
                __result["l.glitch_tarr_slime"] = "Slime Digibrreu";
                __result["l.secret_style_tarr"] = "Jack-o-brreu";
                __result["l.secret_style_glitch_tarr"] = "Corrupção";
            }
            else if (lang == MessageDirector.Lang.KO)
            {
                __result["l.glitch_tarr_slime"] = "디지타르 슬라임";
                __result["l.secret_style_tarr"] = "잭-오-타르";
                __result["l.secret_style_glitch_tarr"] = "부패";
            }
        }
    }
    static class SSTInteractions
    {
        static int cachedHas;
        internal static bool HasSST()
        {
            if (cachedHas == 0)
                try
                {
                    TestSST();
                    cachedHas = 1;
                }
                catch { cachedHas = -1; }
            return cachedHas == 1;
        }
        static void TestSST() => SlimeUtils.ExtraApperanceApplicators?.GetType();
        internal static void SetupTarrGordo(Identifiable.Id tarrGordoId)
        {
            SlimeUtils.ExtraApperanceApplicators[tarrGordoId] = (x, y) =>
            {
                var top = x.Find("Vibrating/bone_root/bone_slime/bone_core/bone_jiggle_top/bone_skin_top");
                if (y.SaveSet == SlimeAppearance.AppearanceSaveSet.SECRET_STYLE)
                {
                    var face = x.GetComponent<GordoFaceComponents>();
                    face.chompOpenMouth = face.happyMouth;
                    var stem = new GameObject("pumpkinStem", typeof(MeshFilter), typeof(MeshRenderer)).transform;
                    stem.SetParent(top, false);
                    stem.GetComponent<MeshRenderer>().sharedMaterial = Main.stemMat;
                    stem.GetComponent<MeshFilter>().sharedMesh = Main.stemMesh;
                }
                else
                {
                    var stem = top.Find("pumpkinStem");
                    if (stem)
                        Object.Destroy(stem.gameObject);
                }
            };
            SlimeUtils.SecretStyleData[tarrGordoId] = new SecretStyleData(Main.tarrGordoSprite);
        }
        internal static void SetupGlitchTarrGordo(Identifiable.Id glitchTarrGordoId)
        {
            SlimeUtils.ExtraApperanceApplicators[glitchTarrGordoId] = (x, y) =>
            {
                if (y.SaveSet == SlimeAppearance.AppearanceSaveSet.SECRET_STYLE)
                {
                    var face = x.GetComponent<GordoFaceComponents>();
                    face.chompOpenMouth = face.happyMouth;
                }
            };
            SlimeUtils.SecretStyleData[glitchTarrGordoId] = new SecretStyleData(Main.digitarrGordoSprite);
        }
        internal static void SetupTarrPlort(Identifiable.Id tarrPlortId)
        {
            SlimeUtils.SecretStyleData.Add(tarrPlortId, new SecretStyleData(Main.tarrPlortSprite));
            SlimeUtils.ExtraApperanceApplicators.Add(tarrPlortId, (x, y) =>
            {
                var stem = new GameObject("pumpkinStem", typeof(MeshFilter), typeof(MeshRenderer)).transform;
                stem.SetParent(x, false);
                stem.localPosition += Vector3.up;
                stem.localScale *= 2;
                stem.GetComponent<MeshRenderer>().sharedMaterial = Main.stemMat;
                stem.GetComponent<MeshFilter>().sharedMesh = Main.stemMesh;
            });
        }
    }
}