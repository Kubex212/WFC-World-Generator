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
        public bool IsValid => !ValidationErrors.Any();

        public List<string> ValidationErrors
        {
            get
            {
                var errors = new List<string>();

                if(!tiles.Any(t => t.Walkable))
                {
                    errors.Add("W zestawie kafelków musi byæ przynajmniej jeden kafelek pod³o¿a.");
                }

                if(!tiles.Any(t => !t.Walkable))
                {
                    errors.Add("W zestawie kafelków musi byæ przynajmniej jeden kafelek œciany.");
                }

                if (edgeTile == null)
                {
                    errors.Add("W zestawie kafelków musi byæ kafelek krawêdzi.");
                }

                return errors;
            }
        }

        public Tile edgeTile;
        public bool diagonal;

        public Tile AddTile()
        {
            var newTile = new Tile();
            newTile.Neighbors.AddRange(Enumerable.Repeat(0, 8).Select((_) => new List<Tile>()));
            tiles.Add(newTile);
            return newTile;
        }

        public string Serialize()
        {
            var exportObject = new ExportObject() { Tiles = tiles, EdgeTile = edgeTile, Diagonal = diagonal };
            return JsonConvert.SerializeObject(exportObject, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
        }

        public void Deserialize(string json)
        {
            var importedObject = JsonConvert.DeserializeObject<ExportObject>(json);
            tiles = importedObject.Tiles;
            edgeTile = importedObject.EdgeTile;
            diagonal = importedObject.Diagonal;
        }

        public class ExportObject
        {
            public List<Tile> Tiles { get; set; }
            public Tile EdgeTile { get; set; }
            public bool Diagonal { get; set; }
        }
    }
}
