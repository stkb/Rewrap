const path = require("path");
const fableUtils = require("fable-utils");
 
function resolve(relativePath) {
    return path.join(__dirname, relativePath);
}
 
function resolve(filePath) {
  return path.resolve(__dirname, filePath)
}

function runTests() {
  try {
      var scriptPath = resolve("../tests.js");
      console.log("Running tests")
      var childProcess = require("child_process");
      var path = require("path");
      var cp = childProcess.fork(scriptPath);
      cp.on("exit", function (code, signal) {
          if (code === 0) {
              console.log("Success");
          } else {
              console.log("Exit", { code: code, signal: signal });
          }
      });
      cp.on("error", console.error.bind(console));
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