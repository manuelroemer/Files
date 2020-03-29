namespace Files.Specification.Tests
{
    using System;
    using System.Collections.Generic;
    using Files.Specification.Tests.Attributes;
    using Files.Specification.Tests.Setup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;

    [TestClass]
    public abstract class StoragePathSpecificationTests : FileSystemTestBase
    {
        // For the default test data provided in this class, we need some paths to start from.
        // Since we're not doing any real file I/O, we can use any path without restrictions.
        private StoragePath CurrentDirectoryPath => FileSystem.GetPath(PathInformation.CurrentDirectorySegment);
        private StoragePath ParentDirectoryPath => FileSystem.GetPath(PathInformation.ParentDirectorySegment);
        private StoragePath AbsolutePath => CurrentDirectoryPath.FullPath;

        public StoragePathSpecificationTests(FileSystemTestContext context)
            : base(context) { }

        #region Kind Tests

        public virtual IEnumerable<object[]> KindRelativePathData => new[]
        {
            new[] { Default.PathNameWithoutExtension },
            new[] { Default.PathName },
            new[] { CurrentDirectoryPath.ToString() },
            new[] { ParentDirectoryPath.ToString() },
        };

        public virtual IEnumerable<object[]> KindAbsolutePathData => new[]
        {
            new[] { AbsolutePath.ToString() },
        };

        [TestMethod]
        [DynamicInstanceData(nameof(KindRelativePathData))]
        public void Kind_RelativePath_ReturnsRelative(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            path.Kind.ShouldBe(PathKind.Relative);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(KindAbsolutePathData))]
        public void Kind_AbsolutePath_ReturnsAbsolute(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            path.Kind.ShouldBe(PathKind.Absolute);
        }

        #endregion

        #region Root Tests

        public virtual IEnumerable<object[]> RootRootedPathsData => new[]
        {
            new[]
            {
                // It is somewhat pointless to test the RootProperty with an expected value that comes from
                // the Root property.
                // If anything, it's better than nothing though and at least ensures consistency. Somewhat.
                // Deriving classes should totally implement more cases.
                AbsolutePath.ToString(),
                AbsolutePath.Root!.ToString(),
            },
        };

        public virtual IEnumerable<object[]> RootNotRootedPathsData => new[]
        {
            new[] { Default.PathNameWithoutExtension },
            new[] { Default.PathName },
            new[] { CurrentDirectoryPath.ToString() },
            new[] { ParentDirectoryPath.ToString() },
        };

        [TestMethod]
        [DynamicInstanceData(nameof(RootRootedPathsData))]
        public void Root_RootedPath_ReturnsRoot(string pathString, string root)
        {
            var path = FileSystem.GetPath(pathString);
            path.Root.ShouldNotBeNull();
            path.Root!.ToString().ShouldBe(root);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(RootNotRootedPathsData))]
        public void Root_NotRootedPath_ReturnsNull(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            path.Root.ShouldBeNull();
        }

        #endregion

        #region Parent Tests

        public virtual IEnumerable<object[]> ParentPathsWithParentData => new[]
        {
            new[]
            {
                (AbsolutePath / Default.PathName).ToString(),
                AbsolutePath.ToString(),
            },
            new[]
            {
                (CurrentDirectoryPath / Default.PathName).ToString(),
                PathInformation.CurrentDirectorySegment,
            },
            new[]
            {
                (ParentDirectoryPath / Default.PathName).ToString(),
                PathInformation.ParentDirectorySegment,
            },
        };

        public virtual IEnumerable<object[]> ParentPathsWithoutParentData => new[]
        {
            new[] { AbsolutePath.Root!.ToString() },
            new[] { PathInformation.DirectorySeparatorChar.ToString() },
            new[] { PathInformation.AltDirectorySeparatorChar.ToString() },
        };

        [TestMethod]
        [DynamicInstanceData(nameof(ParentPathsWithParentData))]
        public void Parent_PathWithParent_ReturnsParent(string pathString, string parent)
        {
            var path = FileSystem.GetPath(pathString);
            path.Parent.ShouldNotBeNull();
            path.Parent!.ToString().ShouldBe(parent);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(ParentPathsWithoutParentData))]
        public void Parent_PathWithoutParent_ReturnsNull(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            path.Parent.ShouldBeNull();
        }

        #endregion

        #region FullPath Tests

        public virtual IEnumerable<object[]> FullPathData => new[]
        {
            new[]
            {
                AbsolutePath.ToString(),
                AbsolutePath.FullPath.ToString(),
            },
            new[]
            {
                CurrentDirectoryPath.ToString(),
                CurrentDirectoryPath.FullPath.ToString(),
            },
            new[]
            {
                ParentDirectoryPath.ToString(),
                ParentDirectoryPath.FullPath.ToString(),
            },
        };

        [TestMethod]
        [DynamicInstanceData(nameof(FullPathData))]
        public void FullPath_StandardPath_ReturnsFullPath(string pathString, string fullPath)
        {
            var path = FileSystem.GetPath(pathString);
            path.FullPath.ToString().ShouldBe(fullPath);
        }

        #endregion

        #region Name Tests

        public virtual IEnumerable<object[]> NameData => new[]
        {
            new[]
            {
                Default.PathName,
                Default.PathName,
            },
            new[]
            {
                Default.PathNameWithoutExtension,
                Default.PathNameWithoutExtension,
            },
            new[]
            {
                Default.PathNameWithoutExtension + PathInformation.ExtensionSeparatorChar,
                Default.PathNameWithoutExtension + PathInformation.ExtensionSeparatorChar,
            },
            new[]
            {
                PathInformation.CurrentDirectorySegment,
                PathInformation.CurrentDirectorySegment,
            },
            new[]
            {
                PathInformation.ParentDirectorySegment,
                PathInformation.ParentDirectorySegment,
            },
            new[]
            {
                PathInformation.DirectorySeparatorChar.ToString(),
                "",
            },
            new[]
            {
                PathInformation.AltDirectorySeparatorChar.ToString(),
                "",
            },
            new[]
            {
                // A single trailing separator is ignored.
                Default.PathName + PathInformation.DirectorySeparatorChar,
                Default.PathName,
            },
            new[]
            {
                // A single trailing separator is ignored.
                Default.PathName + PathInformation.AltDirectorySeparatorChar,
                Default.PathName,
            },
            new[]
            {
                // Multiple trailing separators lead to empty name.
                Default.PathName + PathInformation.DirectorySeparatorChar + PathInformation.DirectorySeparatorChar,
                "",
            },
            new[]
            {
                // Multiple trailing separators lead to empty name.
                Default.PathName + PathInformation.AltDirectorySeparatorChar + PathInformation.AltDirectorySeparatorChar,
                "",
            },
        };

        [TestMethod]
        [DynamicInstanceData(nameof(NameData))]
        public void Name_StandardPath_ReturnsName(string pathString, string name)
        {
            var path = FileSystem.GetPath(pathString);
            path.Name.ShouldBe(name);
        }

        #endregion

        #region NameWithoutExtension Tests

        public virtual IEnumerable<object[]> NameWithoutExtensionData => new[]
        {
            new[]
            {
                Default.PathName,
                Default.PathNameWithoutExtension,
            },
            new[]
            {
                Default.PathNameWithoutExtension,
                Default.PathNameWithoutExtension,
            },
            new[]
            {
                Default.PathNameWithoutExtension + PathInformation.ExtensionSeparatorChar,
                Default.PathNameWithoutExtension,
            },
            new[]
            {
                PathInformation.CurrentDirectorySegment,
                PathInformation.CurrentDirectorySegment,
            },
            new[]
            {
                PathInformation.ParentDirectorySegment,
                PathInformation.ParentDirectorySegment,
            },
            new[]
            {
                PathInformation.DirectorySeparatorChar.ToString(),
                "",
            },
            new[]
            {
                PathInformation.AltDirectorySeparatorChar.ToString(),
                "",
            },
            new[]
            {
                // A single trailing separator is ignored.
                AbsolutePath / Default.PathName + PathInformation.DirectorySeparatorChar,
                Default.PathNameWithoutExtension,
            },
            new[]
            {
                // A single trailing separator is ignored.
                Default.PathName + PathInformation.AltDirectorySeparatorChar,
                Default.PathNameWithoutExtension,
            },
            new[]
            {
                // Multiple trailing separators lead to empty name.
                Default.PathName + PathInformation.DirectorySeparatorChar + PathInformation.DirectorySeparatorChar,
                "",
            },
            new[]
            {
                // Multiple trailing separators lead to empty name.
                Default.PathName + PathInformation.AltDirectorySeparatorChar + PathInformation.AltDirectorySeparatorChar,
                "",
            },
        };

        [TestMethod]
        [DynamicInstanceData(nameof(NameWithoutExtensionData))]
        public void NameWithoutExtension_StandardPath_ReturnsNameWithoutExtension(string pathString, string nameWithoutExtension)
        {
            var path = FileSystem.GetPath(pathString);
            path.NameWithoutExtension.ShouldBe(nameWithoutExtension);
        }

        #endregion

        #region Extension Tests

        public virtual IEnumerable<object[]> ExtensionPathsWithExtensionData => new[]
        {
            new[]
            {
                Default.PathName,
                Default.PathNameExtension,
            },
            new[]
            {
                // A single trailing separator is ignored.
                Default.PathName + PathInformation.DirectorySeparatorChar,
                Default.PathNameExtension,
            },
            new[]
            {
                // A single trailing separator is ignored.
                Default.PathName + PathInformation.AltDirectorySeparatorChar,
                Default.PathNameExtension,
            },
        };

        public virtual IEnumerable<object[]> ExtensionPathsWithoutExtensionData => new[]
        {
            new[] { Default.PathNameWithoutExtension },
            new[] { Default.PathNameWithoutExtension + PathInformation.ExtensionSeparatorChar },
            new[] { PathInformation.CurrentDirectorySegment },
            new[] { PathInformation.ParentDirectorySegment },
            new[] { PathInformation.DirectorySeparatorChar.ToString() },
            new[] { PathInformation.AltDirectorySeparatorChar.ToString() },
            new[] { Default.PathName + PathInformation.DirectorySeparatorChar + PathInformation.DirectorySeparatorChar },
            new[] { Default.PathName + PathInformation.AltDirectorySeparatorChar + PathInformation.AltDirectorySeparatorChar },
        };

        [TestMethod]
        [DynamicInstanceData(nameof(ExtensionPathsWithExtensionData))]
        public void Extension_StandardPath_ReturnsExtension(string pathString, string extension)
        {
            var path = FileSystem.GetPath(pathString);
            path.Extension.ShouldNotBeNull();
            path.Extension.ShouldBe(extension);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(ExtensionPathsWithoutExtensionData))]
        public void Extension_PathWithoutExtension_ReturnsNull(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            path.Extension.ShouldBeNull();
        }

        #endregion

        #region EndsInDirectorySeparator Tests

        public virtual IEnumerable<object[]> EndsInDirectorySeparatorPathsWithTrailingDirectorySeparatorData => new[]
        {
            new[] { PathInformation.DirectorySeparatorChar.ToString() },
            new[] { PathInformation.AltDirectorySeparatorChar.ToString() },
            new[] { Default.PathName + PathInformation.DirectorySeparatorChar },
            new[] { Default.PathName + PathInformation.AltDirectorySeparatorChar },
        };

        public virtual IEnumerable<object[]> EndsInDirectorySeparatorPathsWithoutTrailingDirectorySeparatorData => new[]
        {
            new[] { Default.PathName },
            new[] { Default.PathName + PathInformation.DirectorySeparatorChar + " " },
            new[] { Default.PathName + PathInformation.AltDirectorySeparatorChar + " " },
        };

        [TestMethod]
        [DynamicInstanceData(nameof(EndsInDirectorySeparatorPathsWithTrailingDirectorySeparatorData))]
        public void EndsInDirectorySeparator_PathWithTrailingDirectorySeparator_ReturnsTrue(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            path.EndsInDirectorySeparator.ShouldBeTrue();
        }

        [TestMethod]
        [DynamicInstanceData(nameof(EndsInDirectorySeparatorPathsWithoutTrailingDirectorySeparatorData))]
        public void EndsInDirectorySeparator_PathWithoutTrailingDirectorySeparator_ReturnsFalse(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            path.EndsInDirectorySeparator.ShouldBeFalse();
        }

        #endregion

        #region TrimEndingDirectorySeparator Tests

        public virtual IEnumerable<object[]> TrimEndingDirectorySeparatorPathsWithTrailingDirectorySeparatorData => new[]
        {
            new[] { Default.PathName + PathInformation.DirectorySeparatorChar },
            new[] { Default.PathName + PathInformation.AltDirectorySeparatorChar },
        };

        public virtual IEnumerable<object[]> TrimEndingDirectorySeparatorPathsWithoutTrailingDirectorySeparatorData => new[]
        {
            new[] { Default.PathName },
            new[] { Default.PathName + PathInformation.DirectorySeparatorChar + " " },
            new[] { Default.PathName + PathInformation.AltDirectorySeparatorChar + " " },
        };

        public virtual IEnumerable<object[]> TrimEndingDirectorySeparatorUntrimmablePathsData => new[]
        {
            new[] { PathInformation.DirectorySeparatorChar.ToString() },
            new[] { PathInformation.AltDirectorySeparatorChar.ToString() },
        };

        [TestMethod]
        [DynamicInstanceData(nameof(TrimEndingDirectorySeparatorPathsWithTrailingDirectorySeparatorData))]
        public void TrimEndingDirectorySeparator_PathWithTrailingDirectorySeparator_TrimsSeparator(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            var trimmed = path.TrimEndingDirectorySeparator();
            trimmed.Length.ShouldBe(path.Length - 1);
            // Not asserting that the path doesn't end with a separator.
            // That's legal, because the method should only trim one character.
        }

        [TestMethod]
        [DynamicInstanceData(nameof(TrimEndingDirectorySeparatorPathsWithoutTrailingDirectorySeparatorData))]
        public void TrimEndingDirectorySeparator_PathWithoutTrailingDirectorySeparator_ReturnsSamePath(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            var trimmed = path.TrimEndingDirectorySeparator();
            trimmed.ToString().ShouldBe(path.ToString());
        }

        [TestMethod]
        [DynamicInstanceData(nameof(TrimEndingDirectorySeparatorUntrimmablePathsData))]
        public void TrimEndingDirectorySeparator_UntrimmablePath_ThrowsInvalidOperationException(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            Should.Throw<InvalidOperationException>(() => path.TrimEndingDirectorySeparator());
        }

        #endregion

        #region TryTrimEndingDirectorySeparator Tests

        [TestMethod]
        [DynamicInstanceData(nameof(TrimEndingDirectorySeparatorPathsWithTrailingDirectorySeparatorData))]
        public void TryTrimEndingDirectorySeparator_PathWithTrailingDirectorySeparator_TrimsSeparator(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            var result = path.TryTrimEndingDirectorySeparator(out var trimmed);
            result.ShouldBeTrue();
            trimmed.ShouldNotBeNull();
            trimmed!.Length.ShouldBe(path.Length - 1);
            // Not asserting that the path doesn't end with a separator.
            // That's legal, because the method should only trim one character.
        }

        [TestMethod]
        [DynamicInstanceData(nameof(TrimEndingDirectorySeparatorPathsWithoutTrailingDirectorySeparatorData))]
        public void TryTrimEndingDirectorySeparator_PathWithoutTrailingDirectorySeparator_ReturnsTrueAndSamePath(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            var result = path.TryTrimEndingDirectorySeparator(out var trimmed);
            result.ShouldBeTrue();
            trimmed.ShouldNotBeNull();
            trimmed!.ToString().ShouldBe(path.ToString());
        }

        [TestMethod]
        [DynamicInstanceData(nameof(TrimEndingDirectorySeparatorUntrimmablePathsData))]
        public void TryTrimEndingDirectorySeparator_UntrimmablePath_ReturnsFalseAndNull(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            var result = path.TryTrimEndingDirectorySeparator(out var trimmed);
            result.ShouldBeFalse();
            trimmed.ShouldBeNull();
        }

        #endregion

        #region Append Tests

        public virtual IEnumerable<object[]> AppendPathsWithValidPartsData => new[]
        {
            new[]
            {
                Default.PathName,
                "",
                Default.PathName,
            },
            new[]
            {
                Default.PathName,
                Default.PathName,
                Default.PathName + Default.PathName,
            },
            new[]
            {
                Default.PathName,
                PathInformation.DirectorySeparatorChar.ToString(),
                Default.PathName + PathInformation.DirectorySeparatorChar,
            },
            new[]
            {
                Default.PathName,
                PathInformation.AltDirectorySeparatorChar.ToString(),
                Default.PathName + PathInformation.AltDirectorySeparatorChar,
            },
            new[]
            {
                Default.PathName,
                PathInformation.DirectorySeparatorChar.ToString() + PathInformation.DirectorySeparatorChar,
                Default.PathName + PathInformation.DirectorySeparatorChar + PathInformation.DirectorySeparatorChar,
            },
            new[]
            {
                Default.PathName,
                PathInformation.AltDirectorySeparatorChar.ToString() + PathInformation.AltDirectorySeparatorChar,
                Default.PathName + PathInformation.AltDirectorySeparatorChar + PathInformation.AltDirectorySeparatorChar,
            },
        };

        public abstract IEnumerable<object[]> AppendPathsWithInvalidPartsData { get; }

        [TestMethod]
        public void Append_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => TestFolder.Path.Append(null!));
        }

        [TestMethod]
        [DynamicInstanceData(nameof(AppendPathsWithValidPartsData))]
        public void Append_StandardPathAndValidPart_AppendsStringToPath(string initialPathString, string part, string expectedPathString)
        {
            var path = FileSystem.GetPath(initialPathString);
            var finalPath = path.Append(part);
            finalPath.ToString().ShouldBe(expectedPathString);
        }

        [DataTestMethod]
        [DynamicInstanceData(nameof(AppendPathsWithInvalidPartsData))]
        public void Append_StandardPathAndInvalidPart_ThrowsArgumentException(string initialPathString, string part)
        {
            var path = FileSystem.GetPath(initialPathString);
            Should.Throw<ArgumentException>(() => path.Append(part));
        }

        #endregion

        #region TryAppend Tests

        [TestMethod]
        public void TryAppend_NullParameters_ReturnsFalseAndNull()
        {
            var result = TestFolder.Path.TryAppend(null, out var finalPath);
            result.ShouldBeFalse();
            finalPath.ShouldBeNull();
        }

        [TestMethod]
        [DynamicInstanceData(nameof(AppendPathsWithValidPartsData))]
        public void TryAppend_StandardPathAndValidPart_AppendsStringToPath(string initialPathString, string part, string expectedPathString)
        {
            var path = FileSystem.GetPath(initialPathString);
            var result = path.TryAppend(part, out var finalPath);
            result.ShouldBeTrue();
            finalPath.ShouldNotBeNull();
            finalPath!.ToString().ShouldBe(expectedPathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(AppendPathsWithInvalidPartsData))]
        public void TryAppend_StandardPathAndInvalidPart_ReturnsFalseAndNull(string initialPathString, string part)
        {
            var path = FileSystem.GetPath(initialPathString);
            var result = path.TryAppend(part, out var finalPath);
            result.ShouldBeFalse();
            finalPath.ShouldBeNull();
        }

        #endregion

        #region Combine Tests

        public virtual IEnumerable<object[]> CombinePathsWithValidOthersData => new[]
        {
            new[]
            {
                Default.PathName,
                Default.PathName,
                Default.PathName + PathInformation.DirectorySeparatorChar + Default.PathName,
            },
            new[]
            {
                Default.PathName + PathInformation.DirectorySeparatorChar,
                Default.PathName,
                Default.PathName + PathInformation.DirectorySeparatorChar + Default.PathName,
            },
            new[]
            {
                Default.PathName + PathInformation.AltDirectorySeparatorChar,
                Default.PathName,
                Default.PathName + PathInformation.AltDirectorySeparatorChar + Default.PathName,
            },
            new[]
            {
                // Combine discards first rooted path.
                (AbsolutePath / "Foo").ToString(),
                (AbsolutePath / "Bar").ToString(),
                (AbsolutePath / "Bar").ToString(),
            },
        };

        public abstract IEnumerable<object[]> CombinePathsWithInvalidOthersData { get; }

        [TestMethod]
        public void Combine_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => TestFolder.Path.Combine((string)null!));
        }

        [TestMethod]
        [DynamicInstanceData(nameof(CombinePathsWithValidOthersData))]
        public void Combine_StandardPathAndValidOther_CombinesPaths(string initialPathString, string other, string expectedPathString)
        {
            var path = FileSystem.GetPath(initialPathString);
            var finalPath = path.Combine(other);
            finalPath.ToString().ShouldBe(expectedPathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(CombinePathsWithInvalidOthersData))]
        public void Combine_StandardPathAndValidOther_CombinesPaths(string initialPathString, string other)
        {
            var path = FileSystem.GetPath(initialPathString);
            Should.Throw<ArgumentException>(() => path.Combine(other));
        }

        #endregion

        #region TryCombine Tests

        [TestMethod]
        public void TryCombine_NullParameters_ReturnsFalseAndNull()
        {
            var result = TestFolder.Path.TryCombine((string?)null, out var finalPath);
            result.ShouldBeFalse();
            finalPath.ShouldBeNull();
        }

        [TestMethod]
        [DynamicInstanceData(nameof(CombinePathsWithValidOthersData))]
        public void TryCombine_StandardPathAndValidOther_CombinesPaths(string initialPathString, string other, string expectedPathString)
        {
            var path = FileSystem.GetPath(initialPathString);
            var result = path.TryCombine(other, out var finalPath);
            result.ShouldBeTrue();
            finalPath.ShouldNotBeNull();
            finalPath!.ToString().ShouldBe(expectedPathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(CombinePathsWithInvalidOthersData))]
        public void TryCombine_StandardPathAndValidOther_ReturnsFalseAndNull(string initialPathString, string other)
        {
            var path = FileSystem.GetPath(initialPathString);
            var result = path.TryCombine(other, out var finalPath);
            result.ShouldBeFalse();
            finalPath.ShouldBeNull();
        }

        #endregion

        #region Join Tests

        public virtual IEnumerable<object[]> JoinPathsWithValidOthersData => new[]
        {
            new[]
            {
                Default.PathName,
                Default.PathName,
                Default.PathName + PathInformation.DirectorySeparatorChar + Default.PathName,
            },
            new[]
            {
                Default.PathName + PathInformation.DirectorySeparatorChar,
                Default.PathName,
                Default.PathName + PathInformation.DirectorySeparatorChar + Default.PathName,
            },
            new[]
            {
                Default.PathName + PathInformation.AltDirectorySeparatorChar,
                Default.PathName,
                Default.PathName + PathInformation.AltDirectorySeparatorChar + Default.PathName,
            },
            new[]
            {
                // Join doesn't merge two separators.
                Default.PathName + PathInformation.DirectorySeparatorChar,
                PathInformation.DirectorySeparatorChar.ToString() + Default.PathName,
                Default.PathName + PathInformation.DirectorySeparatorChar + PathInformation.DirectorySeparatorChar + Default.PathName,
            },
            new[]
            {
                // Join preserves any separators >= 2.
                Default.PathName + PathInformation.DirectorySeparatorChar + PathInformation.DirectorySeparatorChar,
                PathInformation.DirectorySeparatorChar.ToString() + PathInformation.DirectorySeparatorChar.ToString() + Default.PathName,
                Default.PathName + new string(PathInformation.DirectorySeparatorChar, 4) + Default.PathName,
            },
            new[]
            {
                // Join doesn't discard rooted paths.
                (AbsolutePath / "Foo").ToString(),
                (AbsolutePath / "Bar").ToString(),
                (AbsolutePath / "Foo").ToString() + PathInformation.DirectorySeparatorChar + (AbsolutePath / "Bar").ToString(),
            },
        };
        public abstract IEnumerable<object[]> JoinPathsWithInvalidOthersData { get; }

        [TestMethod]
        public void Join_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => TestFolder.Path.Join((string)null!));
        }

        [TestMethod]
        [DynamicInstanceData(nameof(JoinPathsWithValidOthersData))]
        public void Join_StandardPathAndValidOther_JoinsPaths(string initialPathString, string other, string expectedPathString)
        {
            var path = FileSystem.GetPath(initialPathString);
            var finalPath = path.Join(other);
            finalPath.ToString().ShouldBe(expectedPathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(JoinPathsWithInvalidOthersData))]
        public void Join_StandardPathAndValidOther_JoinsPaths(string initialPathString, string other)
        {
            var path = FileSystem.GetPath(initialPathString);
            Should.Throw<ArgumentException>(() => path.Join(other));
        }

        #endregion

        #region TryJoin Tests

        [TestMethod]
        public void TryJoin_NullParameters_ReturnsFalseAndNull()
        {
            var result = TestFolder.Path.TryJoin((string?)null, out var finalPath);
            result.ShouldBeFalse();
            finalPath.ShouldBeNull();
        }

        [TestMethod]
        [DynamicInstanceData(nameof(JoinPathsWithValidOthersData))]
        public void TryJoin_StandardPathAndValidOther_JoinsPaths(string initialPathString, string other, string expectedPathString)
        {
            var path = FileSystem.GetPath(initialPathString);
            var result = path.TryJoin(other, out var finalPath);
            result.ShouldBeTrue();
            finalPath.ShouldNotBeNull();
            finalPath!.ToString().ShouldBe(expectedPathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(JoinPathsWithInvalidOthersData))]
        public void TryJoin_StandardPathAndValidOther_ReturnsFalseAndNull(string initialPathString, string other)
        {
            var path = FileSystem.GetPath(initialPathString);
            var result = path.TryJoin(other, out var finalPath);
            result.ShouldBeFalse();
            finalPath.ShouldBeNull();
        }

        #endregion
    }
}
