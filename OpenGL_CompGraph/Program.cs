using OpenTK;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_CompGraph
{
    class Program
    {
        static void Main (string[] args)
        {
            //This line creates a new instance, and wraps the instance in a using stratement so its automatically disposed once we've exited the block
            using (Game game = new Game(800, 600, "LearnOpenTK"))
            { 
                //Run function takes a double, which is how many fps it should strive to reach
                // you can leave that out and itll just update as fast as the hardware will allow it
                game.Run(60.0);
            }
        }
    }
}
