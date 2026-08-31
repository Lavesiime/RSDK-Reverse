namespace RSDKv3_4
{
    public class Tiles128x128
    {
        [System.Serializable]
        public class Block
        {
            [System.Serializable]
            public class Tile
            {
                public enum VisualPlanes
                {
                    Low,
                    High,
                }
                public enum Directions
                {
                    FlipNone,
                    FlipX,
                    FlipY,
                    FlipXY,
                }
                public enum Solidities
                {
                    SolidAll,
                    SolidTop,
                    SolidAllButTop,
                    SolidNone,
                    SolidTopNoGrip,
                }

                /// <summary>
                /// if tile is on the high or low layer
                /// </summary>
                public VisualPlanes visualPlane = VisualPlanes.Low;
                /// <summary>
                /// the flip value of the tile
                /// </summary>
                public Directions direction = Directions.FlipNone;
                /// <summary>
                /// the Tile's index
                /// </summary>
                public ushort tileIndex = 0;
                /// <summary>
                /// the solidity for Collision Path A
                /// </summary>
                public Solidities solidityA = Solidities.SolidNone;
                /// <summary>
                /// the solidity for Collision Path B
                /// </summary>
                public Solidities solidityB = Solidities.SolidNone;

                public Tile() { }

                public void Read(Reader reader)
                {
					// In some cases such as CD's various menu scenes, the 128x128Tiles.bin file only contains half the amount of chunks it should..
					// Because of this, the file just isn't as large as we'd expect it to be (which is bad)
					// Instead of checking EoF every block, we use Read here instead:
					// - when the expected amount of bytes can't be found, the buffer is just left at zero (so we create default chunks)
                    byte[] tileBytes = new byte[3];
                    reader.Read(tileBytes, 0, tileBytes.Length);

					// Tile format in bits: 00VVDDII IIIIIIII AAAABBBB
					// - 00: Unused, always zero
					// - VV: Visual plane
					// - DD: Direction
					// - II: Tile index
					// - AA: Solidity A
					// - BB: Solidity B

                    visualPlane = (VisualPlanes)((tileBytes[0] >> 4) & 3);
                    direction = (Directions)((tileBytes[0] >> 2) & 3);
                    tileIndex = (ushort)(((tileBytes[0] & 3) << 8) | tileBytes[1]);

                    solidityA = (Solidities)(tileBytes[2] >> 4);
                    solidityB = (Solidities)(tileBytes[2] & 0x0F);
                }

                public void Write(Writer writer)
                {
                    byte[] tileBytes = new byte[3];

					// See Read() for format details, we just stuff all the properties back into their bytes now

                    tileBytes[0] = (byte)(((int)visualPlane << 4) | ((int)direction << 2) | (tileIndex >> 8));
                    tileBytes[1] = (byte)(tileIndex & 0xFF);
                    tileBytes[2] = (byte)((int)solidityA << 4 | (int)solidityB);

                    writer.Write(tileBytes[0]);
                    writer.Write(tileBytes[1]);
                    writer.Write(tileBytes[2]);
                }
            }

            /// <summary>
            /// the list of tiles in this chunk
            /// </summary>
            public Tile[][] tiles;

            public Block()
            {
                tiles = new Tile[8][];
                for (int y = 0; y < 8; y++)
                {
                    tiles[y] = new Tile[8];
                    for (int x = 0; x < 8; x++)
                        tiles[y][x] = new Tile();
                }
            }
            public void Read(Reader reader)
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        tiles[y][x].Read(reader);
                    }
                }
            }

            public void Write(Writer writer)
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        tiles[y][x].Write(writer);
                    }
                }
            }
        }

        /// <summary>
        /// The number of chunks that a stage has
        /// </summary>
        public const int CHUNK_LIST_SIZE = 512;

        /// <summary>
        /// the list of chunks in the file
        /// </summary>
        public Block[] chunkList = new Block[CHUNK_LIST_SIZE];

        public Tiles128x128()
        {
            for (int i = 0; i < chunkList.Length; i++)
                chunkList[i] = new Block();
        }

        public Tiles128x128(string filepath) : this(new Reader(filepath)) { }

        public Tiles128x128(System.IO.Stream strm) : this(new Reader(strm)) { }

        public Tiles128x128(Reader reader) : this()
        {
            Read(reader);
        }

        public void Read(Reader reader)
        {
            for (int c = 0; c < CHUNK_LIST_SIZE; c++)
                chunkList[c].Read(reader);

            reader.Close();
        }

        public void Write(string filename)
        {
            using (Writer writer = new Writer(filename))
                Write(writer);
        }

        public void Write(System.IO.Stream stream)
        {
            using (Writer writer = new Writer(stream))
                Write(writer);
        }

        public void Write(Writer writer)
        {
            for (int c = 0; c < CHUNK_LIST_SIZE; c++)
                chunkList[c].Write(writer);

            writer.Close();
        }
    }
}
