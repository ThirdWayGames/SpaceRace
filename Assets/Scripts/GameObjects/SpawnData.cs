using Assets.Scripts.Interfaces;
using Newtonsoft.Json;

namespace Assets.Scripts.GameObjects
{
    public class SpawnData : ISpawnData
    {
        public int? ParentId;

        public string SubParentName;

        public virtual object[] ToOjectArray()
        {
            return new object[] { JsonConvert.SerializeObject(this) };
        }
    }
}

