using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MoshitinEncoded.Editor
{
    public static class VisualElementExtensions
    {
        public static Vector2 ScreenToLocal(this VisualElement visualElement, EditorWindow window, Vector2 point)
        {
            var worldMousePosition = point - window.position.position;
            return visualElement.WorldToLocal(worldMousePosition);
        }
    }
}