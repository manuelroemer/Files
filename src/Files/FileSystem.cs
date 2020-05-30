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
    ///     An application should ideally only use one <see cref="FileSystem"/> instance at once,
    ///     for example via Dependency Injection. Each <see cref="FileSystem"/> implementation is
    ///     supposed to guard against accidential implementation mixing. This means that you will
    ///     get exceptions if you pass the members of one implementation (for example the fictional
    ///     class <c>StoragePathA</c>) to the members of another implemenation (for example the
    ///     fictional class <c>StorageFileB</c>).
    ///     The only exception to this rule are proxy <see cref="FileSystem"/> implementations which
    ///     ensure an appropriate conversion between the different types.
    ///     The code below demonstrates how such errors can occur.
    ///     
    ///     <code>
    ///     // DO NOT USE!
    ///     // This example demonstrates how using multiple file system implementations can lead to exceptions.
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
    ///     // At runtime, the PhysicalFileSystem will throw an ArgumentException in the line
    ///     //   var file = physicalFs.GetPath(path);
    ///     // because "path" is not a path created by the PhysicalFileSystem.
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
        ///     Gets a <see cref="Files.PathInformation"/> instance which provides information about
        ///     special path characteristics in this file system implementation.
        /// </summary>
        public PathInformation PathInformation { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileSystem"/> class.
        /// </summary>
        /// <param name="pathInformation">
        ///     A <see cref="PathInformation"/> instance which provides information about
        ///     special path characteristics in this file system implementation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="pathInformation"/> is <see langword="null"/>.
        /// </exception>
        public FileSystem(PathInformation pathInformation)
        {
            PathInformation = pathInformation ?? throw new ArgumentNullException(nameof(pathInformation));
        }

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
        public bool TryGetPath(string? path, [NotNullWhen(true)] out StoragePath? result)
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
        ///     hold the new <see cref="StoragePath"/> instance which locates the folder represented
        ///     by the <paramref name="knownFolder"/> parameter.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetPath(KnownFolder)"/>
        public bool TryGetPath(KnownFolder knownFolder, [NotNullWhen(true)] out StoragePath? result)
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
        public StorageFile GetFile(string path) =>
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
        ///     <paramref name="path"/> is a <see cref="StoragePath"/> instance which is incompatible
        ///     with this <see cref="FileSystem"/> implementation.
        ///     This exception generally occurs when you are using multiple <see cref="FileSystem"/>
        ///     implementations simultaneously.
        /// 
        ///     This exception is <b>always</b> thrown when the type of <paramref name="path"/>
        ///     doesn't match the specific <see cref="StoragePath"/> type created by the current
        ///     <see cref="FileSystem"/> implementation.
        ///     This condition <b>may</b>, however, be enhanced by any <see cref="FileSystem"/>
        ///     implementation.
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
        ///     hold the new <see cref="StorageFile"/> instance which represents a file at the
        ///     specified <paramref name="path"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetFile(string)"/>
        public bool TryGetFile(string? path, [NotNullWhen(true)] out StorageFile? result)
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
        ///     hold the new <see cref="StorageFile"/> instance which represents a file at the
        ///     specified <paramref name="path"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetFile(StoragePath)"/>
        public bool TryGetFile(StoragePath? path, [NotNullWhen(true)] out StorageFile? result)
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
        public StorageFolder GetFolder(string path) =>
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
        ///     <paramref name="path"/> is a <see cref="StoragePath"/> instance which is incompatible
        ///     with this <see cref="FileSystem"/> implementation.
        ///     This exception generally occurs when you are using multiple <see cref="FileSystem"/>
        ///     implementations simultaneously.
        /// 
        ///     This exception is <b>always</b> thrown when the type of <paramref name="path"/>
        ///     doesn't match the specific <see cref="StoragePath"/> type created by the current
        ///     <see cref="FileSystem"/> implementation.
        ///     This condition <b>may</b>, however, be enhanced by any <see cref="FileSystem"/>
        ///     implementation.
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
        public StorageFolder GetFolder(KnownFolder knownFolder) =>
            GetFolder(GetPath(knownFolder));

        /// <summary>
        ///     Attempts to create and return a <see cref="StorageFolder"/> instance which represents
        ///     the folder at the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path which locates a folder.</param>
        /// <param name="result">
        ///     An <see langword="out"/> parameter which will, if the operation succeedes,
        ///     hold the new <see cref="StorageFolder"/> instance which represents a folder at the
        ///     specified <paramref name="path"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetFolder(string)"/>
        public bool TryGetFolder(string? path, [NotNullWhen(true)] out StorageFolder? result)
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
        ///     hold the new <see cref="StorageFolder"/> instance which represents a folder at the
        ///     specified <paramref name="path"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetFolder(StoragePath)"/>
        public bool TryGetFolder(StoragePath? path, [NotNullWhen(true)] out StorageFolder? result)
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
        ///     hold the new <see cref="StorageFolder"/> instance which represents the folder
        ///     represented by the <paramref name="knownFolder"/> parameter.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the operation succeeded; <see langword="false"/> if not.
        /// </returns>
        /// <seealso cref="GetFolder(KnownFolder)"/>
        public bool TryGetFolder(KnownFolder knownFolder, [NotNullWhen(true)] out StorageFolder? result)
        {
            if (TryGetPath(knownFolder, out var path) && TryGetFolder(path, out result))
            {
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
