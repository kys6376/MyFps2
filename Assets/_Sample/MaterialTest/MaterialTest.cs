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
            //����
            renderer = GetComponent<Renderer>();

            //���͸��� �÷� �ٲٱ�
            renderer.material.SetColor("_BaseColor", Color.white);
            //renderer.sharedMaterial.SetColor("_BaseColor", Color.white);

            //
            materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetColor("_BaseColor", Color.white);
            renderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}
