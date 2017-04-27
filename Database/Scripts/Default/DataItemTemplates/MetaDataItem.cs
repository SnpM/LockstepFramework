using UnityEngine;
using System.Collections; using FastCollections;
using System.Linq;
namespace Lockstep.Data
{
    [System.Serializable]
    public class MetaDataItem : DataItem
    {

        [SerializeField]
        protected string 
            _description = "";
            
        public string Description { get { return _description; } }
            
        [SerializeField]
        private Sprite
            _icon;
            
        public Sprite Icon { get { return _icon; } }
           
        public override string ToString () {
            return string.Format ("Name={0} Description={1}, Icon={2}]",base.Name, Description, Icon);
        }
    }
        
}
