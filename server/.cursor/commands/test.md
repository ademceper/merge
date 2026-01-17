---
title: Generate Tests
description: Creates unit tests for current file
---

Generate comprehensive unit tests for the current file:

1. Create test file at: `Merge.Tests/Unit/[matching path]/[FileName]Tests.cs`
2. Use xUnit + FluentAssertions + Moq
3. Follow AAA pattern (Arrange-Act-Assert)
4. Test naming: `[Method]_[Scenario]_[Expected]`
5. Cover:
   - Happy path scenarios
   - Edge cases
   - Error conditions
6. Run tests: `dotnet test --filter "FullyQualifiedName~[ClassName]"`
