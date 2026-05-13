using UnityEngine;
namespace Items.Effects  // 添加命名空间
{

    [CreateAssetMenu(fileName = "New Item", menuName = "Data/Item effect")]
    public class ItemEffect : ScriptableObject
    {
        public virtual void ExecuteEffect()
        {

        }
    }

}
