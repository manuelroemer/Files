# Changelog

## v0.2.0

TODO.

### Files

* **[Breaking]** Added a `FileShare` parameter to the abstract `StorageFile.OpenAsync(FileAccess, CancellationToken)` method, resulting
  in `StorageFile.OpenAsync(FileAccess, FileShare, CancellationToken)`.
* Added new `StorageFile.OpenAsync` overloads.
* Added two new overloads to the `StorageFileExtensions.OpenRead` and `StorageFileExtensions.OpenWrite` extensions
  which accept a `FileShare` parameter.

### Files.FileSystems.Physical

* Added support for the new `FileShare` parameter in `StorageFile.OpenAsync(FileAccess, FileShare, CancellationToken)`.

### Files.FileSystems.WindowsStorage

* Added support for the new `FileShare` parameter in `StorageFile.OpenAsync(FileAccess, FileShare, CancellationToken)`.

### Files.FileSystems.InMemory

* Added support for the new `FileShare` parameter in `StorageFile.OpenAsync(FileAccess, FileShare, CancellationToken)`.

### Files.Specification.Tests

* Added new tests covering the changes.



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
