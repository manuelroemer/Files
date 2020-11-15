# Changelog

## v0.2.0

This release adds targets for .NET 5.0. Internally, C# 9.0 specific features have been added to the
code base.

### Files

* Added `net5.0` TFM.
* Fixed `cancellationToken`s not being passed everywhere.

### Files.FileSystems.Physical

* Added `net5.0` TFM.
* Fixed `cancellationToken`s not being passed everywhere.

### Files.FileSystems.WindowsStorage

* Fixed scenarios where a wrong exception type could be thrown (throw `IOException` by default when the
  Windows API returns a generic exception).

### Files.FileSystems.InMemory

* Added `net5.0` TFM.



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
