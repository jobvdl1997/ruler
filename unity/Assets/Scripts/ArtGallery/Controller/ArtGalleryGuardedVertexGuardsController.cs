﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArtGallery
{
    //Will be implemented by Karina and Job
    using General.Controller;
    using General.Menu;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Util.Algorithms.Polygon;
    using Util.Geometry.Polygon;
    using Util.Math;

    /// <summary>
    /// Main controller for the art gallery guarded vertex guards game.
    /// Handles the game update loop, as well as level initialization and
    /// advancement.
    /// </summary>
    public class ArtGalleryGuardedVertexGuardsController : AbstractArtGalleryController
    {

        [SerializeField]
        protected GameObject m_unguardedSpritePrefab;

        /// <inheritdoc />
        public override void CheckSolution()
        {
            bool everythingVisible = CheckVisibility();
            bool allGuardsVisible = CheckGuardedGuards();
            Debug.Log("Everything is visible: " + everythingVisible + "; All guards are visible: " + allGuardsVisible);

            if (everythingVisible && allGuardsVisible)
            {
                m_advanceButton.Enable();
            } 
            else
            {
                m_advanceButton.Disable();
            }
        }
        protected override void Update()
        {
            // return if no lighthouse was selected since last update
            if (m_selectedLighthouse == null) return;

            // get current mouseposition
            var worldlocation = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
            worldlocation.z = -2f;

            // turn off all unguarded indicators
            GameObject[] unguardedSprites = GameObject.FindGameObjectsWithTag("UnGuarded");
            foreach (GameObject sprite in unguardedSprites) {
                sprite.SetActive(false);
            }

            // move lighthouse to mouse position
            // will update visibility polygon
            Vector3 location = ClosestVertex(worldlocation);
            location.z = -2f;
            m_selectedLighthouse.Pos = location;

            // see if lighthouse was released 
            if (Input.GetMouseButtonUp(0))
            {
                //check whether lighthouse is over the island
                if (!LevelPolygon.ContainsInside(m_selectedLighthouse.Pos))
                {
                    // destroy the lighthouse
                    m_solution.RemoveLighthouse(m_selectedLighthouse);
                    Destroy(m_selectedLighthouse.gameObject);
                    UpdateLighthouseText();
                }

                // lighthouse no longer selected
                m_selectedLighthouse = null;

                CheckSolution();
            }
        }

        /// <summary> Find closest vertex to a given location. </summary>
        private Vector2 ClosestVertex(Vector2 location)
        {

            var closestVertex2D = LevelPolygon.Vertices.ElementAt(0);
            var minMagnitude = (location - closestVertex2D).magnitude;

            foreach (var vtx2D in LevelPolygon.Vertices)
            {
                var currentMagnitude = (location - vtx2D).magnitude;

                if (currentMagnitude < minMagnitude)
                {                    
                    minMagnitude = currentMagnitude;
                    closestVertex2D = vtx2D;
                }
            }

            return closestVertex2D;
        }

        /// <summary>Check if there is a LightHouse object at a given location.</summary>
        private bool LighthouseExists(Vector2 location)
        {
            bool lighthouseExists = m_solution.LightHouses.Any(
                l => MathUtil.EqualsEps(
                         l.Pos.x,
                         location.x) &&
                     MathUtil.EqualsEps(
                         l.Pos.y,
                         location.y));
            return lighthouseExists;
        }

        /// <summary>Handle a click on the island mesh.</summary>
        public override void HandleIslandClick()
        {

            // return if lighthouse was already selected or player can place no more lighthouses
            if (m_selectedLighthouse != null ||
                m_solution.Count >= m_maxNumberOfLighthouses)
            {
                return;
            }

            // obtain mouse position
            var worldlocation =
                Camera.main.ScreenPointToRay(Input.mousePosition).origin;

            worldlocation.z = -2f;
            Vector2 worldlocation2D = worldlocation;

            var closestVertex2D = ClosestVertex(worldlocation2D);

            // If lighthouse object exists at the closest vertex, return
            if (LighthouseExists(closestVertex2D))
            {
                return;
            }
            // There is no lighthouse  at the closest vertex

            Vector3 closestVertex = closestVertex2D;
            closestVertex.z = -2f;
            
            // create a new lighthouse from prefab
            var go = Instantiate(
                m_lighthousePrefab,
                closestVertex,
                Quaternion.identity) as GameObject;
            // create unguarded sprite from prefab
            Instantiate(m_unguardedSpritePrefab, go.transform);

            // add lighthouse to art gallery solution
            m_solution.AddLighthouse(go);

            // turn off all unguarded indicators
            GameObject[] unguardedSprites = GameObject.FindGameObjectsWithTag("UnGuarded");
            foreach (GameObject sprite in unguardedSprites) {
                sprite.SetActive(false);
            }

            UpdateLighthouseText();

            CheckSolution();
        }
        
        /// <summary>
        /// Checks if the current placed lighthouses completely illuminate
        /// the room
        /// </summary>
        /// <returns>
        /// True if the complete room is illuminated, false otherwise
        /// </returns>
        public bool CheckVisibility()
        {
            var ratio = m_solution.Area / LevelPolygon.Area;

            Debug.Log(ratio + " part is visible");

            // see if entire polygon is covered
            return MathUtil.GEQEps(ratio, 1f, 0.001f);

        }

        /// <summary>
        /// Checks if the current placed lighthouses can each see at lease one
        /// other lighthouse
        /// </summary>
        /// <returns>
        /// True if each lighthouse can see at least one other lighthouse,
        /// false otherwise
        /// </returns>
        public bool CheckGuardedGuards()
        {
            // get current lighthouses
            var lightHouses = m_solution.LightHouses;
            var lightHousesList = lightHouses.Select(
                                  l => new Vector2(l.Pos.x, l.Pos.y))
                              .ToList();

            List<Vector2> unguardedLighthouses =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertex(
                    lightHousesList,
                    LevelPolygon);

            bool allLighthousesAreSeen = true;
            if (unguardedLighthouses.Count() != 0) {
                allLighthousesAreSeen = false;
            }

            Debug.Log("all guards are seen: " + allLighthousesAreSeen);

            // indicate which are unguarded, if any
            if (!allLighthousesAreSeen) {
                UpdateUnguardedSprites(lightHouses, unguardedLighthouses);
            }

            return allLighthousesAreSeen;
        }

        /// <summary>
        /// Indicates in game which of the guards are unguarded
        /// </summary>
        private void UpdateUnguardedSprites(List<ArtGalleryLightHouse> lightHouses, List<Vector2> unguardedLighthouses) {
            //TODO: Update this with new guarded algorithm if we decide to use that
            foreach (ArtGalleryLightHouse lighthouse in lightHouses) {
                foreach (Vector2 unguardedLighthouse in unguardedLighthouses) {
                    if (lighthouse.Pos.x == unguardedLighthouse.x && lighthouse.Pos.y == unguardedLighthouse.y) {
                        // enable that lighthouse's sprite
                        lighthouse.gameObject.transform.Find("NotGuarded(Clone)").gameObject.SetActive(true);
                    }
                }
            }

        }
    }
}
