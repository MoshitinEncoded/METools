using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoshitinEncoded.Editor
{
    public static class SerializedPropertyExtensions
    {
        /// <summary>
        /// Adds an object to an array property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrayProperty"></param>
        /// <param name="objectToAdd"></param>
        /// <exception cref="UnityException"></exception>
        public static void AddToObjectArray<T>(this SerializedProperty arrayProperty, T objectToAdd)
        where T : Object
        {
            if (!arrayProperty.isArray)
                throw new UnityException($"SerializedProperty {arrayProperty.name} is not an array.");

            arrayProperty.InsertArrayElementAtIndex(arrayProperty.arraySize);
            arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1).objectReferenceValue = objectToAdd;
        }

        /// <summary>
        /// Removes an object from an array property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrayProperty"></param>
        /// <param name="objectToRemove"></param>
        /// <returns> True if the object was found and removed from the array. </returns>
        public static bool RemoveFromObjectArray<T>(this SerializedProperty arrayProperty, T objectToRemove)
            where T : Object
        {
            if (!arrayProperty.isArray)
                throw new UnityException($"SerializedProperty {arrayProperty.name} is not an array.");

            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                if (arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue == objectToRemove)
                {
                    arrayProperty.DeleteArrayElementAtIndex(i);
                    return true;
                }
            }

            return false;
        }
    }
}