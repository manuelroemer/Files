namespace Files.Specification.Tests
{
    using System;
    using System.Collections.Generic;
    using Files.Specification.Tests.Assertions;
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

        // Shortcuts for the separator, because typing PathInformation.DirectorySeparatorChar.ToString()
        // adds too much noise in the test data below.
        private string Sep => PathInformation.DirectorySeparatorChar.ToString();
        private string AltSep => PathInformation.AltDirectorySeparatorChar.ToString();

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
            // For paths like "foo///bar", the trailing separators are discarded after removal of the last segment.
            new[]
            {
                (
                    (
                        ParentDirectoryPath +
                        PathInformation.DirectorySeparatorChar.ToString() +
                        PathInformation.DirectorySeparatorChar.ToString() +
                        PathInformation.DirectorySeparatorChar.ToString()
                    )
                    / Default.PathName
                ).ToString(),
                PathInformation.ParentDirectorySegment,
            },
        };

        public virtual IEnumerable<object[]> ParentPathsWithoutParentData => new[]
        {
            new[] { AbsolutePath.Root!.ToString() },
            new[] { Sep },
            new[] { AltSep },
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
            path.FullPath.ShouldBeWithNormalizedPathSeparators(fullPath);
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
                Sep,
                "",
            },
            new[]
            {
                AltSep,
                "",
            },
            new[]
            {
                // A single trailing separator is ignored.
                Default.PathName + Sep,
                Default.PathName,
            },
            new[]
            {
                // A single trailing separator is ignored.
                Default.PathName + AltSep,
                Default.PathName,
            },
            new[]
            {
                // Multiple trailing separators lead to empty name.
                Default.PathName + Sep + Sep,
                "",
            },
            new[]
            {
                // Multiple trailing separators lead to empty name.
                Default.PathName + AltSep + AltSep,
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
                Sep,
                "",
            },
            new[]
            {
                AltSep,
                "",
            },
            new[]
            {
                // A single trailing separator is ignored.
                (AbsolutePath / Default.PathName).ToString() + Sep,
                Default.PathNameWithoutExtension,
            },
            new[]
            {
                // A single trailing separator is ignored.
                Default.PathName + AltSep,
                Default.PathNameWithoutExtension,
            },
            new[]
            {
                // Multiple trailing separators lead to empty name.
                Default.PathName + Sep + Sep,
                "",
            },
            new[]
            {
                // Multiple trailing separators lead to empty name.
                Default.PathName + AltSep + AltSep,
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
                Default.PathName + Sep,
                Default.PathNameExtension,
            },
            new[]
            {
                // A single trailing separator is ignored.
                Default.PathName + AltSep,
                Default.PathNameExtension,
            },
        };

        public virtual IEnumerable<object[]> ExtensionPathsWithoutExtensionData => new[]
        {
            new[] { Default.PathNameWithoutExtension },
            new[] { Default.PathNameWithoutExtension + PathInformation.ExtensionSeparatorChar },
            new[] { PathInformation.CurrentDirectorySegment },
            new[] { PathInformation.ParentDirectorySegment },
            new[] { Sep },
            new[] { AltSep },
            new[] { Default.PathName + Sep + Sep },
            new[] { Default.PathName + AltSep + AltSep },
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

        #region EndsWithDirectorySeparator Tests

        public virtual IEnumerable<object[]> EndsWithDirectorySeparatorPathsWithTrailingDirectorySeparatorData => new[]
        {
            new[] { Sep },
            new[] { AltSep },
            new[] { Default.PathName + Sep },
            new[] { Default.PathName + AltSep },
        };

        public virtual IEnumerable<object[]> EndsWithDirectorySeparatorPathsWithoutTrailingDirectorySeparatorData => new[]
        {
            new[] { Default.PathName },
            new[] { Default.PathName + Sep + " " },
            new[] { Default.PathName + AltSep + " " },
        };

        [TestMethod]
        [DynamicInstanceData(nameof(EndsWithDirectorySeparatorPathsWithTrailingDirectorySeparatorData))]
        public void EndsWithDirectorySeparator_PathWithTrailingDirectorySeparator_ReturnsTrue(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            path.EndsWithDirectorySeparator.ShouldBeTrue();
        }

        [TestMethod]
        [DynamicInstanceData(nameof(EndsWithDirectorySeparatorPathsWithoutTrailingDirectorySeparatorData))]
        public void EndsWithDirectorySeparator_PathWithoutTrailingDirectorySeparator_ReturnsFalse(string pathString)
        {
            var path = FileSystem.GetPath(pathString);
            path.EndsWithDirectorySeparator.ShouldBeFalse();
        }

        #endregion

        #region TrimEndingDirectorySeparator Tests

        public virtual IEnumerable<object[]> TrimEndingDirectorySeparatorPathsWithTrailingDirectorySeparatorData => new[]
        {
            new[] { Default.PathName + Sep },
            new[] { Default.PathName + AltSep },
        };

        public virtual IEnumerable<object[]> TrimEndingDirectorySeparatorPathsWithoutTrailingDirectorySeparatorData => new[]
        {
            new[] { Default.PathName },
            new[] { Default.PathName + Sep + " " },
            new[] { Default.PathName + AltSep + " " },
        };

        public virtual IEnumerable<object[]> TrimEndingDirectorySeparatorUntrimmablePathsData => new[]
        {
            new[] { Sep },
            new[] { AltSep },
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
                Sep,
                Default.PathName + Sep,
            },
            new[]
            {
                Default.PathName,
                AltSep,
                Default.PathName + AltSep,
            },
            new[]
            {
                Default.PathName,
                Sep + Sep,
                Default.PathName + Sep + Sep,
            },
            new[]
            {
                Default.PathName,
                AltSep + AltSep,
                Default.PathName + AltSep + AltSep,
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
            finalPath.ShouldBeWithNormalizedPathSeparators(expectedPathString);
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
            finalPath.ShouldBeWithNormalizedPathSeparators(expectedPathString);
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
                Default.PathName + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName + Sep,
                Default.PathName,
                Default.PathName + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName + AltSep,
                Default.PathName,
                Default.PathName + AltSep + Default.PathName,
            },
            new[]
            {
                Default.PathName + Sep + Sep,
                Default.PathName,
                Default.PathName + Sep + Sep + Default.PathName,
            },
            new[]
            {
                Sep,
                Default.PathName,
                Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName,
                Sep,
                Sep,
            },
            new[]
            {
                Sep,
                Sep,
                Sep,
            },

            // Combine discards the first path on subsequent separators.
            new[]
            {
                Default.PathName + Sep,
                Sep + Default.PathName,
                Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName + AltSep,
                AltSep + Default.PathName,
                AltSep + Default.PathName,
            },

            // Combine returns the first path if the other one is empty.
            new[]
            {
                Default.PathName,
                "",
                Default.PathName,
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
            finalPath.ShouldBeWithNormalizedPathSeparators(expectedPathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(CombinePathsWithInvalidOthersData))]
        public void Combine_StandardPathAndInvalidOther_ThrowsArgumentException(string initialPathString, string other)
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
            finalPath.ShouldBeWithNormalizedPathSeparators(expectedPathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(CombinePathsWithInvalidOthersData))]
        public void TryCombine_StandardPathAndInvalidOther_ReturnsFalseAndNull(string initialPathString, string other)
        {
            var path = FileSystem.GetPath(initialPathString);
            var result = path.TryCombine(other, out var finalPath);
            result.ShouldBeFalse();
            finalPath.ShouldBeNull();
        }

        [TestMethod]
        public void TryCombine_TwoAbsolutePaths_ReturnsSecondPath()
        {
            var path1 = AbsolutePath / "Foo";
            var path2 = AbsolutePath / "Bar";

            // Depending on whether two concatenated absolute paths are considered valid or not,
            // Join is also allowed to return false.
            var result = path1.TryCombine(path2, out var combinedPath);
            result.ShouldBeTrue();
            combinedPath.ShouldNotBeNull();
            combinedPath.ShouldBeWithNormalizedPathSeparators(path2.ToString());
        }

        #endregion

        #region Join Tests

        public virtual IEnumerable<object[]> JoinPathsWithValidOthersData => new[]
        {
            new[]
            {
                Default.PathName,
                Default.PathName,
                Default.PathName + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName + Sep,
                Default.PathName,
                Default.PathName + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName + AltSep,
                Default.PathName,
                Default.PathName + AltSep + Default.PathName,
            },
            new[]
            {
                Default.PathName,
                Sep,
                Default.PathName + Sep,
            },
            new[]
            {
                Sep,
                Default.PathName,
                Sep + Default.PathName,
            },
            // Fails on old .NET versions because // is an invalid UNC path. Should still be supported.
            // new[]
            // {
            //     Sep,
            //     Sep,
            //     Sep + Sep,
            // },
            
            // Join preservers any separators >= 2.
            new[]
            {
                Default.PathName + Sep,
                Sep + Default.PathName,
                Default.PathName + Sep + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName + AltSep,
                AltSep + Default.PathName,
                Default.PathName + AltSep + AltSep + Default.PathName,
            },
            new[]
            {
                Default.PathName + Sep + Sep,
                Sep + Sep + Default.PathName,
                Default.PathName + Sep + Sep + Sep + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName + Sep + Sep,
                Default.PathName,
                Default.PathName + Sep + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName,
                Sep + Sep + Default.PathName,
                Default.PathName + Sep + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName,
                Sep,
                Default.PathName + Sep,
            },

            // Join returns the first path if the other one is empty.
            new[]
            {
                Default.PathName,
                "",
                Default.PathName,
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
            finalPath.ShouldBeWithNormalizedPathSeparators(expectedPathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(JoinPathsWithInvalidOthersData))]
        public void Join_StandardPathAndInvalidOther_ThrowsArgumentException(string initialPathString, string other)
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
            finalPath.ShouldBeWithNormalizedPathSeparators(expectedPathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(JoinPathsWithInvalidOthersData))]
        public void TryJoin_StandardPathAndInvalidOther_ReturnsFalseAndNull(string initialPathString, string other)
        {
            var path = FileSystem.GetPath(initialPathString);
            var result = path.TryJoin(other, out var finalPath);
            result.ShouldBeFalse();
            finalPath.ShouldBeNull();
        }

        #endregion

        #region Link Tests

        public virtual IEnumerable<object[]> LinkPathsWithValidOthersData => new[]
        {
            new[]
            {
                Default.PathName,
                Default.PathName,
                Default.PathName + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName + Sep,
                Default.PathName,
                Default.PathName + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName + AltSep,
                Default.PathName,
                Default.PathName + AltSep + Default.PathName,
            },

            // Link strips all separators but one.
            new[]
            {
                Default.PathName + Sep,
                Sep + Default.PathName,
                Default.PathName + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName + AltSep,
                AltSep + Default.PathName,
                Default.PathName + AltSep + Default.PathName,
            },
            new[]
            {
                Default.PathName + Sep + Sep,
                Sep + Sep + Default.PathName,
                Default.PathName + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName + Sep + Sep,
                Default.PathName,
                Default.PathName + Sep + Default.PathName,
            },
            new[]
            {
                Default.PathName,
                Sep + Sep + Default.PathName,
                Default.PathName + Sep + Default.PathName,
            },

            // Link inserts a separator even if one path is empty after trimming.
            new[]
            {
                Default.PathName,
                Sep,
                Default.PathName + Sep,
            },
            new[]
            {
                Sep,
                Default.PathName,
                Sep + Default.PathName,
            },

            // Link returns a separator if both paths are empty after trimming.
            new[]
            {
                Sep,
                Sep,
                Sep,
            },

            // Link returns the first path if the second one is empty.
            new[]
            {
                Default.PathName,
                "",
                Default.PathName,
            },
        };

        public abstract IEnumerable<object[]> LinkPathsWithInvalidOthersData { get; }

        [TestMethod]
        public void Link_NullParameters_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => TestFolder.Path.Link((string)null!));
        }

        [TestMethod]
        [DynamicInstanceData(nameof(LinkPathsWithValidOthersData))]
        public void Link_StandardPathAndValidOther_LinksPaths(string initialPathString, string other, string expectedPathString)
        {
            var path = FileSystem.GetPath(initialPathString);
            var finalPath = path.Link(other);
            finalPath.ShouldBeWithNormalizedPathSeparators(expectedPathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(LinkPathsWithInvalidOthersData))]
        public void Link_StandardPathAndInvalidOther_ThrowsArgumentException(string initialPathString, string other)
        {
            var path = FileSystem.GetPath(initialPathString);
            Should.Throw<ArgumentException>(() => path.Link(other));
        }

        #endregion

        #region TryLink Tests

        [TestMethod]
        public void TryLink_NullParameters_ReturnsFalseAndNull()
        {
            var result = TestFolder.Path.TryLink((string?)null, out var finalPath);
            result.ShouldBeFalse();
            finalPath.ShouldBeNull();
        }

        [TestMethod]
        [DynamicInstanceData(nameof(LinkPathsWithValidOthersData))]
        public void TryLink_StandardPathAndValidOther_LinksPaths(string initialPathString, string other, string expectedPathString)
        {
            var path = FileSystem.GetPath(initialPathString);
            var result = path.TryLink(other, out var finalPath);
            result.ShouldBeTrue();
            finalPath.ShouldNotBeNull();
            finalPath.ShouldBeWithNormalizedPathSeparators(expectedPathString);
        }

        [TestMethod]
        [DynamicInstanceData(nameof(LinkPathsWithInvalidOthersData))]
        public void TryLink_StandardPathAndInvalidOther_ReturnsFalseAndNull(string initialPathString, string other)
        {
            var path = FileSystem.GetPath(initialPathString);
            var result = path.TryLink(other, out var finalPath);
            result.ShouldBeFalse();
            finalPath.ShouldBeNull();
        }

        #endregion
    }
}
