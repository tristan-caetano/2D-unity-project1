﻿using System;
using System.Linq;
using UnityEngine;

namespace Edgar.Unity.Examples.FogOfWarExample
{
    #region codeBlock:2d_fogOfWar_postProcessing
    [CreateAssetMenu(menuName = "Edgar/Examples/Fog of War/Post-processing", fileName = "FogOfWarPostProcessing")]
    public class FogOfWarExamplePostProcessing : DungeonGeneratorPostProcessingGrid2D
    {
        public override void Run(DungeonGeneratorLevelGrid2D level)
        {
            // To setup the FogOfWar component, we need to get the root game object that holds the level.
            var generatedLevelRoot = level.RootGameObject;

            // If we use the Wave mode, we must specify the point from which the wave spreads as we reveal a room.
            // The easiest way to do so is to get the player game object and use its transform as the wave origin.
            // Change this line if your player game object does not have the "Player" tag.
            var player = GameObject.FindGameObjectWithTag("Player");

            // Now we can setup the FogOfWar component.
            // To make it easier to work with the component, the class is a singleton and provides the Instance property.
            FogOfWarGrid2D.Instance.Setup(generatedLevelRoot, player.transform);

            // After the level is generated, we usually want to reveal the spawn room.
            // To do that, we have to find the room instance that corresponds to the Spawn room.
            // In this example, the spawn room is called "Spawn" so we find it by its name.
            var spawnRoom = level
                .RoomInstances
                .SingleOrDefault(x => x.Room.GetDisplayName() == "Spawn");

            if (spawnRoom == null)
            {
                throw new InvalidOperationException("There must be exactly one room with the name 'Spawn' for this example to work.");
            }

            // When we have the spawn room instance, we can reveal the room from the fog.
            // We use revealImmediately: true so that the first room is revealed instantly,
            // but it is optional.
            FogOfWarGrid2D.Instance.RevealRoom(spawnRoom, revealImmediately: true);
        }
    }
    #endregion
}