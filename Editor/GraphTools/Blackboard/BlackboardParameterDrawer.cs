using System.Linq;
using System.Reflection;
using MoshitinEncoded.GraphTools;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

namespace MoshitinEncoded.Editor.GraphTools
{
    public static class BlackboardParameterDrawer
    {
        public static void Draw(BlackboardParameter parameter, BlackboardSection section)
        {
            // Get the parameter type text
            string typeText;
            var parameterAttribute = parameter.GetType().GetCustomAttribute<AddParameterMenuAttribute>();

            if (parameterAttribute != null)
            {
                var parameterMenuPath = parameterAttribute.MenuPath.Split('/');
                typeText = parameterMenuPath.Last();
            }
            else
            {
                typeText = "Missing Type";
            }

            // Create the parameter field
            var blackboardField = new BlackboardField()
            {
                text = parameter.ParameterName,
                typeText = typeText
            };

            // Create the property value field
            var serializedParameter = new SerializedObject(parameter);
            var valueProperty = serializedParameter.FindProperty("_Value");

            var propertyValueField = new PropertyField(valueProperty);
            propertyValueField.Bind(serializedParameter);

            // Create the blackboard row that contains the property
            var blackboardRow = new BlackboardRow(item: blackboardField, propertyView: propertyValueField)
            {
                expanded = serializedParameter.FindProperty("_IsExpanded").boolValue
            };

            // Add the property to the blackboard
            section.Add(blackboardRow);
        }

        public static void SetExpandedState(BlackboardParameter parameter, bool isExpanded) =>
            new SerializedObject(parameter).FindProperty("_IsExpanded").boolValue = isExpanded;
    }
}
