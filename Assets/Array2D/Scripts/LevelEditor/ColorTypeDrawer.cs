using UnityEditor;
using UnityEngine;
using _Main._Enums;

namespace LevelEditor
{
#if UNITY_EDITOR
    /// <summary>
    /// Custom property drawer for ColorType enum that sets a background color in the dropdown.
    /// </summary>
    [CustomPropertyDrawer(typeof(ColorType))]
    public class ColorTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Ensure the property is an enum
            if (property.propertyType == SerializedPropertyType.Enum)
            {
                // Get the current ColorType
                var colorType = (ColorType)property.enumValueIndex;

                // Set the background color based on ColorType
                var originalColor = GUI.backgroundColor;
                //GUI.backgroundColor = colorType.ToColor();

                // Draw the dropdown with the color
                property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, property.enumDisplayNames);

                // Reset the background color
                GUI.backgroundColor = originalColor;
            }
            else
            {
                // Fallback for non-enum properties
                EditorGUI.LabelField(position, label.text, "Use ColorType with ColorTypeDrawer");
            }
        }
    }

    /// <summary>
    /// Extension methods for ColorType to map enums to Unity colors.
    /// </summary>
    public static class ColorTypeExtensions
    {
        /// <summary>
        /// Maps a ColorType to a Unity color.
        /// </summary>
        /// <param name="colorType">The ColorType value.</param>
        /// <returns>The corresponding Unity color.</returns>
        public static Color ToColor(this ColorType colorType)
        {
            return colorType switch
            {
                ColorType._1Green => Color.green,
                ColorType._2Blue => Color.blue,
                ColorType._3Red => Color.red,
                ColorType._4Yellow => Color.yellow,
                ColorType._5Purple => new Color(0.6f, 0.3f, 1f),
                ColorType._6Pink => Color.magenta,
                ColorType._7Orange => new Color(1f, 0.5f, 0),
                _ => Color.white, // Default color for None or invalid values
            };
        }
    }
#endif
}