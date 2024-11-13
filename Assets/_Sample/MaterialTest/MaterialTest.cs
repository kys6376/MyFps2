using UnityEngine;

namespace MySample
{
    public class MaterialTest : MonoBehaviour
    {
        #region Variables
        private Renderer renderer;

        private MaterialPropertyBlock materialPropertyBlock;
        #endregion

        private void Start()
        {
            //참조
            renderer = GetComponent<Renderer>();

            //메터리얼 컬러 바꾸기
            renderer.material.SetColor("_BaseColor", Color.white);
            //renderer.sharedMaterial.SetColor("_BaseColor", Color.white);

            //
            materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetColor("_BaseColor", Color.white);
            renderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}
