const Path = require('path')
const FS = require('fs')
const JSON = require('comment-json')

const getConfig = (getText, path) => {
    let config = { line: null, block: null }
    let warnings = [], error
    try {
        const c = JSON.parse(getText(path), null, true).comments
        if(c) {
            // 'line' must be a string or null.
            if(typeof c.lineComment === 'string') config.line = c.lineComment
            else warnings.push("lineComment not a string.")
            // 'block' must be an array of 2 strings or null.
            if(Array.isArray(c.blockComment) && c.blockComment.length > 1)
                config.block = c.blockComment.slice(0, 2)
            else warnings.push("blockComment not a length-2 array.")
        }
        else warnings.push("No comments block found.")
    }
    catch(err) { error = err }
    if(error || warnings.length) {
        console.info(`Inspecting JSON file ${path}`)
        if(error) console.error(error.message)
        warnings.forEach(w => console.warn(w))
    }
    return config
}

/** Iterates through all extensions and populates the cache with mappings for
 *  each found language id to a path to a configuration file. */
const createCache = exts => {
    const addConfigFiles = (cache, ext) => {
        try {
            let obj = ext.packageJSON
            if((obj = obj.contributes) && (obj = obj.languages)) {
                for(const l of obj) {
                    if(!l.id) continue
                    if(l.configuration) {
                        const confPath = Path.join(ext.extensionPath, l.configuration)
                        cache[l.id] = confPath
                    }
                }
            }
        }
        catch(err) {
            console.error(err.message)
        }
        return cache
    }

    return exts.reduce(addConfigFiles, {})
}

/* Can take exts & getFileText mocks for testing */
const getCommentMarkers = (exts, getFileText) => {
    exts = exts || require('vscode').extensions.all
    if(!exts.length)
        console.warn("`vscode.extensions.all` returned an empty array. Something is wrong.")

    getFileText = getFileText || (p => FS.readFileSync(p))
    let cache = null
    return lang => {
        cache = cache || createCache(exts)

        if (cache[lang]) {
            if(typeof cache[lang] === 'string')
                cache[lang] = getConfig(getFileText, cache[lang])
            if(cache[lang].line || cache[lang].block) return cache[lang]
            else return null
        }
        else return null
    }
}

module.exports = getCommentMarkers
