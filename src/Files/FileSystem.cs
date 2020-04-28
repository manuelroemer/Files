namespace Files
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     The central entry point for interacting with a file system and its elements.
    ///     See remarks for details.
    /// </summary>
    /// <remarks>
    ///     The <see cref="FileSystem"/> class is the central factory for the various file system
    ///     members exposed by this library, e.g. <see cref="StoragePath"/>, <see cref="StorageFile"/>
    ///     or <see cref="StorageFolder"/>.
    /// 
    ///     <see cref="FileSystem"/> is an abstract class which must be implemented to support a
    ///     specific file system. By only using this abstraction in your code, you will be able
    ///     to seamlessly switch between different file system implementations, for example an implementation
    ///     targeting a user's actual physical file system and an in-memory implementation for testing.
    ///     
    ///     In most cases, an application should only be using one <see cref="FileSystem"/> implementation
    ///     at a single point in time. While using multiple implementations at once is possible
    ///     from an API standpoint (see the code example below), it will, in most cases, lead to
    ///     errors at some point in time.
    ///     An exception to this rule are proxy file system implementations which leverage other
    ///     file system implementations.
    ///     
    ///     <code>
    ///     // DO NOT USE!
    ///     // This example demonstrates how multiple file system implementations *can* be used
    ///     // together and why this shouldn't be done.
    ///     
    ///     FileSystem inMemoryFs = new InMemoryFileSystem();
    ///     FileSystem physicalFs = new PhysicalFileSystem();
    ///     
    ///     var filePath = inMemoryFs.GetPath(KnownFolder.TemporaryData);
    ///     await CreateHelloWorldFile(filePath);
    /// 
    ///     Task CreateHelloWorldFile(StoragePath path)
    ///     {
    ///         var file = physicalFs.GetPath(path);
    ///         await file.WriteTextAsync("Hello World!");
    ///     }
    ///     
    ///     // The code above will compile without problems.
    ///     // At runtime, it will most likely fail though, since the path to the file system's
    ///     // temporary data folder of the real world's physical file system will most likely
    ///     // not match the path of the mocked, in-memory file system.
    ///     </code>
    /// </remarks>
    /// <example>
    ///     The following code demonstrates how the <see cref="FileSystem"/> can be used to interact
    ///     with files and folders.
    ///     
    ///     <code>
    ///     // Please note that the code below can be drastically simplified by calling different
    ///     // methods and method overloads - the main goal is demonstrating how the various file
    ///     // system members play together.
    ///     public void WriteTextToFile(FileSystem fs)
    ///     {
    ///         StoragePath tempFolderPath = fs.GetFolder(KnownFolder.TemporaryData);
    ///         StoragePath parentFolderPath = tempFolderPath / "Parent Folder";
    ///         StoragePath filePath = parentFolderPath / "File.txt";
    ///         
    ///         StorageFolder parentFolder = fs.GetFolder(parentFolderPath);
    ///         StorageFile file = fs.GetFile(filePath);
    ///         
    ///         await parentFolder.CreateAsync();
    ///         await file.CreateAsync();
    ///         await file.WriteTextAsync("Hello World!");
    ///     }
    ///     </code>
    /// </example>
    public abstract class FileSystem
    {
        /// <summary>
        ///     Gets a <see cref="PathInformation"/> instance which provides information about
        ///     special path characteristics in this file system implementation.
        /// </summary>
        public abstract PathInformation PathInformation { get; }

        /// <summary>
        ///     Creates and returns a <see cref="StoragePath"/> instance from a string.
        /// </summary>
        /// <param name="path">
        ///     The string from which a new <see cref="StoragePath"/> instance should be created.
        /// </param>
        /// <returns>
        ///     A new <see cref="StoragePath"/> instance created from the specified
        ///     <paramref name="path"/> string.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path"/> is an empty string or has an otherwise invalid format.
        /// </exception>
        /// <seealso cref="TryGetPath(string?, out StoragePath?)"/>
        public abstract StoragePath GetPath(string path);

        /// <summary>
        ///     Creates and returns a <see cref="StoragePath"/> instance which locates a specific
        ///     folder which is typically present in a file system.
        /// </summary>
        /// <param name="knownFolder">
        ///     A folder which is typically present in a file system.
        /// </param>
        /// <returns>
        ///     A new <see cref="StoragePath"/> instance which locates the folder represented by
        ///     the <paramref name="knownFolder"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     <paramref name="knownFolder"/> is an invalid <see cref="KnownFolder"/> enumeration value.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     The requested folder is not supported by this file system implementation.
        /// </exception>
        /// <seealso cref="TryGetPath(KnownFolder, out StoragePath?)"/>
        public abstract StoragePath GetPath(KnownFolder knownFolder);

        /// <summary>
        ///     Attempts to create and return a <see cref="StoragePath"/> instance from a string.
        /// </summary>
        /// <param name="path">
        ///     The string from which a new <see cref="StoragePath"/> instance should be created.
        /// </param>
        /// <param name="result">
        ///     An <see langword="out"/> parameter which will, if the operation succeedes,
        ///     hold the new <see cref="StoragePath"/> instance created from the specified
        ///     <paramref name="path"/> string.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetPath(string)"/>
        public virtual bool TryGetPath(string? path, [NotNullWhen(true)] out StoragePath? result)
        {
            // Fast path without a (guaranteed) exception.
            if (string.IsNullOrEmpty(path))
            {
                result = null;
                return false;
            }
            
            try
            {
                result = GetPath(path!);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        ///     Attempts to create and return a <see cref="StoragePath"/> instance which locates a
        ///     specific folder which is typically present in a file system.
        /// </summary>
        /// <param name="knownFolder">
        ///     A folder which is typically present in a file system.
        /// </param>
        /// <param name="result">
        ///     An <see langword="out"/> parameter which will, if the operation succeedes,
        ///     holds the new <see cref="StoragePath"/> instance which locates the folder represented
        ///     by the <paramref name="knownFolder"/> parameter.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetPath(KnownFolder)"/>
        public virtual bool TryGetPath(KnownFolder knownFolder, [NotNullWhen(true)] out StoragePath? result)
        {
            try
            {
                result = GetPath(knownFolder);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        ///     Creates and returns a <see cref="StorageFile"/> instance which represents a file
        ///     located by the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        ///     The string representing the path which locates a file.
        /// </param>
        /// <returns>
        ///     A new <see cref="StorageFile"/> instance which represents a file at the specified
        ///     <paramref name="path"/>.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling <see cref="GetFile(StoragePath)"/> with a
        ///     <see cref="StoragePath"/> instance obtained through the <see cref="GetPath(string)"/> method.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path"/> is an empty string or has an otherwise invalid format.
        /// </exception>
        /// <seealso cref="TryGetFile(string?, out StorageFile?)"/>
        public virtual StorageFile GetFile(string path) =>
            GetFile(GetPath(path));

        /// <summary>
        ///     Returns a <see cref="StorageFile"/> instance which represents the file at the specified
        ///     <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path which locates a file.</param>
        /// <returns>
        ///     A new <see cref="StorageFile"/> instance which represents a file at the specified
        ///     <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path"/> is a <see cref="StoragePath"/> instance representing a path
        ///     which is considered invalid by this file system implementation.
        ///     This can occur if you are using multiple <see cref="FileSystem"/> implementations
        ///     simultaneously.
        /// </exception>
        /// <seealso cref="TryGetFile(StoragePath?, out StorageFile?)"/>
        public abstract StorageFile GetFile(StoragePath path);

        /// <summary>
        ///     Attempts to create and return a <see cref="StorageFile"/> instance which represents
        ///     the file at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path which locates a file.</param>
        /// <param name="result">
        ///     An <see langword="out"/> parameter which will, if the operation succeedes,
        ///     holds the new <see cref="StorageFile"/> instance which represents a file at the
        ///     specified <paramref name="path"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetFile(string)"/>
        public virtual bool TryGetFile(string? path, [NotNullWhen(true)] out StorageFile? result)
        {
            // Fast path without a (guaranteed) exception.
            if (string.IsNullOrEmpty(path))
            {
                result = null;
                return false;
            }

            try
            {
                result = GetFile(path!);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        ///     Attempts to create and return a <see cref="StorageFile"/> instance which represents
        ///     the file at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path which locates a file.</param>
        /// <param name="result">
        ///     An <see langword="out"/> parameter which will, if the operation succeedes,
        ///     holds the new <see cref="StorageFile"/> instance which represents a file at the
        ///     specified <paramref name="path"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetFile(StoragePath)"/>
        public virtual bool TryGetFile(StoragePath? path, [NotNullWhen(true)] out StorageFile? result)
        {
            // Fast path without a (guaranteed) exception.
            if (path is null)
            {
                result = null;
                return false;
            }

            try
            {
                result = GetFile(path);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        ///     Creates and returns a <see cref="StorageFolder"/> instance which represents a folder
        ///     located by the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        ///     The string representing the path which locates a folder.
        /// </param>
        /// <returns>
        ///     A new <see cref="StorageFolder"/> instance which represents a folder at the specified
        ///     <paramref name="path"/>.
        /// </returns>
        /// <remarks>
        ///     Calling this method is equivalent to calling <see cref="GetFolder(StoragePath)"/> with a
        ///     <see cref="StoragePath"/> instance obtained through the <see cref="GetPath(string)"/> method.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path"/> is an empty string or has an otherwise invalid format.
        /// </exception>
        /// <seealso cref="TryGetFolder(string?, out StorageFolder?)"/>
        public virtual StorageFolder GetFolder(string path) =>
            GetFolder(GetPath(path));

        /// <summary>
        ///     Creates and returns a <see cref="StorageFolder"/> instance which represents a folder
        ///     located by the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        ///     The string representing the path which locates a folder.
        /// </param>
        /// <returns>
        ///     A new <see cref="StorageFolder"/> instance which represents a folder at the specified
        ///     <paramref name="path"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="path"/> is a <see cref="StoragePath"/> instance representing a path
        ///     which is considered invalid by this file system implementation.
        ///     This can occur if you are using multiple <see cref="FileSystem"/> implementations
        ///     simultaneously.
        /// </exception>
        /// <seealso cref="TryGetFolder(StoragePath?, out StorageFolder?)"/>
        public abstract StorageFolder GetFolder(StoragePath path);

        /// <summary>
        ///     Creates and returns a <see cref="StorageFolder"/> instance which represents a specific
        ///     folder which is typically present in a file system.
        /// </summary>
        /// <param name="knownFolder">
        ///     A folder which is typically present in a file system.
        /// </param>
        /// <returns>
        ///     A new <see cref="StorageFolder"/> instance which represents the folder represented by
        ///     the <paramref name="knownFolder"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     <paramref name="knownFolder"/> is an invalid <see cref="KnownFolder"/> enumeration value.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     The requested folder is not supported by this file system implementation.
        /// </exception>
        /// <seealso cref="TryGetFolder(KnownFolder, out StorageFolder?)"/>
        public virtual StorageFolder GetFolder(KnownFolder knownFolder) =>
            GetFolder(GetPath(knownFolder));

        /// <summary>
        ///     Attempts to create and return a <see cref="StorageFolder"/> instance which represents
        ///     the folder at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path which locates a folder.</param>
        /// <param name="result">
        ///     An <see langword="out"/> parameter which will, if the operation succeedes,
        ///     holds the new <see cref="StorageFolder"/> instance which represents a folder at the
        ///     specified <paramref name="path"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetFolder(string)"/>
        public virtual bool TryGetFolder(string? path, [NotNullWhen(true)] out StorageFolder? result)
        {
            // Fast path without a (guaranteed) exception.
            if (string.IsNullOrEmpty(path))
            {
                result = null;
                return false;
            }

            try
            {
                result = GetFolder(path!);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        ///     Attempts to create and return a <see cref="StorageFolder"/> instance which represents
        ///     the folder at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path which locates a folder.</param>
        /// <param name="result">
        ///     An <see langword="out"/> parameter which will, if the operation succeedes,
        ///     holds the new <see cref="StorageFolder"/> instance which represents a folder at the
        ///     specified <paramref name="path"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetFolder(StoragePath)"/>
        public virtual bool TryGetFolder(StoragePath? path, [NotNullWhen(true)] out StorageFolder? result)
        {
            // Fast path without a (guaranteed) exception.
            if (path is null)
            {
                result = null;
                return false;
            }

            try
            {
                result = GetFolder(path!);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        ///     Attempts to create and return a <see cref="StorageFolder"/> instance which represents a
        ///     specific folder which is typically present in a file system.
        /// </summary>
        /// <param name="knownFolder">
        ///     A folder which is typically present in a file system.
        /// </param>
        /// <param name="result">
        ///     An <see langword="out"/> parameter which will, if the operation succeedes,
        ///     holds the new <see cref="StorageFolder"/> instance which represents the folder
        ///     represented by the <paramref name="knownFolder"/> parameter.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetFolder(KnownFolder)"/>
        public virtual bool TryGetFolder(KnownFolder knownFolder, [NotNullWhen(true)] out StorageFolder? result)
        {
            if (TryGetPath(knownFolder, out var path))
            {
                result = GetFolder(path);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
}
