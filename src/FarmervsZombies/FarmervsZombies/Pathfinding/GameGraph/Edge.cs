namespace FarmervsZombies.Pathfinding.GameGraph
{
    internal sealed class Edge
    {
        public Vertex TargetVertex { get; }
        public float Weight { get; set; }

        public Edge(Vertex targetVertex, float weight)
        {
            TargetVertex = targetVertex;
            Weight = weight;
        }
    }
}
