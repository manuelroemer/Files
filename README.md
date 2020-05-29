# Files

Files is a set of .NET libraries which provide a **modern abstraction** around **file systems**.
While the project was initially started to create an abstraction around .NET's `System.IO` and UWP's
`Windows.Storage` APIs, Files is now a **API consistency** and **ease of development**.

Files has the following key features:

* **Immutability**:
  In comparison to .NET's `FileInfo`/`DirectoryInfo`, the core members of Files are immutable by default.
  **No unexpected mutations** can happen when you, for example, move a file!
* **Async First**:
  All I/O are, whenever possible, **executed asynchronously**.
  This prevents unexpected blocking when, for example, dealing with unexpectedly large files
  or network drives.
* **Consistent**:
  Files fixes many inconsistencies of .NET's APIs.<br/>
  Ever wondered why `System.IO.File` throw `UnauthorizedAccessException`s when a conflicting folder
  exists?<br/>
  Ever wondered why `System.IO.Directory` throws an `IOException` in the same scenario?<br/>
  Ever wondered why you can move directories to the same location, but get an exception when you
  try the same with files?<br/>
  Files **fixes all of these inconsistencies** and **a lot more** (escpecially in the UWP area)
  and provides a thought-through API surface, starting with method headers and ending with possible exceptions.
* **Thorougly Tested**:
  Each `FileSystem` implementation is tested against a self-imposed specification.
  All tests are run on **3 different OSs** (Windows (Win32/UWP), Ubuntu, macOS) using up to
  **5 different .NET SDKs** in order to catch platform-specific problems and provide a
  **consistent developer experience**.


![The Files Core API](./doc/assets/core-api-overview-class-diagram.png)
> The Files Core API as of version 0.1.0.

## FAQ

Why `StorageFile` and not `File`?

## License

See the [LICENSE](./LICENSE) file for details.
