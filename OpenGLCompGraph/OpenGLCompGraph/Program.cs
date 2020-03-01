namespace OpenGL_TK
{
    public static class Program
    {
        private static void Main()
        {
            using (var window = new Window(800, 600, "Assignment1: OpenGL Using OpenTK"))
            {
                window.Run(60.0);
            }
        }
    }
}
