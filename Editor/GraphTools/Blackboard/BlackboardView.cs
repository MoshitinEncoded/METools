using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using MoshitinEncoded.GraphTools;

using UnityEditor;
using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.UIElements;

using GraphView = UnityEditor.Experimental.GraphView;
using Blackboard = MoshitinEncoded.GraphTools.Blackboard;

namespace MoshitinEncoded.Editor.GraphTools
{
    public class BlackboardView : GraphView.Blackboard
    {
        private Blackboard _Blackboard;
        private Type _ParameterBaseType;
        private SerializedObject _SerializedBlackboard;
        private BlackboardSection _ParametersSection;
        private HideFlags _ParameterHideFlags;

        public BlackboardView(GraphView.GraphView graphView) : base(graphView)
        {
            subTitle = "Blackboard";
            SetPosition(new Rect(10, 10, 300, 300));

            addItemRequested += OnAddParameter;
            editTextRequested += OnEditFieldText;
            moveItemRequested += OnMoveParameter;
        }

        public void PopulateView(Blackboard blackboard, string title, Type parameterBaseType, HideFlags parameterHideFlags = HideFlags.HideInHierarchy)
        {
            if (_Blackboard == blackboard)
            {
                SaveParametersExpandedState();
            }
            else
            {
                this.title = title;
                _Blackboard = blackboard;
                _SerializedBlackboard = new SerializedObject(blackboard);
                _ParameterBaseType = parameterBaseType;
                _ParameterHideFlags = parameterHideFlags;

                ResetParametersExpandedState();
            }

            Clear();
            RemoveNullParameters();

            _ParametersSection = new BlackboardSection() { title = "Exposed Parameters" };
            DrawParameters();

            Add(_ParametersSection);
        }

        public void RemoveParameter(BlackboardField parameterField)
        {
            RemoveParameterFromBlackboard(parameterField);
            RemoveParameterFromView(parameterField);
        }

        private void RemoveParameterFromBlackboard(BlackboardField parameterField)
        {
            var parameterToRemove = GetParameter(parameterField.text);

            if (parameterToRemove != null)
            {
                RemoveParameterFromBlackboard(parameterToRemove);
            }
        }

        private void RemoveParameterFromBlackboard(BlackboardParameter parameterToRemove)
        {
            _SerializedBlackboard.Update();
            _SerializedBlackboard.FindProperty("_Parameters").RemoveFromObjectArray(parameterToRemove);
            _SerializedBlackboard.ApplyModifiedProperties();

            Undo.DestroyObjectImmediate(parameterToRemove);
        }

        private void RemoveParameterFromView(BlackboardField parameterField)
        {
            var parameterRow = parameterField.GetFirstAncestorOfType<BlackboardRow>();
            _ParametersSection.Remove(parameterRow);
        }

        private void SaveParametersExpandedState()
        {
            foreach (var parameter in _Blackboard.Parameters)
            {
                var parameterRow = FindParameterRow(parameter);
                if (parameterRow != null)
                {
                    BlackboardParameterDrawer.SetExpandedState(parameter, parameterRow.expanded);
                }
            }
        }

        private void ResetParametersExpandedState()
        {
            foreach (var parameter in _Blackboard.Parameters)
            {
                BlackboardParameterDrawer.SetExpandedState(parameter, false);
            }
        }

        private void RemoveNullParameters()
        {
            var parameters = _Blackboard.Parameters;
            for (var i = parameters.Length - 1; i >= 0; i--)
            {
                if (parameters[i] == null)
                {
                    RemoveNullParameterAtIndex(i);
                }
            }
        }

        private void RemoveNullParameterAtIndex(int i)
        {
            _SerializedBlackboard.Update();
            _SerializedBlackboard.FindProperty("_Parameters").DeleteArrayElementAtIndex(i);
            _SerializedBlackboard.ApplyModifiedPropertiesWithoutUndo();
        }

        private void DrawParameters()
        {
            foreach (var parameter in _Blackboard.Parameters)
            {
                BlackboardParameterDrawer.Draw(parameter, _ParametersSection);
            }
        }

        private void OnAddParameter(GraphView.Blackboard blackboard)
        {
            var menu = new GenericMenu();
            var parameterDatas = GetParameterDatas();

            AddParameterMenuAttribute prevAttribute = null;
            foreach (var parameterData in parameterDatas)
            {
                var attribute = parameterData.Attribute;

                if (IsSeparatorNeeded(attribute))
                {
                    menu.AddSeparator(attribute.SubMenuPath + '/');
                }
                else if (EnteredUnsortedGroupZone(attribute))
                {
                    menu.AddSeparator(prevAttribute.SubMenuPath);
                }

                menu.AddItem(new GUIContent(attribute.MenuPath), false, AddParameterTypeOf, parameterData.Type);
                prevAttribute = attribute;
            }

            menu.ShowAsContext();

            bool IsSeparatorNeeded(AddParameterMenuAttribute attribute)
            {
                return
                    prevAttribute != null &&
                    SubMenuRemains(attribute.SubMenuPath) &&
                    GroupLevelChanged(attribute.GroupLevel);
            }

            bool EnteredUnsortedGroupZone(AddParameterMenuAttribute attribute)
            {
                return
                    prevAttribute != null &&
                    EnteredSubMenu(attribute.SubMenuPath) &&
                    prevAttribute.GroupLevel != AddParameterMenuAttribute.UNSORTED_GROUP;
            }

            bool SubMenuRemains(string subMenuPath) => prevAttribute.SubMenuPath == subMenuPath;

            bool GroupLevelChanged(int groupLevel) => prevAttribute.GroupLevel != groupLevel;

            bool EnteredSubMenu(string subMenuPath)
            {
                if (subMenuPath.Length <= prevAttribute.SubMenuPath.Length)
                {
                    return false;
                }

                for (var i = 0; i < prevAttribute.SubMenuPath.Length; i++)
                {
                    if (prevAttribute.SubMenuPath[i] != subMenuPath[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private void AddParameterTypeOf(object parameterType)
        {
            var parameter = CreateParameter(parameterType);
            
            SetupParameter(parameter);
            AddParameterToBlackboard(parameter);
            BlackboardParameterDrawer.Draw(parameter, _ParametersSection);
        }

        private static BlackboardParameter CreateParameter(object parameterType)
        {
            var newParameter = ScriptableObject.CreateInstance((Type)parameterType) as BlackboardParameter;

            Undo.RegisterCreatedObjectUndo(newParameter, "Create Parameter (Blackboard)");
            Undo.RegisterCompleteObjectUndo(newParameter, "Create Parameter (Blackboard)");

            return newParameter;
        }

        private void SetupParameter(BlackboardParameter parameter)
        {
            var parameterName = MakeParameterNameUnique(parameter, "NewParameter");
            parameter.name = parameterName;
            parameter.ParameterName = parameterName;
            parameter.hideFlags = _ParameterHideFlags;
        }

        private void AddParameterToBlackboard(BlackboardParameter parameter)
        {
            AssetDatabase.AddObjectToAsset(parameter, _Blackboard);
            _SerializedBlackboard.Update();
            _SerializedBlackboard.FindProperty("_Parameters").AddToObjectArray(parameter);
            _SerializedBlackboard.ApplyModifiedProperties();
        }

        private void OnEditFieldText(GraphView.Blackboard blackboard, VisualElement element, string newName)
        {
            if (newName == "")
            {
                return;
            }

            if (element is BlackboardField parameterField)
            {
                if (newName == parameterField.text)
                {
                    return;
                }

                var parameter = GetParameter(parameterField.text);
                if (parameter == null)
                {
                    Debug.LogError("The parameter you are trying to edit doesn't exist anymore.");
                    RefreshView();
                    return;
                }

                newName = MakeParameterNameUnique(parameter, newName);
                if (newName == parameter.ParameterName)
                {
                    return;
                }

                parameterField.text = newName;

                Undo.RecordObject(parameter, "Change Parameter Name (Blackboard)");
                parameter.name = newName;
                parameter.ParameterName = newName;
            }
        }

        private void OnMoveParameter(GraphView.Blackboard blackboard, int newIndex, VisualElement element)
        {
            if (element is BlackboardField parameterField)
            {
                var srcIndex = -1;
                for (var i = 0; i < _Blackboard.Parameters.Length; i++)
                {
                    if (_Blackboard.Parameters[i].ParameterName == parameterField.text)
                    {
                        srcIndex = i;
                        break;
                    }
                }

                if (srcIndex == -1)
                {
                    return;
                }

                if (srcIndex < newIndex)
                {
                    newIndex--;
                }

                var blackboardRow = _ParametersSection.ElementAt(srcIndex);
                _ParametersSection.Remove(blackboardRow);
                _ParametersSection.Insert(newIndex, blackboardRow);

                _SerializedBlackboard.Update();
                _SerializedBlackboard.FindProperty("_Parameters").MoveArrayElement(srcIndex, newIndex);
                _SerializedBlackboard.ApplyModifiedProperties();
            }
        }

        private void RefreshView() => PopulateView(_Blackboard, title, _ParameterBaseType);

        private string MakeParameterNameUnique(BlackboardParameter parameter, string parameterName)
        {
            var index = 0;
            var newParameterName = parameterName;
            while (_Blackboard.Parameters.Any(p => p != parameter && p.ParameterName == newParameterName))
            {
                index++;
                newParameterName = $"{parameterName} ({index})";
            }

            return newParameterName;
        }

        private IEnumerable<ParameterAttributeData> GetParameterDatas()
        {
            var parameterTypes = TypeCache.GetTypesDerivedFrom(_ParameterBaseType).AsEnumerable();
            parameterTypes = parameterTypes.Where(TypeHasAddParameterMenuAttribute);

            var parametersData = parameterTypes.Select(type =>
                new ParameterAttributeData()
                {
                    Type = type,
                    Attribute = type.GetCustomAttribute<AddParameterMenuAttribute>()
                }
            );

            parametersData = parametersData.OrderBy(data =>
            {
                var parameterMenus = data.Attribute.MenuPath.Split('/');
                var pathOrder = string.Empty;
                for (int i = 0; i < parameterMenus.Length - 1; i++)
                {
                    pathOrder += int.MaxValue + parameterMenus[i] + '/';
                }
                pathOrder += data.Attribute.GroupLevel + parameterMenus.Last();

                return pathOrder;
            });

            return parametersData;
        }

        private BlackboardParameter GetParameter(string name) =>
            _Blackboard.GetParameter(name);

        private BlackboardRow FindParameterRow(BlackboardParameter parameter)
        {
            var blackboardRows = this.Query<BlackboardRow>().Build().ToArray();
            return blackboardRows.FirstOrDefault(br => br.Q<BlackboardField>().text == parameter.ParameterName);
        }

        private bool TypeHasAddParameterMenuAttribute(Type type) =>
            type.GetCustomAttribute<AddParameterMenuAttribute>() != null;

        private struct ParameterAttributeData
        {
            public Type Type;
            public AddParameterMenuAttribute Attribute;
        }
    }
}