using System.Runtime.CompilerServices;

namespace Framework.Infrastructure
{
    /// <summary>
    /// Provides access to the singleton instance of the Framework engine.
    /// </summary>
    public static class EngineContext
    {
        private static IEngine defaultEngine = null;

        #region Properties

        /// <summary>
        /// Gets the singleton Framework engine used to access Framework services.
        /// </summary>
        public static IEngine Current
        {
            get
            {
                if (Singleton<IEngine>.Instance == null)
                {
                    Create(defaultEngine);
                }

                return Singleton<IEngine>.Instance;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Create a static instance of the Framework engine.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IEngine Create(IEngine engine)
        {
            defaultEngine = engine;

            if (Singleton<IEngine>.Instance == null)
            {
                Singleton<IEngine>.Instance = engine;
            }

            return Singleton<IEngine>.Instance;
        }

        /// <summary>
        /// Sets the static engine instance to the supplied engine. Use this method to supply your own engine implementation.
        /// </summary>
        /// <param name="engine">The engine to use.</param>
        /// <remarks>Only use this method if you know what you're doing.</remarks>
        public static void Replace(IEngine engine)
        {
            Singleton<IEngine>.Instance = engine;
        }

        #endregion Methods
    }
}