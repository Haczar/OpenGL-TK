using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;


namespace OpenGL_CompGraph
{
    public class Shader
    {
        int Handle;

        readonly Dictionary<string, int> uniformLocations;

        public Shader(string vertexPath, string fragmentPath)
        {
            int VertexShaderID;
            int FragmentShaderID;

            //Generate shaders,bind the source code to the shaders, then compile
            string VertexShaderSource;
            using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
            {
                VertexShaderSource = reader.ReadToEnd();
            }
            VertexShaderID = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShaderID, VertexShaderSource);
            CompileShader(VertexShaderID);

            string FragmentShaderSource;
            using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
            {
                FragmentShaderSource = reader.ReadToEnd();
            }
            FragmentShaderID = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShaderID, FragmentShaderSource);
            CompileShader(FragmentShaderID);

            //create program
            Handle = GL.CreateProgram();
            
            //attach both shaders
            GL.AttachShader(Handle, VertexShaderID);
            GL.AttachShader(Handle, FragmentShaderID);

            //link shaders together
            LinkProgram(Handle);

            //detach and delete leftover shaders
            GL.DetachShader(Handle, VertexShaderID);
            GL.DetachShader(Handle, FragmentShaderID);
            GL.DeleteShader(FragmentShaderID);
            GL.DeleteShader(VertexShaderID);

            // First, we have to get the number of active uniforms in the shader.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (int index = 0; index < numberOfUniforms; index++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(Handle, index, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                uniformLocations.Add(key, location);
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(uniformLocations[name], true, ref data);
        }
        
        private static void CompileShader(int shader)
        {
            //try to compile
            GL.CompileShader(shader);

            //check for compile errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infolog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occured while compiling shader: \n\n {infolog}");
            }
        }

        private static void LinkProgram(int program)
        {
            //try to link program
            GL.LinkProgram(program);

            //check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if(code != (int)All.True)
            {
                throw new Exception($"Error occurred while linking program({program})");
            }
        }

        private bool disposedValue = false;

        public virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            GL.DeleteProgram(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(uniformLocations[name], data);
        }
        
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(uniformLocations[name], data);
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(uniformLocations[name], data);
        }
    }
}
