#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SF.Cubism.Poser
{
    public class CubismPoserGroups : MonoBehaviour
    {
        public List<CubismPoserGroup> Groups = new();
    }

    [Serializable]
    public class CubismPoserGroup
    {
        public string Name;
        public List<string> Parameters = new();
    }
}
#endif