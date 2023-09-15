using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MoshitinEncoded.Editor
{
    public static class VisualElementExtensions
    {
        public static Vector2 ScreenToLocal(this VisualElement visualElement, EditorWindow window, Vector2 point)
        {
            var worldMousePosition = ScreenToWorld(window, point);
            return visualElement.WorldToLocal(worldMousePosition);
        }

        public static Vector2 ScreenToWorld(EditorWindow window, Vector2 point) => 
            point - window.position.position;
    }
}