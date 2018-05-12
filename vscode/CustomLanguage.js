const Path = require('path')
const FS = require('fs')
const JSON = require('comment-json')

const getConfig = (getText, path) => {
    try {
        const cf = JSON.parse(getText(path), null, true)
        return { line: cf.comments.lineComment, block: cf.comments.blockComment }
    }
    catch(err) {
        return {}
    }
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
