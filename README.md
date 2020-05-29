# Files

Files is a **modern file system abstraction** for .NET. As such, Files has the following key features:

✨ **Immutability by Default**:<br/>
In comparison to .NET's `FileInfo`/`DirectoryInfo` and UWP's `IStorageFile`/`IStorageFolder`,
the core members of Files are immutable by default, meaning that **no unexpected mutations** can
happen when you, for example, move a file.
Interacting with a file system suddenly becomes predictable!

✨ **Async First**:<br/>
All I/O operations are, whenever possible, **executed asynchronously**.
This prevents unexpected blocking when, for example, dealing with unexpectedly large files
or network drives.

✨ **Consistent API Design**:<br/>
Files fixes many inconsistencies of .NET's APIs.
Have you ever wondered why `System.IO.File` members throw `UnauthorizedAccessException`s when a conflicting folder exists?
Why `System.IO.Directory` throws an `IOException` in the same scenario?
Why you can move directories to the same location, but get an exception when you try the same with files?
*No?* Well, nontheless, Files **fixes all of these inconsistencies** and **a lot more** (escpecially
in the UWP area) and provides a thought-through API surface, starting with class design and ending
with potential exceptions.

✨ **Thorougly Tested**:<br/>
Each `FileSystem` implementation is tested against a self-imposed specification.
The tests run on **3 different OSs** (Windows (Win32/UWP), Ubuntu, macOS) using up to
**5 different .NET SDKs** in order to catch and fix platform-specific problems (of which there are
many) and provide a **consistent developer experience**.

✨ **.NET Standard 2.0 and Polyfill Support**:<br/>
Files targets .NET Standard 2.0 and officially supports .NET Framework.
In addition, it backports several APIs which have been added to newer .NET SDKs like the
`System.IO.Path.Join(...)` method.



## The Files Core API

The following class diagram shows the five most important members of the Files Core API and should
give a great overview about the library's design.

![The Files Core API](./doc/assets/core-api-overview-class-diagram.png)
> The Files Core API as of version 0.1.0.

Explanation of the members:

**`FileSystem`**:
The central entrypoint into the API, designed with dependency injection in mind.
Mainly serves as a factory for creating the other members (`StoragePath`, `StorageFile` and `StorageFolder`).

**`StoragePath`**:
An abstraction over the file system's path behavior. Since each file system could handle paths differently,
Files provides a wrapper around the raw path strings which are typically used.

**`StorageFile`**:
Represents a file at a specific path and provides methods for interacting with it.

**`StorageFolder`**:
Represents a folder at a specific path and provides methods for interacting with it.


### Code Example



## Installation



## FAQ

Why `StorageFile` and not `File`?

## License

See the [LICENSE](./LICENSE) file for details.
