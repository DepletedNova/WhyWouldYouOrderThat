namespace WWYOT
{
    internal static class GameObjectExt
    {
        public static GameObject Clone(this GameObject gameObject, string name)
        {
            var clonedObject = UnityEngine.Object.Instantiate(gameObject);
            clonedObject.name = name;
            clonedObject.transform.SetParent(gameObject.transform.parent, false);
            return clonedObject;
        }

        public static GameObject GetChild(this GameObject gameObject, string childName)
        {
            return gameObject.transform.Find(childName).gameObject;
        }

        public static GameObject FindChild(this GameObject gameObject, string childName)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i);
                if (!child.name.ToLower().Contains(childName.ToLower()))
                    continue;
                return child.gameObject;
            }
            Debug.LogError($"Could not find child with name \"{childName}\" within \"{gameObject.name}\"");
            return null;
        }

        public static void ApplyMaterials(this GameObject gameObject, params Material[] materials) {
            gameObject.GetComponent<MeshRenderer>().materials = materials;
        }
        public static void ApplyMaterials(this GameObject gameObject, params string[] materials)
        {
            Material[] formattedMaterials = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
                formattedMaterials[i] = MaterialUtils.GetExistingMaterial(materials[i]);
            gameObject.ApplyMaterials(formattedMaterials);
        }
    }
}
