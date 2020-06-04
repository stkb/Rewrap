const path = require("path");
const tests = require('../tests')

function runTests() {
  try {
      tests.run()
  } catch (err) {
      console.error(err);
  }
}

module.exports = {
  entry: "../Core/Core.fsproj",
  outDir: "compiled",
  babel: {
    presets: [[ "@babel/preset-env", { targets: { node: "current" }, modules: "commonjs" }]],
    sourceMaps: false,
  },
  fable: { },
  postbuild: runTests
}
