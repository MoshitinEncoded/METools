using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using MoshitinEncoded.GraphTools;

using UnityEditor;
using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.UIElements;

using Blackboard = UnityEditor.Experimental.GraphView.Blackboard;

namespace MoshitinEncoded.Editor.GraphTools
{
    public class BlackboardView : Blackboard
    {
        private MoshitinEncoded.GraphTools.Blackboard _Blackboard;
        private Type _ParameterBaseType;
        private SerializedObject _SerializedBlackboard;
        private BlackboardSection _ParametersSection;

        public BlackboardView(GraphView graphView) : base(graphView)
        {
            subTitle = "Blackboard";
            SetPosition(new Rect(10, 10, 300, 300));

            addItemRequested += OnAddParameter;
            editTextRequested += OnEditFieldText;
            moveItemRequested += OnMoveParameter;
        }

        public void PopulateView(MoshitinEncoded.GraphTools.Blackboard blackboard, string title, Type parameterBaseType)
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

                ResetParametersExpandedState();
            }

            Clear();

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
                RemoveFromBlackboard(parameterToRemove);
                Undo.DestroyObjectImmediate(parameterToRemove);
            }
        }

        private void RemoveParameterFromView(BlackboardField parameterField)
        {
            var parameterRow = parameterField.GetFirstAncestorOfType<BlackboardRow>();
            _ParametersSection.Remove(parameterRow);
        }

        private void RemoveFromBlackboard(BlackboardParameter parameterToRemove)
        {
            _SerializedBlackboard.Update();
            _SerializedBlackboard.FindProperty("_Parameters").RemoveFromObjectArray(parameterToRemove);
            _SerializedBlackboard.ApplyModifiedProperties();
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

        private void DrawParameters()
        {
            foreach (var parameter in _Blackboard.Parameters)
            {
                BlackboardParameterDrawer.Draw(parameter, _ParametersSection);
            }
        }

        private void OnAddParameter(Blackboard blackboard)
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
            // Create the parameter
            var newParameter = ScriptableObject.CreateInstance((Type)parameterType) as BlackboardParameter;

            var parameterName = MakeParameterNameUnique(newParameter, "NewParameter");
            newParameter.name = parameterName;
            newParameter.ParameterName = parameterName;

            // Add the parameter to the asset
            AssetDatabase.AddObjectToAsset(newParameter, _Blackboard);
            Undo.RegisterCreatedObjectUndo(newParameter, "Create Parameter (Behaviour Tree)");

            // Draw the parameter
            BlackboardParameterDrawer.Draw(newParameter, _ParametersSection);

            // Add the parameter to the behaviour tree
            _SerializedBlackboard.Update();
            _SerializedBlackboard.FindProperty("_Parameters").AddToObjectArray(newParameter);
            _SerializedBlackboard.ApplyModifiedProperties();
        }

        private void OnEditFieldText(Blackboard blackboard, VisualElement element, string newName)
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

                Undo.RecordObject(parameter, "Change Parameter Name (Behaviour Tree)");
                parameter.name = newName;
                parameter.ParameterName = newName;
            }
        }

        private void OnMoveParameter(Blackboard blackboard, int newIndex, VisualElement element)
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