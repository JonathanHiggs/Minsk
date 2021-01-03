#!/bin/bash

# Vars
slndir="$(dirname "${BASH_SOURCE[0]}")"

# Restore + build
dotnet build "$slndir/Minsk.Compiler" --nologo -v q || exit

# Run
dotnet run -p "$slndir/Minsk.Compiler" --no-build -- "$@"