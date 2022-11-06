﻿using Pancake.Editor;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(GUIColorDrawer), DrawerOrder.Decorator)]

namespace Pancake.Editor
{
    public class GUIColorDrawer : AttributeDrawer<GUIColorAttribute>
    {
        public override void OnGUI(Rect position, Property property, InspectorElement next)
        {
            var oldColor = GUI.color;
            var newColor = new Color(Attribute.R, Attribute.G, Attribute.B, Attribute.A);

            GUI.color = newColor;
            GUI.contentColor = newColor;

            next.OnGUI(position);

            GUI.color = oldColor;
            GUI.contentColor = oldColor;
        }
    }
}