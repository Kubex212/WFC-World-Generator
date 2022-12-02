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
        public List<Tile> tiles = new List<Tile>();

        public Tile AddTile()
        {
            var newTile = new Tile();
            newTile.Neighbors.AddRange(Enumerable.Repeat(0, 8).Select((_) => new List<Tile>()));
            tiles.Add(newTile);
            return newTile;
        }

        public string Serialize()
        {
            var exportObject = new ExportObject() { Tiles = tiles };
            return JsonConvert.SerializeObject(exportObject, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
        }

        public void Deserialize(string json)
        {
            var importedObject = JsonConvert.DeserializeObject<ExportObject>(json);
            tiles = importedObject.Tiles;
        }
        public class ExportObject
        {
            public List<Tile> Tiles { get; set; }
        }
    }
}
