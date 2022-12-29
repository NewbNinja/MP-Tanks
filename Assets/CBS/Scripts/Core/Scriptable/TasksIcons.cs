using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBS.Scriptable
{
    [CreateAssetMenu(fileName = "TasksIcons", menuName = "CBS/Add new Tasks Icons pack")]
    public class TasksIcons : IconsData
    {
        public override string ResourcePath => "TasksIcons";

        public override string EditorPath => "Assets/CBS_External/Resources";

        public override string EditorAssetName => "TasksIcons.asset";
    }
}
