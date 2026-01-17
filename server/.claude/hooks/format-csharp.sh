#!/bin/bash
# Auto-format C# files after edit
# Called by PostToolUse hook

FILE_PATH="$CLAUDE_FILE_PATH"

if [[ "$FILE_PATH" == *.cs ]]; then
    # Run dotnet format on the specific file
    dotnet format --include "$FILE_PATH" --verbosity quiet 2>/dev/null || true

    # Optional: Run analyzers
    # dotnet format analyzers --include "$FILE_PATH" --verbosity quiet 2>/dev/null || true
fi
