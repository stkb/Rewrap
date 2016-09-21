declare module 'word-wrap' {
    function wordWrap(
        text: string,
        options: {
            width: number,
            indent?: string,
        }
    ) : string;
    
    export = wordWrap;
}