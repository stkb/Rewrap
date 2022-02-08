import JSON from 'json5'
import vscode from 'vscode'
import {CustomMarkers, noCustomMarkers} from './Core'

const getConfig = (getText, path) => {
  let config = {line: null, block: null}
  let warnings: string[] = [], error
  try {
    const c = JSON.parse(getText(path)).comments
    if (c) {
      // 'line' must be a string or null.
      if (typeof c.lineComment === 'string') config.line = c.lineComment
      else warnings.push("lineComment not a string.")
      // 'block' must be an array of 2 strings or null.
      if (Array.isArray(c.blockComment) && c.blockComment.length > 1)
        config.block = c.blockComment.slice(0, 2)
      else warnings.push("blockComment not a length-2 array.")
    }
    else warnings.push("No comments block found.")
  }
  catch (err) { error = err }
  if (error || warnings.length) {
    console.info(`Inspecting JSON file ${path}`)
    if (error) console.error(error.message)
    warnings.forEach(w => console.warn(w))
  }
  return config
}

type Cache = Record<string, string | CustomMarkers>

/** Iterates through all extensions and populates the cache with mappings for each found
 *  language id to a path to a configuration file. */
function createCache (exts) : Cache {
  const addConfigFiles = (cache, ext: vscode.Extension<any>) => {
    try {
      let obj = ext.packageJSON
      if ((obj = obj.contributes) && (obj = obj.languages)) {
        for (const l of obj) {
          if (!l.id) continue
          if (l.configuration) {
            const confPath = ext.extensionPath + '/' + l.configuration
            cache[l.id] = confPath
          }
        }
      }
    }
    catch (err) {
      console.error(err.message)
    }
    return cache
  }

  return exts.reduce(addConfigFiles, {})
}

/* Can take exts & getFileText mocks for testing */
export default function (exts?, getFileText?) {
  exts = exts || vscode.extensions.all
  if (!exts.length)
    console.warn("`vscode.extensions.all` returned an empty array. Something is wrong.")
  if (!getFileText) {
    try { getFileText = require('fs').readFileSync }
    catch { getFileText = () => "" }
  }

  let cache
  return lang => {
    cache = cache || createCache(exts)

    if (typeof cache[lang] === 'string') {
      const config = getConfig(getFileText, cache[lang])
      cache[lang] = (config.line || config.block) ?
        {line: config.line, block: config.block || ["", ""]} : noCustomMarkers
    }
    else if (!cache[lang]) cache[lang] = noCustomMarkers
    return cache[lang]
  }
}
