const path = require("path");
const fableUtils = require("fable-utils");
 
function resolve(relativePath) {
    return path.join(__dirname, relativePath);
}
 
module.exports = {
  entry: resolve("Fable.fsproj"),
  outDir: resolve("compiled"),
  babel: fableUtils.resolveBabelOptions({
    presets: [[ "env", { targets: { node: "current" }, modules: "commonjs" }]],
    sourceMaps: false,
  }),
  fable: { }
}