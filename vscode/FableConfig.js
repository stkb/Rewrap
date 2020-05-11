const path = require("path");
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
  // These absolute paths may no longer be necessary with Fable 2
  entry: resolve("Fable.fsproj"),
  outDir: resolve("compiled"),
  babel: {
    presets: [[ "@babel/preset-env", { targets: { node: "current" }, modules: "commonjs" }]],
    sourceMaps: false,
  },
  fable: { },
  postbuild: runTests
}
