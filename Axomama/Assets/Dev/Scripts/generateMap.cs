using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dev.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

public class generateMap : MonoBehaviour
{

    public float tileSize = 5f;

    public float numberOfIslands = 10f;

    public GameObject tilePrefab;
    // Start is called before the first frame update
    void Start()
    {
        generate();
    }

    public void generate()
    {
        List<SafeZoneIsland> safeZoneIslands = createSafeZones();
        // trimCantLink(safeZoneIslands);
        foreach (var island in safeZoneIslands)
        {
            Instantiate(tilePrefab, new Vector3(island.originPoint.x,0,island.originPoint.y), Quaternion.identity);
            Instantiate(tilePrefab, new Vector3(island.originPoint.x+tileSize,0,island.originPoint.y), Quaternion.identity);
            Instantiate(tilePrefab, new Vector3(island.originPoint.x,0,island.originPoint.y+tileSize), Quaternion.identity);
            Instantiate(tilePrefab, new Vector3(island.originPoint.x+tileSize,0,island.originPoint.y+tileSize), Quaternion.identity);
            if (island.size > 2)
            {
                if (!island.vertical)
                {
                    Instantiate(tilePrefab, new Vector3(island.originPoint.x+tileSize+tileSize,0,island.originPoint.y+tileSize), Quaternion.identity);
                    Instantiate(tilePrefab, new Vector3(island.originPoint.x+tileSize+tileSize,0,island.originPoint.y), Quaternion.identity);
                }
                else
                {
                    Instantiate(tilePrefab, new Vector3(island.originPoint.x+tileSize,0,island.originPoint.y+tileSize+tileSize), Quaternion.identity);
                    Instantiate(tilePrefab, new Vector3(island.originPoint.x,0,island.originPoint.y+tileSize+tileSize), Quaternion.identity);
                }
            }
        }

    }

    private List<SafeZoneIsland> createSafeZones()
    {
        List<SafeZoneIsland> safeZoneIslands = new List<SafeZoneIsland>();
        // Init islands
        safeZoneIslands.Add(new SafeZoneIsland(2,true,new Vector2(0,0)));
        while (safeZoneIslands.Count < numberOfIslands)
        {
            expandSafeZones(safeZoneIslands);
        }

        return safeZoneIslands;
    }

    private void expandSafeZones( List<SafeZoneIsland> safeZoneIslands)
    {
        List<SafeZoneIsland> islandsToAdd = new List<SafeZoneIsland>();
        foreach (var island in safeZoneIslands) {
            if (!island.expanded && (safeZoneIslands.Count + islandsToAdd.Count) < numberOfIslands)
            {
                int islandsAdded = 0;
                for (int x = 1; x > -2; x--) 
                {
                    for (int y = 1; y > -2; y--)
                    {
                        if (x == y) continue;
                        if (islandsAdded > 1) goto Expanded;

                        int randomSize = Random.Range(2, 4);
                        int randomDist = Random.Range(2, 5);
                        bool randomVerticality = Random.Range(0, 2) == 1;
                        SafeZoneIsland newIsland = new SafeZoneIsland(randomSize, randomVerticality, getIslandNeighbourPosition(x, y, island, randomDist, randomSize, randomVerticality));
                        if (isIslandSpawnable(newIsland, islandsToAdd, safeZoneIslands, island))
                        {
                            islandsToAdd.Add(newIsland);
                            islandsAdded++;
                        }
                    }
                }
            }
            Expanded:
                island.expanded = true;
        }
        safeZoneIslands.AddRange(islandsToAdd);
    }

    private bool isIslandSpawnable(SafeZoneIsland newIsland, List<SafeZoneIsland> islandsToAdd, List<SafeZoneIsland> safeZoneIslands, SafeZoneIsland motherIsland)
    {
        bool canSpawnIsland = true;
        
        List<SafeZoneIsland> concatList = new List<SafeZoneIsland>();
        concatList.AddRange(islandsToAdd);
        concatList.AddRange(safeZoneIslands);
        foreach (var island in concatList) 
        {
            if (island.originPoint == motherIsland.originPoint) continue;
            if(Mathf.Abs(island.originPoint.x-newIsland.originPoint.x) + Mathf.Abs(island.originPoint.y-newIsland.originPoint.y) < 6*tileSize) canSpawnIsland = false;
        }
        Debug.Log(canSpawnIsland);
        return canSpawnIsland;
    }

    private Vector2 getIslandNeighbourPosition(int x, int y, SafeZoneIsland island, int dist, int newIslandSize, bool newIslandVerticality)
    {
        // Right
        if (y == 0 & x==1)
        {
            int xOffset = 2+dist;
            if (!island.vertical && island.size > 2) xOffset++;
            return new Vector2(island.originPoint.x + xOffset * tileSize, island.originPoint.y);
        }
        // Left
        if (y == 0 & x==-1)
        {
            int xOffset = -dist-2;
            if (!newIslandVerticality && newIslandSize > 2) xOffset--;
            return new Vector2(island.originPoint.x + (xOffset * tileSize), island.originPoint.y);
        }
        // Top
        if (y == 1 & x==0)
        {
            int yOffset = 2+dist;
            if (island.vertical && island.size > 2) yOffset++;
            return new Vector2(island.originPoint.x, island.originPoint.y + (yOffset * tileSize));

        }
        // Bottom
        if (y == -1 & x==0)
        {
            int yOffset = -dist-2;
            if (newIslandVerticality && newIslandSize > 2) yOffset--;
            return new Vector2(island.originPoint.x, island.originPoint.y + (yOffset * tileSize));
        }

        return new Vector2(0, 0);
    }

    private void trimCantLink(List<SafeZoneIsland> safeZoneIslands)
    {
        List<SafeZoneIsland> newlist = new List<SafeZoneIsland>();
        newlist.AddRange(safeZoneIslands);
        foreach (var islandSelected in safeZoneIslands)
        {
            int linkCounter = 0;
            Vector2 bottomLeft = islandSelected.originPoint;
            float height = islandSelected.vertical? 3:2;
            float width = islandSelected.vertical? 2:3;
            Vector2 topRight = new Vector2(islandSelected.originPoint.x + (width*tileSize), islandSelected.originPoint.y + (height*tileSize)) ;
            foreach (var islandLinked in newlist)
            {
                if (linkCounter > 1) break;
                Vector2 bottomLeftLinked = islandSelected.originPoint;
                float heightLinked = islandSelected.vertical? 3:2;
                float widthLinked = islandSelected.vertical? 2:3;
                Vector2 topRightLinked = new Vector2(islandLinked.originPoint.x + (widthLinked*tileSize), islandLinked.originPoint.y + (heightLinked*tileSize));

                // if (isLinkable(bottomLeft, topRight, bottomLeftLinked, topRightLinked))
                // {
                //     linkCounter++;
                // }
                
            }
            if (linkCounter > 1)
            {
                
            }
        }
    }

    // private bool isLinkable(Vector2 bottomLeft,  Vector2 topRight,  Vector2 bottomLeftLinked,  Vector2 topRightLinked)
    // {
    //     if bottomLeft.x < bottomLeftLinked.x && bottomLeft.x < bottomLeftLinked.x  
    // }

}
