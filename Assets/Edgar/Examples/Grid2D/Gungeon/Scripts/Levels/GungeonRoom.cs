﻿using System.Collections.Generic;
using UnityEngine;

namespace Edgar.Unity.Examples.Gungeon
{
    #region codeBlock:2d_gungeon_room
    public class GungeonRoom : RoomBase
    {
        public GungeonRoomType Type;

        public override List<GameObject> GetRoomTemplates()
        {
            // We do not need any room templates here because they are resolved based on the type of the room.
            return null;
        }

        public override string GetDisplayName()
        {
            // Use the type of the room as its display name.
            return Type.ToString();
        }

        public override RoomEditorStyle GetEditorStyle(bool isFocused)
        {
            var style = base.GetEditorStyle(isFocused);

            var backgroundColor = style.BackgroundColor;

            // Use different colors for different types of rooms
            switch (Type)
            {
                case GungeonRoomType.Entrance:
                    backgroundColor = new Color(38/256f, 115/256f, 38/256f);
                    break;

                case GungeonRoomType.Boss:
                    backgroundColor = new Color(128/256f, 0/256f, 0/256f);
                    break;

                case GungeonRoomType.Shop:
                    backgroundColor = new Color(102/256f, 0/256f, 204/256f);
                    break;

                case GungeonRoomType.Reward:
                    backgroundColor = new Color(102/256f, 0/256f, 204/256f);
                    break;
            }

            style.BackgroundColor = backgroundColor;

            // Darken the color when focused
            if (isFocused)
            {
                style.BackgroundColor = Color.Lerp(style.BackgroundColor, Color.black, 0.7f);
            }

            return style;
        }
    }
    #endregion
}