using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

namespace OpenGLCompGraph
{
    public struct Face
    {
        public uint vert0, vert1, vert2;
    }

    public class Model
    {
        private int _vertexBufferID;
        private int _vertexArrayID;
        private int _normalBufferID;
        private int _elementBufferID;

        string filePath;

        List<Vector3> vertices;
        List<Vector3> normals;

        Vector3 rotation = Vector3.Zero;
        Vector3 translation = Vector3.Zero;
        Vector3 scalar = Vector3.One;

        List<Face> faces;

        Face[] faceArray;

        public Matrix4 transform;

        public Model(string filepath)
        {
            this.filePath = filepath;

            vertices = new List<Vector3>();
            normals = new List<Vector3>();
            faces = new List<Face>();

            transform = Matrix4.Identity;
            rotation = Vector3.Zero;
        }

        public bool IsReady()
        {
            //if model is loaded
            if (vertices.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void GenerateNormals()
        {
            normals.Resize(vertices.Count());

            for (int index = 0; index < faces.Count(); index++)
            {
                uint vertexIndex0 = faces[index].vert0;
                uint vertexIndex1 = faces[index].vert1;
                uint vertexIndex2 = faces[index].vert2;

                Vector3 vert0 = vertices[(int)vertexIndex0];
                Vector3 vert1 = vertices[(int)vertexIndex1];
                Vector3 vert2 = vertices[(int)vertexIndex2];

                Vector3 edge0 = vert1 - vert0;
                Vector3 edge1 = vert2 - vert0;

                Vector3 normal = Vector3.Cross(edge0, edge1);

                normals[(int)vertexIndex0] += normal;
                normals[(int)vertexIndex1] += normal;
                normals[(int)vertexIndex2] += normal;
            }

            for (int index = 0; index < normals.Count(); index++)
            {
                normals[index] = Vector3.Normalize(normals[index]);
            }
        }

        public void LoadModel()
        {
            using (var sr = new StreamReader(filePath, Encoding.UTF8))
            {
                string   line      ;
                string   lineType  ;
                string[] lineTokens;

                //reads obj file line by line to determine type of information and adds data to specified lists
                while ((line = sr.ReadLine()) != null)
                {
                    StringReader lineStream = new StringReader(line);

                    lineTokens = line.Split(' ');

                    lineType = lineTokens[0];

                    if (lineType.Equals("v"))
                    {
                        Vector3 newVector = new Vector3();

                        newVector.X = float.Parse(lineTokens[1]);
                        newVector.Y = float.Parse(lineTokens[2]);
                        newVector.Z = float.Parse(lineTokens[3]);

                        vertices.Add(newVector);
                    }
                    else if (lineType.Equals("f"))
                    {
                        Face newface = new Face();

                        newface.vert0 = uint.Parse(lineTokens[1]) - 1;
                        newface.vert1 = uint.Parse(lineTokens[2]) - 1;
                        newface.vert2 = uint.Parse(lineTokens[3]) - 1;

                        faces.Add(newface);
                    }
                }

                if (normals.Count == 0)
                {
                    GenerateNormals();
                }
            }
        }

        public void BufferModel()
        {
            _vertexBufferID = GL.GenBuffer();
            _vertexArrayID = GL.GenBuffer();
            _normalBufferID = GL.GenBuffer();
            _elementBufferID = GL.GenBuffer();
            //vertex position buffer
            GL.BindVertexArray(_vertexArrayID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferID);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Vector3.SizeInBytes, vertices.ToArray(), BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);

            ////normal buffer
            if (normals.Count != 0)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _normalBufferID);
                GL.BufferData(BufferTarget.ArrayBuffer, normals.Count * Vector3.SizeInBytes, normals.ToArray(), BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(1);
            }

            faceArray = faces.ToArray();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, faces.Count * ListExtras.FaceSizeInBytes, faceArray, BufferUsageHint.StaticDraw);
            //Console.Write(GL.GetError());
            GL.BindVertexArray(0);
        }

        public void RotateModelByYAxis(double deltaTime)
        {
            rotation[1] += 1.0f * (float)deltaTime;      
        }

        public void TranslateModelUp   (double deltaTime)
        {            
            translation[1] += 1.0f * (float)deltaTime;               
        }
        public void TranslateModelDown (double deltaTime)
        {           
            translation[1] -= 1.0f * (float)deltaTime;            
        }
        public void TranslateModelLeft (double deltaTime)
        {           
            translation[0] -= 1.0f * (float)deltaTime;           
        }
        public void TranslateModelRight(double deltaTime)
        {            
            translation[0] += 1.0f * (float)deltaTime;            
        }

        public void UpScaleModel(double deltaTime)
        {
            scalar[0] += 1.0f * (float)deltaTime;
            scalar[1] += 1.0f * (float)deltaTime;
            scalar[2] += 1.0f * (float)deltaTime;

            Console.WriteLine(scalar);
        }
        public void DownScaleModel(double deltaTime)
        {
            scalar[0] -= 1.0f * (float)deltaTime;
            scalar[1] -= 1.0f * (float)deltaTime;
            scalar[2] -= 1.0f * (float)deltaTime;
            Console.WriteLine(scalar);
        }

        public void UpdateTransform()
        {
            transform = Matrix4.Identity;
            //Rotation about Y
            transform *= Matrix4.CreateRotationY(rotation[1]);
            //Translation modifying x and y
            transform *= Matrix4.CreateTranslation(translation);
            //Scale by all axis equally
            transform *= Matrix4.CreateScale(scalar);
        }

        public void RenderModel()
        {
            GL.BindVertexArray(_vertexArrayID);

            long[] sizeRef = new long[1];

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, sizeRef);

            int count = ((int)sizeRef[0] / sizeof(uint));

            GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);

        }
    }

    public static class ListExtras
    {
        public static void Resize<T>(this List<T> list, int size, T element = default(T))
        {
            int count = list.Count;

            if (size < count)
            {
                list.RemoveRange(size, count - size);
            }
            else if (size > count)
            {
                if (size > list.Capacity)
                {
                    list.Capacity = size;
                }

                list.AddRange(Enumerable.Repeat(element, size - count));
            }
        }

        public static int FaceSizeInBytes = sizeof(uint) * 3;
    }
    



}
