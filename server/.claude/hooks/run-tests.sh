#!/bin/bash
# Run related tests after code changes
# Can be used with PostToolUse hook

FILE_PATH="$CLAUDE_FILE_PATH"
PROJECT_ROOT="/Users/adem/Desktop/merge/server"

# Extract the file name without extension
FILE_NAME=$(basename "$FILE_PATH" .cs)

# Find related test files
TEST_FILES=$(find "$PROJECT_ROOT/Merge.Tests" -name "*${FILE_NAME}*Test*.cs" 2>/dev/null)

if [ -n "$TEST_FILES" ]; then
    echo "Running related tests for $FILE_NAME..."

    for TEST_FILE in $TEST_FILES; do
        TEST_CLASS=$(basename "$TEST_FILE" .cs)
        dotnet test "$PROJECT_ROOT/Merge.Tests" --filter "FullyQualifiedName~$TEST_CLASS" --verbosity minimal
    done
else
    echo "No test files found for $FILE_NAME"
fi
