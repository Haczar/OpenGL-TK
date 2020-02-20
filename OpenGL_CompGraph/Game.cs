using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;


namespace OpenGL_CompGraph
{
    public class Game : GameWindow
    {
        int     elementBufferObject;
        int     vertexBufferObject ;
        int     vertexArrayObject  ;
        Shader  shader             ;

        float[] triVertices  = { -0.5f, -0.5f, 0.0f,    //bottom-left vertex
                                  0.5f, -0.5f, 0.0f,    //bottom-right vertex
                                  0.0f,  0.5f, 0.0f };   // top vertex  = all form a triangle shape
        float[] rectVertices = { 0.5f,  0.5f, 0.0f,   //top-right    vertex
                                 0.5f, -0.5f, 0.0f,   //bottom-right vertex
                                -0.5f, -0.5f, 0.0f,   //bottom-left  vertex
                                -0.5f,  0.5f, 0.0f }; //top-left     vertex
        float[] cubeVertices = { 0.5f,  0.5f, 0,0f,    //top-right
                                 0.5f, -0.5f, 0.0f,    //bottom-right
                                -0.5f, -0.5f, 0.0f,    //bottom-left
                                -0,5f,  0.5f, 0.0f, }; //top-left
        
        //this array controls how the EBO (element buffer object) will use those 
        // vertices to create triangles
        uint[] indices = { 0, 1, 3,     //bottom-right half of the triangle
                           1, 2, 3 };   //top-right half of triangle

        //override base constructor of game 
        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
        }

        //override of a gamewindow function "OnLoad", runs one time after window first opens. 
        // Any initialization code goes here.
        protected override void OnLoad(EventArgs e)
        {
            //decides the color of the window after it gets cleared between frames
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            ///<image src = "C:\Users\Hacza\OneDrive\Documents\ShareX\Screenshots\2020-02\color.png" scale="1.0"/>

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            InitBufferForCube(); // PLACE TO PUT SHAPE

            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            InitBufferforElement();

            shader = new Shader("shader.vert", "shader.frag");
            shader.Use();

            //generate vertex array object and bind
            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            //bind VBO and EBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);

            var vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            

            base.OnLoad(e);
        }

        //override of a gamewindow function "OnRenderFrame", 
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //clears the screen, using the color set in OnLoad.
            //Should always be the first function called when rendering
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //Bind the VertexArrayObject
            GL.BindVertexArray(vertexArrayObject);

            //create a transform that rotates the rectangle by 20 degrees
            var transform = Matrix4.Identity;
            transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(90f));
            transform *= Matrix4.CreateScale(1.1f);
            transform *= Matrix4.CreateTranslation(0.1f, 0.1f, 0.0f);

            shader.Use();

            shader.SetMatrix4("transform", transform);

            DrawElement();

            //Double-buffering means that there are two areas that OpenGL draws to. 
            //In essence: One area is displayed, while the other is being rendered to. 
            //In SwapBuffers(), the two are reversed.
            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        //override of a gamewindow function "OnUpdateFrame" in order to exit when we press escape key
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //Detecting key presses example where when we press escape we quit program.
            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            base.OnUpdateFrame(e);
        }

        //runs every time the window gets resized
        protected override void OnResize(EventArgs e)
        {
            //maps the NDC (normalized device coordinates) to the window
            GL.Viewport(0, 0, Width, Height);

            base.OnResize(e);
        }

        //override of a gamewindow function "OnUnload", runs when application exists. 
        protected override void OnUnload(EventArgs e)
        {
            //Unbind all the resources by binding targets to 0/null
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); //binding the buffer to 0 sets it to null  so that any calls that modify a buffer without binding one first results in a crash
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            //Delete all the resources
            GL.DeleteBuffer(vertexBufferObject);
            GL.DeleteVertexArray(vertexArrayObject);

            shader.Dispose();

            base.OnUnload(e);
        }

        //BufferData(Creates and Initializes a buffer object's data store)
        private void InitBufferforElement()
        {
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
        }
        private void InitBufferforTriangle()
        {
            GL.BufferData(BufferTarget.ArrayBuffer, triVertices.Length * sizeof(float), triVertices, BufferUsageHint.StaticDraw);
        }
        private void InitBufferForRect()
        {
            GL.BufferData(BufferTarget.ArrayBuffer, rectVertices.Length * sizeof(float), rectVertices, BufferUsageHint.StaticDraw);
        }
        private void InitBufferForCube()
        {
            GL.BufferData(BufferTarget.ArrayBuffer, cubeVertices.Length * sizeof(float), cubeVertices, BufferUsageHint.StaticDraw);
        }
        
        //Draw functions
        private void DrawTriangle()
        {
            //Draw triangle (shape, vertexindex, count)
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }   
        private void DrawElement()
        {
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        }

    }
}
