﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using RSDKv3_4;

namespace RSDKv4
{
    public class Model
    {
        public static readonly byte[] signature = new byte[] { (byte)'R', (byte)'3', (byte)'D', 0 };

        public class Vertex
        {
            /// <summary>
            /// Vertex X
            /// </summary>
            public float x = 0.0f;
            /// <summary>
            /// Vertex Y
            /// </summary>
            public float y = 0.0f;
            /// <summary>
            /// Vertex Z
            /// </summary>
            public float z = 0.0f;

            /// <summary>
            /// Vertex Normal X
            /// </summary>
            public float nx = 0.0f;
            /// <summary>
            /// Vertex Normal Y
            /// </summary>
            public float ny = 0.0f;
            /// <summary>
            /// Vertex Normal Z
            /// </summary>
            public float nz = 0.0f;

            public Vertex() { }
        }

        public class TextureUV
        {
            public float u = 0.0f;
            public float v = 0.0f;

            public TextureUV() { }
        }

        public class Frame
        {
            public List<Vertex> vertices = new List<Vertex>();

            public Frame() { }
        };

        /// <summary>
        /// the list of frames, used to animate the model
        /// </summary>
        public List<Frame> frames = new List<Frame>();
        /// <summary>
        /// the list of all the indices in the model
        /// </summary>
        public List<ushort> indices = new List<ushort>();
        /// <summary>
        /// the list of all the texture UV positions
        /// </summary>
        public List<TextureUV> textureUVs = new List<TextureUV>();

        public Model() { }

        public Model(string filename) : this(new Reader(filename)) { }

        public Model(Stream stream) : this(new Reader(stream)) { }

        public Model(Reader reader)
        {
            read(reader);
        }

        public void read(Reader reader)
        {
            if (!reader.readBytes(4).SequenceEqual(signature))
            {
                reader.Close();
                throw new Exception("Invalid Model v4 signature");
            }

            ushort vertCount = reader.ReadUInt16();

            textureUVs.Clear();
            for (int t = 0; t < vertCount; t++)
            {
                TextureUV uv = new TextureUV();
                uv.u = reader.ReadSingle();
                uv.v = reader.ReadSingle();
                textureUVs.Add(uv);
            }

            int indexCount = reader.ReadUInt16() * 3;
            indices.Clear();
            for (int i = 0; i < indexCount; ++i)
            {
                indices.Add(reader.ReadUInt16());
            }

            ushort frameCount = reader.ReadUInt16();
            frames.Clear();
            for (int f = 0; f < frameCount; ++f)
            {
                Frame frame = new Frame();
                for (int v = 0; v < vertCount; ++v)
                {
                    Vertex vert = new Vertex();
                    vert.x = reader.ReadSingle();
                    vert.y = reader.ReadSingle();
                    vert.z = reader.ReadSingle();

                    vert.nx = reader.ReadSingle();
                    vert.ny = reader.ReadSingle();
                    vert.nz = reader.ReadSingle();
                    frame.vertices.Add(vert);
                }
                frames.Add(frame);
            }

            reader.Close();
        }

        public void write(string filename)
        {
            using (Writer writer = new Writer(filename))
                write(writer);
        }

        public void write(System.IO.Stream stream)
        {
            using (Writer writer = new Writer(stream))
                write(writer);
        }

        public void write(Writer writer)
        {
            writer.Write(signature);

            int vertCount = frames.Count >= 0 ? frames[0].vertices.Count : 0;
            writer.Write((ushort)vertCount);

            for (int v = 0; v < vertCount; ++v)
            {
                writer.Write(textureUVs[v].u);
                writer.Write(textureUVs[v].v);
            }

            writer.Write((ushort)(indices.Count / 3));
            for (int i = 0; i < indices.Count; ++i) writer.Write(indices[i]);

            writer.Write((ushort)frames.Count);
            for (int f = 0; f < frames.Count; ++f)
            {
                for (int v = 0; v < vertCount; ++v)
                {
                    writer.Write(frames[f].vertices[v].x);
                    writer.Write(frames[f].vertices[v].y);
                    writer.Write(frames[f].vertices[v].z);

                    writer.Write(frames[f].vertices[v].nx);
                    writer.Write(frames[f].vertices[v].ny);
                    writer.Write(frames[f].vertices[v].nz);
                }
            }

            writer.Close();
        }

        public void writeAsOBJ(string filename, int exportFrame = -1)
        {
            for (int f = (exportFrame < 0 ? 0 : exportFrame); f < frames.Count; ++f)
            {
                string path = filename;
                string extLess = path.Replace(Path.GetExtension(path), "");
                string streamName = extLess + (frames.Count > 1 ? (" Frame " + f + "") : "") + Path.GetExtension(path);

                StringBuilder builder = new StringBuilder();

                // Object
                builder.AppendLine("o RSDKModelv4");

                builder.AppendLine("");
                for (int v = 0; v < frames[f].vertices.Count; ++v)
                    builder.AppendLine(string.Format("v {0} {1} {2}", frames[f].vertices[v].x, frames[f].vertices[v].y, frames[f].vertices[v].z));

                builder.AppendLine("");
                for (int v = 0; v < frames[f].vertices.Count; ++v)
                    builder.AppendLine(string.Format("vn {0} {1} {2}", frames[f].vertices[v].nx, frames[f].vertices[v].ny, frames[f].vertices[v].nz));

                builder.AppendLine("");
                for (int t = 0; t < textureUVs.Count; ++t)
                    builder.AppendLine(string.Format("vt {0} {1}", textureUVs[t].u, textureUVs[t].v));

                builder.AppendLine("");
                builder.AppendLine("usemtl None");
                builder.AppendLine("s off");
                builder.AppendLine("");

                for (int i = 0; i < indices.Count; i += 3)
                {
                    List<ushort> verts = new List<ushort>();
                    for (int v = 0; v < 3; ++v) verts.Add(indices[i + v]);

                    builder.AppendLine(string.Format("f {0} {1} {2}", verts[0] + 1, verts[1] + 1, verts[2] + 1));
                    builder.AppendLine("");
                }

                File.WriteAllText(streamName, builder.ToString());
            }
        }
    }
}
