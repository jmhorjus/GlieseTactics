using System;

namespace Gliese581g
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MainApplication game = new MainApplication())
            {
                game.Run();
            }
        }
    }
#endif
}

