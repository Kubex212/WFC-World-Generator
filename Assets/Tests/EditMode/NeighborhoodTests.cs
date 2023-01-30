using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Tiles;
public class NeighboorhoodTests
{
    [Test]
    public void TestSymmetry()
    {
        var tc = GetSampleTileCollection();
        foreach(var tile in tc.tiles)
        {
            foreach(var n in tile.Neighbors[(int)Direction.North])
            {
                Assert.IsTrue(n.Neighbors[(int)Direction.South].Contains(tile));
            }

            foreach (var n in tile.Neighbors[(int)Direction.South])
            {
                Assert.IsTrue(n.Neighbors[(int)Direction.North].Contains(tile));
            }

            foreach (var n in tile.Neighbors[(int)Direction.East])
            {
                Assert.IsTrue(n.Neighbors[(int)Direction.West].Contains(tile));
            }

            foreach (var n in tile.Neighbors[(int)Direction.West])
            {
                Assert.IsTrue(n.Neighbors[(int)Direction.East].Contains(tile));
            }
        }
        tc = GetTileCollectionFromJson();
        foreach (var tile in tc.tiles)
        {
            foreach (var n in tile.Neighbors[(int)Direction.North])
            {
                Assert.IsTrue(n.Neighbors[(int)Direction.South].Contains(tile));
            }

            foreach (var n in tile.Neighbors[(int)Direction.South])
            {
                Assert.IsTrue(n.Neighbors[(int)Direction.North].Contains(tile));
            }

            foreach (var n in tile.Neighbors[(int)Direction.East])
            {
                Assert.IsTrue(n.Neighbors[(int)Direction.West].Contains(tile));
            }

            foreach (var n in tile.Neighbors[(int)Direction.West])
            {
                Assert.IsTrue(n.Neighbors[(int)Direction.East].Contains(tile));
            }
        }
    }

    [Test]
    public void TestValidity()
    {
        var tc = GetInvalidTileCollection();
        Assert.IsFalse(tc.IsValid);
    }

    [Test]
    public void TestDeserialization()
    {
        var tc = GetTileCollectionFromJson();
        Assert.AreEqual(14, tc.tiles.Count);
        foreach(var t in tc.tiles)
        {
            Assert.AreEqual(8, t.Neighbors.Count);
            for(int i = 0; i < t.Neighbors.Count; i += 2)
            {
                var n = t.Neighbors[i];
                Assert.IsFalse(n.Count == 0);
            }
        }
    }

    private TileCollection GetSampleTileCollection()
    {
        TileCollection tiles = new TileCollection();
        var tile0 = tiles.AddTile();
        var tile1 = tiles.AddTile();
        var tile2 = tiles.AddTile();

        tiles.edgeTile = tile0;
        tile1.Walkable = true;
        var dir = Direction.North;
        tile2.AddNeighbor(tile0, dir);
        tile0.AddNeighbor(tile2, dir.Opposite());
        dir = Direction.East;
        tile1.AddNeighbor(tile0, dir);
        tile0.AddNeighbor(tile1, dir.Opposite());

        return tiles;
    }

    private TileCollection GetInvalidTileCollection()
    {
        TileCollection tiles = new TileCollection();
        var tile0 = tiles.AddTile();
        var tile1 = tiles.AddTile();
        var tile2 = tiles.AddTile();

        tile1.Walkable = true;
        var dir = Direction.North;
        tile2.AddNeighbor(tile0, dir);
        tile0.AddNeighbor(tile2, dir.Opposite());
        dir = Direction.East;
        tile1.AddNeighbor(tile0, dir);
        tile0.AddNeighbor(tile1, dir.Opposite());
        return tiles;
    }

    private TileCollection GetTileCollectionFromJson()
    {
        var json = "{\"$id\":\"1\",\"Tiles\":[{\"$id\":\"2\",\"Index\":0,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$id\":\"3\",\"Index\":1,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$id\":\"4\",\"Index\":13,\"Walkable\":true,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$ref\":\"4\"},{\"$id\":\"5\",\"Index\":6,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$id\":\"6\",\"Index\":2,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$ref\":\"6\"},{\"$id\":\"7\",\"Index\":5,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$ref\":\"4\"}],[],[{\"$ref\":\"4\"}],[],[{\"$ref\":\"6\"},{\"$id\":\"8\",\"Index\":11,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$ref\":\"6\"},{\"$ref\":\"7\"}],[],[{\"$ref\":\"3\"},{\"$ref\":\"7\"}],[],[{\"$ref\":\"2\"}],[],[{\"$ref\":\"2\"}],[]]},{\"$ref\":\"5\"}],[],[{\"$ref\":\"3\"},{\"$ref\":\"8\"},{\"$id\":\"9\",\"Index\":8,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$ref\":\"4\"}],[],[{\"$ref\":\"3\"},{\"$ref\":\"7\"},{\"$id\":\"10\",\"Index\":12,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$id\":\"11\",\"Index\":4,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$ref\":\"9\"},{\"$id\":\"12\",\"Index\":9,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$ref\":\"2\"}],[],[{\"$ref\":\"2\"}],[],[{\"$ref\":\"11\"},{\"$id\":\"13\",\"Index\":7,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$ref\":\"11\"},{\"$ref\":\"9\"},{\"$ref\":\"12\"}],[],[{\"$id\":\"14\",\"Index\":3,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$ref\":\"2\"}],[],[{\"$ref\":\"14\"},{\"$ref\":\"5\"},{\"$ref\":\"12\"}],[],[{\"$ref\":\"4\"}],[],[{\"$ref\":\"14\"},{\"$ref\":\"13\"},{\"$id\":\"15\",\"Index\":10,\"Walkable\":false,\"IsDoor\":false,\"Lock\":0,\"Key\":0,\"Neighbors\":[[{\"$ref\":\"2\"}],[],[{\"$ref\":\"14\"},{\"$ref\":\"5\"}],[],[{\"$ref\":\"6\"},{\"$ref\":\"5\"}],[],[{\"$ref\":\"2\"}],[]]}],[]]},{\"$ref\":\"5\"},{\"$ref\":\"12\"}],[],[{\"$ref\":\"4\"}],[],[{\"$ref\":\"4\"}],[]]}],[],[{\"$ref\":\"14\"},{\"$ref\":\"13\"}],[]]},{\"$ref\":\"11\"}],[],[{\"$ref\":\"2\"}],[],[{\"$ref\":\"13\"},{\"$ref\":\"10\"},{\"$ref\":\"11\"}],[],[{\"$ref\":\"4\"}],[]]},{\"$ref\":\"9\"}],[],[{\"$ref\":\"2\"}],[],[{\"$ref\":\"2\"}],[],[{\"$ref\":\"3\"},{\"$ref\":\"9\"}],[]]}],[],[{\"$ref\":\"11\"},{\"$ref\":\"13\"},{\"$ref\":\"10\"}],[],[{\"$ref\":\"4\"}],[]]}],[]]},{\"$ref\":\"15\"}],[],[{\"$ref\":\"4\"}],[],[{\"$ref\":\"6\"},{\"$ref\":\"5\"},{\"$ref\":\"8\"}],[],[{\"$ref\":\"2\"}],[]]},{\"$ref\":\"7\"},{\"$ref\":\"15\"}],[],[{\"$ref\":\"4\"}],[],[{\"$ref\":\"4\"}],[],[{\"$ref\":\"14\"},{\"$ref\":\"13\"},{\"$ref\":\"15\"}],[]]},{\"$ref\":\"13\"},{\"$ref\":\"14\"}],[],[{\"$ref\":\"11\"},{\"$ref\":\"13\"},{\"$ref\":\"9\"},{\"$ref\":\"4\"}],[],[{\"$ref\":\"3\"},{\"$ref\":\"7\"},{\"$ref\":\"9\"},{\"$ref\":\"4\"}],[],[{\"$ref\":\"6\"},{\"$ref\":\"7\"},{\"$ref\":\"5\"},{\"$ref\":\"4\"}],[]]}],[],[{\"$ref\":\"3\"},{\"$ref\":\"7\"},{\"$ref\":\"10\"}],[],[{\"$ref\":\"2\"}],[],[{\"$ref\":\"3\"},{\"$ref\":\"9\"},{\"$ref\":\"8\"}],[]]},{\"$ref\":\"8\"},{\"$ref\":\"10\"},{\"$ref\":\"2\"}],[],[{\"$ref\":\"6\"},{\"$ref\":\"15\"},{\"$ref\":\"8\"},{\"$ref\":\"2\"}],[],[{\"$ref\":\"14\"},{\"$ref\":\"12\"},{\"$ref\":\"15\"},{\"$ref\":\"2\"}],[],[{\"$ref\":\"11\"},{\"$ref\":\"12\"},{\"$ref\":\"10\"},{\"$ref\":\"2\"}],[]]},{\"$ref\":\"3\"},{\"$ref\":\"6\"},{\"$ref\":\"14\"},{\"$ref\":\"11\"},{\"$ref\":\"7\"},{\"$ref\":\"5\"},{\"$ref\":\"13\"},{\"$ref\":\"9\"},{\"$ref\":\"12\"},{\"$ref\":\"15\"},{\"$ref\":\"8\"},{\"$ref\":\"10\"},{\"$ref\":\"4\"}],\"EdgeTile\":{\"$ref\":\"2\"},\"Diagonal\":false}";
        var tc = new TileCollection();
        tc.Deserialize(json);
        return tc;
    }
}
