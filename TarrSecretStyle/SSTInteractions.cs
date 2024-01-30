using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Secret_Style_Things.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TarrSecretStyle
{
    static class SSTInteractions6
    {
        internal static void SetupTarrGordo()
        {
            var tarrGordoId = (Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), "TARR_GORDO");
            SlimeUtils.ExtraApperanceApplicators.Add(tarrGordoId, (x, y) =>
            {
                var top = x.Find("Vibrating/bone_root/bone_slime/bone_core/bone_jiggle_top/bone_skin_top");
                if (y.SaveSet == SlimeAppearance.AppearanceSaveSet.SECRET_STYLE)
                {
                    var face = x.GetComponent<GordoFaceComponents>();
                    face.chompOpenMouth = face.happyMouth;
                    var stem = new GameObject("pumpkinStem", typeof(MeshFilter), typeof(MeshRenderer)).transform;
                    stem.SetParent(top,false);
                    stem.GetComponent<MeshRenderer>().sharedMaterial = Main.stemMat;
                    stem.GetComponent<MeshFilter>().sharedMesh = Main.stemMesh;
                }
                else
                {
                    var stem = top.Find("pumpkinStem");
                    if (stem)
                        Object.Destroy(stem.gameObject);
                }
            });
            SlimeUtils.SecretStyleData.Add(tarrGordoId, new SecretStyleData( Main.tarrGordoSprite));
        }
        internal static void SetupTarrPlort()
        {
            var tarrPlortId = (Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), "TARR_PLORT");
            SlimeUtils.SecretStyleData.Add(tarrPlortId, new SecretStyleData(Main.tarrPlortSprite));
            SlimeUtils.ExtraApperanceApplicators.Add(tarrPlortId, (x, y) =>
            {
                var stem = new GameObject("pumpkinStem", typeof(MeshFilter), typeof(MeshRenderer)).transform;
                stem.SetParent(x,false);
                stem.localPosition += Vector3.up;
                stem.localScale *= 2;
                stem.GetComponent<MeshRenderer>().sharedMaterial = Main.stemMat;
                stem.GetComponent<MeshFilter>().sharedMesh = Main.stemMesh;
            });
        }
    }
}
