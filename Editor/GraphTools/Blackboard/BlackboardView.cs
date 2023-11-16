using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using GraphView = UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using MoshitinEncoded.GraphTools;
using System.Collections.Generic;

namespace MoshitinEncoded.Editor.GraphTools
{
    public class BlackboardView : GraphView.Blackboard
    {
        private Blackboard _Blackboard;
        private Type _ParameterBaseType;
        private SerializedObject _SerializedBlackboard;
        private GraphView.BlackboardSection _ParametersSection;

        public BlackboardView(GraphView.GraphView graphView) : base(graphView)
        {
            subTitle = "Blackboard";
            SetPosition(new Rect(10, 10, 300, 300));

            addItemRequested += OnAddParameter;
            editTextRequested += OnEditFieldText;
            moveItemRequested += OnMoveParameter;
        }

        public void PopulateView(Blackboard blackboard, string title, Type parameterBaseType)
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

            _ParametersSection = new GraphView.BlackboardSection() { title = "Exposed Parameters" };
            DrawParameters();

            Add(_ParametersSection);
        }

        public void DeleteParameter(GraphView.BlackboardField parameterField)
        {
            var parameterToDelete = GetParameter(parameterField.text);

            if (parameterToDelete != null)
            {
                _SerializedBlackboard.Update();
                _SerializedBlackboard.FindProperty("_Parameters").RemoveFromObjectArray(parameterToDelete);
                _SerializedBlackboard.ApplyModifiedProperties();

                Undo.DestroyObjectImmediate(parameterToDelete);
            }

            var parameterRow = parameterField.GetFirstAncestorOfType<GraphView.BlackboardRow>();
            _ParametersSection.Remove(parameterRow);
        }

        private void SaveParametersExpandedState()
        {
            var blackboardRows = this.Query<GraphView.BlackboardRow>().ToList();
            foreach (var parameter in _Blackboard.Parameters)
            {
                var parameterRow = FindParameterRow(blackboardRows, parameter);
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

        private void OnAddParameter(GraphView.Blackboard blackboard)
        {
            var menu = new GenericMenu();
            var parameterDatas = GetParameterDatas();

            var isFirst = true;
            var prevGroupLevel = 0;
            foreach (var parameterData in parameterDatas)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    if (prevGroupLevel != parameterData.Attribute.GroupLevel)
                    {
                        menu.AddSeparator("");
                    }
                }

                prevGroupLevel = parameterData.Attribute.GroupLevel;
                menu.AddItem(new GUIContent(parameterData.Attribute.MenuPath), false, AddParameterTypeOf, parameterData);
            }

            menu.ShowAsContext();
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

        private void OnEditFieldText(GraphView.Blackboard blackboard, VisualElement element, string newName)
        {
            if (newName == "")
            {
                return;
            }

            if (element is GraphView.BlackboardField parameterField)
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

        private void OnMoveParameter(GraphView.Blackboard blackboard, int newIndex, VisualElement element)
        {
            if (element is GraphView.BlackboardField parameterField)
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
                return data.Attribute.GroupLevel + data.Attribute.MenuPath.Split('/').Last();
            });

            return parametersData;
        }

        private BlackboardParameter GetParameter(string name) => _Blackboard.Parameters.FirstOrDefault(p => p.ParameterName == name);

        private GraphView.BlackboardRow FindParameterRow(List<GraphView.BlackboardRow> blackboardRows, BlackboardParameter parameter) =>
            blackboardRows.FirstOrDefault(br => br.Q<GraphView.BlackboardField>().text == parameter.ParameterName);

        private bool TypeHasAddParameterMenuAttribute(Type type) =>
            type.GetCustomAttribute<AddParameterMenuAttribute>() != null;
        
        private struct ParameterAttributeData
        {
            public Type Type;
            public AddParameterMenuAttribute Attribute;
        }
    }
}