namespace Files
{
    using static System.Environment;

    /// <summary>
    ///     Defines specific folders which are typically present in a file system.
    /// </summary>
    public enum KnownFolder
    {

        /// <summary>
        ///     A folder in which temporary data can be stored.
        ///     Temporary data may be erased at any point in time.
        /// </summary>
        TemporaryData = ushort.MaxValue, // "Random" value which is most likely not going to end up in SpecialFolder.

        /// <summary>
        ///     A folder in which user-specific application data can be stored.
        ///     This folder is intended for roaming application data which might be synchronized
        ///     between multiple devices.
        /// </summary>
        RoamingApplicationData = SpecialFolder.ApplicationData,

        /// <summary>
        ///     A folder in which user-specific application data can be stored.
        ///     This folder is intended for application data which is only required on the local
        ///     device.
        /// </summary>
        LocalApplicationData = SpecialFolder.LocalApplicationData,

        /// <summary>
        ///     A folder in which global, i.e. non user-specific, application data can be stored.
        /// </summary>
        ProgramData = SpecialFolder.CommonApplicationData,

        /// <summary>
        ///     The current user's profile folder.
        /// </summary>
        UsersProfile = SpecialFolder.UserProfile,

        /// <summary>
        ///     The folder in which the user's desktop data is located.
        /// </summary>
        Desktop = SpecialFolder.Desktop,

        /// <summary>
        ///     The folder in which the user's documents are stored.
        /// </summary>
        DocumentsLibrary = SpecialFolder.MyDocuments,

        /// <summary>
        ///     The folder in which the user's pictures are stored.
        /// </summary>
        PicturesLibrary = SpecialFolder.MyPictures,

        /// <summary>
        ///     The folder in which the user's videos are stored.
        /// </summary>
        VideosLibrary = SpecialFolder.MyVideos,

        /// <summary>
        ///     The folder in which the user's music is stored.
        /// </summary>
        MusicLibrary = SpecialFolder.MyMusic,

    }

}
