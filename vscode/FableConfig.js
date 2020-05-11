const path = require("path");
const fableUtils = require("fable-utils");
const tests = require('../tests')

function resolve(filePath) {
  return path.resolve(__dirname, filePath)
}

function runTests() {
  try {
      tests.run()
  } catch (err) {
      console.error(err);
  }
}

module.exports = {
  entry: resolve("Fable.fsproj"),
  outDir: resolve("compiled"),
  babel: fableUtils.resolveBabelOptions({
    presets: [[ "env", { targets: { node: "current" }, modules: "commonjs" }]],
    sourceMaps: false,
  }),
  fable: { },
  postbuild: runTests
}
