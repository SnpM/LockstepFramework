using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
    public class PathObjectFactory
    {
  

		private static AssetBundle mainBundle;

        public static AssetBundle MainBundle
        {
            get
            {
                return mainBundle;
            }
            set
            {
                mainBundle = value;
            }
        }

		private static AssetBundle artBundle;
		public static AssetBundle ArtBundle {
			get { return artBundle; }
			set {
				artBundle = value;
			}
		}

        public static UnityEngine.Object Load(PathObject pathObject)
        {
			return Load(pathObject.Path, pathObject.BundleType);
        }

		const bool ForceResourcesLoad = true;
		public static UnityEngine.Object Load(string prefabName, BundleType bundleType = BundleType.Resources)
        {
			if (bundleType == BundleType.Resources || ForceResourcesLoad)
				return ResourcesLoad (prefabName);
			
			UnityEngine.Object obj = null;

            {
				string ABPrefabName = prefabName.Remove (0, prefabName.LastIndexOf ('/') + 1);

				if (bundleType == BundleType.Main) {
					if (MainBundle == null)
						return ResourcesLoad (prefabName);
					obj = MainBundle.LoadAsset (ABPrefabName);
				} else {
					if (bundleType == BundleType.Art)
						return ResourcesLoad (prefabName);
					obj = ArtBundle.LoadAsset (ABPrefabName);
				}

            }
            return obj;
        }

		public static UnityEngine.Object ResourcesLoad (string prefabName) {
			return Resources.Load (prefabName);
		}

    }
	public enum BundleType {
		Main,
		Art,
		Resources
	}
}