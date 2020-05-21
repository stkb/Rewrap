const Path = require('path')
const FS = require('fs')
const JSON = require('comment-json')

const getConfig = (getText, path) => {
    let config = { line: null, block: null }
    try {
        const c = JSON.parse(getText(path), null, true).comments
        if(c) {
            // 'line' must be a string or null.
            if(typeof c.lineComment === 'string') config.line = c.lineComment
            // 'block' must be an array of 2 strings or null.
            if(Array.isArray(c.blockComment) && c.blockComment.length > 1)
                config.block = c.blockComment.slice(0, 2)
        }
    }
    catch(err) { }
    return config
}

/** Iterates through all extensions and populates the cache with mappings for
 *  each found language id to a path to a configuration file. */
const createCache = exts => {
    return exts.reduce((cache, e) => {
        try {
            let obj = e.packageJSON
            if((obj = obj.contributes) && (obj = obj.languages)) {
                for(const l of obj) {
                    if(l.id && !cache[l.id] && l.configuration) {
                        cache[l.id] = Path.join(e.extensionPath, l.configuration)
                    }
                }
            }
        }
        catch(_) {}
        return cache
    }, {})
}

const getCommentMarkers = (exts, getFileText) => {
    exts = exts || require('vscode').extensions.all
    getFileText = getFileText || (p => FS.readFileSync(p))
    let cache = null
    return lang => {
        cache = cache || createCache(exts)
        if (cache[lang]) {
            if(typeof cache[lang] === 'string')
                cache[lang] = getConfig(getFileText, cache[lang])
            return (cache[lang].line || cache[lang].block) ? cache[lang] : null
        }
        else return null
    }
}

module.exports = getCommentMarkers
