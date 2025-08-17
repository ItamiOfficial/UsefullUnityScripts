using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Prototype.Scripts.GenricScripts
{
    public class Graph
    {
        private class Edge
        {
            public int To;
            public int Weight;

            public Edge(int to, int weight)
            {
                To = to;
                Weight = weight;
            }
        }
        private readonly List<Edge>[] _graph;

        // edges = (NodeA_ID, NodeB_ID, Weight)
        public Graph(int nodeCount, List<(int, int, int)> edges)
        {
            _graph = new List<Edge>[nodeCount];

            // Init Edge Lists
            for (int i = 0; i < nodeCount; i++)
            {
                _graph[i] = new List<Edge>();
            }

            // Set Edges
            foreach (var edge in edges)
            {
                SetEdge(edge.Item1, edge.Item2, edge.Item3);
            }
        }

        private void SetEdge(int nodeA, int nodeB, int weight)
        {
            // Check for valid inputs
            if (nodeA == nodeB || nodeA < 0 || nodeB < 0 || nodeA >= _graph.Length || nodeB >= _graph.Length || weight < 0)
            {
                Debug.LogError($"Edge between {nodeA} and {nodeB}, with weight of {weight}, could not get set");
            }

            // try to find Edge
            var edge = _graph[nodeA].Find((edge) => edge.To == nodeB);
            
            // update or create Edge
            if (edge == null)
            {
                _graph[nodeA].Add(new Edge(nodeB, weight));
            }
            else
            {
                edge.Weight = weight;
            }
        }

        public List<int> FindPath(int start, int end, int distanceLimit = Int32.MaxValue)
        {
            // declare path
            var path = new List<int>();

            // create Arrays
            List<int> unvisited = new List<int>();
            int[] distances = new int[_graph.Length];
            int[] previous = new int[_graph.Length];

            // init distances and previous
            for (int i = 0; i < _graph.Length; i++)
            {
                distances[i] = int.MaxValue;    // infinite
                previous[i] = -1;               // undefined
                unvisited.Add(i);
            }

            // starting Node has distance of 0
            distances[start] = 0;

            while (unvisited.Count > 0)
            {
                // finding closest
                int current = -1;
                int minDist = int.MaxValue;
                foreach (int node in unvisited)
                {
                    if (distances[node] < minDist)
                    {
                        minDist = distances[node];
                        current = node;
                    }
                }
                
                // check if current == end
                if (current == end)
                {
                    int temp = end;
                    while (temp != -1)
                    {
                        path.Add(temp);
                        temp = previous[temp];
                    }
                    path.Reverse();
                    return path;
                }

                
                // remove closest from Q
                unvisited.Remove(current);

                // Update Neighbours
                foreach (var edge in _graph[current])
                {
                    var newDist = distances[current] + edge.Weight;

                    if (newDist > distanceLimit)
                        continue;
                    
                    if (newDist < distances[edge.To])
                    {
                        distances[edge.To] = newDist;
                        previous[edge.To] = current;
                    }
                }
            }
            
            // return path
            Debug.LogWarning($"No path found from {start} to {end} within distance limit {distanceLimit}");
            return path;
        }
    }
}
