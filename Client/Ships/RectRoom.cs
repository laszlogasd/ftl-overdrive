﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFML.Graphics;

namespace FTLOverdrive.Client.Ships
{
    public class RectRoom : Room
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        private Tile[] tiles;

        public RectRoom(Ship ship, int id, float x = 0, float y = 0, int w = 1, int h = 1) : base(ship, id, x, y)
        {
            Width = w;
            Height = h;

            tiles = new Tile[w * h];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    tiles[i + Width * j] = new Tile(this, i, j);
                }
            }
        }

        public override IEnumerable<Tile> GetTiles()
        {
            return tiles;
        }

        public override Tile GetTile(int x, int y)
        {
            if (x < 0 || x >= Width) return null;
            if (y < 0 || y >= Height) return null;
            return tiles[x + Width * y];
        }

        public override bool HasTile(int x, int y)
        {
            if (x < 0 || x >= Width) return false;
            if (y < 0 || y >= Height) return false;
            return true;
        }

        public override IntRect GetBoundingBox()
        {
            return new IntRect(0, 0, Width, Height);
        }
    }
}
