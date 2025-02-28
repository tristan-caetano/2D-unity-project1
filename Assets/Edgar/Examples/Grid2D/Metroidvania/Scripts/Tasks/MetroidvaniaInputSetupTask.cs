﻿using System.Linq;
using UnityEngine;

namespace Edgar.Unity.Examples.Metroidvania
{
    #region codeBlock:2d_metroidvania_inputSetup
    [CreateAssetMenu(menuName = "Edgar/Examples/Metroidvania/Input setup", fileName = "Metroidvania Input Setup")]
    public class MetroidvaniaInputSetupTask : DungeonGeneratorInputBaseGrid2D
    {
        public LevelGraph LevelGraph;

        public MetroidvaniaRoomTemplatesConfig RoomTemplates;

        /// <summary>
        /// This is the main method of the input setup.
        /// It prepares the description of the level for the procedural generator.
        /// </summary>
        /// <returns></returns>
        protected override LevelDescriptionGrid2D GetLevelDescription()
        {
            var levelDescription = new LevelDescriptionGrid2D();

            // Go through individual rooms and add each room to the level description
            // Room templates are resolved based on their type
            foreach (var room in LevelGraph.Rooms.Cast<MetroidvaniaRoom>())
            {
                levelDescription.AddRoom(room, RoomTemplates.GetRoomTemplates(room).ToList());
            }

            // Go through individual connections and for each connection create a corridor room
            foreach (var connection in LevelGraph.Connections.Cast<MetroidvaniaConnection>())
            {
                var corridorRoom = ScriptableObject.CreateInstance<MetroidvaniaRoom>();
                corridorRoom.Type = MetroidvaniaRoomType.Corridor;
                levelDescription.AddCorridorConnection(connection, corridorRoom, RoomTemplates.CorridorRoomTemplates.ToList());
            }

            return levelDescription;
        }
    }
    #endregion
}