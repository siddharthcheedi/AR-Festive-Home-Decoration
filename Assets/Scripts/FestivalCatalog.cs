using System;
using UnityEngine;

namespace FestiveSpaces
{
    public enum Festival { Diwali, Christmas, Halloween }

    [Serializable]
    public class DecorationDefinition
    {
        public string displayName;
        public PrimitiveType shape;
        public Color color;
        public Vector3 scale;
        public bool emissive;

        public DecorationDefinition(string name, PrimitiveType primitive, Color tint, Vector3 size, bool glow = false)
        { displayName = name; shape = primitive; color = tint; scale = size; emissive = glow; }
    }

    /// <summary>Small, model-free starter catalogue. Replace CreateDecoration visuals with imported prefab references later.</summary>
    public static class FestivalCatalog
    {
        public static string Title(Festival festival) => festival.ToString();
        public static Color Accent(Festival festival)
        {
            switch (festival)
            {
                case Festival.Diwali: return new Color(1f, .55f, .12f);
                case Festival.Christmas: return new Color(.15f, .67f, .42f);
                default: return new Color(.55f, .32f, .82f);
            }
        }

        public static DecorationDefinition[] Items(Festival festival)
        {
            switch (festival)
            {
                case Festival.Diwali: return new[] {
                    new DecorationDefinition("Clay diya", PrimitiveType.Cylinder, new Color(1f,.49f,.12f), new Vector3(.18f,.055f,.18f)),
                    new DecorationDefinition("Rangoli bloom", PrimitiveType.Sphere, new Color(.95f,.18f,.45f), new Vector3(.32f,.025f,.32f)),
                    new DecorationDefinition("Sky lantern", PrimitiveType.Cube, new Color(1f,.68f,.15f), new Vector3(.18f,.25f,.18f), true),
                    new DecorationDefinition("Marigold bowl", PrimitiveType.Sphere, new Color(1f,.47f,.06f), new Vector3(.23f,.12f,.23f)),
                    new DecorationDefinition("Golden star", PrimitiveType.Sphere, new Color(1f,.78f,.2f), new Vector3(.16f,.16f,.16f), true),
                    new DecorationDefinition("Festival pot", PrimitiveType.Cylinder, new Color(.72f,.19f,.12f), new Vector3(.16f,.18f,.16f)),
                    new DecorationDefinition("Lotus light", PrimitiveType.Sphere, new Color(1f,.28f,.55f), new Vector3(.2f,.08f,.2f), true)
                };
                case Festival.Christmas: return new[] {
                    new DecorationDefinition("Pine tree", PrimitiveType.Cylinder, new Color(.12f,.48f,.25f), new Vector3(.32f,.62f,.32f)),
                    new DecorationDefinition("Gift box", PrimitiveType.Cube, new Color(.85f,.13f,.2f), new Vector3(.23f,.23f,.23f)),
                    new DecorationDefinition("Snow pal", PrimitiveType.Sphere, Color.white, new Vector3(.23f,.3f,.23f)),
                    new DecorationDefinition("Wreath", PrimitiveType.Cylinder, new Color(.1f,.47f,.23f), new Vector3(.3f,.3f,.08f)),
                    new DecorationDefinition("Bauble", PrimitiveType.Sphere, new Color(.9f,.16f,.22f), new Vector3(.14f,.14f,.14f), true),
                    new DecorationDefinition("Candy cane", PrimitiveType.Capsule, Color.white, new Vector3(.09f,.28f,.09f)),
                    new DecorationDefinition("Gold star", PrimitiveType.Sphere, new Color(1f,.72f,.12f), new Vector3(.18f,.18f,.08f), true)
                };
                default: return new[] {
                    new DecorationDefinition("Pumpkin", PrimitiveType.Sphere, new Color(1f,.34f,.04f), new Vector3(.3f,.25f,.3f)),
                    new DecorationDefinition("Friendly ghost", PrimitiveType.Capsule, Color.white, new Vector3(.2f,.32f,.2f), true),
                    new DecorationDefinition("Skull", PrimitiveType.Sphere, new Color(.88f,.85f,.75f), new Vector3(.22f,.2f,.2f)),
                    new DecorationDefinition("Cobweb orb", PrimitiveType.Sphere, new Color(.55f,.49f,.7f), new Vector3(.22f,.22f,.22f)),
                    new DecorationDefinition("Witch potion", PrimitiveType.Cylinder, new Color(.32f,.78f,.27f), new Vector3(.13f,.22f,.13f), true),
                    new DecorationDefinition("Spooky candle", PrimitiveType.Cylinder, new Color(.9f,.77f,.48f), new Vector3(.1f,.22f,.1f)),
                    new DecorationDefinition("Black cat", PrimitiveType.Sphere, new Color(.14f,.12f,.18f), new Vector3(.21f,.18f,.26f))
                };
            }
        }

        public static GameObject CreateDecoration(DecorationDefinition item)
        {
            string key = item.displayName.Replace(" ", "");
            var prefab = Resources.Load<GameObject>("Decorations/" + key);
            if (prefab != null) return UnityEngine.Object.Instantiate(prefab);
            return CreatePrimitiveDecoration(item);
        }

        public static GameObject CreatePrimitiveDecoration(DecorationDefinition item)
        {
            var root = GameObject.CreatePrimitive(item.shape);
            root.name = item.displayName;
            root.transform.localScale = item.scale;
            var renderer = root.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = item.color;
            if (item.emissive && material.HasProperty("_EmissionColor"))
            { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", item.color * .65f); }
            renderer.material = material;
            root.AddComponent<DecorationInstance>();
            return root;
        }
    }

    public class DecorationInstance : MonoBehaviour { }
}
