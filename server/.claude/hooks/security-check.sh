#!/bin/bash
# Security check hook for dangerous commands
# This script is called before each Bash command execution

COMMAND="$CLAUDE_TOOL_INPUT"

# Check for dangerous patterns
DANGEROUS_PATTERNS=(
    "rm -rf /"
    "drop database"
    "truncate table"
    "delete from.*where 1"
    "--force.*origin"
    "push.*--force"
    "format c:"
    "mkfs"
    "> /dev/sda"
    "dd if=/dev/zero"
)

for pattern in "${DANGEROUS_PATTERNS[@]}"; do
    if echo "$COMMAND" | grep -qiE "$pattern"; then
        echo "{\"decision\": \"deny\", \"reason\": \"Dangerous command pattern detected: $pattern\"}"
        exit 0
    fi
done

# Check for secret exposure
SECRET_PATTERNS=(
    "POSTGRES_PASSWORD"
    "JWT_SECRET"
    "API_KEY"
    "GITHUB_TOKEN"
)

for pattern in "${SECRET_PATTERNS[@]}"; do
    if echo "$COMMAND" | grep -qE "echo.*$pattern"; then
        echo "{\"decision\": \"deny\", \"reason\": \"Potential secret exposure detected\"}"
        exit 0
    fi
done

echo "{\"decision\": \"allow\"}"
