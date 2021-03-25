#!/bin/bash -e
# This script will break if args have spaces

args="$@"
operations="installdeps clean build watch test package"
components="core vscode"

supplied() {
    for a in $args; do [[ $a = $1 ]] && return; done
    false
}
suppliedAny () {
    for a in $@; do supplied $a && return; done
    false
}
suppliedAll () {
    for a in $@; do supplied $a || return; done
    true
}

production=false
suppliedAny --prod --production && production=true

if ! $production && ! suppliedAny $operations && ! suppliedAny $components; then
    echo 'Usage: ./do <operation(s) and/or component(s)> [--production]'
    echo
    echo "- Operations: $operations"
    echo "- Components: $components"
    echo
    echo "If no operations are given, does a 'build'. If no components are given, does all components."
    echo 'Adding the --production flag (alias --prod) does the operation in production mode.'
    echo
    echo 'Examples:'
    echo '    ./do build core'
    echo '    ./do package --production'
    echo
    exit 1
fi

# If no other operations given then 'build' is default
suppliedAny $operations || args="$args build"
# If no components given then assume all
suppliedAny $components || args="$args $components"

# If operation is package, only applying to vscode at the moment
supplied package && args="$args clean build core vscode"
# In the case of production build, always do clean & test & always build core
supplied build && $production && args="$args clean test core"

if supplied installdeps; then
    dotnet tool restore
    dotnet restore core/Core.Test.fsproj
    npm install
fi

if supplied clean; then
    supplied core && rm -rf core/bin
    supplied vscode && rm -rf vscode/bin && (cd vscode && npm install)
fi

if suppliedAll build core; then
    dotnet fable core/Core.Test.fsproj -o core/bin/Debug/js --noRestore
    $production && npx --silent parcel build core
fi

if suppliedAll test core; then
    if $production; then
        if [ -f core/bin/Release/js/index.js ]; then node core
        else ./do core --production; fi # Also does a test
    else
        [ -f core/bin/Debug/js/Tests.js ] || ./do core
        node core/bin/Debug/js/Tests.js
    fi
fi

if suppliedAll build vscode; then
    [ -d "/path/to/dir" ] || (cd vscode && npm install)
    npx --silent tsc -p vscode --noEmit
    if $production; then
        (cd vscode && npm install)
        npx --silent eslint vscode --ext .ts
        npx --silent parcel build vscode --no-source-maps
    else npx --silent parcel build vscode --no-optimize
    fi
fi

if suppliedAll test vscode; then
    # It won't run if we're in vscode terminal
    [ ! "$TERM_PROGRAM" == "vscode" ] && node vscode.test/run && echo 'VS Code tests passed.'
fi

if supplied package; then
    (cd vscode && npx --silent vsce package -o Rewrap-VSCode.vsix)
fi

# Watch only works on core so far
if supplied watch; then
    dotnet fable watch core/Core.Test.fsproj -o core/bin/Debug/js --noRestore \
        --runWatch 'node core/bin/Debug/js/Tests.js'
fi

exit 0
