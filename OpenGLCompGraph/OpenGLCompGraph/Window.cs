﻿using OpenGLCompGraph;
using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
namespace OpenGL_TK
{
    public class Window : GameWindow
    {
        private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);

        private int _vertexBufferObject;
        private int _vertexArrayObject_Model;
        private int _vertexArrayObject_Lamp;

        private bool _firstMove = true;

        private Shader _lampShader;
        private Shader _modelShader;

        private Model importedModel;

        private Camera _camera;

        private Vector2 _lastPos;

        public Window(int width, int height, string title)
            : base(width, height, GraphicsMode.Default, title)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            
            CursorVisible = false;

            _modelShader = new Shader("Shaders/shader.vert", "Shaders/lighting.frag");
            _lampShader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

            GL.Enable(EnableCap.DepthTest);

            _camera = new Camera(Vector3.UnitZ * 3, Width / (float)Height);

            //Model Load
            //importedModel = new Model("Models/gargoyle.obj");
            importedModel = new Model("Models/bunny.obj");
            //importedModel = new Model("Models/eight.obj");
            //importedModel = new Model("Models/hand.obj");
            //importedModel = new Model("Models/horse.obj");
            //importedModel = new Model("Models/sculpture.obj");
            //importedModel = new Model("Models/topology.obj");
            //importedModel = new Model("Models/torus.obj");

            importedModel.LoadModel();
            importedModel.BufferModel();              
            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObject_Model);

            _modelShader.Use();

            _modelShader.SetMatrix4("model"     , importedModel.transform);
            _modelShader.SetMatrix4("view"      , _camera.GetViewMatrix());
            _modelShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            _modelShader.SetVector3("viewPos"   , _camera.Position);

            // Here we set the material values of the cube
            _modelShader.SetVector3("material.ambient" , new Vector3(1.0f, 0.5f, 0.31f));
            _modelShader.SetVector3("material.diffuse" , new Vector3(1.0f, 0.5f, 0.31f));
            _modelShader.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
            _modelShader.SetFloat("material.shininess" , 32.0f);

            // This is where we change the lights color over time using the sin function
            Vector3 lightColor;
            float time = DateTime.Now.Second + DateTime.Now.Millisecond / 1000f;
            lightColor.X = (float)Math.Sin(time * 2.0f);
            lightColor.Y = (float)Math.Sin(time * 0.7f);
            lightColor.Z = (float)Math.Sin(time * 1.3f);

            // The ambient light is less intensive than the diffuse light in order to make it less dominant
            Vector3 ambientColor = lightColor * new Vector3(0.2f);
            Vector3 diffuseColor = lightColor * new Vector3(0.5f);

            _modelShader.SetVector3("light.position", _lightPos);
            _modelShader.SetVector3("light.ambient", ambientColor);
            _modelShader.SetVector3("light.diffuse", diffuseColor);
            _modelShader.SetVector3("light.specular", new Vector3(1.0f, 1.0f, 1.0f));

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            importedModel.RenderModel();
            importedModel.UpdateTransform();

            GL.BindVertexArray(_vertexArrayObject_Model);

            _lampShader.Use();

            Matrix4 lampMatrix = Matrix4.Identity;
            lampMatrix *= Matrix4.CreateScale(0.2f);
            lampMatrix *= Matrix4.CreateTranslation(_lightPos);

            _lampShader.SetMatrix4("model", lampMatrix);
            _lampShader.SetMatrix4("view", _camera.GetViewMatrix());
            _lampShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            
            GL.BindVertexArray(_vertexArrayObject_Lamp);
            GL.BindVertexArray(0);

            SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!Focused)
            {
                return;
            }

            var input = Keyboard.GetState();

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }
            if (input.IsKeyDown(Key.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }
            if (input.IsKeyDown(Key.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Key.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Key.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Key.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Key.LShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }
            if (input.IsKeyDown(Key.R))
            {
                importedModel.RotateModelByYAxis(e.Time);
            }
            if (input.IsKeyDown(Key.Up))
            {
                importedModel.TranslateModelUp(e.Time);
            }
            if (input.IsKeyDown(Key.Left))
            {
                importedModel.TranslateModelLeft(e.Time);
            }
            if (input.IsKeyDown(Key.Down))
            {
                importedModel.TranslateModelDown(e.Time);
            }
            if (input.IsKeyDown(Key.Right))
            {
                importedModel.TranslateModelRight(e.Time);
            }
            if (input.IsKeyDown(Key.Minus))
            {
                importedModel.DownScaleModel(e.Time);
            }
            if (input.IsKeyDown(Key.Plus))
            {
                importedModel.UpScaleModel(e.Time);
            }

            var mouse = Mouse.GetState();

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (Focused)
            {
                Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
            }

            base.OnMouseMove(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            _camera.AspectRatio = Width / (float)Height;
            base.OnResize(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject_Model);
            GL.DeleteVertexArray(_vertexArrayObject_Lamp);

            GL.DeleteProgram(_lampShader.Handle);
            GL.DeleteProgram(_modelShader.Handle);

            base.OnUnload(e);
        }
    }
}