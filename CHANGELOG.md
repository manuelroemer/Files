# Changelog

## v0.2.0

This release adds targets for .NET 5.0. Internally, C# 9.0 specific features have been added to the
code base.
The `StorageFile` has been extended with `CreateAndOpenAsync` overloads.

### Files

* Added `net5.0` TFM.
* Added `StorageFile.CreateAndOpenAsync` overloads which allow to (pseudo-)atomically create and
  open a file (atomicity depends on the corresponding `FileSystem` implementation).
* Fixed `cancellationToken`s not being passed everywhere.
* Added a package icon.

### Files.FileSystems.Physical

* Added `net5.0` TFM.
* Fixed `cancellationToken`s not being passed everywhere.
* Added a package icon.

### Files.FileSystems.WindowsStorage

* Fixed scenarios where a wrong exception type could be thrown (throw `IOException` by default when the
  Windows API returns a generic exception).
* Added a package icon.

### Files.FileSystems.InMemory

* Added `net5.0` TFM.
* Added a package icon.

### Files.Specification.Tests

* Added `net5.0` TFM.
* Added a package icon.



## v0.1.0

This is the initial release of the Files library, featuring the core abstractions, three
implementations and a public test suite which can be used to test your Files implementation
against the specification.

### Files

✨ Initial release.

### Files.FileSystems.Physical

✨ Initial release.

### Files.FileSystems.WindowsStorage

✨ Initial release.

### Files.FileSystems.InMemory

✨ Initial release.

### Files.Specification.Tests

✨ Initial release.
