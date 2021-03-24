import * as FS from 'fs'

// Reads the contents of a file as an array of strings
export const readFile = path => FS.readFileSync(path, {encoding: 'utf8'}).split(/\r?\n/)

export const files = readSpecs(".")

function readSpecs(dir) {
  if(FS.existsSync(dir + "/specs")) return readDir(dir + "/specs")
  else return readSpecs(dir + "/..")
}

// Gets as an array the full paths of all *.md files in a dir and all subdirs
function readDir (path) {
  const step = (acc, x) => {
    const fullName = path + "/" + x.name
    return x.isDirectory() ? [...acc, ...(readDir (fullName))]
          : fullName.endsWith(".md") ? [...acc, fullName]
          : acc
  }
  return FS.readdirSync(path, {withFileTypes: true}).reduce(step, [])
}
