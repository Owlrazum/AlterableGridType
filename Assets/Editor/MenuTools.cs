using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MenuTools : Editor
{
    [MenuItem("Window/Grid")]
    public static void ArrayMenuOption()
    { 
        Selection.activeGameObject = ObjectFactory.CreateGameObject("Camera", typeof(Camera));
    }
}
