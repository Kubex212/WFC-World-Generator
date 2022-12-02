using Graphs;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Tiles
{
    public class TileCollection
    {
        public List<Tile> Tiles = new List<Tile>();

        public Tile AddTile()
        {
            var newTile = new Tile();
            newTile.Neighbors.AddRange(Enumerable.Repeat(0, 8).Select((_) => new List<Tile>()));
            Tiles.Add(newTile);
            return newTile;
        }

        public string Serialize()
        {
            var exportObject = new ExportObject() { Tiles = Tiles };
            return JsonConvert.SerializeObject(exportObject, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
        }

        public void Deserialize(string json)
        {
            var importedObject = JsonConvert.DeserializeObject<ExportObject>(json);
            Tiles = importedObject.Tiles;
        }
        public class ExportObject
        {
            public List<Tile> Tiles { get; set; }
        }
    }
}
