﻿using UnityEngine;

namespace Edgar.Unity.Examples.FogOfWarExample
{
    #region codeBlock:2d_fogOfWar_triggerHandler
    public class FogOfWarExampleTriggerHandler : MonoBehaviour
    {
        private RoomInstanceGrid2D roomInstance;

        private void Start()
        {
            roomInstance = GetRoomInstance();
        }

        private RoomInstanceGrid2D GetRoomInstance()
        {
            // The goal of this method is to get the RoomInstance of the corresponding room template
            // so that we can pass it to the FogOfWar script.

            // Get the root game object of the room template
            var roomTemplate = transform.parent.gameObject;

            // Each room template has a RoomInfo component attached
            var roomInfo = roomTemplate.GetComponent<RoomInfoGrid2D>();

            // The RoomInfo component has a RoomInstance property containing information about the room
            return roomInfo.RoomInstance;
        }

        private void OnTriggerEnter2D(Collider2D otherCollider)
        {
            // Make sure that the player game object has the "Player" tag
            // or remove/modify this line.
            if (otherCollider.gameObject.CompareTag("Player"))
            {
                FogOfWarGrid2D.Instance.RevealRoomAndNeighbors(roomInstance);
            }
        }
    }
    #endregion
}